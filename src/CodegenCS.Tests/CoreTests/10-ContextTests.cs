using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenCS.Tests.CoreTests
{
    public class CodegenContextTests
    {
        ICodegenContext _ctx = null;

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


        [Test]
        public void DefaultOutputfileTests()
        {
            using (var f1 = _ctx["File1.cs"])
            {
                f1.Write("Test");
            }
            Assert.That(_ctx.OutputFiles.Count == 1);

            // DefaultOutputFile added to OutputFiles when it gets first written
            _ctx.DefaultOutputFile.Write("Test");
            Assert.That(_ctx.OutputFiles.Count == 2);
            Assert.That(_ctx.OutputFilesPaths.Contains("File1.cs"));
            Assert.That(_ctx.OutputFilesPaths.Contains(""));

            // Renaming DefaultOutputFile
            _ctx.DefaultOutputFile.RelativePath = "Renamed.cs";
            Assert.That(_ctx.OutputFilesPaths.Contains("File1.cs"));
            Assert.That(_ctx.OutputFilesPaths.Contains("Renamed.cs"));
            Assert.That(!_ctx.OutputFilesPaths.Contains(""));

            // Renaming other file
            _ctx["File1.cs"].RelativePath = "File1Renamed.cs";
            Assert.That(_ctx.OutputFilesPaths.Contains("File1Renamed.cs"));
            Assert.That(_ctx.OutputFilesPaths.Contains("Renamed.cs"));
            Assert.That(!_ctx.OutputFilesPaths.Contains("File1.cs"));
            Assert.That(!_ctx.OutputFilesPaths.Contains(""));
        }

        [Test]
        public void DefaultOutputfileTests2()
        {
            // DefaultOutputFile added to OutputFiles when it gets first written
            _ctx.DefaultOutputFile.Write("Test");
            Assert.That(_ctx.OutputFiles.Count == 1);

            Assert.Throws<Exception>(() => { _ctx.SaveFiles(Environment.GetEnvironmentVariable("TEMP")); });

            // Renaming DefaultOutputFile
            _ctx.DefaultOutputFile.RelativePath = "Renamed.cs";
            Assert.AreEqual(1, _ctx.SaveFiles(Environment.GetEnvironmentVariable("TEMP")));
        }


    }
}
