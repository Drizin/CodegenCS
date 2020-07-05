# CodegenCS (Core Library)

C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.


# Description

CodegenCS is a class library for code generation:
- Input can be a SQL Server Database, any other RDBMS or NoSQL database, or even JSON, XML, or any kind of structured data that you can read using C#.  
- Output can be C# code, CSHTML, HTML, XML, Javascript, Java, Python, or any other text-based output.  

Basically it provides a custom TextWriter tweaked to solve common code generation difficulties:
- Keeps track of current Indent level.  
  When you write new lines it will automatically indent the line according to current level.  
  This was inspired by [Scripty](https://github.com/daveaglick/Scripty).
- Helpers to concisely write C-style blocks  
  (IDisposable context will automatically close blocks)
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code (you can align the multi-line blocks anywhere).
- Allows writing **Interpolated strings** (FormattableString) and will process any kind of arguments (can be strings or callbacks), while "keeping cursor position" of inline arguments.

Besides the TextWriter, there are some helpers for common code generation tasks:
- Keeps all code in memory until you can save all files at once (no need to save anything if something fails)
- Adding files to old csproj (.NET Framework), possibly multiple files under a single file
- Adding files to new csproj (.NET Core), possibly multiple files under a single file


This class library targets both netstandard2.0 and net472 and therefore it can be used both in .NET Framework or .NET Core.


This project contains C# code and a CSX (C# Script file) which executes the C# code. There's also a PowerShell Script which helps to launch the CSX script.  
This is cross-platform code and can be embedded into any project (even a class library, there's no need to build an exe since CSX is just invoked by a scripting runtime).  

Actually the scripts are executed using CSI (C# REPL), which is a scripting engine - the CSPROJ just helps us to test/compile, use NuGet packages, etc.  

## Installation
Just install nuget package **[CodegenCS](https://www.nuget.org/packages/CodegenCS/)**.



## Documentation

In this [blog post](http://drizin.io/yet-another-code-generator/) I describe why I've created this library 
(why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own),
and also I give some examples of the basic problems that CodegenCS solves.

Some examples below, which are similar to what you'll see in my post:

**Automatically indenting C-like blocks, which when disposed (`using` keyword) will automatically decrease indent level**:

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

**Realigning multi-line blocks to the left, docking the longest line to the margin**:
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
Even though the text-block is indented by 16 spaces (to match the control logic around it), when it's written to the output it will honor the internal indentation level.  
To the whole block is realigned to the left, docking the outermost line to the left, so assuming that the current TextWriter was at IndentLevel 0 we would get this output:
```cs
namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
```

If it weren't for the auto-docking, you would have to mix different indentations, which is how other engines (including T4 templates) work. You'd have to code like this:

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


**Writing multi-line blocks (starting in any horizontal position) will preserve that same horizontal indenting for all lines**:
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
As you can see, the inner block has multiple lines, and yet all those lines were written in the same position where the first line started.  
In other words, the inner block was fully written in the "current cursor position".  
And again, if the text writer had indent level 1, all that output (outer and inner template) would have 4 more spaces before each line.  
Cool, uh?

For my inner template I used a `Func<FormattableString>` but it could be other types like `string`, `Func<string>`, or `FormattableString` itself.  
They would all be evaluated "on demand", only by the moment we need to output those parameters.




## Contributing
This is a brand new project, and I hope with your help it can grow a lot.  
As I like to say, **If you’re writing repetitive code by hand, you’re stealing from your employer or from your client.**

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
- 2019-11-03: Published [nuget package 1.0.0](https://www.nuget.org/packages/CodegenCS/)
- 2019-11-04: Published [nuget package 1.0.1](https://www.nuget.org/packages/CodegenCS/) 
- 2020-07-05: New project/scripts [CodegenCS.SqlServer](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.SqlServer) to reverse engineer a SQL Server database into JSON schema


## License
MIT License
