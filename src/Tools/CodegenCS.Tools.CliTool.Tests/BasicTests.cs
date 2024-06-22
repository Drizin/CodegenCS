using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Net;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using NUnit.Framework.Legacy;

namespace CodegenCS.Tools.CliTool.Tests
{
    internal class BasicTests : BaseTest
    {

        [Test]
        public async Task GetHelp()
        {
            var result = await Run("/?");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains($"dotnet-codegencs.exe version {typeof(DotNetTool.Program).Assembly.GetName().Version}", _stdOut);
            StringAssert.Contains($"CodegenCS.Core.dll version {typeof(CodegenCS.CodegenContext).Assembly.GetName().Version}", _stdOut);
            StringAssert.Contains("Usage:\r\n  dotnet-codegencs [command] [options]\r\n", _stdOut);
        }

        #region Template Clone
        [Test]
        public async Task CloneByAlias1()
        {
            var result = await Run("template clone simplepocos");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("""
                Output file 'SimplePocos.cs'
                Downloading from 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs'
                Template 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs' was successfully saved into 'SimplePocos.cs'
                Building 'SimplePocos.cs'...
                """, _stdOut);
            StringAssert.Contains("""
                Successfully built template into 'SimplePocos.dll'.
                Loading 'SimplePocos.dll'...
                WARNING: Templating interfaces ICodegenTemplate/ICodegenMultifileTemplate/ICodegenStringTemplate are deprecated and should be replaced by TemplateMain() entrypoint.
                Template entry-point: 'SimplePOCOGenerator.Render()'...
                To generate a DatabaseSchema model use: 'dotnet-codegencs model dbschema extract <MSSQL|PostgreSQL> <connectionString> <output>'
                For help: 'dotnet-codegencs model dbschema extract /?'
                For a sample schema please check out: 'https://github.com/Drizin/CodegenCS/blob/master/src/Models/CodegenCS.Models.DbSchema.SampleDatabases/AdventureWorksSchema.json'
                To run this template use: 'dotnet-codegencs template run SimplePocos.dll <DatabaseSchemaModel>'
                For help: 'dotnet-codegencs template run /?'
                """, _stdOut);
            StringAssert.Contains("To generate a DatabaseSchema model use: 'dotnet-codegencs model dbschema extract <MSSQL|PostgreSQL> <connectionString> <output>'", _stdOut);
            FileAssert.Exists("SimplePocos.cs");
            FileAssert.Exists("SimplePocos.dll");
        }

        [Test]
        public async Task CloneByFullUrl()
        {
            var result = await Run("template clone https://github.com/CodegenCS/Templates/DatabaseSchema/SimplePocos/SimplePocos.cs");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("""
                Output file 'SimplePocos.cs'
                Downloading from 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs'
                Template 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs' was successfully saved into 'SimplePocos.cs'
                Building 'SimplePocos.cs'...
                """, _stdOut);
            StringAssert.Contains("""
                Successfully built template into 'SimplePocos.dll'.
                Loading 'SimplePocos.dll'...
                WARNING: Templating interfaces ICodegenTemplate/ICodegenMultifileTemplate/ICodegenStringTemplate are deprecated and should be replaced by TemplateMain() entrypoint.
                Template entry-point: 'SimplePOCOGenerator.Render()'...
                To generate a DatabaseSchema model use: 'dotnet-codegencs model dbschema extract <MSSQL|PostgreSQL> <connectionString> <output>'
                For help: 'dotnet-codegencs model dbschema extract /?'
                For a sample schema please check out: 'https://github.com/Drizin/CodegenCS/blob/master/src/Models/CodegenCS.Models.DbSchema.SampleDatabases/AdventureWorksSchema.json'
                To run this template use: 'dotnet-codegencs template run SimplePocos.dll <DatabaseSchemaModel>'
                For help: 'dotnet-codegencs template run /?'
                """, _stdOut);
            FileAssert.Exists("SimplePocos.cs");
            FileAssert.Exists("SimplePocos.dll");
        }

        [Test]
        public async Task CloneByShortUrl()
        {
            var result = await Run("template clone DatabaseSchema/SimplePocos/SimplePocos.cs");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("""
                Output file 'SimplePocos.cs'
                Downloading from 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs'
                Template 'https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/SimplePocos/SimplePocos.cs' was successfully saved into 'SimplePocos.cs'
                Building 'SimplePocos.cs'...
                """, _stdOut);
            StringAssert.Contains("""
                Successfully built template into 'SimplePocos.dll'.
                Loading 'SimplePocos.dll'...
                WARNING: Templating interfaces ICodegenTemplate/ICodegenMultifileTemplate/ICodegenStringTemplate are deprecated and should be replaced by TemplateMain() entrypoint.
                Template entry-point: 'SimplePOCOGenerator.Render()'...
                To generate a DatabaseSchema model use: 'dotnet-codegencs model dbschema extract <MSSQL|PostgreSQL> <connectionString> <output>'
                For help: 'dotnet-codegencs model dbschema extract /?'
                For a sample schema please check out: 'https://github.com/Drizin/CodegenCS/blob/master/src/Models/CodegenCS.Models.DbSchema.SampleDatabases/AdventureWorksSchema.json'
                To run this template use: 'dotnet-codegencs template run SimplePocos.dll <DatabaseSchemaModel>'
                For help: 'dotnet-codegencs template run /?'
                """, _stdOut);
            FileAssert.Exists("SimplePocos.cs");
            FileAssert.Exists("SimplePocos.dll");
        }
        #endregion

        #region Template Build

        [TestCase("https://raw.githubusercontent.com/Drizin/CodegenCS/master/src/Models/CodegenCS.Models.DbSchema.SampleDatabases/AdventureWorksSchema.json")]
        [TestCase("https://raw.githubusercontent.com/CodegenCS/Templates/main/OpenAPI/SampleModels/petstore-openapi3.json")]
        public async Task DownloadSampleModels(string url)
        {
            string localPath = url.Substring(url.LastIndexOf("/") + 1);
            if (File.Exists(localPath))
                File.Delete(localPath);
            await new WebClient().DownloadFileTaskAsync(new Uri(url), localPath);
            FileAssert.Exists(localPath);           
        }

        [Test]
        public async Task BuildTemplate()
        {
            if (File.Exists("SimplePocos.dll"))
                File.Delete("SimplePocos.dll");
            FileAssert.Exists("SimplePocos.cs");
            var result = await Run("template build SimplePocos");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Building 'SimplePocos.cs'...", _stdOut);
            StringAssert.Contains("Successfully built template into 'SimplePocos.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("SimplePocos.dll");
        }

        [Test]
        public async Task BuildTemplateWithCliReferences()
        {
            if (File.Exists("TemplateWithReferences.cs"))
                File.Delete("TemplateWithReferences.cs");
            File.WriteAllText("TemplateWithReferences.cs", """
                using System.IO;
                using System.Xml;
                using System;

                class MyTemplate
                {
                    async Task<FormattableString> Main(ILogger logger) 
                    {
                        XmlDocument doc = new XmlDocument();
                        await logger.WriteLineAsync($"Generating MyTemplate...");
                        return $"My first template";
                    }
                }
                """);
            var result = await Run("template build TemplateWithReferences.cs -r:System.Xml.dll -r:System.Xml.ReaderWriter.dll -r:System.Private.Xml.dll");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Building 'TemplateWithReferences.cs'...", _stdOut);
            StringAssert.Contains("Successfully built template into 'TemplateWithReferences.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("TemplateWithReferences.dll");
            File.Delete("TemplateWithReferences.cs");

            //TODO: will template run work with third-party (non-framework) references?
            string cmd = $"template run TemplateWithReferences.dll";
            result = await Run(cmd);
            Assert.AreEqual(0, result.ExitCode);
            FileAssert.Exists("TemplateWithReferences.generated.cs");
            StringAssert.Contains("My first template", File.ReadAllText(("TemplateWithReferences.generated.cs")));
        }
        #endregion

        #region Template Run
        [Test]
        public async Task RunFromDLL()
        {
            FileAssert.Exists("SimplePocos.dll");
            var result = await Run("template run SimplePocos.dll AdventureWorksSchema.json MyNamespace");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 71 files at folder '", _stdOut);
            StringAssert.Contains("Successfully executed template 'SimplePocos.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("Person.Address.generated.cs");
            StringAssert.Contains("namespace MyNamespace\r\n", File.ReadAllText(("Person.Address.generated.cs")));
        }

        [Test]
        public async Task BuildAndRunFromCS()
        {
            FileAssert.Exists("SimplePocos.cs");
            var result = await Run("template run SimplePocos.cs AdventureWorksSchema.json MyNamespace");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Building 'SimplePocos.cs'...", _stdOut);
            StringAssert.Contains("Successfully built template into '", _stdOut);
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 71 files at folder '", _stdOut);
            StringAssert.Contains("Successfully executed template", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("Person.Address.generated.cs");
            StringAssert.Contains("namespace MyNamespace\r\n", File.ReadAllText(("Person.Address.generated.cs")));
        }

        [Test]
        public async Task BuildAndRunFromCSInferExtension()
        {
            if (File.Exists("SimplePocos.dll"))
                File.Delete("SimplePocos.dll");
            FileAssert.Exists("SimplePocos.cs");
            var result = await Run("template run SimplePocos AdventureWorksSchema.json MyNamespace");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 71 files at folder '", _stdOut);
            StringAssert.Contains("Successfully executed template", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("Person.Address.generated.cs");
            StringAssert.Contains("namespace MyNamespace\r\n", File.ReadAllText(("Person.Address.generated.cs")));
        }

        [Test]
        public async Task RunFromDLLInferExtension()
        {
            FileAssert.Exists("SimplePocos.dll");
            var result = await Run("template run SimplePocos AdventureWorksSchema.json MyNamespace");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 71 files at folder '", _stdOut);
            StringAssert.Contains("Successfully executed template", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("Person.Address.generated.cs");
            StringAssert.Contains("namespace MyNamespace\r\n", File.ReadAllText(("Person.Address.generated.cs")));
        }

        [Test]
        public async Task RunTemplateWithCliReferences()
        {
            if (File.Exists("TemplateWithReferences.cs"))
                File.Delete("TemplateWithReferences.cs");
            File.WriteAllText("TemplateWithReferences.cs", """
                using System.IO;
                using System.Xml;
                using System;

                class MyTemplate
                {
                    async Task<FormattableString> Main(ILogger logger) 
                    {
                        XmlDocument doc = new XmlDocument();
                        await logger.WriteLineAsync($"Generating MyTemplate...");
                        return $"My first template";
                    }
                }
                """);
            var result = await Run("template run TemplateWithReferences.cs -r:System.Xml.dll -r:System.Xml.ReaderWriter.dll -r:System.Private.Xml.dll");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Building 'TemplateWithReferences.cs'...", _stdOut);
            StringAssert.Contains("Successfully built template into", _stdOut); // ... 'TemplateWithReferences.dll'
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("TemplateWithReferences.dll");
            FileAssert.Exists("TemplateWithReferences.generated.cs");
            StringAssert.Contains("My first template", File.ReadAllText(("TemplateWithReferences.generated.cs")));
            File.Delete("TemplateWithReferences.cs");
            File.Delete("TemplateWithReferences.generated.cs");
        }

        #endregion

        #region Template Arguments and Options
        [Test]
        public async Task MissingArgument()
        {
            FileAssert.Exists("SimplePocos.dll");
            var result = await Run("template run SimplePocos.dll AdventureWorksSchema.json");
            StringAssert.Contains("ERROR: Required argument 'Namespace' missing for command", _stdErr);
            StringAssert.Contains("Usage:\r\n  dotnet-codegencs template run SimplePocos.dll <Model> <Namespace> [options]\r\n", _stdOut);
            Assert.Negative(result.ExitCode);
        }


        [Test]
        public async Task Options1()
        {
            FileAssert.Exists("SimplePocos.dll");
            Directory.GetFiles(Directory.GetCurrentDirectory(), "*.generated.cs").ToList().ForEach(path => File.Delete(path));
            FileAssert.DoesNotExist("SimplePocos.generated.cs");
            var result = await Run("template run SimplePocos.dll AdventureWorksSchema.json MyNamespace2 -p:SingleFile");
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 1 file: '", _stdOut);
            StringAssert.Contains("Successfully executed template 'SimplePocos.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists("SimplePocos.generated.cs");
            FileAssert.DoesNotExist("Person.Address.generated.cs");
            StringAssert.Contains("namespace MyNamespace2\r\n", File.ReadAllText(("SimplePocos.generated.cs")));
            StringAssert.Contains("public partial class Address", File.ReadAllText(("SimplePocos.generated.cs")));
            StringAssert.Contains("public override bool Equals(object obj)", File.ReadAllText(("SimplePocos.generated.cs")));
        }

        [Test]
        public async Task Options2()
        {
            FileAssert.Exists("SimplePocos.dll");
            Directory.GetFiles(Directory.GetCurrentDirectory(), "*.generated.cs").ToList().ForEach(path => File.Delete(path));
            FileAssert.DoesNotExist("SimplePocos.generated.cs");
            var result = await Run("template run SimplePocos.dll AdventureWorksSchema.json MyNamespace2 -p:SingleFile -o SubFolder -f MyPocos.cs -p:GenerateEqualsHashCode false");
            StringAssert.Contains("Loading 'SimplePocos.dll'...", _stdOut);
            Assert.AreEqual(0, result.ExitCode);
            StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            StringAssert.Contains("Model successfuly loaded from 'AdventureWorksSchema.json'...", _stdOut);
            StringAssert.Contains("Generated 1 file: '", _stdOut);
            StringAssert.Contains("Successfully executed template 'SimplePocos.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists(".\\SubFolder\\MyPocos.cs");
            StringAssert.Contains("namespace MyNamespace2\r\n", File.ReadAllText((".\\SubFolder\\MyPocos.cs")));
            StringAssert.Contains("public partial class Address", File.ReadAllText((".\\SubFolder\\MyPocos.cs")));
            StringAssert.DoesNotContain("public override bool Equals(object obj)", File.ReadAllText((".\\SubFolder\\MyPocos.cs")));
        }

        #endregion

        #region Checking if all other Catalog Templates work
        [TestCase("SimplePocos", "DatabaseSchema")]
        [TestCase("DapperExtensionPocos", "DatabaseSchema")]
        [TestCase("DapperDalPocos", "DatabaseSchema")]
        [TestCase("DapperActiveRecordPocos", "DatabaseSchema")]
        [TestCase("NSwagClient", "OpenAPI")]
        public async Task TestTemplate(string templateAlias, string modelType)
        {
            if (File.Exists($"{templateAlias}.dll"))
                File.Delete($"{templateAlias}.dll");
            if (File.Exists($"{templateAlias}.cs"))
                File.Delete($"{templateAlias}.cs");

            var result = await Run($"template clone {templateAlias}");
            Assert.AreEqual(0, result.ExitCode);
            FileAssert.Exists($"{templateAlias}.cs");
            FileAssert.Exists($"{templateAlias}.dll");

            string model = (modelType == "DatabaseSchema" ? "AdventureworksSchema.json" : "petstore-openapi3.json");

            string cmd = $"template run {templateAlias}.dll {model} MyNamespace";
            if (modelType == "DatabaseSchema")
                cmd += " -p:SingleFile";
            result = await Run(cmd);
            Assert.AreEqual(0, result.ExitCode);

            StringAssert.Contains($"Loading '{templateAlias}.dll'...", _stdOut);
            if (modelType == "DatabaseSchema")
                StringAssert.Contains("Model type is 'CodegenCS.Models.DbSchema.DatabaseSchema'...", _stdOut);
            else
                StringAssert.Contains("Model type is 'NSwag.OpenApiDocument'...", _stdOut);
            StringAssert.Contains($"Model successfuly loaded from '{model}'...", _stdOut);
            StringAssert.Contains($"Successfully executed template '{templateAlias}.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists($"{templateAlias}.generated.cs");
            StringAssert.Contains("namespace MyNamespace\r\n", File.ReadAllText(($"{templateAlias}.generated.cs"))); 
        }
        #endregion
    }
}
