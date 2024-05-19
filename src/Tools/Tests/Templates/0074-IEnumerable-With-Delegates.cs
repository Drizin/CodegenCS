class MyTemplate
{

    void Main(ICodegenOutputFile w, IModelFactory factory)
    {
        var schema = factory.LoadModelFromFile<DatabaseSchema>(@"Models\AdventureWorks.json");

        w.Write($$"""
          namespace MyNamespace
          {
              {{GenerateTables(schema)}}
          }
          """);
    }

    // THIS! This Func gets a DatabaseSchema and returns an IEnumerable of delegates (GenerateTable),
    // but the delegate is enriched with the "this is the table you need, and auto-inject any other arguments"
    static Func<DatabaseSchema, object> GenerateTables = (DatabaseSchema schema) =>
        schema.Tables.Select(table => GenerateTable.WithArguments(null, table))
            .Render(tableSeparatorOptions);

    // This Func requires 2 arguments, and the first one (ILogger) 
    // will be automatically injected (because WithArguments with null)
    static Func<CodegenCS.Runtime.ILogger, Table, FormattableString> GenerateTable = (logger, table) =>
    {
        logger.WriteLineAsync($"Generating {table.TableName}...");
        return (FormattableString)$$"""
          /// <summary>
          /// POCO for {{table.TableName}}
          /// </summary>
          public class {{table.TableName}}
          {
              {{GenerateColumns(table)}}
          }
          """;
    };

    // return type is IEnumerable<FormattableString>, but for simplicity let's define as object.
    static Func<Table, object> GenerateColumns = (table) =>
        table.Columns.Select(column => (FormattableString)$$"""
          public {{column.ClrType}} {{column.ColumnName}} { get; set; }
          """);

    // Since tables render into many lines let's ensure an empty line between each table, improving readability
    static RenderEnumerableOptions tableSeparatorOptions = new RenderEnumerableOptions()
    {
        BetweenItemsBehavior = ItemsSeparatorBehavior.EnsureFullEmptyLine
    };
}