using NUnit.Framework;
using CodegenCS;
using CodegenCS.DbSchema;
using System.IO;

namespace CodegenCS.Tests.TemplateTests;

/************************************************************************************************************************************************************************/
/// <summary>
/// Multifile template: receives a single model, writes to multiple files by explicitly loading subtemplates (directly on ICodegenTextWriter FluentAPI) and passing submodels to them
/// </summary>
class MyDbTemplate : ICodegenMultifileTemplate<DatabaseSchema>
{
    public void Render(ICodegenContext context, DatabaseSchema model)
    {
        foreach (var table in model.Tables)
        {
            context[table.TableName + ".cs"].LoadTemplate<MyPocoTemplate>().Render(table);
        }
    }
}
partial class ICodegenMultifileTemplateTests : BaseTest
{
    [Test]
    public void Test21()
    {
        var context = new CodegenContext();
        var model = base.MyDbSchema;

        context.LoadTemplate<MyDbTemplate>().Render(model);

        Assert_That_ContextOutput_IsEqual_To_TestOutputFolder(context);
    }
}




/************************************************************************************************************************************************************************/
/// <summary>
/// Multifile template like the previous one but embedding subtemplates (with submodels) in the interpolated strings (mixed with other contents).
/// </summary>
class MyDbTemplate2 : ICodegenMultifileTemplate<DatabaseSchema>
{
    public void Render(ICodegenContext context, DatabaseSchema model)
    {
        foreach (var table in model.Tables)
        {
            var writer = context[table.TableName + ".cs"];
            
            // Fluent API
            writer
                .WriteLine("/// Copyright Rick Drizin (just kidding - this is MIT license - use however you like it!)")
                .WriteLine($@"
                    using System;
                    using System.IO;
                    using System.Collections.Generic;")
                .WriteLine()
                .Write($@"
                    namespace MyNamespace
                    {{
                        {Template.Load<MyPocoTemplate>().Render(table)}
                    }}");
        }
    }
}
partial class ICodegenMultifileTemplateTests : BaseTest
{
    [Test]
    public void Test22()
    {
        var model = base.MyDbSchema;

        var context = new CodegenContext();
        context.LoadTemplate<MyDbTemplate2>().Render(model);

        Assert_That_ContextOutput_IsEqual_To_TestOutputFolder(context);
    }
}
