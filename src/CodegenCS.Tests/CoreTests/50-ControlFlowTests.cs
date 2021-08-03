using CodegenCS;
using static CodegenCS.Symbols;
using CodegenCS.Extensions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using CodegenCS.ControlFlow;

namespace Tests
{
    public class ControlFlowTests
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region IF-ENDIF false
        [Test]
        public void TestIF_false_ENDIF()
        {
            _w.Write($@"WILL SHOW{IF(false)} WON'T SHOW{ENDIF}");

            string expected = @"WILL SHOW";
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region IF-ENDIF true
        [Test]
        public void TestIF_true_ENDIF()
        {
            _w.Write($@"WILL SHOW{IF(true)} WILL ALSO SHOW{ENDIF}");

            string expected = @"WILL SHOW WILL ALSO SHOW";
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region IF-ELSE-ENDIF false
        [Test]
        public void TestIF_false_ELSE_ENDIF()
        {
            _w.Write($@"WILL SHOW{IF(false)} WON'T SHOW{ELSE} BUT THIS SHOWS{ENDIF}");

            string expected = @"WILL SHOW BUT THIS SHOWS";
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region IF-ELSE_ENDIF true
        [Test]
        public void TestIF_true_ELSE_ENDIF()
        {
            _w.Write($@"WILL SHOW{IF(true)} WILL ALSO SHOW{ELSE} BUT THIS WON'T{ENDIF}");

            string expected = @"WILL SHOW WILL ALSO SHOW";
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region Unbalanced Ifs
        [Test]
        public void UnbalancedIfs()
        {
            _w.Write($@"WILL SHOW{IF(true)} WILL ALSO SHOW{ELSE} BUT THIS WON'T");
            Assert.Throws<UnbalancedIfsException>(() => _w.GetContents());
        }
        [Test]
        public void UnbalancedIfs2()
        {
            _w.Write($@"WILL SHOW{IF(true)} WILL {IF(true)}ALSO SHOW{ENDIF} BUT THIS WON'T");
            Assert.Throws<UnbalancedIfsException>(() => _w.GetContents());
        }
        #endregion

        #region IIF
        [TestCase(true)]
        [TestCase(false)]
        public void IIfTest(bool isVisibilityPublic)
        {
            _w.Write($@"{IIF(isVisibilityPublic, $"public ")}string FirstName {{ get; set; }}");

            if (isVisibilityPublic) 
                Assert.AreEqual("public string FirstName { get; set; }", _w.GetContents());
            else
                Assert.AreEqual("string FirstName { get; set; }", _w.GetContents());
        }
        [TestCase(true)]
        [TestCase(false)]
        public void IIfTest2(bool isVisibilityPublic)
        {
            _w.Write($@"{IIF(isVisibilityPublic, $"public ", $"protected ")}string FirstName {{ get; set; }}");

            if (isVisibilityPublic)
                Assert.AreEqual("public string FirstName { get; set; }", _w.GetContents());
            else
                Assert.AreEqual("protected string FirstName { get; set; }", _w.GetContents());
        }

        #endregion


        #region Nested Ifs
        [Test]
        public void NestedIf1()
        {
            _w.Write($@"A{IF(true)}B{IF(false)}C{ELSE}D{ENDIF}E{ELSE}F{ENDIF}G");

            string expected = @"ABDEG";
            Assert.AreEqual(expected, _w.GetContents());
        }

        #endregion

        #region Ifs with LineBreaks

        [TestCase(true)]
        [TestCase(false)]
        public void IfWithLineBreaks(bool injectHttpClient)
        {
            _w.Write($@"
                public class MyApiClient
                {{
                    public MyApiClient({IF(injectHttpClient)}HttpClient httpClient{ENDIF})
                    {{{IF(injectHttpClient)}
                        _httpClient = httpClient;{ENDIF}
                    }}
                }}");

            string expected = null;

            if (injectHttpClient)
                expected = @"
public class MyApiClient
{
    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}";
            if (!injectHttpClient)
                expected = @"
public class MyApiClient
{
    public MyApiClient()
    {
    }
}";

            Assert.AreEqual(expected.TrimStart(), _w.GetContents());
        }


        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, false)]
        [TestCase(false, true)]
        public void NestedIfWithLineBreaks(bool generateConstructor, bool injectHttpClient)
        {
            _w.Write($@"
                {IF(generateConstructor)}public class MyApiClient
                {{
                    public MyApiClient({IF(injectHttpClient)}HttpClient httpClient{ENDIF})
                    {{{IF(injectHttpClient)}
                        _httpClient = httpClient;{ENDIF}
                    }}
                }}{ENDIF}");

            string expected = null;

            if (generateConstructor && injectHttpClient)
                expected = @"
public class MyApiClient
{
    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}";
            if (generateConstructor && !injectHttpClient)
                expected = @"
public class MyApiClient
{
    public MyApiClient()
    {
    }
}";

            if (!generateConstructor)
                expected = @"";

            Assert.AreEqual(expected.TrimStart(), _w.GetContents());
        }

        #endregion


    }
}
