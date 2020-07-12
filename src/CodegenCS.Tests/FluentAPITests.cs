using CodegenCS;
using CodegenCS.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class FluentAPITests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }


        [Test]
        public void TestCallback()
        {
            _w
                .WriteLine("// MyMethod will test Action delegates")
                .WithCurlyBraces("void method()", () =>
                {
                    _w.WriteLine("Hello");
                })
                ;
            string expected = @"
// MyMethod will test Action delegates
void method()
{
    Hello
}
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }

        [Test]
        public void TestCallback2()
        {
            _w
                .WriteLine("// MyMethod will test Action delegates")
                .WithCurlyBraces("void method()", (w) =>
                {
                    _w.WriteLine("Hello");
                    w.WriteLine("Hello2"); // callbacks can by of type Action or Action<CodegenTextWriter> - it doesn't matter, it's all same instance
                })
                ;

            string expected = @"
// MyMethod will test Action delegates
void method()
{
    Hello
    Hello2
}
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }


        [Test]
        public void TestCBlock()
        {
            _w
                .WriteLine("// will open a C block")
                .WriteLine("// and write-inline")
                .WithCurlyBraces("void method()", (w) =>
                {
                    _w.WriteLine("Hello");
                    w.WriteLine("Hello2"); // doesn't matter, it's all same instance
                    w.WithCurlyBraces("if (1==1)", (w2) =>
                    {
                        _w.WriteLine("Hello3");
                        w2.WriteLine("Hello4");
                    });
                    w.WithCurlyBraces("if (1==1)", (w2) =>
                    {
                    });
                });
            string expected = @"
// will open a C block
// and write-inline
void method()
{
    Hello
    Hello2
    if (1==1)
    {
        Hello3
        Hello4
    }
    if (1==1)
    {
    }
}
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }

        [Test]
        public void TestJavaBlock()
        {
            _w.CurlyBracesStyle = CodegenTextWriter.CurlyBracesStyleType.Java;
            _w
                .WriteLine("// will open a Java block")
                .WriteLine("// and write-inline")
                .WithCurlyBraces("void method()", (w) =>
                {
                    _w.WriteLine("Hello");
                    w.WriteLine("Hello2"); // doesn't matter, it's all same instance
                    w.WithCurlyBraces("if (1==1)", (w2) =>
                    {
                        _w.WriteLine("Hello3");
                        w2.WriteLine("Hello4");
                    });
                    w.WithCurlyBraces("if (1==1)", (w2) =>
                    {
                    });
                });
            string expected = @"
// will open a Java block
// and write-inline
void method() {
    Hello
    Hello2
    if (1==1) {
        Hello3
        Hello4
    }
    if (1==1) {
    }
}
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
        }

    [Test]
        public void TestPython()
        {
            _w
                .WriteLine("// will open a Java block")
                .WriteLine("// and write-inline")
                .WithPythonBlock("if a == b", (w) =>
                {
                    _w.WriteLine("print b");
                    w.WithPythonBlock("if b > c", (w2) =>
                    {
                        _w.WriteLine("print 'b > c'");
                    });
                    w.WithPythonBlock("for a in range(1,n)", (w2) =>
                    {
                        w.WithPythonBlock("for b in range(a,n)", (_) =>
                        {
                            _w.WriteLine("print 'b > c'");
                        });
                    });
                });

            string expected = @"
// will open a Java block
// and write-inline
if a == b :{
    print b
    if b > c :{
        print 'b > c'
    for a in range(1,n) :{
        for b in range(a,n) :{
            print 'b > c'
";
            Assert.AreEqual(_w.ToString(), expected.TrimStart());
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
    }

}
