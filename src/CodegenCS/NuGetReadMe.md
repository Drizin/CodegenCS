# CodegenCS (Core Library)

**C# Library for Code Generation**

# Description

CodegenCS is a class library for code generation:
- Input can be a database (SQL Server or any other type), a NoSQL database, JSON, YAML, XML, or any kind of structured data that you can read using C#.  
- Output can be C# code, CSHTML, HTML, XML, Javascript, Java, Python, or any other text-based output.  

Basically it provides a custom TextWriter tweaked to solve common code generation difficulties:
- **Keeps track of current Indent level**.  
  When you write new lines it will automatically indent the line according to current level.  
- Helpers to concisely write indented blocks (C-style, Java-style or Python-style) using a **Fluent API**
  (IDisposable context will automatically close blocks)
- Helpers to write **multi-line blocks without having to worry about different indentations** for control logic and output code (you can align the multi-line blocks anywhere where it fits better inside your control code).
- Allows writing **Interpolated strings** (FormattableString) and will process any kind of arguments (can be strings or Action delegates (callbacks)), while "keeping cursor position" of inline arguments.

Besides the TextWriter, there are some helpers for common code generation tasks:
- Keeps all code in memory until you can save all files at once (no need to save anything if something fails)
- **Adding generated files to old csproj** (non-SDK style), with the option to nest the generated files under a single file
- Adding generated files to new csproj (SDK style) nested under a single file

Besides CodegenCS Core Library, there are some other related projects [here](https://github.com/Drizin/CodegenCS), including Scripts to **reverse engineer a SQL Server database into JSON schema**, and templates to **build C# POCOs or EF Entities from that JSON schema**.

This class library targets both netstandard2.0 and net472 and therefore it can be used both in .NET Framework or .NET Core.


This project contains C# code and a CSX (C# Script file) which executes the C# code. There's also a PowerShell Script which helps to launch the CSX script.  
This is cross-platform code and can be embedded into any project (even a class library, there's no need to build an exe since CSX is just invoked by a scripting runtime).  

Actually the scripts are executed using CSI (C# REPL), which is a scripting engine - the CSPROJ just helps us to test/compile, use NuGet packages, etc.  

## Installation
Just install nuget package **[CodegenCS](https://www.nuget.org/packages/CodegenCS/)**, add `using CodegenCS`, and start using (see below).


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

**How to add generated files to a .NET Framework project (csproj)**:

```cs
var ctx = new DotNetCodegenContext();

var f1 = ctx["File1.cs"];
f1.WriteLine("Line1");

ctx.SaveFiles(outputFolder);
ctx.AddToProject(csProj, outputFolder);
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

See more examples in [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests/CoreTests).