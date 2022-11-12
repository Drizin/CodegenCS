using CodegenCS.Models.DbSchema;
using NUnit.Framework;
using System;
using System.Linq;
using static CodegenCS.Symbols;

namespace CodegenCS.Tests.CoreTests
{
    /// <summary>
    /// "Basics" - misc tests (raw string literals, implicit indentation, ienumerables) to show the basics.
    /// </summary>
    internal class WhitespaceControl : BaseTest
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        [Test]
        public void CommentIsIgnored()
        {
            _w.Write($$"""
                {{COMMENT("Comments are ignored but can be used to document templates")}}
                """);
            Assert.AreEqual("", _w.GetContents());
        }

        [Test]
        public void CommentEmptyLine()
        {
            _w.Write($$"""
                public class MyClass
                {
                    {{COMMENT("Comments are ignored but can be used to document templates")}}
                }
                """);
            Assert.AreEqual("""
                public class MyClass
                {
                    
                }
                """, _w.GetContents());
        }

        [Test]
        public void EmptyLineWithAction()
        {
            string my_variable = "";
            // First line is an Action delegate
            // Second action is Func delegate to capture the same variable scope
            _w.Write($$"""
                {{() => { my_variable = "tomato"; }}}
                {{() => my_variable}}
                """);
            
            // Empty line still there
            Assert.AreEqual("\r\ntomato", _w.GetContents());
        }

        [Test]
        public void EmptyLineWithComment()
        {
            _w.Write($$"""
                {{COMMENT("Action was just an example. All we need for the testis this empty line")}}
                {{"tomato"}}
                """);

            // Empty line still there
            Assert.AreEqual("\r\ntomato", _w.GetContents());
        }

        [Test]
        public void EmptyLineWithCommentAndTLW()
        {
            _w.Write($$"""
                {{COMMENT("Action was just an example. All we need for the testis this empty line")}}
                {{TLW}}{{"tomato"}}
                """);

            // Leading Empty line was trimmed
            Assert.AreEqual("tomato", _w.GetContents());
        }

        [Test]
        public void Empty3LinesWithCommentAndTLW()
        {
            _w.Write($$"""

                {{COMMENT("Action was just an example. All we need for the testis this empty line")}}

                {{TLW}}{{"tomato"}}
                """);

            // All Leading Empty LINES were trimmed
            Assert.AreEqual("tomato", _w.GetContents());
        }

        [Test]
        public void IF_Blocks_Spaced_Indented()
        {
            string my_variable = "tomato";
            _w.Write($$"""

                {{IF(!string.IsNullOrEmpty(my_variable))}}
                    {{TLW}}{{"tomato"}}
                {{TLW}}{{ENDIF}}
                """);

            // All Empty LINES were trimmed
            Assert.AreEqual("tomato", _w.GetContents());
        }

        [Test]
        public void TestTLW()
        {
            _w.Write($$"""
                Start


                My paragraph


                {{TLW}}{{ "The end." }}
                """);

            Assert.AreEqual("""
                Start
                
                
                My paragraphThe end.                
                """, _w.GetContents());
        }

        [Test]
        public void TestTTW()
        {
            _w.Write($$"""
                {{"Start"}}{{TTW}}


                My paragraph


                The end.
                """);

            Assert.AreEqual("""
                StartMy paragraph
                
                
                The end.
                """, _w.GetContents());
        }



    }

}
