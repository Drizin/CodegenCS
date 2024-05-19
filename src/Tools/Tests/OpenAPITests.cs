using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;
using CodegenCS.Runtime;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using CodegenCS.Models.NSwagAdapter;
using NSwag;


namespace CodegenCS.Tools.Tests
{
    internal class OpenAPITests : BaseTest
    {
        string _modelPath = Path.Combine(GetSourceFileFolder(), @"..\..\Models\CodegenCS.Models.NSwagAdapter\SampleModels\petstore-openapi3.json");

        [SetUp]
        public override Task Setup()
        {
            Assert.That(File.Exists(_modelPath));
            _modelPath = new FileInfo(_modelPath).FullName;
            return Task.CompletedTask;
        }




        [Test]
        public async Task TestOpenAPI()
        {
            FormattableString template = $$"""
                public class MyTemplate
                {
                    void Main(ICodegenTextWriter writer, OpenApiDocument model)
                    {
                        writer.WriteLine(model.Definitions.First().Key);
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync(models: new string[] { _modelPath }, templateArgs: new string[] { "MyNamespace" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "Pet" + "\r\n");
        }


        [Test]
        public async Task TestOpenAPIAdapterCli()
        {
            FormattableString template = $$"""
                public class MyTemplate
                {
                    void Main(ICodegenTextWriter writer, OpenApiDocument model)
                    {
                        writer.WriteLine(model.Definitions.First().Key);
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync(models: new string[] { _modelPath });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "Pet" + "\r\n");
        }


        [Test]
        public async Task TestModelFactory()
        {
            FormattableString template = $$"""
                public class MyTemplate
                {
                    void Main(ICodegenTextWriter writer, IModelFactory factory)
                    {
                        var model = factory.LoadModelFromFile<OpenApiDocument>(@"{{_modelPath}}");
                        writer.WriteLine(model.Definitions.First().Key);
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync();

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "Pet" + "\r\n");
        }

        [Test]
        public async Task TestOpenApiDocumentAdapter() // should read using the right adapter, not just deserializing the JSON
        {
            var factory = ModelFactoryBuilder.CreateModelFactory(new string[] { TemplatesFolder });

            // async
            var doc = await factory.LoadModelFromFileAsync<OpenApiDocument>(@"Models\Petstore-OpenAPI3.json");
            var op = doc.Operations.Single(o => o.Operation.OperationId == "listPets");
            Assert.GreaterOrEqual(op.Operation.Parameters.Count, 1);

            // sync
            doc = factory.LoadModelFromFile<OpenApiDocument>(@"Models\Petstore-OpenAPI3.json");
            op = doc.Operations.Single(o => o.Operation.OperationId == "listPets");
            Assert.GreaterOrEqual(op.Operation.Parameters.Count, 1);

        }

    }
}
