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
    internal class DelegateTests : BaseTest
    {
        CodegenTextWriter _w = null;

        #region Setup
        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }
        #endregion

        #region Embedding IEnumerables (of any supported type, like FormattableString or others) can be done directly inside interpolated strings


        string expected = """
                namespace MyNamespace
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
                }
                """;

        [Test]
        public void TestActionDelegatesWithArguments()
        {

            Action<ICodegenTextWriter, Table> GenerateColumns = (w, table) =>
            {
                foreach(var column in table.Columns)
                {
                    w.Write($$"""
                        public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }
                        """);

                    // outer template already adds a linebreak after this block, so no need to add twice
                    // yeah, manually controlling linebreaks is ugly (that's why you should prefer embedding IEnumerables which automatically control line breaks!)
                    if (table.Columns.Last() != column)
                        w.WriteLine();
                }
            };

            Action<ICodegenTextWriter, DatabaseSchema> GenerateTables = (ICodegenTextWriter w, DatabaseSchema schema) =>
            {
                foreach(var table in schema.Tables)
                {
                    w.Write($$"""
                        /// <summary>
                        /// POCO for {{ table.TableName }}
                        /// </summary>
                        public class {{ table.TableName }}
                        {
                            {{ GenerateColumns.WithArguments(null, table) }}
                        }
                        """);
                    // outer template already adds a linebreak after this block, so no need to add twice
                    // yeah, manually controlling linebreaks is ugly (that's why you should prefer embedding IEnumerables which automatically control line breaks!)
                    if (schema.Tables.Last() != table)
                        w.WriteLine().WriteLine(); // we want an extra line between multiblock items (embedded IEnumerables already do that automatically!)
                }
            };

            string myNamespace = "MyNamespace";

            _w.Write($$"""
                namespace {{myNamespace}}
                {
                    {{ GenerateTables.WithArguments(null, MyDbSchema) }}
                }
                """);

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestFuncDelegatesWithArguments()
        {

            Func<Table, IEnumerable<FormattableString>> GenerateColumns = (table) =>
                table.Columns.Select(column => (FormattableString) $$"""
                    public {{column.ClrType}} {{column.ColumnName}} { get; set; }
                    """);

            Func<ICodegenTextWriter, DatabaseSchema, IEnumerable<FormattableString>> GenerateTables = (ICodegenTextWriter w, DatabaseSchema schema) =>
                schema.Tables.Select(table => (FormattableString)$$"""
                    /// <summary>
                    /// POCO for {{table.TableName}}
                    /// </summary>
                    public class {{table.TableName}}
                    {
                        {{ GenerateColumns.WithArguments(table) }}
                    }
                    """);

            string myNamespace = "MyNamespace";

            _w.Write($$"""
                namespace {{myNamespace}}
                {
                    {{GenerateTables.WithArguments(null, MyDbSchema)}}
                }
                """);

            Assert.AreEqual(expected, _w.GetContents());
        }

        [Test]
        public void TestIEnumerableCallingFuncDelegatesWithArguments()
        {

            // return type is IEnumerable<FormattableString>, but for simplicity let's define as object.
            Func<Table, object> GenerateColumns = (table) =>
                table.Columns.Select(column => (FormattableString)$$"""
                    public {{column.ClrType}} {{column.ColumnName}} { get; set; }
                    """);

            // return type is a wrapped IEnumerable<>, but for simplicity let's define as object.
            Func<ICodegenTextWriter, DatabaseSchema, object> GenerateTables = (ICodegenTextWriter w, DatabaseSchema schema) =>
                schema.Tables.Select(table => (FormattableString)$$"""
                    /// <summary>
                    /// POCO for {{table.TableName}}
                    /// </summary>
                    public class {{table.TableName}}
                    {
                        {{GenerateColumns.WithArguments(table)}}
                    }
                    """).Render(new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.WriteLineBreak });

            string myNamespace = "MyNamespace";

            _w.Write($$"""
                namespace {{myNamespace}}
                {
                    {{GenerateTables.WithArguments(null, MyDbSchema)}}
                }
                """);

            // In the .Render() extension we overwrote the default IEnumerable behavior which would be EnsureFullEmptyLineAfterMultilineItems (add 2 linebreaks between blocks)
            // so we DON'T expect anymore an empty line 

            string expectedWithoutEmptyLines = expected.Replace("""
                }
                
                    /// <summary>
                """, """
                }
                    /// <summary>
                """);

            Assert.AreEqual(expectedWithoutEmptyLines, _w.GetContents());
        }



        #endregion


    }

}
