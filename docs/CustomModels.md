# Writing Templates with Custom Models

# <a name="quickstart"></a>Quickstart

## Creating our Model

Use any text editor to create a JSON file (code below) and save as "MyModel.json":

```json
{
    "Tables": ["Users", "Products"]
}
```

(In the example above it's just a single property with an array of strings, but model could be more complex)

## Create our Template

Inside our template we can just create a type representing that model (MyModel class), and then we can just expect the model to be injected into our template. `MyTemplate.cs` code: 

Use any text editor to create a CS Template file (code below) and save as "MyTemplate.cs"

```cs
// Type representing our model
// IJsonInputModel because the input is a JSON file
public class MyModel : IJsonInputModel
{
    public string[] Tables { get; set; }
}

public class MyTemplate
{
    // MyModel is automatically injected into Main() method below
    // The runtime will automatically deserialize the JSON input file
    void Main(ICodegenTextWriter writer, MyModel model)
    {
        writer.WriteLine($$"""
            namespace MyNamespace
            {
                {{ model.Tables.Select(t => GenerateTable(t)) }}
            }
            """);
    }
    void GenerateTable(ICodegenTextWriter writer, string tableName)
    {
        writer.WriteLine($$"""
            public class {{ tableName }}
            {
                // my properties...
            }
            """);
    }
}
```


## Run the Template (using dotnet-codegencs) and check the results

`dotnet-codegencs template run MyTemplate.cs MyModel.json`

Now open `MyTemplate.generated.cs` and you'll see the generated code:

```cs
namespace MyNamespace
{
    public class Users
    {
        // my properties...
    }

    public class Products
    {
        // my properties...
    }
}
```

## Or using Visual Studio Extension

The Visual Studio Extension doesn't allow models to be provided as command-line arguments, so we would have to use `IModelFactory`:
```cs
public class MyModel : IJsonInputModel
{
    public string[] Tables { get; set; }
}

public class MyTemplate
{
    void Main(ICodegenTextWriter writer, IModelFactory factory)
    {
        // file path can be relative to the template file
        MyModel model = factory.LoadModelFromFile<MyModel>("MyModel.json");
        writer.WriteLine($$"""
            namespace MyNamespace
            {
                {{ model.Tables.Select(t => GenerateTable(t)) }}
            }
            """);
    }
    void GenerateTable(ICodegenTextWriter writer, string tableName)
    {
        writer.WriteLine($$"""
            public class {{ tableName }}
            {
                // my properties...
            }
            """);
    }
}
```

# Another variation

A more "functional" approach is just returning the interpolated string instead of explicitly writing to a text writer:

```cs
// Type representing our model
// IJsonInputModel because the input is a JSON file
public class MyModel : IJsonInputModel
{
    public string[] Tables { get; set; }
}
public class MyTemplate
{
    FormattableString Main(MyModel model)
    {
        return $$"""
            namespace MyNamespace
            {
                {{ model.Tables.Select(t => GenerateTable(t)) }}
            }
            """;
    }
    FormattableString GenerateTable(string tableName)
    {
        return $$"""
            public class {{ tableName }}
            {
                // my properties...
            }
            """;
    }
}
```

