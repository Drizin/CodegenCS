using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class CodegenContextTests
    {
        CodegenContext _ctx = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _ctx = new CodegenContext();
        }
        #endregion

        [Test]
        public void TestSingleFile()
        {
            using (var f1 = _ctx["File1.cs"])
            {
                f1.Write("Test");
            }

            Assert.That(_ctx.OutputFiles.Count == 1);
            Assert.That(_ctx["File1.cs"].GetContents() == "Test");
        }

        [Test]
        public void TestFileDisposal()
        {
            var f1 = _ctx["File1.cs"];
            using (f1)
            {
                f1.Write("Test");
            }

            Assert.That(_ctx["File1.cs"].GetContents() == "Test");

            Assert.Throws<ObjectDisposedException>(() => f1.WriteLine("Is Disposed"));
        }

        [Test]
        public void TestCaseInsensitive()
        {
            using (var f1 = _ctx["File1.cs"])
            {
                f1.Write("Test");
            }
            Assert.That(_ctx.OutputFiles.Count == 1);

            var f2 = _ctx["FILE1.CS"];
            Assert.That(_ctx.OutputFiles.Count == 1);
        }

        [Test]
        public void TestMultipleFiles()
        {
            using (var f1 = _ctx["File1.cs"])
            {
                f1.Write("Test");
            }
            var f2 = _ctx["Subfolder/File2.cs"];
            
            var f3 = _ctx["SUBFOLDER/File2.cs"];

            Assert.That(_ctx.OutputFiles.Count == 2);

            Assert.That(_ctx.OutputFiles.Any(of => of.RelativePath == "File1.cs"));
            Assert.That(_ctx.OutputFiles.Any(of => of.RelativePath == "Subfolder/File2.cs"));
            Assert.That(!_ctx.OutputFiles.Any(of => of.RelativePath == "File2.cs"));
        }

    }
}
