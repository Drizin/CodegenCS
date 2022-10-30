# CodegenCS.Models.NSwagAdapter

**CodegenCS.Models.NSwagAdapter** allows CodegenCS templates to read OpenAPI models (JSON or YAML format) using NSWag

# <a name="quickstart"></a>Quickstart

## Install Command-line Tool (dotnet-codegencs) or Visual Studio Extension

- [Install Command-line tool (dotnet-codegencs)]((https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs#quickstart)) by running this command: ```dotnet tool install -g dotnet-codegencs```  
- [Install Visual Studio Extension](https://github.com/CodegenCS/CodegenCS/tree/master/src/VSExtensions#quickstart) by downloading it [here](https://marketplace.visualstudio.com/items?itemName=Drizin.CodegenCS)
- Create a template file (e.g. Template.csx). See some sample templates below.  
- To run your template in Visual Studio Extension right-click the template in Solution Explorer and select "Run Template".  
  Or to get the template running automatically after each save, select the template in Solution Explorer and in the Properties Window set the "Custom Tool" to be "CodegenCS"
- To run your template with the Command-line tool use `dotnet-codegencs template run Template.csx`

Sample Template:

```cs
public class MyTemplate
{
    void Main(ICodegenContext context, IModelFactory factory)
    {
        var model = factory.LoadModelFromFile<OpenApiDocument>("petstore-openapi3.json");
        foreach (var entity in model.Definitions)
            context[entity.Key + ".cs"].WriteLine(GenerateEntity(entity.Key, entity.Value));
    }
    FormattableString GenerateEntity(string definitionName, NJsonSchema.JsonSchema schema)
    {
        return $$"""
            namespace MyNamespace
            {
                public class {{definitionName}}
                {
                    // my properties...
                    {{ schema.Properties.Select(prop => GenerateProperty(prop)) }}
                }
            }
            """;
    }
    FormattableString GenerateProperty(KeyValuePair<string, NJsonSchema.JsonSchemaProperty> prop)
    {
        return $$"""
        etc
        """;
    }
}
```