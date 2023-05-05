using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

using static CodegenCS.Symbols;

namespace CodegenCS.Tests.CoreTests
{
    public class MultilineTests
    {
        CodegenTextWriter _w = null;
        CodegenTextWriter _w2 = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
            _w2 = new CodegenTextWriter();
            _w.MultilineBehavior = CodegenTextWriter.MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;
            _w2.MultilineBehavior = CodegenTextWriter.MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;
        }


        [Test]
        public void TestRawStringLiteralWrite()
        {
            string methodName = "MyMethod";
            _w.Write($$"""
                public void {{methodName}}()
                {
                    // My method...
                }
                """);
            // Raw String Literals will automatically remove left padding, remove first and last empty line
            string expected = string.Join(Environment.NewLine, new string[]
            {
                "public void MyMethod()",
                "{",
                "    // My method...",
                "}",
            });
            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestRawStringLiteralWriteLine()
        {
            string methodName = "MyMethod";
            _w.WriteLine($$"""
                public void {{methodName}}()
                {
                    // My method...
                }
                """);
            // Raw String Literals will automatically remove left padding, remove first and last empty line
            string expected = string.Join(Environment.NewLine, new string[]
            {
                "public void MyMethod()",
                "{",
                "    // My method...",
                "}",
                ""
            });
            Assert.AreEqual(expected, _w.GetContents());
        }




        [Test]
        public void TestMultiline1()
        {
            _w.MultilineBehavior = CodegenTextWriter.MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;
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
            // Using Raw Strings
            _w.WriteLine("""
                public void MyMethod1()
                {
                    //...
                }
                """);
            if (1 == 1)
            {
                if (2 == 2) // the text blocks can be aligned wherever they fit better under the control code
                    _w.WriteLine("""
                        public void MyMethod2()
                        {
                            //...
                        }
                        """);
            }



            // Using the legacy MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine
            _w2.MultilineBehavior = CodegenTextWriter.MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine;
            _w2.WriteLine(@"
                public void MyMethod1()
                {
                    //...
                }");
            if (1 == 1)
            {
                if (2 == 2) // the text blocks can be aligned wherever they fit better under the control code
                    _w2.WriteLine(@"
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
            Assert.AreEqual(expected, _w2.GetContents());
        }


        [Test]
        public void TestMultiline3()
        {
            // Using Raw Strings
            _w.IncreaseIndent(); // this can also be set by WithIndent, WithCBlock, WithJavaBlock, etc.. and indent is automatically decreased in the end of the block
            _w.WriteLine("""
                public void MyMethod1()
                {
                    //...
                }
                """);
            _w.DecreaseIndent();


            // Using the legacy MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine
            _w2.IncreaseIndent(); // this can also be set by WithIndent, WithCBlock, WithJavaBlock, etc.. and indent is automatically decreased in the end of the block
            _w2.WriteLine(@"
                public void MyMethod1()
                {
                    //...
                }");
            _w2.DecreaseIndent();

            string expected =
@"    public void MyMethod1()
    {
        //...
    }
";

            Assert.AreEqual(expected, _w.GetContents());
            Assert.AreEqual(expected, _w2.GetContents());
        }


        [Test]
        public void TestMultilineBlock()
        {
            // Using Raw Strings
            _w
                .WithCurlyBraces("namespace MyNameSpace", (w) =>
                {
                    _w.WithCurlyBraces("class MyClass", (w2) =>
                    {
                        _w.WriteLine("""
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block

                         """);
                        _w.WriteLine("""
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block

                             """);
                        _w.WriteLine("""
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block

                                 """);
                        _w.WriteLine("""
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        """);
                    });
                })
                ;


            // Using the legacy MultilineBehaviorType.TrimLeftPaddingAndRemoveFirstEmptyLine
            _w2
                .WithCurlyBraces("namespace MyNameSpace", (w) =>
                {
                    _w2.WithCurlyBraces("class MyClass", (w2) =>
                    {
                        _w2.WriteLine(@"
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block
                         This is a multi-line block
                         ");
                        _w2.WriteLine(@"
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block
                             This is a multi-line block
                             ");
                        _w2.WriteLine(@"
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block
                                 This is a multi-line block
                                 ");
                        _w2.WriteLine(@"
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block
        This is a multi-line block");
                    });
                });

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
}".TrimStart();
            Assert.AreEqual(expected, _w.GetContents());
            Assert.AreEqual(expected, _w2.GetContents());
        }

        [Test]
        public void TestMultilineBlock2()
        {
            string pocoNamespace = "AdventureWorks";
            _w.WithCBlock($@"
		            //------------------------------------------------------------------------------
		            // <auto-generated>
		            //     This code was generated by a tool.
		            //     Changes to this file may cause incorrect behavior and will be lost if
		            //     the code is regenerated.
		            // </auto-generated>
		            //------------------------------------------------------------------------------
		            using System;

		            namespace {pocoNamespace}", () =>
                    {
                        for (int i = 0; i < 2; i++)
                        {
                            _w.WithCBlock($@"
                            // MyClass{i}
                            public class MyClass{i}", () =>
                            {
                                for (int j = 0; j < 2; j++)
                                {
                                    _w.WriteLine($"public int MyProp{j} {{ get; set; }}");
                                }
                            });
                        }
                    });
            string expected = @"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;

namespace AdventureWorks
{
    // MyClass0
    public class MyClass0
    {
        public int MyProp0 { get; set; }
        public int MyProp1 { get; set; }
    }
    // MyClass1
    public class MyClass1
    {
        public int MyProp0 { get; set; }
        public int MyProp1 { get; set; }
    }
}";
            Assert.AreEqual(expected.TrimStart(), _w.GetContents());
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
}";

            Assert.AreEqual(expected, _w.GetContents());
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
}";

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestMultilineStringWithRAW()
        {
            var s = @"a
empty:

b
indented:
    c
empty:

d";

            _w.WriteLine($$"""
                public string Test()
                {
                    // comment1
                    return @"{{RAW(s)}}";
                    // comment2
                }
                """);

            var expected = @$"public string Test()
{{
    // comment1
    return @""{s}"";
    // comment2
}}
";

            Assert.AreEqual(expected, _w.GetContents());
        }

    }
}
