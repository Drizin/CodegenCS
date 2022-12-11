using CodegenCS.Models.DbSchema;
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Linq;

namespace CodegenCS.Tests.CoreTests
{
    /// <summary>
    /// "Basics" - misc tests (raw string literals, implicit indentation, ienumerables) to show the basics.
    /// </summary>
    internal class BasicTests : BaseTest
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region Raw String Literals (This is the fundamental syntax for writing Multiline blocks and for writing Interpolated STrings)
        [Test]
        public void RawStringLiteral()
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
        #endregion

        #region Different types of callbacks can embedded inside interpolated strings (and will preserve the implicit indentation if any)
        [Test]
        public void EmbedFormattableString()
        {
            FormattableString writeMethod = $$"""
                void MyMethod1()
                {
                    Hello
                }
                """;
            FormattableString writeClass = $$"""
                public class MyClass
                {
                    {{writeMethod}}
                }
                """;

            _w.Write($$"""
                public namespace MyNamespace
                {
                    {{writeClass}}
                }
                """);

            string expected = @"
public namespace MyNamespace
{
    public class MyClass
    {
        void MyMethod1()
        {
            Hello
        }
    }
}
".Trim();
            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void EmbedActions()
        {
            Action<ICodegenTextWriter> writeMethod = (w) => w.Write("""
                void MyMethod1()
                {
                    Hello
                }
                """);
            Action<ICodegenTextWriter> writeClass = (w) => w.Write($$"""
                public class MyClass
                {
                    {{writeMethod}}
                }
                """);

            _w.Write($$"""
                public namespace MyNamespace
                {
                    {{writeClass}}
                }
                """);

            string expected = @"
public namespace MyNamespace
{
    public class MyClass
    {
        void MyMethod1()
        {
            Hello
        }
    }
}
".Trim();
            Assert.AreEqual(expected, _w.GetContents());
        }

        #endregion

        #region Embedding IEnumerables (of any supported type, like FormattableString or others) can be done directly inside interpolated strings

        [Test]
        public void TestIEnumerable()
        {
            _w.Write($$"""
                I have a LOT of things to do today:
                    {{todoList.Select(item => $"- {item.Description}").Render()}}
                """);
            string expected = @"
I have a LOT of things to do today:
    - Get milk
    - Clean the house
    - Mow the lawn".TrimStart(Environment.NewLine.ToCharArray());

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestEmptyIEnumerable()
        {
            _w.Write($$"""
                I have a LOT of things to do today:
                    {{emptyTodoList.Select(item => $"- {item.Description}").Render()}}
                """);
            string expected = @"I have a LOT of things to do today:";

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestIEnumerable2()
        {
            _w.Write($@"I have a LOT of things to do today: {todoList.Select(item => $"{item.Description}").RenderAsSingleLineCSV()}");
            string expected = "I have a LOT of things to do today: Get milk, Clean the house, Mow the lawn";

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestEmptyIEnumerable2()
        {
            _w.Write($@"I have a LOT of things to do today: {emptyTodoList.Select(item => $"{item.Description}").RenderAsSingleLineCSV()}");
            string expected = "I have a LOT of things to do today:";

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestIEnumerable3()
        {
            FormattableString RenderProp(Column column) => $$"""public {{column.ClrType}} {{column.ColumnName}} { get; set; }""";

            // table.Columns.Select will render IEnumerable<FormattableString>,
            // which by default (even if we don't call Render()) will add linebreak separators between each item
            FormattableString RenderTable(Table table) => $$"""
                /// <summary>
                /// POCO for {{table.TableName}}
                /// </summary>
                public class {{table.TableName}}
                {
                    {{table.Columns.Select(column => RenderProp(column)).Render()}}
                }
                """;

            var usersTable = MyDbSchema.Tables.Single(t => t.TableName == "Users");
            var productsTable = MyDbSchema.Tables.Single(t => t.TableName == "Products");
            string myNamespace = "MyPocos";

            _w.Write($$"""
                namespace {{myNamespace}}
                {
                    {{RenderTable(usersTable)}}
                    {{RenderTable(productsTable)}}
                }
                """);

            string expected = @"
namespace MyPocos
{
    /// <summary>
    /// POCO for Users
    /// </summary>
    public class Users
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
    /// <summary>
    /// POCO for Products
    /// </summary>
    public class Products
    {
        public int Description { get; set; }
        public string ProductId { get; set; }
    }
}".Trim();
            Assert.AreEqual(expected, _w.GetContents());
        }


        #endregion

        #region Literal Braces
        [Test]
        public void EmptyBraces()
        {
            _w.Write($"{{}}");
            Assert.AreEqual("{}", _w.GetContents());
        }

        [Test]
        public void LiteralBraces1()
        {
            string var = "variable";
            _w.Write($$"""
                Testing {{var}}{0} {1} {3}
                   {5}
                """);
            Assert.AreEqual("Testing variable{0} {1} {3}\r\n   {5}", _w.GetContents());
        }

        [Test]
        public void LiteralBraces2()
        {
            string myMethod = "Method";
            _w.Write($$"""
                void {{myMethod}}()
                {
                    return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
                }
                """);
            string expected = $$"""
                void Method()
                {
                    return string.Format("HTTP Response: \n\n{0}\n\n{1}", Response, base.ToString());
                }
                """;
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region Auto Indenting
        [Test]
        public void AutoIndentWhitespace()
        {
            string myMultilineBlock = "My\r\nMultiline\r\nstring";
            _w.Write($$"""
                    {{myMultilineBlock}}
                void MyMethod()
                {
                    return 0;
                }
                """);
            string expected = $$"""
                    My
                    Multiline
                    string
                void MyMethod()
                {
                    return 0;
                }
                """;
            Assert.AreEqual(expected, _w.GetContents());
        }
        [Test]
        public void AutoIndentCSharpComments()
        {
        	//_w.PreserveNonWhitespaceIndent = true;
            string myMultilineBlock = "My\r\nMultiline\r\nstring";
            _w.Write($$"""
                /// {{myMultilineBlock}}
                void MyMethod()
                {
                    return 0;
                }
                """);
            string expected = $$"""
                /// My
                /// Multiline
                /// string
                void MyMethod()
                {
                    return 0;
                }
                """;
            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void AutoIndentSQLComments()
        {
        	_w.PreserveNonWhitespaceIndent = true;
            string myMultilineBlock = "My\r\nMultiline\r\nstring";
            _w.Write($$"""
                -- {{myMultilineBlock}}
                INSERT INTO MyTable (...)
                """);
            string expected = $$"""
                -- My
                -- Multiline
                -- string
                INSERT INTO MyTable (...)
                """;
            Assert.AreEqual(expected, _w.GetContents());
        }
        #endregion

        #region Text-Manipulation
        [Test]
        public void TestClearLastLine()
        {
            _w.Write($$"""
                Line1
                Line2
                Line3
                """);

            Assert.AreEqual("Line1\r\nLine2\r\nLine3", _w.GetContents());
            _w.ClearLastLine();
            Assert.AreEqual("Line1\r\nLine2\r\n", _w.GetContents());
        }
        [Test]
        public void TestRemoveLastLine()
        {
            _w.Write($$"""
                Line1
                Line2
                Line3
                """);

            Assert.AreEqual("Line1\r\nLine2\r\nLine3", _w.GetContents());
            _w.RemoveLastLine();
            Assert.AreEqual("Line1\r\nLine2", _w.GetContents());
        }
        #endregion
    }

}
