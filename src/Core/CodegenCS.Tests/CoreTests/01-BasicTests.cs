using CodegenCS;
using CodegenCS.DbSchema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            _w.Write($@"
                I have a LOT of things to do today:
                    {todoList.Select(item => $"- {item.Description}").Render()}");
            string expected = @"
I have a LOT of things to do today:
    - Get milk
    - Clean the house
    - Mow the lawn".TrimStart(Environment.NewLine.ToCharArray());

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


    }

}
