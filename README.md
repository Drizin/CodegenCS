# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

This project is comprised of a few different components (maybe you're looking for a specific template, and not the core library):

- [**CodegenCS (Core)**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS): Class library for code generation. Basically it provides a custom TextWriter tweaked to solve common code generation difficulties
- [**CodegenCS.SqlServer**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.SqlServer): Scripts to reverse engineer a SQL Server database into JSON schema
- [**CodegenCS.POCO**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO): Scripts to read a JSON database schema (above) and generate POCO classes
- [EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator):
  This is a port of [Simon Hughes T4 templates for EF6](https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator) converted from T4 templates to C#/CodegenCS.
  It's a Console application that reads a SQL Server Database and generates a DbContext and POCOS for Entity Framework 6.  
  It's provided here only as a sample template. In his repository you may find up-to-date code, with support for EFCore.

# CodegenCS (Core Library)

CodegenCS is a class library for code generation (for writing code or any text-based output) using pure C#.  
Basically it provides a custom TextWriter tweaked to solve common issues in code generation:
- Keeps track of current Indent level.  
  When you write new lines it will automatically indent the line according to current level. 
- Helpers to concisely write C-style blocks  
  (IDisposable context will automatically close blocks)
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code.

Sample code:
```cs
string tableFilePath = GetFileNameForTable(table);
Console.WriteLine($"Generating {tableFilePath}...");
using (var writer = _generatorContext.GetOutputFile(relativePath: tableFilePath).Writer)
{
  writer.WriteLine(@"using System;");
  writer.WriteLine(@"using System.Collections.Generic;");
  writer.WriteLine();
  using (writer.WithCBlock($"namespace {Namespace}"))
  {
    using (writer.WithCBlock($"public partial class {entityClassName}"))
    {
      var columns = table.Columns.Where(c => ShouldProcessColumn(table, c));
      foreach (var column in columns)
      {
        string propertyName = GetPropertyNameForDatabaseColumn(column);
        writer.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(column)} {propertyName} {{ get; set; }}");
      }
    }
  }
}
```

See full documentation and features [here](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS).

# Why write templates in C# instead of T4, Mustache, Razor Engine, etc?

Templating Engines are usually good for end-users to write their templates (like Email templates), due to their sandboxed model, but what's better for a developer than a full-featured language?

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.  
We can use Dapper, Newtonsoft, and other amazing libraries.  
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  


# Collaborate

**Would you like to collaborate and share your own template?**  
Please submit a pull-request or [contact me](http://drizin.io/pages/Contact/) with your idea.

Some ideas for templates:
- Generate Dapper/Petapoco classes from database schema files - check [**CodegenCS.POCO**](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO)
- Generate EF Core Entities/DBContext
- Generate Nancy endpoints for retrieving/updating business entities
- Generate ASP.NET MVC (Razor Views CSHTML and Controllers) to display and edit business entities
- Data Access Objects from database schema files
- Web service wrappers (SOAP, REST)
- Object caching
- Application-level database journaling




## License
MIT License
