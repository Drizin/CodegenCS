using System;

namespace CodegenCS
{
    /// <summary>
    /// CodegenTextWriter with added properties that describe Outputfile location (RelativePath)
    /// </summary>
    public class CodegenOutputFile : CodegenTextWriter, ICodegenOutputFile
    {
        private string _relativePath;

        /// <summary>
        /// Relative path of the output file (relative to the outputFolder).
        /// For the <see cref="ICodegenContext.DefaultOutputFile"/> it will be initially empty
        /// </summary>
        public string RelativePath { 
            get => _relativePath; 
            set 
            {
                if (_relativePath != value)
                    _context?.OnOutputFileRenamed(_relativePath, value);  
                _relativePath = value; 
            } 
        }

        protected ICodegenContext _context { get; set; }

        /// <summary>
        /// Creates a new OutputFile, with a relative path
        /// </summary>
        /// <param name="relativePath"></param>
        internal CodegenOutputFile(string relativePath) : base()
        {
            _relativePath = relativePath;
            _dependencyContainer.RegisterSingleton<ICodegenOutputFile>(() => this);
            _dependencyContainer.RegisterSingleton<CodegenOutputFile>(() => this);
        }

        public void SetContext(ICodegenContext context)
        {
            _context = context;
        }
    }

    /// <summary>
    /// CodegenTextWriter with added properties that describe Outputfile location (RelativePath) <br />
    /// and type of output (regarding .NET Project - if file is Compiled, NotCompiled, etc)
    /// </summary>
    public class CodegenOutputFile<FT> : CodegenOutputFile, ICodegenOutputFile<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {

        /// <summary>
        /// Type of file
        /// </summary>
        public FT FileType { get; set; }

        /// <summary>
        /// Creates a new OutputFile, with a relative path
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="fileType"></param>
        public CodegenOutputFile(string relativePath, FT fileType) : base(relativePath)
        {
            this.FileType = fileType;
        }
    }
}
