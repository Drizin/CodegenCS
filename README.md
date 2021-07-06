# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

This repo contains the [**CodegenCS core library**](#CodegenCS-Core), the dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs), and some out-of-the-box [templates and utilities](#dotnet-codegencs-templates) which are available through dotnet-codegencs.
 

# <a name="CodegenCS-Core"></a> CodegenCS ([Core Library](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS))

CodegenCS is a class library for code generation (for writing code or any text-based output) using pure C#.  
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

If you want to use the out-of-the-box templates and utilities (without making changes to the templates), this is all you need.

**How to Install**

```dotnet tool install -g dotnet-codegencs```

## <a name="dotnet-codegencs-templates"></a> Templates and Utilities

### <a name="dotnet-codegencs-dbschema-extractor"> DbSchema Extractor

This is a command-line tool (part of dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs)) which extracts the schema of a MSSQL or PostgreSQL database and save it in a JSON file.  

Sample usage:

```codegencs dbschema-extractor /postgresql /cn="Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" /output=AdventureWorks.json```

```codegencs dbschema-extractor /mssql /cn="Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" /output=AdventureWorks.json```

```codegencs dbschema-extractor /mssql /cn="Server=MYSERVER; Database=AdventureWorks; Integrated Security=True" /output=AdventureWorks.json```

### <a name="dotnet-codegencs-poco"> Simple POCO Generator

This is a template (part of [dotnet command-line tool **dotnet-codegencs**](#dotnet-codegencs)) that generates POCO classes from a JSON schema extracted with [dbschema-extractor](#dotnet-codegencs-dbschema-extractor).

Sample usage:

```codegencs poco /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs```

### Entity Framework Core Generator

This is a template (part of [dotnet command-line tool **dotnet-codegencs**](#dotnet-codegencs)) that generates EntityFrameworkCore Entities and DbContext from a JSON schema extracted with [dbschema-extractor](#dotnet-codegencs-dbschema-extractor).

Sample usage:

```CodegenCSEFCore.exe /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs /dbcontextname=AdventureWorksDbContext```



# Why write templates in C# instead of T4, Mustache, Razor Engine, etc?

Templating Engines are usually good for end-users to write their templates (like Email templates), due to their sandboxed model, but what's better for a developer than a full-featured language?

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.  
We can use Dapper, Newtonsoft, and other amazing libraries.  
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  

In this [blog post](http://drizin.io/yet-another-code-generator/) I've explained why I've created this library, why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own.


# Collaborate

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




## License
MIT License
