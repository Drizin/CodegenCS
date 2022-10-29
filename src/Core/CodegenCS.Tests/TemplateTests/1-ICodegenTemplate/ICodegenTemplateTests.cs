using CodegenCS.Models.DbSchema;
using NUnit.Framework;

namespace CodegenCS.Tests.TemplateTests;

/************************************************************************************************************************************************************************/
/// <summary>
/// Simple template: takes a single model, writes to a single file (using the Fluent API), and doesn't include any other template.
/// </summary>
class MyPocoTemplate : ICodegenTemplate<Table>
{
    public void Render(ICodegenTextWriter writer, Table model)
    {
        writer.Write($@"
                /// <summary>
                /// POCO for {model.TableName}
                /// </summary>
            ");
        writer.WithCurlyBraces($"public class {model.TableName}", (w) =>
        {
            foreach (var column in model.Columns)
            {
                w.WriteLine($"public {column.ClrType} {column.ColumnName} {{ get; set; }}");
            }
        });
    }
}

partial class ICodegenTemplateTests : BaseTest
{
    [Test]
    public void Test01()
    {
        var model = base.MyDbSchema;

        var writer = new CodegenTextWriter(); // or you can use:  var ctx = new CodegenContext(); var writer = ctx["YourFile.cs"];
        writer.LoadTemplate<MyPocoTemplate>().Render(model.Tables[0]);

        Assert_That_Content_IsEqual_To_File(writer, @"Users.cs");
    }
}

