using CodegenCS;
using CodegenCS.DbSchema;
using CodegenCS.DotNet;
using CodegenCS.InputModels;
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
            Assert.NotNull(dbSchema);
            Assert.AreEqual("http://codegencs.com/schemas/dbschema.json", dbSchema.Id);
        }

    }
}
