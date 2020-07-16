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

        /// <summary>
        /// Ensures that all files generated under this Context are added to the MSBuild project (csproj, vbproj) <br />
        /// This is mostly for old non-SDK-Style projects (before Visual Studio 2017), <br />
        /// since the new SDK-Style automatically builds all CS files which are under the csproj folder (they don't need to be explicitly referenced in csproj).
        /// </summary>
        /// <param name="csproj">Full path of csproj/vbproj</param>
        /// <param name="outputFolder">Base folder where your output files were saved. Same as used in SaveFiles()</param>
        public void AddToProject(string csproj, string outputFolder)
        {
            var projectEditor = new MSBuildProjectEditor(csproj);
            foreach (var file in this.OutputFiles)
                projectEditor.AddItem(Path.GetFullPath(Path.Combine(outputFolder, file.RelativePath)));
            projectEditor.Save();
        }
    }


}
