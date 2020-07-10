using CodegenCS;
using CodegenCS.DotNet;
using CodegenCS.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class MSBuildProjectEditorTests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }

        [Test]
        public void TestMSProj()
        {
            MSBuildProjectEditor editor = //new MSBuildProjectEditor(@"D:\Repositories\CodegenCS\src\CodegenCS.Tests\CodegenCS.Tests.csproj");
                new MSBuildProjectEditor(@"D:\Repositories\EntityFramework-Scripty-Templates4\src\Test.csproj");
            editor.AddItem(itemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\lala\Test.cs", parentItemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\GenerateEFModel.csx");
            editor.RemoveUnusedDependentItems(parentItemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\GenerateEFModel.csx");
            editor.Save();
        }

        [Test]
        public void TestContextMSProj()
        {
            MSBuildProjectEditor editor = new MSBuildProjectEditor(@"D:\Repositories\EntityFramework-Scripty-Templates4\src\Test.csproj");
            string outputFolder = @"D:\Repositories\CodegenCS\src\CodegenCS.Tests\";
            DotNetCodegenContext context = new DotNetCodegenContext();
            var file1 = context["File1.cs"];
            var file2 = context["Path2\\File2.cs"];
            file1.WriteLine("// Helloooooo");
            file2.WriteLine("// Hello from File2");
            string masterFile = @"D:\Repositories\CodegenCS\src\CodegenCS.Tests\CodegenTextWriterTests.cs";
            context.SaveFiles(outputFolder);
            editor.AddItem(masterFile);
            foreach (var o in context.OutputFilesAbsolute(outputFolder))
                editor.AddItem(itemPath: o.Key, parentItemPath: masterFile, itemType: o.Value.FileType);
            editor.Save();
        }

        [Test]
        public void TestDatabase()
        {
            MSBuildProjectEditor editor = new MSBuildProjectEditor(@"D:\Repositories\CodegenCS\src\CodegenCS.Tests\CodegenCS.Tests.csproj");
            string outputFolder = @"D:\Repositories\CodegenCS\src\CodegenCS.Tests\";
            DotNetCodegenContext context = new DotNetCodegenContext();

            string templateFile = @"D:\Repositories\CodegenCS\src\CodegenCS.Tests\CodegenTextWriterTests.cs";
            editor.AddItem(templateFile);

            Database db = Database.CreateSQLServerConnection(@"Data Source=LENOVOFLEX5\SQLEXPRESS;Initial Catalog=northwind;Integrated Security=True;Application Name=CodegenCS");
            var tables = db.Query("SELECT Name FROM sys.tables");
            foreach (var table in tables)
            {
                var file = context[$"{table.Name.ToString()}.cs"];
                file.WriteLine("// Helloooooo");
                foreach (var o in context.OutputFilesAbsolute(outputFolder))
                    editor.AddItem(itemPath: o.Key, parentItemPath: templateFile, itemType: o.Value.FileType);
            }

            context.SaveFiles(outputFolder);
            editor.Save();
        }


    }
}
