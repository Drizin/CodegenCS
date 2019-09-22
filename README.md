# CodegenCS
C# Library for Code Generation

... or

Yet Another Code Generator. Maybe a little better than T4 templates.

# Description

This is basically a TextWriter tweaked to be used for code generation:
- Keeps track of current Indent level. When you write new lines it will automatically indent the line according to current level. 
  This was inspired in [Scripty](https://github.com/daveaglick/Scripty).
- Helpers to concisely writing C-style blocks.
- Helpers to writing multi-line blocks without having to worry about different indentations for control logic and output code (you can align the multi-line blocks anywhere).
- Allows writing **Interpolated strings** (FormattableString) and will process any kind of arguments (can be strings or callbacks), while "keeping cursor position" of inline arguments.
- This allows to write complex scripts with reusable code, using pure C# (using the best IDE, strong-typing, intellisense, debugging, etc).
- Much easier than T4.


## Installation / Usage
Currently the project is only a netstandard class library. In the future we'll add more templates and helpers to automate it.

Example 1:

```cs
var w = new TemplateTextWriter();
string myNamespace = "codegencs";
string myClass = "Test1";
using (w.WithCStyleBlock($"namespace {myNamespace}"))
{
    using (w.WithCStyleBlock($"public class {myClass}"))
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
- This is a brand new project, and I hope it can grow a lot.

If you you want to contribute, you can either:
- Fork it, optionally create a feature branch, commit your changes, push it, and submit a Pull Request.
- Drop me an email (http://drizin.io/pages/Contact/) and let me know how you can help. I really don't have much time and would appreciate your help.

Some ideas for next steps:
- Helper to write files to CSPROJ
- Integrate Newtonsoft Json / Create helper to Write/Read json files (mostly for caching database schemas)
- Support for running templates through ScriptCS CSX files
- Template to generate Dapper classes
- Template to generate Petapoco classes
- Template to generate EF 6 POCOs/Context (probably based on the great [Simon Hughes T4 templates](https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator) )
- Template to generate EF Core classes
- Template to generate CSPROJ, SLN
- Template to generate Nancy endpoints
- Templates to write CSHTML, ASPX

## History
- 2019-09-22: Initial public version. See [blog post here](http://drizin.io/yet-another-code-generator/)

## License
MIT License
