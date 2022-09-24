**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/CodegenCS/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about **CodegenCS Visual Studio Extension**:
- If you are **writing a template** (code generator) and want to learn more about CodegenCS features (and internals) then check out the [CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) documentation.
- If you want to **compile and run templates** or **reverse-engineer a database schema** check out the [`Command-line Tool dotnet-codegencs`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) documentation
- If you want to **browse the sample templates** (POCO Generators, DAL generators, etc) check out [https://github.com/CodegenCS/Templates/](https://github.com/CodegenCS/Templates/)
- If you just want to **download the Visual Studio Extension** this is the right place.

# Visual Studio Extension

## Quickstart

- Install it from Visual Studio (Tools - Extensions - search for "CodegenCS") or download it [here](https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS)
- Create a new file with `csx` extension. See some sample templates below.

```cs
// Single-file output, where `Main()` returns directly the main template
// "Text" approach
public class MyTemplate
{
    FormattableString Main()
    {
        var model = new { Tables = new string[] { "Users", "Products" } };
        return $$"""
            namespace MyNamespace
            {
                {{model.Tables.ToList().Select(t => GenerateTable(t))}}
            }
            """;
    }
    FormattableString GenerateTable(string tableName)
    {
        return $$"""
            public class {{tableName}}
            {
                // my properties...
            }
            """;
    }
}
```

```cs
// Multiple-files output, with programmatic approach
public class MyTemplate
{
    void Main(ICodegenContext context)
    {
        var model = new { Tables = new string[] { "Users", "Products" } };
        foreach (var table in model.Tables)
            context[table + ".cs"]
            .WriteLine("//"+ table)
            .WriteLine(GenerateTable(table));
    }
    FormattableString GenerateTable(string tableName)
    {
        return $$"""
            namespace MyNamespace
            {
                public class {{tableName}}
                {
                    // my properties...
                }
            }
            """;
    }
}
```

```cs
// Reading model from JSON file
public class MyTemplate
{
    void Main(ICodegenContext context)
    {
        var model = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
        foreach (var table in model.Tables)
            context[table.TableName + ".cs"].WriteLine(GenerateTable(table));
    }
    FormattableString GenerateTable(Table table)
    {
        return $$"""
            namespace MyNamespace
            {
                public class {{table.TableName}}
                {
                    // my properties...
                }
            }
            """;
    }
}
```