using EnvDTE;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace RunTemplate.CustomToolGenerator
{
    /// <summary>
    /// This CustomTool is invoked not only by the custom tool (auto executed or manually executed) but also by the custom Menu/Command
    /// So the File Extension doesn't matter (CSX, CGCS, or CS which can be set to Compile Remove to avoid breaking the owner csproj)
    /// The custom tool can also be lenient if the template did NOT implement the expected interfaces - it can search for different entrypoints/signatures and inject whatever is needed.
    /// </summary>
    [Guid("CA74B7A2-1AFE-4503-B4D7-67207DE213FD")]
    public sealed class RunTemplateCustomTool : BaseCodeGeneratorWithSite
    {
        public const string CustomToolName = "CodegenCS"; // this name is registered by [ProvideCodeGenerator] (so Custom Tool in Properties should be set to "CodegenCS")

        protected override string GetDefaultExtension()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            // if GenerateCode returns null then int IVsSingleFileGenerator.Generate returns VSConstants.E_FAIL and this isn't even called.
            return null;
        }

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            //return Encoding.UTF8.GetBytes("Testing" + Guid.NewGuid().ToString());

            ProjectItem projectItem = GetProjectItem();
            //Solution solution = projectItem.DTE.Solution;
            //Project project = projectItem.ContainingProject;
            //inputFileName is same as projectItem.Properties.Item("FullPath").Value.ToString() or projectItem.FileNames[1]
            string dir = new FileInfo(inputFileName).Directory.FullName;

            string f1 = Path.Combine(dir, "File1.cs");
            File.WriteAllText(f1, "/* File1.cs */");
            ProjectItem outputItem1 = projectItem.ProjectItems.AddFromFile(f1);
            outputItem1.Properties.Item("DependentUpon").Value = projectItem.Name; // TODO: check if DependentUpon works for old non-sdk-style. If needed check https://github.com/madskristensen/FileNesting 
            outputItem1.Properties.Item("ItemType").Value = "Compile"; /// When we use DotNetCodegenContext it will automatically set ICodegenOutputFile{FT}.FileType to BuildActionType.Compile (code), EmbeddedResource (resx) or else None (templates may override those types)

            string f2 = Path.Combine(dir, "File2.cs");
            File.WriteAllText(f2, "/* File2.cs */");
            ProjectItem outputItem2 = projectItem.Collection.AddFromFile(f2);
            outputItem2.Properties.Item("DependentUpon").Value = projectItem.Name;
            outputItem1.Properties.Item("ItemType").Value = "Compile";


            string f3 = Path.Combine(dir, Path.GetFileNameWithoutExtension(projectItem.Name) + ".log");
            File.WriteAllText(f3, "Output of project");
            ProjectItem outputItem3 = projectItem.Collection.AddFromFile(f3);
            outputItem3.Properties.Item("ItemType").Value = "None";
            outputItem3.Properties.Item("DependentUpon").Value = projectItem.Name;

            // When we use DotNetCodegenContext (or custom) we'll have to skip NonProjectItem, and for sdk-style depending on the file extension we may have to explicitly <Compile Remove> it.

            return null;
        }
    }
}
