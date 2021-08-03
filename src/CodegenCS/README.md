This page is only about the Core Library - if you're looking for utilities (e.g. database extractors) and templates (e.g. POCOs), please check the [Main Page](https://github.com/Drizin/CodegenCS/).

# CodegenCS (Core Library)

C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.


# Description

CodegenCS is a class library for code generation:
- Input can be the JSON schema of a relational database (check [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) which currently has [extractors](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) for MSSQL and PostgreSQL), or can be any other structured source data source that you can read using C#: JSON, YAML, XML, etc, including the schema of NoSQL databases.  
- Output can be C# code, CSHTML, HTML, XML, Javascript, Java, Python, or any other text-based output.  

Basically CodegenCS provides a custom TextWriter tweaked to solve common code generation difficulties:
- Keeps track of current Indent level.  
  When you write new lines it will automatically indent the line according to current level.  
  This was inspired by [Scripty](https://github.com/daveaglick/Scripty).
- Helpers to concisely write indented blocks (C-style, Java-style or Python-style) using a Fluent API
  (IDisposable context will automatically close blocks)
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code (you can align the multi-line blocks anywhere where it fits better inside your control code).
- Allows writing **Interpolated strings** (FormattableString) and will process any kind of arguments (can be strings or Action delegates (callbacks)), while "keeping cursor position" of inline arguments.
- **IF / ELSE / ENDIF symbols** that can be embedded within the text strings and allow concise syntax for **Control Blocks**
- Immediate IF (**IIF**) symbol for concise conditionals;

Besides the TextWriter, there are some helpers for common code generation tasks:
- Keeps all code in memory until you can save all files at once (no need to save anything if something fails)
- Adding generated files to old csproj (non-SDK style), with the option to nest the generated files under a single file
- Adding generated files to new csproj (SDK style) nested under a single file

This class library targets both netstandard2.0 and net472 and therefore it can be used both in .NET Framework or .NET Core.


## Installation
Just install nuget package **[CodegenCS](https://www.nuget.org/packages/CodegenCS/)**, add `using CodegenCS`, and start using.  
See documentation below, or more examples in [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests/CoreTests).



## Documentation

**Creating a TextWriter, writing lines, saving to file**

```cs
var w = new CodegenTextWriter();
w.WriteLine("Line1");
w.SaveToFile("File1.cs");
```

**Creating a Context to keep track of multiple files, and save all files at once**

```cs
var ctx = new CodegenContext();

var f1 = ctx["File1.cs"];
var f2 = ctx["File2.cs"];

f1.WriteLine("Line1");
f2.WriteLine("Line1");

ctx.SaveFiles(outputFolder);
```

**Writing C-like block using FluentAPI and `WithCBlock()`**
```cs
var w = new CodegenTextWriter();
w
  .WriteLine("// Testing FluentAPI")
  .WithCBlock("void MyMethod()", () =>
  {
    w.WriteLine("OtherMethod();");
  });

w.SaveToFile("File1.cs"); 
```
... will output this:

```cs
// Testing FluentAPI
void MyMethod()
{
  OtherMethod();
}
```
... while `WithJavaBlock()` would output this:
```java
// Testing FluentAPI
void MyMethod() {
  OtherMethod();
}
```
**Writing Python-like block using FluentAPI and `WithPythonBlock()`**

```cs
var w = new CodegenTextWriter();
w
  .WriteLine("# Testing FluentAPI")
  .WithPythonBlock("if a == b", () =>
  {
    w.WriteLine("print b");
  });
```
... will output this:

```python
# Testing FluentAPI
if a == b :
    print b
```

**Using interpolated strings with variables**

```cs
string ns = "myNamespace";
string cl = "myClass";
string method = "MyMethod";

w.WithCurlyBraces($"namespace {ns}", () =>
{
  w.WithCurlyBraces($"public class {cl}", () => {
    w.WithCurlyBraces($"public void {method}()", () =>
    {
      w.WriteLine(@"test");
    });
  });
});
```

... will output this:

```cs
namespace myNamespace
{
  public class myClass
  {
    public vod MyMethod()
    {
      test
    }
  }
}
```

**Writing multi-line blocks without worrying about mixed indentation**

```cs
w.WithCurlyBraces($"public void MyMethod()", () =>
{
  w
    .WriteLine("// I can add one-line text")
    .WriteLine(@"
    // And I can write multi-line texts
    // which can be indented wherever it fits best (according to the outer control logic)
    // ... and in the end, it will be "realigned to the left" (left padding trimmed, docking the longest line to the margin)
    // so that the extra spaces are all ignored
    ")
    .WriteLine("// No more worrying about mixed-indentations between literals and control logic");
});
```

... will output this:

```cs
public void MyMethod()
{
    // I can add one-line text
    // And I can write multi-line texts
    // which can be indented wherever it fits best (according to the outer control logic)
    // ... and in the end, it will be "realigned to the left" (left padding trimmed, docking the longest line to the margin)
    // so that the extra spaces are all ignored
    // No more worrying about mixed-indentations between literals and control logic
}
```

**Another example of how multi-line blocks are realigned to the left (docking the longest line to the margin)**:
```cs
if (something)
{
    // As you can see below, I can add any number of whitspace before all my lines, and that will be removed
    // The final block will respect the current indentation level of the TextWriter.
    w.WriteLine(@"
        namespace codegencs
        {
            public class Test1
            {
                // etc..
            }
        }");
}

// In other code-generation engines (including T4 templates) you would have to code like this:

if (something)
{
    // Mixed indentation levels can get pretty confusing. 
    // And if the outer indentation level is changed (e.g. if this is put inside an if block) 
    // you would have to add more spaces to each line, since the TextWriter does not have any context information about the current indentation level
    w.WriteLine(@"namespace codegencs
{
    public class Test1
    {
        // etc..
    }
}");
}

```

**Writing multi-line blocks with embedded IF statement**

```cs
w.WriteLine($@"
public class MyApiClient
{{
    public MyApiClient({IF(injectHttpClient)}HttpClient httpClient{ENDIF})
    {{{IF(injectHttpClient)}
        _httpClient = httpClient;{ENDIF}
    }}
}}");
```

... will output this:

```cs
public class MyApiClient
{
    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
```

**Writing nested IF statement**

```cs
w.WriteLine($@"
{IF(generateConstructor)}public class MyApiClient
{{
    public MyApiClient({IF(injectHttpClient)}HttpClient httpClient{ENDIF})
    {{{IF(injectHttpClient)}
        _httpClient = httpClient;{ENDIF}
    }}
}}{ENDIF}");
```

**IIF (Immediate IF):**

```cs
w.Write($@"{IIF(isVisibilityPublic, $"public ")}string FirstName {{ get; set; }}");
w.Write($@"{IIF(isVisibilityPublic, $"public ", $"protected ")}string FirstName {{ get; set; }}");
```



**How to add automatically add the generated files to a .NET Framework project (csproj in the old non-SDK format)**:

```cs
var ctx = new DotNetCodegenContext();

var f1 = ctx["File1.cs"];
f1.WriteLine("Line1");

ctx.SaveFiles(outputFolder);
ctx.AddToProject(csProj, outputFolder);
```


**Reusable Action delegates can be used inside interpolated strings.**

```cs
// This is a reusable method which you can embed anywhere inside your string-interpolated templates
Func<FormattableString> RenderProperties(List<Property> props)
{
    // This will select multiple lines, and join then with line breaks separators
    // Nice feature: since RenderProperties is invoked after 8 spaces (added manually), 
    // all subsequent lines (after the first one which obviously is padded by 8 spaces)
    // will also be padded by 8 spaces (and possibly they can also have other indentation if defined in other indentation blocks (like WithCurlyBraces)
    // In other words, when you're writing multi-line blocks "inline" inside another interpolated string, 
    // all lines will have the same horizontal indenting as the first line.
    return () => $@"
        {string.Join(Environment.NewLine, props.Select(prop => $"public {prop.Type} {prop.Name} {{ get; set; }}"))}"
    ;
}

List<Property> props = new List<Property>() 
{ 
  new Property() { Name = "Name", Type = "string" }, 
  new Property() { Name = "Age", Type = "int" } 
};

public void GenerateMyClass()
{
    var writer = new CodegenTextWriter();
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

... will output this:

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

For my inner template I used a `Func<FormattableString>` but it could be other types like `FormattableString`,  `string`, `Func<string>`, `Action`, or `Action<CodegenTextWriter>`. They would all be evaluated "on demand", only by the moment that we need to output those parameters.

See more examples in [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests/CoreTests).


## Why write templates in C# instead of T4, Mustache, Razor Engine, etc?

Templating Engines are usually good for end-users to write their templates (like Email templates), due to their sandboxed model, but what's better for a developer than a full-featured language?

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.  
We can use Dapper, Newtonsoft, and other amazing libraries.  
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  

In this [blog post](https://rdrizin.com/yet-another-code-generator/) I've explained why I've created this library, why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own.


## License
MIT License
