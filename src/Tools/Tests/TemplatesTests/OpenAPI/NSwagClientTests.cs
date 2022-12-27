using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using CliWrap;
using System.Text;
using System.Net;

namespace CodegenCS.Tools.Tests
{
    internal class NSwagClientTests : BaseTest
    {
        [OneTimeSetUp]
        #region Download Templates and Models
        public async Task DownloadSampleModels()
        {
            await base.Setup();
            string[] templatesToClone = new string[] { "NSwagClient" };

            string[] modelsToDownload = new string[]
            {
                "https://raw.githubusercontent.com/CodegenCS/Templates/main/OpenAPI/SampleModels/petstore-openapi3.json"
            };

            foreach (var templateAlias in templatesToClone)
            {
                if (File.Exists($"{templateAlias}.dll"))
                    File.Delete($"{templateAlias}.dll");
                if (File.Exists($"{templateAlias}.cs"))
                    File.Delete($"{templateAlias}.cs");

                var result = await Run($"template clone {templateAlias}");
                Assert.AreEqual(0, result.ExitCode);
                FileAssert.Exists($"{templateAlias}.cs");
                FileAssert.Exists($"{templateAlias}.dll");
            }

            foreach (var url in modelsToDownload)
            {
                string localPath = url.Substring(url.LastIndexOf("/") + 1);
                if (File.Exists(localPath))
                    File.Delete(localPath);
                await new WebClient().DownloadFileTaskAsync(new Uri(url), localPath);
                FileAssert.Exists(localPath);
            }

        }
        #endregion

        #region Checking if template output matches the previous snapshot (no breaking changes)
        [TestCase("NSwagClient", "petstore-openapi3.json")]
        public async Task TestTemplate(string templateAlias, string model)
        {
            string cmd = $"template run {templateAlias}.dll {model} MyNamespace";
            
            var result = await Run(cmd);
            Assert.AreEqual(0, result.ExitCode);

            // Debug:
            //var result = await new TemplateCliArgsTests().LaunchAsync($"{templateAlias}.dll", new string[] { model }, new string[] { "MyNamespace" });
            //Assert.AreEqual(0, result);

            StringAssert.Contains($"Loading '{templateAlias}.dll'...", _stdOut);
            StringAssert.Contains("Model type is 'NSwag.OpenApiDocument'...", _stdOut);
            StringAssert.Contains($"Model successfuly loaded from '{model}'...", _stdOut);
            StringAssert.Contains($"Successfully executed template '{templateAlias}.dll'.", _stdOut);
            StringAssert.AreEqualIgnoringCase(string.Empty, _stdErr);
            FileAssert.Exists($"{templateAlias}.generated.cs");
            string snapshot = Path.Combine(GetCurrentFolder(), "Snapshots", "petstore-openapi3.generated.cs");
            FileAssert.AreEqual(snapshot, $"{templateAlias}.generated.cs");
        }
        #endregion




    }
}
