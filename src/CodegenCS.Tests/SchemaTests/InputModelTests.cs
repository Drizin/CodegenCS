using CodegenCS;
using CodegenCS.DbSchema;
using CodegenCS.DotNet;
using CodegenCS.InputModels;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tests
{
    public class InputModelTests
    {
        public static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

        [Test]
        public void TestDatabaseSchema()
        {
            string folder = GetCurrentFolder();
            string jsonPath = Path.Combine(folder, @"..\..\CodegenCS.POCO\AdventureWorksSchema.json");
            string jsonModel = File.ReadAllText(jsonPath);
            var dbSchema = DatabaseSchema.TryParse(jsonModel);
            //File.WriteAllText(jsonPath, JsonConvert.SerializeObject(dbSchema, Formatting.Indented));
            Assert.NotNull(dbSchema);
            Assert.AreEqual("http://codegencs.com/schemas/dbschema/2021-07/dbschema.json", dbSchema.Schema);
        }

    }
}
