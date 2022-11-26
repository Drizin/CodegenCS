using System.IO;
using IOUtils = CodegenCS.Utils.IOUtils;

namespace CodegenCS.Runtime
{
    /// <summary>
    /// Provides information about the template being executed in Visual Studio
    /// </summary>
    public class VSExecutionContext : ExecutionContext
    {
        /// <summary>
        /// Full path of the Visual Studio Project that contains the template being executed
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// Full path of the Visual Studio Solution that contains the template being executed
        /// </summary>
        public string SolutionPath { get; set; }

        public string TemplateRelativePath { get; set; }

        //TODO: ProjectNamespace: 
        // first try Properties.Item("DefaultNamespace") - https://github.com/dotnet/roslyn/blob/afd10305a37c0ffb2cfb2c2d8446154c68cfa87a/src/VisualStudio/Core/Def/Implementation/ProjectSystem/VisualStudioProjectManagementService.cs#L50
        // else $(MSBuildProjectName.Replace(" ", "_"))

        // TODO: TemplateCalculatedNamespace - based on ProjectNamespace and TemplateRelativePath

        public VSExecutionContext(string templatePath, string projectPath, string solutionPath) : base(templatePath)
        {
            ProjectPath = projectPath;
            SolutionPath = solutionPath;
            TemplateRelativePath = IOUtils.MakeRelativePath(new FileInfo(this.ProjectPath).Directory.FullName + Path.DirectorySeparatorChar, this.TemplatePath);
        }
    }
}
