using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenCS.Tests.CoreTests
{
    /// <summary>
    /// Explicit Indentation is NOT recommended - prefer using Implicit Indentation
    /// </summary>
    public class ExplicitIndentTests
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region C-Blocks (Fluent API & using IDisposable)

        string expected_C = @"
// will open a C block
void MyMethod()
{
    int i = 0;
}".TrimStart();

        [Test]
        public void TestCBlockFluentAPI()
        {
            _w
                .WriteLine("// will open a C block")
                .WithCBlock("void MyMethod()", () =>
                {
                    _w.WriteLine("int i = 0;");
                });
            Assert.AreEqual(expected_C, _w.GetContents());
        }

        [Test]
        public void TestCBlockUsingIDisposable()
        {
            _w.WriteLine("// will open a C block");
            using (_w.WithCBlock("void MyMethod()"))
            {
                _w.WriteLine("int i = 0;");
            }
            Assert.AreEqual(expected_C, _w.GetContents());
        }


        #endregion

        #region Java-Blocks (Fluent API & using IDisposable)

        string expected_java = @"
// will open a Java block
void MyMethod() {
    int i = 0;
}".TrimStart();

        [Test]
        public void TestJavaBlockFluentAPI()
        {
            _w
                .WriteLine("// will open a Java block")
                .WithJavaBlock("void MyMethod()", (w) =>
                {
                    _w.WriteLine("int i = 0;");
                });
            Assert.AreEqual(expected_java, _w.GetContents());
        }

        [Test]
        public void TestJavaBlockUsingIDisposable()
        {
            _w.WriteLine("// will open a Java block");
            using (_w.WithJavaBlock("void MyMethod()"))
            {
                _w.WriteLine("int i = 0;");
            }
            Assert.AreEqual(expected_java, _w.GetContents());
        }

        #endregion

        #region Python-Blocks (Fluent API & using IDisposable)

        string expectedPython = @"
# will open a Python block
if a == b :
    print b
".TrimStart();


        [Test]
        public void TestPythonFluentAPI()
        {
            _w
                .WriteLine("# will open a Python block")
                .WithPythonBlock("if a == b", (w) =>
                {
                    _w.WriteLine("print b");
                });

           Assert.AreEqual(expectedPython, _w.GetContents());
        }


        [Test]
        public void TestPythonUsingIDisposable()
        {
            _w.WriteLine("# will open a Python block");
            using (_w.WithPythonBlock("if a == b"))
            {
                _w.WriteLine("print b");
            }
            Assert.AreEqual(expectedPython, _w.GetContents());
        }
        #endregion

        #region Generic indented Block (Fluent API, using IDisposable, and manually indenting)

        string expectedGeneric =
@"
Line1
    Line2
        Line3
    Line4
".TrimStart();


        [Test]
        public void TestGenericIndentedFluentAPI()
        {
            // This is the old style. Please prefer using Fluent API
            _w.WriteLine("Line1");
            using (_w.WithIndent())
            {
                _w.WriteLine("Line2");
                using (_w.WithIndent())
                {
                    _w.WriteLine("Line3");
                }
                _w.WriteLine("Line4");
            }

            Assert.AreEqual(expectedGeneric, _w.GetContents());
        }

        [Test]
        public void TestGenericIndentedUsingIDisposable()
        {
            _w.WriteLine("Line1");
            using (_w.WithIndent())
            {
                _w.WriteLine("Line2");
                using (_w.WithIndent())
                {
                    _w.WriteLine("Line3");
                }
                _w.WriteLine("Line4");
            }

            Assert.AreEqual(expectedGeneric, _w.GetContents());
        }

        [Test]
        public void TestManualIndent()
        {
            // Under normal circunstances you don't need to manual control indentation
            _w.WriteLine("Line1");
            _w.IncreaseIndent();
            _w.WriteLine("Line2");
            _w.IncreaseIndent();
            _w.WriteLine("Line3");
            _w.DecreaseIndent();
            _w.WriteLine("Line4");

            Assert.AreEqual(expectedGeneric, _w.GetContents());
        }



        #endregion

    }

}
