using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class MultilineTests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }


        [Test]
        public void TestMultiline1()
        {
            _w.WriteLine(@"
                public void MyMethod1()
                {
                    //...
                }");

            string expected =
@"public void MyMethod1()
{
    //...
}
";

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestMultiline2()
        {
            _w.WriteLine(@"
                public void MyMethod1()
                {
                    //...
                }");
            if (1 == 1)
            {
                if (2 == 2) // the text blocks can be aligned wherever they fit better under the control code
                    _w.WriteLine(@"
                        public void MyMethod2()
                        {
                            //...
                        }");
            }

            string expected =
@"public void MyMethod1()
{
    //...
}
public void MyMethod2()
{
    //...
}
";

            Assert.AreEqual(expected, _w.GetContents());
        }


        [Test]
        public void TestMultiline3()
        {
            _w.IncreaseIndent(); // this can also be set by WithIndent, WithCBlock, WithJavaBlock, etc.. and indent is automatically decreased in the end of the block
            _w.WriteLine(@"
                public void MyMethod1()
                {
                    //...
                }");
            _w.DecreaseIndent();

            string expected =
@"    public void MyMethod1()
    {
        //...
    }
";

            Assert.AreEqual(expected, _w.GetContents());
        }


        [Test]
        public void TestMultilineBlock()
        {
            _w
                .WithCurlyBraces("namespace MyNameSpace", (w) =>
                {
                    _w.WithCurlyBraces("class MyClass", (w2) =>
                    {
                        _w.WriteLine(@"
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block
                        ");
                        _w.WriteLine(@"
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block
                            ");
                        _w.WriteLine(@"
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block
                            ");
                        _w.WriteLine(@"
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
                            ");
                    });
                })
                ;
            string expected = @"
namespace MyNameSpace
{
    class MyClass
    {
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block

        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block

        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block

        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
                            
    }
}
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }




        /// <summary>
        /// Tests CurlyBraces blocks.
        /// Tests multi-line blocks.
        /// </summary>
        [Test]
        public void TestCurlyBracesBlock()
        {
            using (_w.WithCurlyBraces($"namespace {"mynamespace"}"))
            {
                using (_w.WithCurlyBraces($"public class {"myClass"}"))
                {
                    using (_w.WithCurlyBraces($"public void {"MyMethod1()"}"))
                    {
                        /* a linebreak in the start of a multi-line string will be ignored - 
                            * it's considered as a helper to correctly aline your multi-line block */
                        _w.WriteLine(@"
                            I can use any number of left-padding spaces
                            to make my texts aligned with outer control code
                            it will be all left-aligned (trimmed)");
                        /* the last line doesn't need to add a line break - 
                            * when the indented block finishes it will automatically break line and close scope */
                    }
                }
            }

            string content = _w.GetContents();

            string expected =
@"namespace mynamespace
{
    public class myClass
    {
        public void MyMethod1()
        {
            I can use any number of left-padding spaces
            to make my texts aligned with outer control code
            it will be all left-aligned (trimmed)
        }
    }
}
";

            Assert.AreEqual(content, expected);
        }



        /// <summary>
        /// Tests CurlyBraces blocks.
        /// Tests multi-line blocks.
        /// </summary>
        [Test]
        public void TestCurlyBracesBlockFluentAPI()
        {
            string ns = "myNamespace";
            string cl = "myClass";
            string method = "MyMethod";
            _w.WithCurlyBraces($"namespace {ns}", () =>
            {
                _w.WithCurlyBraces($"public class {cl}", () => {
                    _w.WithCurlyBraces($"public void {method}()", () =>
                    {
                        /* a linebreak in the start of a multi-line string will be ignored - 
                            * it's considered as a helper to correctly aline your multi-line block */
                        _w.WriteLine(@"
                            I can use any number of left-padding spaces
                            to make my texts aligned with outer control code
                            it will be all left-aligned (trimmed)");
                        /* the last line doesn't need to add a line break - 
                            * when the indented block finishes it will automatically break line and close scope */
                    });
                });
            });

            string content = _w.GetContents();

            string expected =
@"namespace myNamespace
{
    public class myClass
    {
        public void MyMethod()
        {
            I can use any number of left-padding spaces
            to make my texts aligned with outer control code
            it will be all left-aligned (trimmed)
        }
    }
}
";

            Assert.AreEqual(content, expected);
        }




    }
}
