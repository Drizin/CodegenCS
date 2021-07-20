# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

This repository contains the [**CodegenCS core library**](#CodegenCS-Core), and the dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs) which contains some [utilities](#dotnet-codegencs-utilities) (like extracting MSSQL/PostgreSQL schemas) and some out-of-the-box [templates](#dotnet-codegencs-templates) (like POCO generator).
 

# <a name="CodegenCS-Core"></a> CodegenCS ([Core Library](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS))

CodegenCS is a class library for code generation using pure C#.  
Basically it provides a custom TextWriter tweaked to solve common issues in code generation:
- Preserves indent (keeps track of current Indent level).  
  When you write new lines it will automatically indent the line according to current level. 
- Helpers to concisely write indented blocks (C-style, Java-style or Python-style) using a Fluent API
  (IDisposable context will automatically close blocks)
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code.
- Helpers to keep track of multiple files which can be saved at once in the output folder.

**Sample usage**:

```cs
var w = new CodegenTextWriter();
w.WithCBlock("public class MyClass", () =>
{
  w
    .WriteLine("// Testing FluentAPI")
    .WithCBlock("void MyMethod()", () =>
    {
      w.WriteLine("OtherMethod();");
    });
  foreach (var column in columns)
    w.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(column)} {propertyName} {{ get; set; }}");
});

w.SaveToFile("File1.cs"); 
```

Want to learn more? Check out the [full documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS) and the [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests/CoreTests).

# <a name="dotnet-codegencs"></a> dotnet-codegencs (.NET global tool)

**This is a .NET 5 global tool with some out-of-the-box templates and utilities.**

**How to Install**: ```dotnet tool install -g dotnet-codegencs```

## <a name="dotnet-codegencs-utilities"></a> Utilities

### <a name="dotnet-codegencs-dbschema-extractor"> DbSchema Extractor

This is a command-line tool (part of dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs)) which extracts the schema of a MSSQL or PostgreSQL database and save it in a JSON file.  

**Sample usage**:

```codegencs dbschema-extractor /postgresql /cn="Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" /output=AdventureWorks.json```

```codegencs dbschema-extractor /mssql /cn="Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" /output=AdventureWorks.json```

```codegencs dbschema-extractor /mssql /cn="Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" /output=AdventureWorks.json```

If you need to modify this utility (or port it to another database provider), check the [DbSchema.Extractor source code](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor).

## <a name="dotnet-codegencs-templates"></a> Templates

### <a name="dotnet-codegencs-poco"> Template: Simple POCO

This is a template (part of dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs)) that generates POCO classes from a JSON schema extracted with [dbschema-extractor](#dotnet-codegencs-dbschema-extractor).

**Sample usage**:

```codegencs poco /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs```

```codegencs poco /input=AdventureWorks.json /targetFolder=. /namespace=MyProject.POCOs /SingleFile=POCOs.generated.cs /CrudExtensions /CrudClassMethods```

For more options use ```codegencs poco /?``` or check out [Simple POCO documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO)


### Template: Entity Framework Core

This is a template (not yet part of dotnet-codegencs) that generates EntityFrameworkCore Entities and DbContext from a JSON schema extracted with [dbschema-extractor](#dotnet-codegencs-dbschema-extractor).

Sample usage:

```CodegenCSEFCore.exe /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs /dbcontextname=AdventureWorksDbContext```




## Contributing

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](https://rdrizin.com/pages/Contact/) to discuss your idea.


Some ideas for templates:
- Generate Dapper/Petapoco classes from database schema files - check [**Simple POCO Generator**](#dotnet-codegencs-poco)
- Generate EF Core Entities/DBContext
- Generate REST Web API endpoints from OpenAPI YAML
- Generate Nancy endpoints for retrieving/updating business entities
- Generate REST or SOAP web service wrappers (client)
- Generate ASP.NET MVC (Razor Views CSHTML and Controllers) to display and edit business entities
- Data Access Objects from database schema files
- Object caching
- Application-level database journaling


## History
- 2020-07-19: New project/scripts [CodegenCS.POCO](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)



## License
MIT License
