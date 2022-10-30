[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)  
[![Nuget](https://img.shields.io/nuget/v/CodegenCS.Core?label=CodegenCS.Core)](https://www.nuget.org/packages/CodegenCS.Core)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.Core.svg)](https://www.nuget.org/packages/CodegenCS.Core)


# CodegenCS Toolkit

**CodegenCS is a Code Generation Toolkit where templates are written using plain C#** (no need to learn a new syntax).

It's an alternative to [T4 Templates](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022), but much more powerful and yet much easier to use, with many cool features for writing reusable/concise/maintanable templates.

**Templates are written using C# but they can generate any text-based output**: you can generate C#, Java, Javascript, Python, HTML, SQL Scripts, CSHTML, XML, Markdown, Terraform files, or any text-based language.

Templates can read from any input, and there are some ready-to-use models for common tasks like generating code based on a database schema or based on a REST API definition.


# Text-Blocks Approach vs Imperative Approach

Code generators like T4 toolkit work like PHP pages (or Razor): You get a **single output stream** and you can write a text block into that stream (HTML or anything), and by default whatever you write in the template goes directly to the output stream.  
Then they all have some escape characters that let you **add control logic (and variables) mixed within a text block** (e.g. add control blocks mixed with HTML tags), but yet they were all primarily designed to be **"text-block oriented"**, while control logic is a second-class citizen.

CodegenCS uses the opposite approach - it's primarily designed to be used with imperative programming: **you get a TextWriter** (or multiple writers if writing to multiple files) and you have to **programmatically** write to it using plain C#.  
The programmatic approach gives a lot of **advantages** like more control/flexibility/extensibility, and **no need to learn a new syntax** - you can rely on the well-established C# syntax (invoking methods, passing parameters, interpolating strings, looping, formatting, LINQ expressions, etc).  
The **disavantage** of the programmatic approach (if you were writing to a regular TextWriter **without this library**) is that some tasks would be impossible or too complicated:
- In a regular TextWriter you can't **embed a subtemplate inside another template using string interpolation**
- In a regular TextWriter you can't **render a list of items** without "leaving" the block and manually running a "foreach". For subtemplates you'd have to run one by one, and even for a simple list you'd have to append one by one.
- In a regular TextWriter you have to **control indent on your own** (each inner block needs to know how many indent spaces should be added to be aligned with the parent block). Even popular templating engines lack indent control.
- In a regular TextWriter you can't embed simple `IF` conditions mixed within your text blocks

CodegenCS has a magic TextWriter that leverages string interpolation to solve the aforementioned issues and allows us to seamlessly switch between imperative and text-block approach.  

In other words, **CodegenCS toolkit gives the best of both worlds** (and much more!).


# CodegenCS Features and Advantages

- You can **write templates using your favorite language (C#)** - **no need to learn a new syntax** for control flow, loops, variable assignments, etc.
- You can **write templates using your favorite IDE (Visual Studio)** - with **intellisense**, **syntax highlighting**, and full **debugging** capabilities (most code generators lack decent intellisense/debugging)
- Templates can **leverage the power of .NET** and any **.NET library**  
  (think LINQ, Dapper, Newtonsoft, Swashbuckle, RestSharp, Humanizer, etc.)
- Templates start with the **programmatic approach** and you can **use it whenever it makes sense**:  
  It all starts with a regular C# method that gets a CodegenTextWriter (or a CodegenContext if writing to multiple files).  
  Then you can read from any source and you can write to output using familiar methods like `Write()`, `WriteLine()`, etc.  
- You can **break down complex templates** into smaller reusable methods (it's all C# anyway).  
- You can write **interpolated strings** (or plain strings) whenever it makes sense. No need to learn new syntax for simple things like concatenating strings or formatting dates.  
  Obviously you can also write multiline blocks (no need to write line by line) and our textwriter will magically handle indentation.
- It supports the powerful [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals) that allows multiline-blocks to be aligned (padded) wherever they look better, making templates **cleaner and easier to read** since you won't see **mixed indentation** between strings and control logic.  
  Works even in legacy projects using C# 10 or older (templates are always built using C# 11, no matter your project version)
- With Raw String Literals there is also **no need to escape special characters** like curly braces (mustaches), double-quotes, blackslashes, double-mustaches or any other.  
  It's just about using the right delimiters and then your literal can use any character without conflicting with the string delimiter: it's easier to write curly-braces (C#/C/Java/Javascript), double-mustaches (JSX/Angular), Razor/Blazor/ASPX, etc.
- Implicit indent control (**indentation is magically controlled**) - you can embed any object in interpolated strings and writer will **preserve the parent indentation** and/or cursor position.
- Supports **string interpolation of many object types** (other than strings): `Action<>`, `Func<>`, `IEnumerable<>` (lists), other templates, if blocks, etc.  
  **No need to "leave" the string block and go back to the programmatic approach** just to render a list or just to add a simple conditional block.  
  But yet, whenever you need more control (**it's your choice!**) you can "leave" the string block and go back to the programmatic approach (it's as simple as interpolating an `Action` within your string)
- In other words with our Magical TextWriter **you can seamlessly switch (back and forth) between programmatic mode** (imperative) and **text mode** (string blocks).
- Native dependency injection support: you can inject (in the template constructor or in an entrypoint method) classes like `ICodegenTextWriter` (to write to standard output), `ICodegenContext` (to write to multiple files), `CommandLineArgs` (to get command-line arguments), `ExecutionContext` (to get info about the template being executed), `VSExecutionContext` (get info about the hosting Visual Studio Project and Solution), and others.
- You can inject our [**out-of-the-box models**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models) like `DatabaseSchema`, `OpenApiDocument`, or even your own model based on a JSON file.
- Tools to reverse engineer a `DatabaseSchema` from existing MSSQL/PostgreSQL databases into a JSON model.

To sum, **CodegenCS is the only code-generator where ["Simple things are simple, and Complex things are possible"](https://en.wikiquote.org/wiki/Alan_Kay).**


<br/><br/>  

# Project Components

There are basically 4 components (detailed below):
- **Core Library**: class library that contains the "Magic TextWriter" and related stuff
- **Command-line Tool** (dotnet-codegencs): command-line tool that can be used to download, build, and run templates.
- **Visual Studio Extension**: can be used to run templates directly from Visual Studio (output files are automatically added to your project)
- **Models**: you can use your own input models, but we provide some out-of-the-box models for common tasks
- **Templates**: ready-to-use templates

<hr/>

## Core Library

[CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) is a .NET class library for doing code generation using plain C# and it's the backbone of the toolkit.  
  
[CodegenTextWriter](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenTextWriter.cs) is the **heart of the library** - some people would say that it's just a custom TextWriter, but we prefer to say it's a **Magic TextWriter** (or a **TextWriter on Steroids**) since it solves lots of code generation issues and provides a lot of helpers:
- Like with any TextWriter, **you are in control** - you can manually write to it using `Write()`, `WriteLine()`, etc, and using any C# constructs (`foreach`, `if`, etc).  
- Like with any TextWriter, you can use interpolated strings (to mix variables within strings).  
  But **CodegenTextWriter supports the interpolation of MANY other object types** other than strings: **you can interpolate `Action<>`, `Func<>`, `IEnumerable<>` (lists), other templates**, and any combination of those.  
  By using those different object types it's easier to organize complex templates as **clean, concise, and reusable code**.
- It has **implicit indent control**, meaning that **indentation is magically controlled and preserved**.  
  Whatever object that you embed in your interpolated strings (even if it's a multiline string or if it's a callback/template that will render into multiple lines) the indentation of the outer block will still be preserved.  
  In most other engines the inner blocks (nested blocks) need to know "how much padding" should be added according to the parent block.  
  In CodegenCS it's just magically captured from the interpolated strings.
- Supports interpolation of special symbols like `IF/ELSE/ENDIF` or `IIF` can be used to write simple control blocks within your interpolated strings. No need to "leave" the literal and go back to C# programming for simple stuff.

[CodegenContext](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenContext.cs) is the second most important class - basically it manages multiple in-memory TextWriters so we can save them into multiple files. T4 Templates have [poor support](https://stackoverflow.com/questions/33575419/how-to-create-multiple-output-files-from-a-single-t4-template-using-tangible-edi) for managing multiple files (basically T4 has a single output stream and requires some hacks/workarounds to save the output buffer into individual files).


Check out [CodegenCS Core library documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) to learn more about CodegenTextWriter (and how indenting is magically controlled), CodegenContext (and how to write multiple files), learn how to write clean and reusable templates using String Interpolation / Raw String Literals / IEnumerables.

Most users won't ever need to download or interact directly with CodegenCS library - most likely all they need is the **Visual Studio Extension or Command-line Tool**.

  
<hr/>

## Command-line Tool (dotnet-codegencs)

[dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) is a .NET tool (command-line tool) that can be used to download, build, and run templates.  
It can also be used to extract models (reverse engineer) from existing sources.  
In other words this is our **"batteries included"** tool.  

<hr/>

## Visual Studio Extension

Our [Visual Studio Extension](https://github.com/CodegenCS/CodegenCS/tree/master/src/VSExtensions/) allows running templates directly from Visual Studio.


<hr/>

## Models

Templates can read from any data source (a file or a database or anything else) but **Models** are our built-in mechanism for easily providing inputs to templates:

- Templates don't need boilerplate code to read from a file, deserialize it, check if file exists, etc
- You can rely on our [**out-of-the-box models**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models) for common tasks like generating code based on a [Database Schema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema/DbSchema) or based on a [REST API specification](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSwagAdapter) - so you don't have to reinvent the wheel
- You can write your own model types

Click [**here**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models) to learn more about our **Out-of-the-box Models** or learn **How to Write a Custom Model**



<hr/>

## Sample Templates

Our [Templates repository (https://github.com/CodegenCS/Templates)](https://github.com/CodegenCS/Templates) contains some **sample fully-functional templates** that you can use, customize, or use as a reference for building your own templates.  

<hr/>

<br/><br/>

# Quickstart

- To start using the command-line tool, check out [dotnet-codegencs Quickstart](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs#quickstart)
- To start using the Visual Studio Extension, check out [Visual Studio Extension Quickstart](https://github.com/CodegenCS/CodegenCS/tree/master/src/VSExtensions#quickstart)
- To generate code based on a database, check out [DbSchema Quickstart](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema#quickstart)
- To generate code based on a REST API, check out [NSwagAdapter Quickstart](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSwagAdapter#quickstart)

# FAQ

## How does CodegenCS compare to T4?

Check [CodegenCS vs T4 Templates](https://github.com/CodegenCS/CodegenCS/tree/master/docs/Comparison-T4.md)

## How does CodegenCS compare to Roslyn?

Check [CodegenCS vs Roslyn](https://github.com/CodegenCS/CodegenCS/tree/master/docs/Comparison-Roslyn.md)



<!-- 

## History
- 2020-07-19: New project/scripts [Simple POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)
 -->



# Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=CodegenCS/CodegenCS&type=Date)](https://star-history.com/#CodegenCS/CodegenCS&Date)

# License
MIT License

