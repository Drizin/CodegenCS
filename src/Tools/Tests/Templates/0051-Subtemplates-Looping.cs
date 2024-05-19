class MyTemplate
{
    void Main(IModelFactory factory, ICodegenOutputFile writer)
    {
        // Hold on, we'll explain this shortly
        var model = factory.LoadModelFromFile<DatabaseSchema>(@"Models\AdventureWorks.json");

        foreach (var table in model.Tables)
        {
            writer.WriteLine($$"""
        /// <summary>
        /// POCO for {{table.TableName}}
        /// </summary>
        public class {{table.TableName}}
        {
            {{RenderColumns.WithArguments(table, null)}}{{Symbols.TLW}}
        }
        """);
        }
    }

    // This delegate can't be interpolated directly because it depends on Table type which is not registered
    // That's why it's "enriched" with WithArguments that specify that "table" variable should be passed as the first argument
    Action<Table, ICodegenOutputFile> RenderColumns = (table, writer) =>
    {
        foreach (var column in table.Columns)
            writer.WriteLine($$"""
              public {{column.ClrType}} {{column.ColumnName}} { get; set; }
              """);
    };
}