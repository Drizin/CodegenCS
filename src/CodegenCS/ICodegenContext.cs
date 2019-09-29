using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    interface ICodegenContext
    {
        CodegenTextWriter GetTextWriter(string fileName);
        List<string> Errors { get; }
    }
}
