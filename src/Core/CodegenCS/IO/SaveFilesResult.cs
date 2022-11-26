using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.IO
{
    public class SaveFilesResult
    {
        public List<string> SavedFiles { get; internal set; }

        /// <summary>
        /// List of files that exist under outputFolder and that were NOT saved as part of this ICodegenContext.
        /// Only returned if getUnknownFiles is true
        /// </summary>
        public List<string> UnknownFiles { get; internal set; }

    }
}
