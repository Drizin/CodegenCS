using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    interface ICodegenContext
    {
        CodegenOutputFile this[string relativePath] { get; }
        List<string> Errors { get; }
        void SaveFiles(string outputFolder);
    }
    interface ICodegenContext<FT>
        where FT : struct, IComparable, IConvertible, IFormattable // FT should be enum. 
    {
        CodegenOutputFile<FT> this[string relativePath] { get; }
        CodegenOutputFile<FT> this[string relativePath, FT fileType] { get; }
        List<string> Errors { get; }
        void SaveFiles(string outputFolder);
    }
}
