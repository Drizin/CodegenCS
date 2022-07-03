using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace CodegenCS
{

    /// <inheritdoc/>
    public class CodegenContext : ICodegenContext
    {
        #region Members
        /// <inheritdoc />
        public List<string> Errors { get; } = new List<string>();

        /// <summary>
        /// Output files indexed by their relative paths
        /// </summary>
        protected Dictionary<string, ICodegenOutputFile> _outputFiles = new Dictionary<string, ICodegenOutputFile>(StringComparer.InvariantCultureIgnoreCase); // key insensitive
        // TODO: https://stackoverflow.com/questions/4942624/how-to-convert-dictionarystring-object-to-dictionarystring-string-in-c-sha

        /// <inheritdoc />
        public IReadOnlyList<ICodegenOutputFile> OutputFiles { get { return _outputFiles.Values.ToList(); } }

        /// <inheritdoc />
        public ICodegenOutputFile DefaultOutputFile { get { return _defaultOutputFile; } }
        protected ICodegenOutputFile _defaultOutputFile;

        public DependencyContainer DependencyContainer { get { return _dependencyContainer; } }

        public HashSet<string> OutputFilesPaths => new HashSet<string>(_outputFiles.Keys);

        protected DependencyContainer _dependencyContainer = new DependencyContainer();
        #endregion

        #region ctors
        /// <inheritdocs />

        public CodegenContext() : this(new CodegenOutputFile(null))
        {
        }
        protected CodegenContext(ICodegenOutputFile defaultOutputFile)
        {
            // Register this context iself in it's own container
            _dependencyContainer.RegisterSingleton<ICodegenContext>(this);
            _dependencyContainer.RegisterSingleton<CodegenContext>(this);

            _defaultOutputFile = defaultOutputFile;
            RegisterDefaultOutputFile();
        }
        protected CodegenContext(Func<ICodegenContext, ICodegenOutputFile> defaultOutputFileFactory)
        {
            // Register this context iself in it's own container
            _dependencyContainer.RegisterSingleton<ICodegenContext>(this);
            _dependencyContainer.RegisterSingleton<CodegenContext>(this);

            _defaultOutputFile = defaultOutputFileFactory(this);
            RegisterDefaultOutputFile();
        }

        protected void RegisterDefaultOutputFile()
        {
            // Then the defaultOutputFile in it's own container
            _dependencyContainer.RegisterSingleton<ICodegenOutputFile>(() => _defaultOutputFile);
            _dependencyContainer.RegisterSingleton<ICodegenTextWriter>(() => _defaultOutputFile);
            if (_defaultOutputFile is CodegenOutputFile)
                _dependencyContainer.RegisterSingleton<CodegenOutputFile>(() => (CodegenOutputFile)_defaultOutputFile);
            if (_defaultOutputFile is CodegenTextWriter)
                _dependencyContainer.RegisterSingleton<CodegenTextWriter>(() => (CodegenTextWriter)_defaultOutputFile);

            // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context
            _defaultOutputFile.DependencyContainer.RegisterSingleton<ICodegenContext>(this);
            _defaultOutputFile.DependencyContainer.RegisterSingleton<CodegenContext>(this);
        }
        #endregion

        #region Indexer this[relativeFilePath]
        /// <inheritdoc/>
        public ICodegenOutputFile this[string relativePath]
        {
            get
            {
                if (!this._outputFiles.ContainsKey(relativePath))
                {
                    var newOutputFile = new CodegenOutputFile(relativePath);

                    // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context
                    newOutputFile.DependencyContainer.RegisterSingleton<ICodegenContext>(this);
                    newOutputFile.DependencyContainer.RegisterSingleton<CodegenContext>(this);

                    this._outputFiles[relativePath] = newOutputFile;
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
            var outputFilesAbsolutePaths = _outputFiles.Values.ToDictionary(v => Path.Combine(outputFolder, v.RelativePath), v => v);
            var generatedFiles = outputFilesAbsolutePaths.Keys.Select(p => p.ToLower()).ToList();
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
            FormattableString formattable = template.Render();
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


    /// <summary>
    /// CodegenContext keeps track of multiple files which can be saved at once in the output folder, <br />
    /// while also tracking the type for each output file
    /// </summary>
    /// <typeparam name="O">Class of OutputFiles. Should inherit from CodegenOutputFile</typeparam>
    /// <typeparam name="FT">Enum which defines the Types that each file can have</typeparam>
    public class CodegenContext<FT, O> : CodegenContext, ICodegenContext<FT, O>
        where O : ICodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        #region Members
        /// <summary>
        /// Default Type for new OutputFiles, if it's a fixed value.
        /// </summary>
        protected FT? _defaultType { get; } = null;

        /// <summary>
        /// Default Type for new OutputFiles, if it's a Func
        /// </summary>
        protected Func<string, FT> _getDefaultType { get; } = null;

        IReadOnlyList<ICodegenOutputFile<FT>> ICodegenContext<FT>.OutputFiles => new ReadOnlyCollection<ICodegenOutputFile<FT>>(new List<ICodegenOutputFile<FT>>(_outputFiles.Values.Cast<ICodegenOutputFile<FT>>()));
        IReadOnlyList<O> ICodegenContext<FT, O>.OutputFiles => new ReadOnlyCollection<O>(new List<O>(_outputFiles.Values.Cast<O>()));

        public new O DefaultOutputFile { get { return (O)_defaultOutputFile; } }
        O ICustomWriterCodegenContext<O>.DefaultOutputFile => (O)_defaultOutputFile;
        ICodegenOutputFile<FT> ICodegenContext<FT>.DefaultOutputFile => (ICodegenOutputFile<FT>)_defaultOutputFile;

        IReadOnlyList<O> ICustomWriterCodegenContext<O>.OutputFiles => _outputFiles.Values.Cast<O>().ToList();


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
            _defaultOutputFile = outputFileFactory(null, defaultType, this);
        }

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="getDefaultType">Default Type for files (if file type is not defined)</param>
        /// <param name="outputFileFactory">Factory to build output files</param>
        public CodegenContext(Func<string, FT> getDefaultType, Func<string, FT, ICodegenContext, O> outputFileFactory) : base((ctx) => outputFileFactory(null, getDefaultType(null), ctx))
        {
            _getDefaultType = getDefaultType;
            _outputFileFactory = outputFileFactory;
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
                    var newOutputFile = _outputFileFactory(relativePath, fileType, this);

                    // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context
                    newOutputFile.DependencyContainer.RegisterSingleton<ICodegenContext>(this);
                    newOutputFile.DependencyContainer.RegisterSingleton<CodegenContext>(this);
                    newOutputFile.FileType = fileType;

                    this._outputFiles[relativePath] = newOutputFile;
                }
                return (O)this._outputFiles[relativePath];
            }
        }
        #region Explicitly Implementing Conflicting Interfaces
        ICodegenOutputFile<FT> ICodegenContext<FT>.this[string relativePath, FT fileType] => this[relativePath, fileType];
        ICodegenOutputFile<FT> ICodegenContext<FT>.this[string relativePath] => this[relativePath];
        #endregion

        #endregion
    }


    public class CodegenContext<FT> : CodegenContext<FT, ICodegenOutputFile<FT>>, ICodegenContext<FT>
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
        public CodegenContext(FT defaultType)
            : base(defaultType, outputFileFactory: (string relativePath, FT fileType, ICodegenContext context) => new CodegenOutputFile<FT>(relativePath, fileType))
        {
        }

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="getDefaultType">Default Type for files (if file type is not defined)</param>
        public CodegenContext(Func<string, FT> getDefaultType)
            : base(getDefaultType, outputFileFactory: (string relativePath, FT fileType, ICodegenContext context) => new CodegenOutputFile<FT>(relativePath, fileType))
        {
        }
        #endregion

    }


}
