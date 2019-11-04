# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

# Description

This is a class library (targeting netstandard2.0 and .net full framework net472) that helps on code generation.  
It's much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  
Using a full-featured language (pure C#) and a full featured IDE (Visual Studio or Visual Studio Code) you can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.

This library can be used for any kind of code generation:  
- Your input can be a SQL Server Database, any other RDBMS or NoSQL database, or even JSON, XML, or any kind of structured data that you can read using C#.
- Your output can be C# code, CSHTML, HTML, Javascript, Java, or any other text-based output.

This is basically a TextWriter tweaked to solve common code generation difficulties:
- Keeps track of current Indent level. When you write new lines it will automatically indent the line according to current level. 
  This was inspired in [Scripty](https://github.com/daveaglick/Scripty).
- Helpers to concisely writing C-style blocks.
- Helpers to writing multi-line blocks without having to worry about different indentations for control logic and output code (you can align the multi-line blocks anywhere).
- Allows writing **Interpolated strings** (FormattableString) and will process any kind of arguments (can be strings or callbacks), while "keeping cursor position" of inline arguments.
- Helper to write files to CSPROJ


## Installation
Just install nuget package **CodegenCS**.

##  Sample Template

[EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator) is a sample template using CodegenCS - 
it's just a Console application that reads a SQL Server Database and generates a DbContext and POCOS for Entity Framework 6.  
This was based on [Simon Hughes T4 templates](https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator) , but using pure C# instead of T4 makes it much easier to use/extend.

In the future I'll add more sample templates.  
**Would you like to collaborate and share your own template?**  
Please submit a pull-request or [contact me](http://drizin.io/pages/Contact/) with your idea.

Some ideas for templates:
- Generate Dapper classes
- Generate Petapoco classes
- Generate EF Core classes
- Generate Nancy endpoints for retrieving/updating business entities
- Generate ASP.NET MVC (Razor Views CSHTML and Controllers) to display and edit business entities

## Documentation

In this [blog post](http://drizin.io/yet-another-code-generator/) I describe why I've created this library 
(why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own),
and also I give some examples of the basic problems that CodegenCS solves.

Some examples below, which are similar to what you'll see in my post:

Example 1:

```cs
var w = new TemplateTextWriter();
string myNamespace = "codegencs";
string myClass = "Test1";
using (w.WithCBlock($"namespace {myNamespace}"))
{
    using (w.WithCBlock($"public class {myClass}"))
    {
         w.WriteLine("// My Properties start here");
    }
}
```
...this generates this code:
```cs
namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
```

Example 2:
```cs
if (something)
{
    if (something)
    {
        if (something)
        {
            w.WriteLine(@"
                namespace codegencs
                {
                    public class Test1
                    {
                        // My Properties start here
                    }
                }");
        }
    }
}
```
Will realign this whole block to the left, docking the outermost line to the left, while respecting "internal" indentation. So assuming that the current TextWriter was at IndentLevel 0 we get this output:
```cs
namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
```

Example 3:
```cs
// This is a reusable method which you can embed anywhere inside your string-interpolated templates
Func<FormattableString> RenderProperties(List<Property> props)
{
    return () => $@"
        {string.Join(Environment.NewLine, props.Select(prop => $"public {prop.Type} {prop.Name} {{ get; set; }}"))}"
    ;
}
public void GenerateMyClass()
{
    List<Property> props = new List<Property>() 
	{ 
		new Property() { Name = "Name", Type = "string" }, 
		new Property() { Name = "Age", Type = "int" } 
	};
    var writer = new TemplateTextWriter();
    string myNamespace = "codegencs";
    string myClass = "Test1";
    writer.Write($@"
        namespace {myNamespace}
        {{
            public class {myClass}
            {{
                // My Properties start here
                { RenderProperties(props) }
            }}
        }}");
}
```

And the output is:
```cs
namespace codegencs
{
    public class Test1
    {
        // My Properties start here
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
```
As you can see, the inner block has multiple lines, and yet all those lines were written in the same position where the first line started. In other words, the inner block was fully written in the "current cursor position". And again, if the text writer had indent level 1, all that output (outer and inner template) would have 4 more spaces before each line. Cool, uh?

For my inner template I used a `Func<FormattableString>` but it could be other types like `string`, `Func<string>`, or `FormattableString` itself. They would all be evaluated "on demand", only by the moment we need to output those parameters.


## Contributing
- This is a brand new project, and I hope with your help it can grow a lot. As I like to say, **If you’re writing repetitive code by hand, you’re stealing from your employer or from your client.**

If you you want to contribute, you can either:
- Fork it, optionally create a feature branch, commit your changes, push it, and submit a Pull Request.
- Drop me an email (http://drizin.io/pages/Contact/) and let me know how you can help. I really don't have much time and would appreciate your help.

Some ideas for next steps:
- Helpers to Write/Read json files (mostly for caching database schemas) - using Newtonsoft Json
- Support for running templates without needing a dedicated csproj (probably using ScriptCS CSX files or Powershell scripts)
- Helpers to Generate CSPROJ and SLN files


## History
- 2019-09-22: Initial public version. See [blog post here](http://drizin.io/yet-another-code-generator/)
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-11-03: Published [nuget package](https://www.nuget.org/packages/CodegenCS/)

## License
MIT License
