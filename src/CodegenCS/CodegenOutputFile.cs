using System;

namespace CodegenCS
{
    /// <summary>
    /// CodegenTextWriter with added properties that describe Outputfile location (RelativePath)
    /// </summary>
    public class CodegenOutputFile : CodegenTextWriter, ICodegenOutputFile
    {
        /// <summary>
        /// Relative path of the output file (relative to the outputFolder)
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        /// Creates a new OutputFile, with a relative path
        /// </summary>
        /// <param name="relativePath"></param>
        public CodegenOutputFile(string relativePath) : base()
        {
            this.RelativePath = relativePath;
            _dependencyContainer.RegisterSingleton<ICodegenOutputFile>(() => this);
            _dependencyContainer.RegisterSingleton<CodegenOutputFile>(() => this);
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
