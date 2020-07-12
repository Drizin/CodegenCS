# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

This project is comprised of a few different components (maybe you're looking for a specific template, and not the core library):

Project | Description
------------ | -------------
[**CodegenCS (Core)**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS) | Class library for code generation. Basically it provides a custom TextWriter tweaked to solve common code generation difficulties
 [**CodegenCS.SqlServer**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.SqlServer) | C#/CSX/Powershell Scripts to reverse engineer a SQL Server database into JSON schema
[**CodegenCS.POCO**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO) | C# Templates / CSX/Powershell Scripts to read a JSON database schema (created with [CodegenCS.SqlServer](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.SqlServer) above) and generate POCO classes
[EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator) | This is a port of [Simon Hughes T4 templates for EF6](https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator) converted from T4 templates to C#/CodegenCS. <br/>  It's a Console application that reads a SQL Server Database and generates a DbContext and POCOS for Entity Framework 6. <br/> It's provided here only as a sample template. In his repository you may find up-to-date code, which now supports EFCore.


# CodegenCS (Core Library)

CodegenCS is a class library for code generation (for writing code or any text-based output) using pure C#.  
Basically it provides a custom TextWriter tweaked to solve common issues in code generation:
- Keeps track of current Indent level.  
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
        .WriteLine("// Testing FluentAPI");
        .WithCBlock("void MyMethod()", () =>
        {
            w.WriteLine("OtherMethod();");
        });
    foreach (var column in columns)
        w.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(column)} {propertyName} {{ get; set; }}");
});
    
w.SaveToFile("File1.cs"); 
```

See full documentation with more examples and list of features [here](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS).

# Why write templates in C# instead of T4, Mustache, Razor Engine, etc?

Templating Engines are usually good for end-users to write their templates (like Email templates), due to their sandboxed model, but what's better for a developer than a full-featured language?

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.  
We can use Dapper, Newtonsoft, and other amazing libraries.  
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  

In this [blog post](http://drizin.io/yet-another-code-generator/) I've explained why I've created this library, why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own.


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) to discuss your idea.


Some ideas for templates:
- Generate Dapper/Petapoco classes from database schema files - check [**CodegenCS.POCO**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO)
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
