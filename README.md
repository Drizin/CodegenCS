[![Nuget](https://img.shields.io/nuget/v/CodegenCS?label=CodegenCS)](https://www.nuget.org/packages/CodegenCS)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.svg)](https://www.nuget.org/packages/CodegenCS)

CodegenCS is a Class Library and Toolkit for Code Generation


# <a name="CodegenCS-Core"></a> CodegenCS Core Library (see [full documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS))

CodegenCS is a class library for doing code generation using plain C#.  
Basically it provides a custom TextWriter tweaked to solve common issues in code generation:
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

Want to learn more? Check out the [full documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS)

# <a name="dotnet-codegencs"></a> dotnet-codegencs

This is a **[.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) global tool** that contains utilities to build and run templates, and also contains some embedded out-of-the-box templates.**

**How to Install**: ```dotnet tool install -g dotnet-codegencs```

Usage (see all options): ```codegencs -?``` 

# <a name="dotnet-codegencs-utilities"></a><a name="dotnet-codegencs-extract-dbschema"></a> DbSchema Extractor

This is a command-line tool which extracts the schema of a MSSQL or PostgreSQL database and save it in a JSON file.  

**Sample usage**:

```codegencs extract-dbschema postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

# <a name="dotnet-codegencs-templates"></a><a name="dotnet-codegencs-simplepocogenerator"></a> Template: Simple POCO Generator

This is a template that generates POCO classes from a JSON schema extracted with [extract-dbschema](#dotnet-codegencs-extract-dbschema).

**Sample usage**:

```codegencs simplepocogenerator AdventureWorks.json --Namespace=MyProject.POCOs```

```codegencs simplepocogenerator AdventureWorks.json --Namespace=MyProject.POCOs --TargetFolder=OutputFolder --SingleFile=POCOs.generated.cs --CrudExtensions --CrudClassMethods```

**To see all available options use** ```codegencs simplepocogenerator -?``` or check out [Simple POCO documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/)

**It's also easy to customize the template output** - [check out how to do it](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/#customizing)


<!-- # Template: Entity Framework Core

This is a template (still in beta) that generates EntityFrameworkCore Entities and DbContext from a JSON schema extracted with [extract-dbschema](#dotnet-codegencs-extract-dbschema).

Sample usage:

```codegencs efcoregenerator AdventureWorks.json --TargetFolder=OutputFolder --Namespace=MyProject.POCOs --DbContextName=AdventureWorksDbContext``` -->


<!-- # Contributing

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](https://rdrizin.com/pages/Contact/) to discuss your idea.


Some ideas for new features or templates:
- Port [DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to other database vendors
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
- 2020-07-19: New project/scripts [Simple POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)
 -->



# Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=Drizin/CodegenCS&type=Date)](https://star-history.com/#Drizin/CodegenCS&Date)

# License
MIT License

