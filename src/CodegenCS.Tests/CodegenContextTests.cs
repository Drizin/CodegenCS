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

        [SetUp]
        public void Setup()
        {
            _ctx = new CodegenContext();
        }


        [Test]
        public void TestMultipleFiles()
        {
            using (var f1 = _ctx["File1.cs"])
            {
                using(f1.WithCurlyBraces($"namespace {"mynamespace"}"))
                {
                    using (f1.WithCurlyBraces($"public class {"myClass"}"))
                    {
                        using (f1.WithCurlyBraces($"public void {"MyMethod1()"}"))
                        {
                        }
                    }
                }
            }

            Assert.That(_ctx.OutputFiles.Count == 1);
            using (var f2 = _ctx["File2.cs"])
            {
                using (f2.WithCurlyBraces($"namespace {"mynamespace"}"))
                {
                    using (f2.WithCurlyBraces($"public class {"myClass"}"))
                    {
                        using (f2.WithCurlyBraces($"public void {"MyMethod1()"}"))
                        {
                        }
                    }
                }
                f2.Write("Foo");
            }
            var f3 = _ctx["File3.cs"];
            f3.WriteLine("Bar");


            Assert.That(_ctx.OutputFiles.Count == 3);
            Assert.That(_ctx.OutputFiles.Any(of => of.RelativePath == "File1.cs"));
            Assert.That(_ctx.OutputFiles.Any(of => of.RelativePath == "File2.cs"));
            Assert.That(_ctx["File1.cs"].GetContents().Length > 10);
            Assert.That(_ctx["FILE1.cs"].GetContents().Length > 10);
            Assert.That(_ctx["File2.cs"].GetContents().Length > 10);

            string content1 = _ctx["FILE1.cs"].GetContents();
            string content2 = _ctx["FILE2.cs"].GetContents();
            string content3 = _ctx["FILE3.cs"].GetContents();

            string expected =
@"namespace mynamespace
{
    public class myClass
    {
        public void MyMethod1()
        {
        }
    }
}
";

            Assert.AreEqual(content1, expected);
            Assert.AreEqual(content2, expected + "Foo");
            Assert.AreEqual(content3, "Bar"+Environment.NewLine);
        }

    }
}
