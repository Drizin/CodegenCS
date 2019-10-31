using CodegenCS;
using System;
using System.IO;

namespace EF6POCOGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            string projectFolder = System.IO.Path.Combine(CodegenCS.Utils.IO.GetCurrentDirectory().FullName, @"..\EF6-POCO-Generator.SampleOutput");
            CodegenContext context = new CodegenContext(outputFolder: projectFolder + "\\GeneratedCode");
            Generator generator = new Generator(
                context: context,
                createConnection: () => new System.Data.SqlClient.SqlConnection(@"
                    Data Source=LENOVOFLEX5\SQLEXPRESS;
                    Initial Catalog=Northwind;
                    Integrated Security=True;
                    Application Name=EntityFramework POCO Generator"
                ),
                targetFrameworkVersion: 4.5m
                );
            generator.GenerateMultipleFiles(); // generates in memory

            // since no errors, first modify csproj, then we save all files

            // One option is to generate all files and add each ont to the csproj
            MSBuildProjectEditor editor = new MSBuildProjectEditor(projectFilePath: projectFolder + @"\EF6-POCO-Generator.SampleOutput.csproj");
            //string templateFile = Path.Combine(CodegenCS.Utils.IO.GetCurrentDirectory().FullName);
            foreach (var o in context.OutputFilesAbsolute)
                editor.AddItem(itemPath: o.Key, itemType: o.Value.ItemType);
            editor.Save();
            context.SaveFiles(); //context.SaveFiles(deleteOtherFiles: true);

            // Other alternative is to generate a single file
            generator.GenerateSingleFile("Database.cs"); // generates in memory
            context.SaveFiles();
        }
    }
}
