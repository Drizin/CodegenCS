using CodegenCS;
using CodegenCS.DotNet;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class DotNetCodegenContextTests
    {
        DotNetCodegenContext _ctx = null;

        [SetUp]
        public void Setup()
        {
            _ctx = new DotNetCodegenContext();
        }


        [Test]
        public void TestMultipleFiles()
        {
            var f1 = _ctx["File1.cs"];
            var f2 = _ctx["README.TXT"];
            var f3 = _ctx["Transaltions.RESX"];
            Assert.That(f1.FileType == BuildActionType.Compile);
            Assert.That(f2.FileType == BuildActionType.None);
            Assert.That(f3.FileType == BuildActionType.EmbeddedResource);
        }

    }
}
