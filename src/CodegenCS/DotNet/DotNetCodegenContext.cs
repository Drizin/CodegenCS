using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CodegenCS.DotNet
{
    /// <summary>
    /// DotNetCodegenContext is like CodegenContext (keeps track of multiple files which can be saved at once in the output folder) <br />
    /// but additionally it also tracks the BuildActionType for each output file, <br />
    /// making it easier to save those files in the csproj (when applicable) with MSBuildProjectEditor class <br />
    /// If you're generating only .CS files for a SDK-Style project (those created with Visual Studio 2017 or newer) you can just use the regular CodegenContext,
    /// since you don't need to add the output cs files to the csproj file.
    /// </summary>
    public class DotNetCodegenContext : CodegenContext<BuildActionType>
    {
        /// <inheritdocs />
        public DotNetCodegenContext() : base(getDefaultType: GetFileType)
        {
        }

        /// <summary>
        /// Will infer BuildActionType from the file extension
        /// </summary>
        /// <param name="relativePath"></param>
        /// <returns></returns>
        private static BuildActionType GetFileType(string relativePath)
        {
            string extension = Path.GetExtension(relativePath)?? "";
            switch (extension.ToLower())
            {
                case ".cs":
                case ".vb":
                case ".fs":
                    return BuildActionType.Compile;
                case ".resx":
                    return BuildActionType.EmbeddedResource;
                default:
                    return BuildActionType.None;
            }
        }
    }


}
