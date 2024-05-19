class MyTemplate
{
    string[] cols = new string[] { "AddressLine1", "AddressLine2", "City" };

    FormattableString Main() => $$"""
        INSERT INTO [Person].[Address]
        (
            {{cols.Select(col => "[" + col + "]").Render(RenderEnumerableOptions.MultiLineCSV)}}
        )
        VALUES
        (
            {{cols.Select(col => "@" + col).Render(RenderEnumerableOptions.MultiLineCSV)}}
        )
        """;
}
