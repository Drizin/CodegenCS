using CodegenCS.___InternalInterfaces___;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using IOExtensions = global::CodegenCS.IO.Extensions;
using SaveFilesResult = global::CodegenCS.IO.SaveFilesResult;

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
        protected Dictionary<string, ICodegenOutputFile> _outputFiles = new Dictionary<string, ICodegenOutputFile>(StringComparer.InvariantCultureIgnoreCase); // key insensitive //TODO: support case-sensitive filesystems
        // TODO: https://stackoverflow.com/questions/4942624/how-to-convert-dictionarystring-object-to-dictionarystring-string-in-c-sha

        /// <inheritdoc />
        public IReadOnlyList<ICodegenOutputFile> OutputFiles => new ReadOnlyCollection<ICodegenOutputFile>(_outputFiles.Values.ToList());

        public virtual bool OnOutputFileRenamed(string oldRelativePath, string newRelativePath, ICodegenOutputFile outputFile)
        {
            if (!_outputFiles.ContainsKey(oldRelativePath))
                return false;
            if (oldRelativePath.Equals(newRelativePath))
                return false;
            if (_outputFiles.ContainsKey(oldRelativePath))
            {
                _outputFiles.Remove(oldRelativePath);
                _outputFiles.Add(newRelativePath, outputFile);
            }
            return true;
        }

        /// <inheritdoc />
        public ICodegenOutputFile DefaultOutputFile { get { return _defaultOutputFile; } }
        protected bool _defaultOutputFileIsInContext = false;
        protected ICodegenOutputFile _defaultOutputFile;
        protected Func<string, ICodegenContext, ICodegenOutputFile> _outputFileFactory;

        public DependencyContainer DependencyContainer { get { return _dependencyContainer; } }

        public HashSet<string> OutputFilesPaths => new HashSet<string>(_outputFiles.Keys);

        protected DependencyContainer _dependencyContainer = new DependencyContainer();
        #endregion

        #region ctors
        /// <inheritdocs />
        /// <param name="defaultOutputFileName">Filename for DefaultOutput</param>
        public CodegenContext(string defaultOutputFileName = "") 
            : this(
                    outputFileFactory: (string relativePath, ICodegenContext ctx) => new CodegenOutputFile(relativePath),
                    defaultOutputFileName: defaultOutputFileName
                  )
        {
        }

        /// <inheritdocs />
        /// <param name="defaultOutputFileName">Filename for DefaultOutput</param>
        /// <param name="outputFileFactory">Factory to build new Output Files (created through <see cref="this[string]"/> indexer)</param>
        public CodegenContext(Func<string, ICodegenContext, ICodegenOutputFile> outputFileFactory, string defaultOutputFileName = "")
        {
            _outputFileFactory = outputFileFactory;

            // Register this context iself in it's own container
            _dependencyContainer.RegisterSingleton<ICodegenContext>(this);
            _dependencyContainer.RegisterSingleton<CodegenContext>(this);

            string relativePath = defaultOutputFileName;

            var newOutputFile = _outputFileFactory(relativePath, this);

            // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context (into ICodegenOutputFile dependency container)
            newOutputFile.SetContext(this);

            //this._outputFiles[relativePath] = newOutputFile; 
            // The defaultOutputFile is added to the list of OutputFiles only at the first write:
            _defaultOutputFile = newOutputFile;
            _defaultOutputFile.Written += (s, e) =>
            {
                if (!_defaultOutputFileIsInContext)
                {
                    _outputFiles.Add(_defaultOutputFile.RelativePath, _defaultOutputFile);
                    _defaultOutputFileIsInContext = true;
                }
            };

            RegisterDefaultOutputFile();
        }

        protected void RegisterDefaultOutputFile()
        {
            // Register the defaultOutputFile in ICodegenContext dependency container
            _dependencyContainer.RegisterSingleton<ICodegenOutputFile>(() => _defaultOutputFile);
            _dependencyContainer.RegisterSingleton<ICodegenTextWriter>(() => _defaultOutputFile);
            if (_defaultOutputFile is CodegenOutputFile)
                _dependencyContainer.RegisterSingleton<CodegenOutputFile>(() => (CodegenOutputFile)_defaultOutputFile);
            if (_defaultOutputFile is CodegenTextWriter)
                _dependencyContainer.RegisterSingleton<CodegenTextWriter>(() => (CodegenTextWriter)_defaultOutputFile);
            if (_defaultOutputFile is TextWriter)
                _dependencyContainer.RegisterSingleton<TextWriter>(() => (TextWriter)_defaultOutputFile);

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
                    var newOutputFile = _outputFileFactory(relativePath, this);

                    // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context (into ICodegenOutputFile dependency container)
                    newOutputFile.SetContext(this);

                    this._outputFiles[relativePath] = newOutputFile;
                }
                return this._outputFiles[relativePath];
            }
        }
        #endregion

        #region I/O
        /// <inheritdoc/>
        [Obsolete("Please use CodegenCS.IO extension SaveToFolder()")]
        public virtual SaveFilesResult SaveFiles(string outputFolder)
        {
            return IOExtensions.SaveToFolder(this, outputFolder);
        }

        /// <summary>
        /// Saves all files in the current directory. <br />
        /// According to the RelativePath of each file they may be saved in different folders
        /// </summary>
        [Obsolete("Please use CodegenCS.IO extension SaveToFolder()")]
        public virtual void SaveFiles()
        {
            SaveFiles(Environment.CurrentDirectory);
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
        public virtual IContextedTemplateWrapper<T, ICodegenContext> LoadTemplate<T>(params object[] dependencies) where T : IBaseTemplate
        {
            return new ContextedTemplateWrapper<T, ICodegenContext>(typeof(T), dependencies) { CodegenContext = this };
        }

        public virtual ICodegenContext RenderTemplate(ICodegenTemplate template)
        {
            template.Render(this.DefaultOutputFile);
            return this;
        }
        public virtual ICodegenContext RenderTemplate(ICodegenStringTemplate template)
        {
            FormattableString formattable = template.Render();
            this.DefaultOutputFile.Write(formattable);
            return this;
        }
        public virtual ICodegenContext RenderTemplate(ICodegenMultifileTemplate template)
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
        /// Func to get the Default Type for new OutputFiles
        /// </summary>
        protected Func<string, FT> _getDefaultType { get; } = null;

        #region ICodegenContext<FT, O> Implementation (this is the default implementation, meaning it will hide base class)
        public new IReadOnlyList<O> OutputFiles => ((ICodegenContext<FT, O>)this).OutputFiles;
        IReadOnlyList<O> ICodegenContext<FT, O>.OutputFiles => new ReadOnlyCollection<O>(new List<O>(_outputFiles.Values.Cast<O>()));
        public new O DefaultOutputFile => ((ICodegenContext<FT, O>)this).DefaultOutputFile;
        O ICodegenContext<FT, O>.DefaultOutputFile => (O)_defaultOutputFile;
        #endregion

        #region ICodegenContext<FT> Implementation (Conflicting interface)
        IReadOnlyList<ICodegenOutputFile<FT>> ICodegenContext<FT>.OutputFiles => new ReadOnlyCollection<ICodegenOutputFile<FT>>(new List<ICodegenOutputFile<FT>>(_outputFiles.Values.Cast<ICodegenOutputFile<FT>>()));
        ICodegenOutputFile<FT> ICodegenContext<FT>.DefaultOutputFile => (ICodegenOutputFile<FT>)_defaultOutputFile;
        #endregion
        
        #region ICustomWriterCodegenContext<O> (Conflicting Interface)
        IReadOnlyList<O> ICustomWriterCodegenContext<O>.OutputFiles => new ReadOnlyCollection<O>(_outputFiles.Values.Cast<O>().ToList());
        O ICustomWriterCodegenContext<O>.DefaultOutputFile => (O)_defaultOutputFile;
        #endregion




        protected new Func<string, FT, ICodegenContext, O> _outputFileFactory;
        #endregion

        #region ctors
        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="defaultType">Default Type for files (if file type is not defined)</param>
        /// <param name="outputFileFactory">Factory to build new Output Files (created through <see cref="this[string]"/> indexer)</param>
        /// <param name="defaultOutputFileName">Filename for DefaultOutput</param>
        public CodegenContext(FT defaultType, Func<string, FT, ICodegenContext, O> outputFileFactory, string defaultOutputFileName = "")
            : this(
                      getDefaultType: (relativePath) => defaultType,
                      outputFileFactory: outputFileFactory,
                      defaultOutputFileName: defaultOutputFileName
                  )
        {
            _outputFileFactory = outputFileFactory;
            _defaultOutputFile = outputFileFactory(null, defaultType, this);
        }

        /// <summary>
        /// Creates new in-memory context.
        /// </summary>
        /// <param name="getDefaultType">Default Type for files (if file type is not defined)</param>
        /// <param name="outputFileFactory">Factory to build new Output Files (created through <see cref="this[string]"/> indexer)</param>
        /// <param name="defaultOutputFileName">Filename for DefaultOutput</param>
        public CodegenContext(Func<string, FT> getDefaultType, Func<string, FT, ICodegenContext, O> outputFileFactory, string defaultOutputFileName = "") 
            : base(
                      outputFileFactory: (string relativePath, ICodegenContext ctx) => outputFileFactory(relativePath, getDefaultType(relativePath), ctx),
                      defaultOutputFileName: defaultOutputFileName
                  )
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
                FT type = _getDefaultType(relativePath);
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

                    // After creating ANY ICodegenOutputFile/ICodegenTextWriter we have to register the parent context (into ICodegenOutputFile dependency container)
                    newOutputFile.SetContext(this);
                    newOutputFile.FileType = fileType;

                    this._outputFiles[relativePath] = newOutputFile;
                }
                return (O)this._outputFiles[relativePath];
            }
        }
        #region ICodegenContext<FT> and ICustomWriterCodegenContext<O> implementation
        ICodegenOutputFile<FT> ICodegenContext<FT>.this[string relativePath, FT fileType] => this[relativePath, fileType];
        ICodegenOutputFile<FT> ICodegenContext<FT>.this[string relativePath] => this[relativePath];

        O ICustomWriterCodegenContext<O>.this[string relativePath] => this[relativePath];
        #endregion

        #endregion

        #region Overrides
        public override bool OnOutputFileRenamed(string oldRelativePath, string newRelativePath, ICodegenOutputFile outputFile)
        {
            bool renamed = base.OnOutputFileRenamed(oldRelativePath, newRelativePath, outputFile);
            if (Path.GetExtension(oldRelativePath)?.ToLower() != Path.GetExtension(newRelativePath)?.ToLower())
            {
                FT type = _getDefaultType(newRelativePath);
                ((ICodegenOutputFile<FT>)outputFile).FileType = type;
            }
            return renamed;
        }
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
