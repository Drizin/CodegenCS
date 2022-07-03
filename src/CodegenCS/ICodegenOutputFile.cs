using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    public interface ICodegenOutputFile : ICodegenTextWriter
    {
        string RelativePath { get; }
    }
    public interface ICodegenOutputFile<FT> : ICodegenOutputFile
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        FT FileType { get; set; }
    }

}
