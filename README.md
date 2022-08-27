[![Nuget](https://img.shields.io/nuget/v/CodegenCS?label=CodegenCS)](https://www.nuget.org/packages/CodegenCS)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.svg)](https://www.nuget.org/packages/CodegenCS)

CodegenCS is Toolkit for Code Generation

# <a name="dotnet-codegencs"></a> dotnet-codegencs

**dotnet-codegencs** is a **[.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)** that contains utilities to build and run templates, and utilities to extract models to be used with the templates.

### Installation
```dotnet tool install -g dotnet-codegencs```

### Extracting the Schema of a Database

CodegenCS templates may be based on any JSON input model, but currently the only out-of-the-box model is [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS.DbSchema/DbSchema) which represents the schema of a relational database.  

Extracting a Microsoft SQL Server schema into a JSON file:  

`dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json` (using SQL server authentication)

`dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json` (using Windows authentication)

Extracting a PostgreSQL schema into a JSON file:  

`dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json`

### Downloading a template

The `template clone` command is used to download (to your local folder) a copy of any online template.  
As an example this is how you would clone **SimplePocos.cs** (which is a sample template that generates simple POCO classes from the Database Schema):  

`dotnet-codegencs template clone https://github.com/CodegenCS/CodegenCS.Templates/SimplePocos/SimplePocos.cs`

For downloading any template at [https://github.com/CodegenCS/CodegenCS.Templates](https://github.com/CodegenCS/CodegenCS.Templates) you can just use the short name (e.g. `dotnet-codegencs template clone SimplePocos`). For downloading third-party templates you can use `dotnet-codegencs template clone <template-url>`.

### Rebuilding a template (optional)

Templates can be executed directly from source (no need to build, they will be compiled on-the-fly), but building them into DLLs can save you a few milliseconds if you'll invoke it multiple times.  
When templates are downloaded (using `template clone`) they are automatically compiled (into DLL), but if you modify the template source you can rebuild it on your own:

`dotnet-codegencs template build SimplePocos.cs`

### Running a template

As explained earlier you can invoke the templates either using the source (.cs) or from the built DLL:  

`dotnet-codegencs template run <template> <model> [template-args]`

Templates may define their own options and arguments using [.NET System.CommandLine](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options).  
SimplePocos.cs for example defines [1 mandatory argument](https://github.com/CodegenCS/CodegenCS.Templates/blob/main/SimplePocos/SimplePocos.cs#L49) (the namespace for the generated POCOs) so it should be invoked like this:

`dotnet-codegencs template run SimplePocos.cs AdventureWorks.json MyProject.POCOs`

`template run` also accepts some options like `--OutputFolder <outputFolder>` (base folder for all output files), or `--File <defaultOutputFile>` (name of the default output file, useful for templates that write to a single file). In the specific example of SimplePocos we can define the default output file (`template run` option) and we can define that all pocos should be rendered into a single file (`SimplePocos` option):

`dotnet-codegencs template run SimplePocos.cs --File MyPOCOS.cs AdventureWorks.json MyProject.POCOs -p:SingleFile`

For any template that [define their own arguments/options](https://github.com/CodegenCS/CodegenCS.Templates/blob/main/SimplePocos/SimplePocos.cs#L49) we can also get help (see options):

`dotnet-codegencs template run SimplePocos.cs -?`


# <a name="CodegenCS-Core"></a> CodegenCS Core Library

CodegenCS is a class library for doing code generation using plain C#.  
Basically it's a "TextWriter on steroids" tweaked to help with common code generation tasks and challenges:
- Preserves indentation (when we write new lines it will automatically indent the line according to current level) - indent can be controlled explicitly or implicitly.
- Implicit control of indentation means we can embed complex templates inside other templates and their indentation is automatically "captured" by the position where they are embedded
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code
- Helpers to keep track of multiple files which can be saved at once in the output folder.
- Supports string interpolation of IEnumerables (items are rendered one by one, and between the items we can have separators like line-breaks or others)
- Supports string interpolation of Actions, Functions or Templating Interfaces (to break complex templates into smaller parts)
- **IF / ELSE / ENDIF symbols** that can be embedded within the text strings and allow concise syntax for **Control Blocks**

**Sample usage**:

```cs
FormattableString RenderTable(Table table) => $$"""
    /// <summary>
    /// POCO for {{ table.TableName }}
    /// </summary>
    public class {{ table.TableName }}
    {
        // class members...
        {{ table.Columns.Select(column => $$"""public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }""" ).Render() }}
    }
    """;
var w = new CodegenTextWriter();

var schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));

w.WriteLine($$"""
    namespace {{myNamespace}}
    {
        {{ schema.Tables.Select(t => RenderTable(t)).Render() }}
    }
    """);

w.SaveToFile("MyPOCOs.cs"); 
```

Want to learn more? Check out the [full CodegenCS Library documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS), where you can learn more about CodegenTextWriter (and how indenting works), CodegenContext, how to write clean and reusable templates using String Interpolation / Raw String Literals / IEnumerables.


<!-- # Template: Entity Framework Core

This is a template (still in beta) that generates EntityFrameworkCore Entities and DbContext from a JSON schema extracted with [extract-dbschema](#dotnet-codegencs-extract-dbschema).

Sample usage:

```dotnet-codegencs efcoregenerator AdventureWorks.json --TargetFolder=OutputFolder --Namespace=MyProject.POCOs --DbContextName=AdventureWorksDbContext``` -->


<!-- # Contributing

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](https://rdrizin.com/pages/Contact/) to discuss your idea.


Some ideas for new features or templates:
- Port [DbSchema.Extractor](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to other database vendors
- Generate Dapper/Petapoco classes from database schema files - check [**Simple POCO Generator**](#dotnet-codegencs-simplepocogenerator)
- Generate EF Core Entities/DBContext
- Generate REST Web API endpoints from OpenAPI YAML
- Generate Nancy endpoints for retrieving/updating business entities
- Generate REST or SOAP web service wrappers (client)
- Generate ASP.NET MVC (Razor Views CSHTML and Controllers) to display and edit business entities
- Data Access Objects from database schema files
- Object caching
- Application-level database journaling


## History
- 2020-07-19: New project/scripts [Simple POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/CodegenCS/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)
 -->



<!-- # Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=CodegenCS/CodegenCS&type=Date)](https://star-history.com/#CodegenCS/CodegenCS&Date) -->

# License
MIT License

