using CodegenCS;
using CodegenCS.DotNet;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EF6POCOGenerator
{
    class Program
    {
        // Helpers to get the location of the current CSX script or Program.cs
        public static string GetScriptPath([CallerFilePath] string path = null) => path;
        public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

        static void Main(string[] args)
        {
            string projectFolder = new DirectoryInfo(System.IO.Path.Combine(GetScriptFolder(), @"..\EF6-POCO-Generator.SampleOutput")).FullName;
            DotNetCodegenContext context = new DotNetCodegenContext();
            Generator generator = new Generator(
                context: context,
                createConnection: () => new System.Data.SqlClient.SqlConnection(@"
                    Data Source=LENOVOFLEX5\SQLEXPRESS;
                    Initial Catalog=AdventureWorks;
                    Integrated Security=True;
                    Application Name=EntityFramework POCO Generator"
                ),
                targetFrameworkVersion: 4.5m
                );

            bool multipleFiles = true;

            if (multipleFiles)
            {
                generator.GenerateMultipleFiles(); // render files (in memory)
                                                   // since no errors, first modify csproj, then we save all files
                MSBuildProjectEditor editor = new MSBuildProjectEditor(projectFilePath: projectFolder + @"\EF6-POCO-Generator.SampleOutput.csproj");
                //string templateFile = Path.Combine(CodegenCS.Utils.IO.GetCurrentDirectory().FullName);
                foreach (var o in context.OutputFilesAbsolute(projectFolder + "\\GeneratedCode"))
                    editor.AddItem(itemPath: o.Key, itemType: o.Value.FileType);
                editor.RemoveUnusedItems(projectFolder + "\\GeneratedCode"); // remove from csproj all files under GeneratedCode except those which were added by AddItem above
                editor.Save();
                context.SaveFiles(outputFolder: projectFolder + "\\GeneratedCode");
            }
            else
            {
                // Other alternative is to generate a single file
                generator.GenerateSingleFile(file: projectFolder + "\\GeneratedCode\\Database.cs"); // render all classes in a single file (in memory)
                context.SaveFiles(); // since the file path was defined using absolute path, we don't have to specify base folder
            }
        }
    }
}
