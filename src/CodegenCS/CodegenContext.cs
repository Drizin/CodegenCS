using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS
{

    /// <inheritdoc/>
    public class CodegenContext : ICodegenContext
    {
        #region Members
        /// <summary>
        /// Output files indexed by their relative paths
        /// </summary>
        private Dictionary<string, CodegenOutputFile> _outputFiles = new Dictionary<string, CodegenOutputFile>(StringComparer.InvariantCultureIgnoreCase); // key insensitive

        /// <summary>
        /// Output files
        /// </summary>
        public List<CodegenOutputFile> OutputFiles { get { return _outputFiles.Values.ToList(); } }

        /// <summary>
        /// Output files, indexed by their relative paths
        /// </summary>
        public Dictionary<string, CodegenOutputFile> OutputFilesRelative { get { return _outputFiles; } }

        /// <summary>
        /// Output files, indexed by their absolute paths
        /// </summary>
        public Dictionary<string, CodegenOutputFile> OutputFilesAbsolute(string outputFolder)
        {
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            return _outputFiles.Values.ToDictionary(v => Path.Combine(outputFolder, v.RelativePath), v => v);
        }

        /// <summary>
        /// If your template finds any error you can just append the errors here in this list <br />
        /// SaveFiles() does not work if there is any error.
        /// </summary>
        public List<string> Errors { get; } = new List<string>();

        public ICodegenOutputFile DefaultOutputFile { get { return _defaultOutputFile; } }
        protected CodegenOutputFile _defaultOutputFile;
        public DependencyContainer DependencyContainer { get { return _dependencyContainer; } }
        protected DependencyContainer _dependencyContainer;
        #endregion

        #region ctors
        /// <inheritdocs />
        public CodegenContext()
        {
            _defaultOutputFile = new CodegenOutputFile(null);
            
            _dependencyContainer = new DependencyContainer();
            _dependencyContainer.RegisterSingleton<ICodegenContext>(this);
            _dependencyContainer.RegisterSingleton<CodegenContext>(this);
            _dependencyContainer.RegisterSingleton<ICodegenOutputFile>(() => this.DefaultOutputFile);
            _dependencyContainer.RegisterSingleton<CodegenOutputFile>(() => this._defaultOutputFile);
            _dependencyContainer.RegisterSingleton<ICodegenTextWriter>(() => this.DefaultOutputFile);
            _dependencyContainer.RegisterSingleton<CodegenTextWriter>(() => this._defaultOutputFile);

        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public ICodegenOutputFile this[string relativePath]
        {
            get
            {
                if (!this._outputFiles.ContainsKey(relativePath))
                {
                    this._outputFiles[relativePath] = new CodegenOutputFile(relativePath);
                    this._outputFiles[relativePath].DependencyContainer.RegisterSingleton<ICodegenContext>(this);
                    this._outputFiles[relativePath].DependencyContainer.RegisterSingleton<CodegenContext>(this);
                }
                return this._outputFiles[relativePath];
            }
        }
        #endregion

        //TODO: protected IPersistenceProvider PersistenceProvider { get; protected set; } = new DiskPersistenceProvider()
        #region I/O
        /// <summary>
        /// Saves all files in the outputFolder. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        /// <param name="outputFolder"></param>
        public void SaveFiles(string outputFolder)
        {
            if (this.Errors.Any())
                throw new Exception(this.Errors.First());

            outputFolder = new DirectoryInfo(outputFolder).FullName;
            foreach (var f in this._outputFiles)
            {
                string absolutePath = Path.Combine(outputFolder, f.Value.RelativePath);
                f.Value.SaveToFile(absolutePath);
            }
        }
        /// <summary>
        /// Saves all files in the current directory. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        public void SaveFiles()
        {
            SaveFiles(Environment.CurrentDirectory);
        }

        /// <summary>
        /// After calling SaveFiles() you may decide to clean-up the Outputfolder (assuming it only has code-generation output). <br />
        /// This returns all files which are in the Outputfolder (and subfolders) and which were NOT generated as part of this Context. <br />
        /// Beware that files which are deleted using File.Delete do NOT get moved to Recycle bin.
        /// </summary>
        /// <returns></returns>
        public List<string> GetUnknownFiles(string outputFolder)
        {
            var files = new DirectoryInfo(outputFolder).GetFiles("*.*", SearchOption.AllDirectories);
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            var generatedFiles = OutputFilesAbsolute(outputFolder).Keys.Select(p => p.ToLower()).ToList();
            List<string> unknownFiles = new List<string>();

            if (!generatedFiles.Any())
                return unknownFiles;

            foreach (var file in files)
            {
                if (!generatedFiles.Contains(file.FullName.ToLower()))
                    unknownFiles.Add(file.FullName);
            }
            return unknownFiles;
        }

        #endregion

        #region Dependency Injection Container
        /// <summary>
        /// Creates an instance of a dependency <typeparamref name="T"/> (usually a Template) and (if constructor needs) it injects ICodegenContext or ICodegenTextWriter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        protected T ResolveDependency<T>(params object[] otherDependencies) where T : class
        {
            return (T)_dependencyContainer.Resolve(typeof(T), otherDependencies);
        }

        /// <summary>
        /// Creates an instance of a dependency <paramref name="type"/> (usually a Template) and (if constructor needs) it injects ICodegenContext or ICodegenTextWriter
        /// </summary>
        protected object ResolveDependency(Type type, params object[] otherDependencies)
        {
            return _dependencyContainer.Resolve(type, otherDependencies);
        }
        #endregion

        #region Templates
        public IContextedTemplateWrapper<T, ICodegenContext> LoadTemplate<T>(params object[] dependencies) where T : IBaseTemplate
        {
            return new ContextedTemplateWrapper<T, ICodegenContext>(typeof(T), dependencies) { CodegenContext = this };
        }

        public ICodegenContext RenderTemplate(ICodegenTemplate template)
        {
            template.Render(this.DefaultOutputFile);
            return this;
        }
        public ICodegenContext RenderTemplate(ICodegenStringTemplate template)
        {
            FormattableString formattable = template.GetTemplate();
            this.DefaultOutputFile.Write(formattable);
            return this;
        }
        public ICodegenContext RenderTemplate(ICodegenMultifileTemplate template)
        {
            template.Render(this);
            return this;
        }
        #endregion
    }

    public class CustomWriterCodegenContext<O> : CodegenContext, ICustomWriterCodegenContext<O>
        where O : CodegenOutputFile
    {
        #region Members
        private Dictionary<string, O> _outputFiles = new Dictionary<string, O>(StringComparer.InvariantCultureIgnoreCase); // key insensitive
        public new List<O> OutputFiles { get { return _outputFiles.Values.ToList(); } }
        public new Dictionary<string, O> OutputFilesRelative { get { return _outputFiles; } }

        /// <summary>
        /// Output files, indexed by their absolute paths
        /// </summary>
        public new Dictionary<string, O> OutputFilesAbsolute(string outputFolder)
        {
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            return _outputFiles.Values.ToDictionary(v => Path.Combine(outputFolder, v.RelativePath), v => v);
        }

        public new O DefaultOutputFile { get { return _defaultOutputFile; } }
        protected new O _defaultOutputFile;
        protected Func<string, ICodegenContext, O> _outputFileFactory;
        #endregion

        #region ctors
        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="outputFileFactory">Factory to build output files</param>
        public CustomWriterCodegenContext(Func<string, ICodegenContext, O> outputFileFactory) : base()
        {
            _defaultOutputFile = _outputFileFactory(null, this);
        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public new O this[string relativePath]
        {
            get
            {
                return (O) base[relativePath];
            }
        }
        #endregion
    }

    public class MultipleFiletypeCodegenContext<FT> : CodegenContext<CodegenOutputFile<FT>, FT>, IMultipleFiletypeCodegenContext<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        #region Members
        new protected Func<string, FT, ICodegenContext, CodegenOutputFile> _outputFileFactory;
        #endregion

        #region ctors
        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="defaultType">Default Type for files (if file type is not defined)</param>
        public MultipleFiletypeCodegenContext(FT defaultType) 
            : base(defaultType, outputFileFactory: (string relativePath, FT fileType, ICodegenContext context) => new CodegenOutputFile<FT>(relativePath, fileType))
        {
        }

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="getDefaultType">Default Type for files (if file type is not defined)</param>
        public MultipleFiletypeCodegenContext(Func<string, FT> getDefaultType) 
            : base(getDefaultType, outputFileFactory: (string relativePath, FT fileType, ICodegenContext context) => new CodegenOutputFile<FT>(relativePath, fileType))
        {
        }
        #endregion

    }

    /// <summary>
    /// CodegenContext keeps track of multiple files which can be saved at once in the output folder, <br />
    /// while also tracking the type for each output file
    /// </summary>
    /// <typeparam name="O">Class of OutputFiles. Should inherit from CodegenOutputFile</typeparam>
    /// <typeparam name="FT">Enum which defines the Types that each file can have</typeparam>
    public class CodegenContext<O, FT> : CodegenContext, ICodegenContext<O, FT>
        where O : CodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        #region Members
        /// <summary>
        /// Default Type for new OutputFiles, if it's a fixed value.
        /// </summary>
        protected FT? _defaultType { get; /*set;*/ } = null;

        /// <summary>
        /// Default Type for new OutputFiles, if it's a Func
        /// </summary>
        protected Func<string, FT> _getDefaultType { get; /*set;*/ } = null;

        private Dictionary<string, O> _outputFiles = new Dictionary<string, O>(StringComparer.InvariantCultureIgnoreCase); // key insensitive
        public new List<O> OutputFiles { get { return _outputFiles.Values.ToList(); } }
        public new Dictionary<string, O> OutputFilesRelative { get { return _outputFiles; } }

        /// <summary>
        /// Output files, indexed by their absolute paths
        /// </summary>
        public new Dictionary<string, O> OutputFilesAbsolute(string outputFolder)
        {
            outputFolder = new DirectoryInfo(outputFolder).FullName;
            return _outputFiles.Values.ToDictionary(v => Path.Combine(outputFolder, v.RelativePath), v => v);
        }
        public new O DefaultOutputFile { get { return (O)_defaultOutputFile; } }

        protected Func<string, FT, ICodegenContext, O> _outputFileFactory;
        #endregion

        #region ctors
        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="defaultType">Default Type for files (if file type is not defined)</param>
        /// <param name="outputFileFactory">Factory to build output files</param>
        public CodegenContext(FT defaultType, Func<string, FT, ICodegenContext, O> outputFileFactory) : base()
        {
            _defaultType = defaultType;
            _outputFileFactory = outputFileFactory;
            _defaultOutputFile = _outputFileFactory(null, defaultType, this);
        }

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="getDefaultType">Default Type for files (if file type is not defined)</param>
        /// <param name="outputFileFactory">Factory to build output files</param>
        public CodegenContext(Func<string, FT> getDefaultType, Func<string, FT, ICodegenContext, O> outputFileFactory) : base()
        {
            _getDefaultType = getDefaultType;
            _outputFileFactory = outputFileFactory;
            _defaultOutputFile = outputFileFactory(null, _getDefaultType(null), this);
        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        public new O this[string relativePath]
        {
            get
            {
                FT type = _defaultType ?? _getDefaultType(relativePath);
                return this[relativePath, type];
            }
        }

        /// <summary>
        /// Output files are indexed by their relative path. <br />
        /// If context doesn't have an OutputFile with this relative path, a new one will automatically be created
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileType"></param>
        /// <returns></returns>
        public O this[string relativePath, FT fileType]
        {
            get
            {
                if (!this._outputFiles.ContainsKey(relativePath))
                {
                    this._outputFiles[relativePath] = _outputFileFactory(relativePath, fileType, this);
                    this._outputFiles[relativePath].FileType = fileType;
                    this._outputFiles[relativePath].DependencyContainer.RegisterSingleton<ICodegenContext>(this);
                    this._outputFiles[relativePath].DependencyContainer.RegisterSingleton<CodegenContext>(this);
                }
                return this._outputFiles[relativePath];
            }
        }
        #region Explicit IMultiplefiletypeCodegenContext<FT>
        ICodegenOutputFile<FT> IMultipleFiletypeCodegenContext<FT>.this[string relativePath, FT fileType] { 
            get => this[relativePath, fileType];
            //set => this[relativePath, fileType] = (O)value; 
        }
        ICodegenOutputFile<FT> IMultipleFiletypeCodegenContext<FT>.this[string relativePath] {
            get => this[relativePath];
            //set => this[relativePath] = (O)value;
        }
        #endregion

        #endregion
    }
    
}
