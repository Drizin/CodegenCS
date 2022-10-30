**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/CodegenCS/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

# Models

Templates can read from any data source (a file or a database or anything else) but **Models** are our built-in mechanism for easily providing inputs to templates.

Both Visual Studio Extension and Command-line tool can load models using `IModelFactory`:

```cs
void Main(ICodegenTextWriter writer, IModelFactory factory)
{
    MyModel model = factory.LoadModelFromFile<MyModel>("MyModel.json");
    // ...
}
```

Command-line tool can also receive models as arguments - like `dotnet-codegencs template run <template> <model>`.  
Models are expected to be a path to a file in JSON format, and dotnet-codegencs will load/deserialize them according to the type expected by the template.


# Templates with Custom Models

See [Writing Templates with Custom Models](https://github.com/CodegenCS/CodegenCS/tree/master/docs/CustomModels.md)


# Out-of-the-box Models

Common tasks like generating code based on a [Database Schema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema) or based on a [REST API specification](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSwagAdapter) can be achieved using our out-of-the-box models, so that you don't have to reinvent the wheel.


## [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema) 

Represents the **schema of a relational database**.  

  The dotnet-codegencs tool has commands to **extract** a database schema (reverse engineer) creating a json model from existing **MSSQL or PostgreSQL** databases.


## [NSwagAdapter](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSwagAdapter) 

Can read a REST API model using **OpenAPI (aka Swagger)** (using [RicoSuter NSwag](https://github.com/RicoSuter/NSwag)).
