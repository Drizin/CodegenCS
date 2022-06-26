using CodegenCS.DbSchema;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CodegenCS.Tests.TemplateTests
{
    public class RawStringLiteralsTests
    {
        /**** Using C# 11 Raw String Literals (Requires Visual Studio 2012 17.2+ and requires <LangVersion>preview</LangVersion> in the csproj file ***/


        public class MyDatabaseTemplate : ICodegenMultifileTemplate
        {
            DatabaseSchema _databaseSchema;
            public MyDatabaseTemplate(DatabaseSchema databaseSchema) { _databaseSchema = databaseSchema; } // injected by dependency injection container

            public void Render(ICodegenContext context)
            {
                foreach (var table in _databaseSchema.Tables)
                {
                    context[$"{table.TableName}.cs"].RenderSinglefileTemplate<MyTableTemplate>(table);
                }
            }
        }
        public class MyTableTemplate : ICodegenSinglefileTemplate
        {
            Table _table;
            public MyTableTemplate(Table table) { _table = table; } // injected by dependency injection container
            public void Render(ICodegenTextWriter writer)
            {
                writer.Write($$"""
                    public class {{_table.TableName}}
                    {
                        {{ string.Join("\n", _table.Columns.Select(c => $$"""public {{c.ClrType}} {{c.ColumnName}} { get; set; }""")) }}
                    }
                    """);
            }
        }


        [Test]
        public void RawStringLiteralsTest()
        {
            var ctx = new CodegenContext();
            ctx.RenderMultifileTemplate<MyDatabaseTemplate>(databaseSchema);
            Assert.That(ctx.OutputFiles.Count == 2);
            string expected = @"
public class Users
{
    public int UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}";
            Assert.AreEqual(expected.TrimStart(), ctx["Users.cs"].GetContents());
        }




        DatabaseSchema databaseSchema = new DatabaseSchema()
        {
            Tables = new List<Table>()
                {
                    new Table()
                    {
                        TableName = "Users",
                        Columns = new List<Column>()
                        {
                            new Column() {  ColumnName = "UserId", ClrType = "int"},
                            new Column() {  ColumnName = "FirstName", ClrType = "string"},
                            new Column() {  ColumnName = "LastName", ClrType = "string"},
                        }
                    },
                    new Table()
                    {
                        TableName = "Products",
                        Columns = new List<Column>()
                        {
                            new Column() {  ColumnName = "Description", ClrType = "int"},
                            new Column() {  ColumnName = "ProductId", ClrType = "string"}
                        }
                    }
                }
        };




    }
}
