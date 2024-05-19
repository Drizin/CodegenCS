class MyTemplate
{
    FormattableString RenderTable(Table table) => $$"""
        /// <summary>
        /// POCO for {{table.TableName}}
        /// </summary>
        public class {{table.TableName}}
        {
            // class members...
            {{table.Columns.Select(column => (FormattableString)$$"""public {{column.ClrType}} {{column.ColumnName}} { get; set; }""").Render()}}
        }
        """;

    void Main(ICodegenOutputFile w, IModelFactory factory)
    {
        var schema = factory.LoadModelFromFile<DatabaseSchema>(@"Models\AdventureWorks.json");

        w.WriteLine($$"""
            namespace MyNamespace
            {
                {{schema.Tables.Select(t => RenderTable(t)).Render()}}
            }
            """);
    }
}
