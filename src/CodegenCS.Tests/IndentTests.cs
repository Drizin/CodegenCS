using CodegenCS;
using CodegenCS.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class IndentTests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }


        /// <summary>
        /// Tests CurlyBraces blocks.
        /// Tests multi-line blocks.
        /// </summary>
        [Test]
        public void TestCurlyBracesBlock()
        {
            using(_w.WithCurlyBraces($"namespace {"mynamespace"}"))
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

    }
}
