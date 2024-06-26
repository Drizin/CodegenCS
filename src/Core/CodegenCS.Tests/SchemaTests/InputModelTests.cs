using CodegenCS.Models.DbSchema;
using NUnit.Framework;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Assert = NUnit.Framework.Legacy.ClassicAssert;

namespace CodegenCS.Tests.SchemaTests
{
    public class InputModelTests
    {
        public static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

        [Test]
        public async Task LoadValidDatabaseSchema()
        {
            string folder = GetCurrentFolder();
            string jsonPath = Path.Combine(folder, @"..\..\..\Models\CodegenCS.Models.DbSchema.SampleDatabases\AdventureWorksSchema.json");
            string jsonModel = File.ReadAllText(jsonPath);
            var dbSchema = await DatabaseSchema.TryParseAsync(jsonModel);
            //File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dbSchema, Formatting.Indented));
            Assert.NotNull(dbSchema);
            Assert.AreEqual("http://codegencs.com/schemas/dbschema/2021-07/dbschema.json", dbSchema.Schema);
        }

        [Test]
        public async Task LoadInvalidDatabaseSchema()
        {
            string invalidSchema = @"
{
  ""$schema"": ""http://codegencs.com/schemas/dbschema/2021-07/dbschema.json"",
  ""LastRefreshed"": ""2020-07-19T21:34:55.6930822-04:00"",
  ""TabEls"": []
}";
            var dbSchema = await DatabaseSchema.TryParseAsync(invalidSchema);
            //File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dbSchema, Formatting.Indented));
            Assert.IsNull(dbSchema);
        }

        [Test]
        public async Task LoadLegacyDatabaseSchema()
        {
            string invalidSchema = @"
{
  ""Schema"": ""http://codegencs.com/schemas/dbschema/2021-07/dbschema.json"",
  ""Id"": ""ThisWasPartOfOldSchema"",
  ""LastRefreshed"": ""2020-07-19T21:34:55.6930822-04:00"",
  ""Tables"": []
}";
            var dbSchema = await DatabaseSchema.TryParseAsync(invalidSchema);
            //File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dbSchema, Formatting.Indented));
            Assert.AreEqual("http://codegencs.com/schemas/dbschema/2021-07/dbschema.json", dbSchema.Schema);
        }


    }
}
