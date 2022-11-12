[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)  
[![Nuget](https://img.shields.io/nuget/v/CodegenCS.Core?label=CodegenCS.Core)](https://www.nuget.org/packages/CodegenCS.Core)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.Core.svg)](https://www.nuget.org/packages/CodegenCS.Core)


# CodegenCS Toolkit

**CodegenCS is a Code Generation Toolkit where templates are written using plain C#**.

It's an alternative to [T4 Templates](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022), but much more powerful and yet much easier to use, with many cool features for writing reusable/concise/maintanable templates.

**Templates are written using C# but they can generate any text-based output**: you can generate C#, Java, Javascript, Python, HTML, SQL Scripts, CSHTML, XML, Markdown, Terraform files, or any text-based language.

Templates can read from any input, and there are some ready-to-use models for common tasks like generating code based on a database schema or based on a REST API definition.

**It is the only code-generator where ["Simple things are simple, and Complex things are possible"](https://en.wikiquote.org/wiki/Alan_Kay).**

# Our Hybrid Approach 

Code generators like T4 toolkit use a **Text-Block Approach** (similar to PHP/ASP3): You get a **single output stream** and you can write a text block into that stream (HTML or anything). By default whatever you write in the template goes directly to the output stream.  
Then there are some escape characters that let you **mix control logic (and variables) within a text block** (e.g. add control blocks mixed with HTML tags), but yet they were all primarily designed to be **"text-block oriented"**, while control logic is a second-class citizen.

CodegenCS uses a **Programmatic Approach**: you get one (ore more) TextWriter(s) and you have to **programmatically** write to them using plain C#.  

The programmatic approach has many **advantages** like more control/flexibility/extensibility, and **you don't have to learn a new syntax** (you can rely on the well-established C# syntax for invoking methods, passing parameters, interpolating strings, looping, formatting, LINQ expressions, etc).  

The issue with the programmatic approach is that **regular .NET TextWriters are dumb** and using them for code generation can frequently become **too complicated (or too ugly) even for simple tasks, leading to unmaintainable code**.  

CodegenCS solves this problem with a **"magic TextWriter"** that is very smart about **interpolated objects** and allows clever interpolation of many object types, enabling (or facilitating) things that would be impossible (or very difficult) with a regular TextWriter. This means that when writing a text-block to our textwriter you can easily embed delegates, lists, and even control logic - and therefore you can seamlessly switch between text-block approach and programmatic approach. In other words our textwriter enables a **hybrid approach** where you can use text-blocks (easier to read and maintain) whenever it makes sense, and you can switch to the **programmatic approach** whenever you want, and you can freely mix both approaches.

# The best of both worlds

As explained before, CodegenCS combines the best parts of code generators, templating-engines, and the best of imperative programming. And much more!

## Better IDE, Better Language, Better Cross-Platform

- You can **write templates using your favorite language (C#)** - **no need to learn a new syntax** for control flow, loops, variable assignments, LINQ expressions, etc.
- You can **write templates using your favorite IDE (Visual Studio)** - with **intellisense**, **syntax highlighting**, and full **debugging** capabilities (most templating engines lack decent tooling and debugging)
- Templates can **leverage the power of .NET** and any **.NET library**  
  (think LINQ, Dapper, Newtonsoft, Swashbuckle, RestSharp, Humanizer, etc.)
- There's a cross-platform Command-line Tool (Windows/Linux/MacOS)
- There's a Visual Studio Extension (currently only for Visual Studio 2012+ and Windows)

## Tooling: Smart Compiler, Smart Runtime, Native Dependency Injection

CodegenCS templates are written using pure C# code, but you don't need to write boilerplate code:

- Templates are built using Roslyn - but our **smart compilation** uses some smart defaults and will automatically include many references and namespaces. This means that all you need is a `Template.cs` file with the classes/methods - no need to have a `csproj` or worry about adding references and namespaces.
- Templates are executed using pure .NET - but our **smart runtime engine** will not only load and invoke your template but also will provide the associated objects.
- Runtime provides dependency injection, so templates can easily get whatever they need - you can inject objects in the template constructor (**constructor injection**) or in the `Main()` method (**method injection**).
- If there are no exceptions/errors during template execution the runtime will automatically save the output file (or files), with smart defaults for file names and folder location.
- Helpers classes like `IModelFactory` can be used to load models from JSON files (you can load your own model or use our [**out-of-the-box models**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models))
- Other types that can be injected are `CommandLineArgs` (provides command-line arguments, when the template is executed through CLI), `ExecutionContext` (provides info about the template being executed), `VSExecutionContext` (provides info about the Visual Studio Project and Solution, when the template is executed through Visual Studio Extension).

The features above are available both in the Command-line tool and in the Visual Studio Extension, and they let your templates be as simple as possible (it can be just a few lines) - and you won't have to worry about boilerplate code.

## Models

`IModelFactory` can be injected into your templates and can be used to load (deserialize) any models from JSON files.  

It can be used to load your own model or one of our [**out-of-the-box models**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models):

- `DatabaseSchema` represents the schema of a relational database. Our command-line tool (dotnet-codegencs) has a command to reverse engineer from existing MSSQL/PostgreSQL databases into a JSON model.
- [`NSwag OpenApiDocument`](https://github.com/RicoSuter/NSwag/blob/master/src/NSwag.Core/OpenApiDocument.cs) represents an OpenAPI (Swagger) model. This is a (well-established) third-party model, we just have a factory to load it from file.



## Multiple-files support, Better Output Control

- If you want to generate a single file you just have to inject `ICodegenTextWriter` into your template and start writing to it.  
  `ICodegenTextWriter` is like a "standard output" (a single output stream).
  After execution (if no errors) the output is automatically saved.  
- If you want to generate multiple files you just have to inject `ICodegenContext` into your template and start writing to it.  
  `ICodegenContext` can hold multiple in-memory instances of TextWriters (`ICodegenTextWriter`) - each one will output to an individual file.

As a comparison, T4 Templates have [poor support for managing multiple files](https://stackoverflow.com/questions/33575419/how-to-create-multiple-output-files-from-a-single-t4-template-using-tangible-edi): basically there is a single output stream and it requires a lot of hacks to break down the output buffer into individual files. This also means that multiple output streams cannot live together (you can't write to a file until you're done with the previous one).

Other templating engines don't support multiple files at all.


## Automatic Indent Control

`CodegenTextWriter` has **implicit indent control**, meaning that **indentation is magically controlled**: Whatever object that you embed in your interpolated strings (even if it's a multiline block or if it's a delegate that will render into multiple lines) the indentation of the outer block (or "cursor position") is automatically captured and preserved.  

T4, other templating engines and regular TextWriters lack any indent control: you have to **control indent on your own**  - each nested block needs to know how much padding (how many indent spaces or tabs) should be added so that the child block gets correctly aligned with the parent block.

## No Mixed Indentation

T4 and other templating engines suffer from **Mixed Indentation** problem, where control logic and text blocks have conflicting indentation, making code harder to read.

CodegenCS does not have this problem (so our templates are **cleaner and easier to read**) for two reasons:

- Our templates are compiled using C# 11 so they support the powerful [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals) that allows multiline-blocks to be aligned (padded) wherever they look better  .
  It works even if your project is using C# 10 or older (templates compilation does not depend on your project)
- `CodegenTextWriter` has implicit indent control (explained earlier) which mean that nested blocks do NOT need to know about parent block indentation

## Programmatic Approach

As explained earlier, CodegenCS uses a **programmatic approach**:

- Templates start with a regular C# method that gets a `ICodegenTextWriter` or a `ICodegenContext` (if writing to multiple files) injected into the method or into the constructor.
- You can read from any source (you can rely on helpers like `IModelFactory`)
- You can write to output using familiar TextWriter methods like `Write()`, `WriteLine()`, etc
- You can use any C# constructs (`foreach`, `if`, etc)

Programmatically writing to a TextWriter gives you **more control** while using a **familiar syntax**, but sometimes it makes sense to use an approach more oriented to text-blocks...

## Strings, Interpolated Strings, and Multiline blocks

- Like any other TextWriter, `ICodegenTextWriter` accept strings and string interpolation (to mix variables within text blocks)
- Interpolating variables in strings is very helpful, easy to read, and an important part of templating engines.
- By using plain C# (instead of templating engines like Mustache/Handlebars/Liquid) there's no need to learn new syntax for simple things like concatenating strings, converting strings to uppercase/lowercase, or formatting dates.
- Like any other TextWriter, there's no need to write line by line - `ICodegenTextWriter` can accept multiline blocks (which is also preferred since it's easier to read, less noise)
- Since templates are compiled using C# 11 the preferred method for writing strings is using the new (and more powerful) [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals). This is a new way of doing string interpolation (that has many advantages over the previous way):

```cs
// Writing multi-line
writer.WriteLine($$"""
    public class {{_myClassName}}
    {
        public {{_myClassName}}()
        {

        }
    }
    """);

// Writing single-line
writer.WriteLine($$"""public int {{_myProperty}} { get; set; }""");
```

This new syntax should be prefered over the old ways of writing interpolated strings (`$"my string {myVariable}`), multiline/verbatim strings (`@"my string"`) or interpolated verbatim strings (`$@"my string {myVariable}"`).  

The major problem with using string interpolation with a regular TextWriter is that by default the interpolated objects would be converted to strings in a dumb way...

## Smart String Interpolation: Actions, Funcs, IEnumerables

`CodegenTextWriter` (also known as our "Magic TextWriter") is very clever when evaluating string interpolation, and instead of calling `ToString()` in the interpolated objects (like a regular TextWriter would do) it will instead evaluate each object and render properly, meaning that embedded types can carry powerful behavior.

One of the most powerful types that can be interpolated are delegates (callbacks):

- `Action<>` and `Func<>` delegates are supported
- `Actions<>` delegates will be invoked
- `Funcs<>` delegates will be "evaluated" and rendered accordingly (they can return types like `string`, `FormattableString`, `IEnumerable<>`, or even other delegates)
- Delegates may expect parameters that can be automatically injected or explicitly provided.  
  Example: if a delegate expects `ICodegenContext` it will get it automatically injected, and it can write multiple files.  
  Example: if a delegate expects `ICodegenTextWriter` it will get the same text writer that the interpolated string is being written to
  Example: if a delegate expects some part of your model (like a `Table` or an API operation) you can just pass it to the delegate (like a `foreach`)
- Delegates can be used to **break down complex templates** into smaller reusable methods, making templates easier to read
- Delegates can be **mixed within text blocks**, enabling **seamless switching between programmatic mode and text mode**:  
  When we have a large block that is mostly static (or doesn't require much logic) we can just write a large text-block to `ICodegenTextWriter`.  
  When we are inside a text-block and we need to **"leave" the text-mode and gain more control (going back to the programmatic mode)** we just have to embed a delegate.  
  **It's your choice!**

As explained earlier, `CodegenTextWriter` will **magically control indentation** of interpolated objects - so when you are embedding a multi-line string (even if it's done by a delegate) you still don't have to worry about indentation - it's automatically captured and preserved.  

## Smart String Interpolation: Lists, foreach, `IEnumerable<>`, and IF/ELSE

If you are inside a text-block and want to render a simple list there's **no need to "leave" the string block and go back to the programmatic approach** (to run a `foreach` and append item by item). All you have to do is interpolate an `IEnumerable<T>` and the list will be automatically iterated (item by item) and rendered. `CodegenTextWriter` has some very smart defaults that will control how each list item is separated (e.g. linebreaks, or commas, empty lines between large blocks, etc), and it's very configurable.

The type `T` (of `IEnumerable<T>`) doesn't have to be a primitive type (like `string`) - it can be any supported type, including delegates (`Action<>`, `Func<>`). This means that we can invoke a subtemplate (delegate) for each item in the list.

If you want to add simple conditional blocks mixed within interpolated strings you can also use special symbols `IF`/`ELSE`/`ENDIF`/`IIF` - no need to switch to the programmatic approach.

## Our Hybrid Approach

To sum, CodegenCS has a magic TextWriter that leverages and augments string interpolation:

- Interpolated objects are evaluated in smart way, not using `ToString()`
- It's possible to interpolate simple types (like `string`, `FormattableString`), complex types (like `Action<>`, `Func<>`, `IEnumerable<>`), and any combination of those
- Types required by `Action<>` and `Func<>` delegates are automatically injected
- Concise syntax for rendering a list of objects (or for processing lists of objects using delegates)
- Easy to switch back and forth between text mode and programmatic mode.  
  This means we can rely on large text blocks whenever possible (or whenever it makes sense), and yet we can gain more control whenever we need it (by invoking delegates).

This hybrid approach enables the development of templates that are **clean, concise, and maintanable**.
 
## Characters Escaping is not an issue anymore

Since CodegenCS supports the use of Raw String Literals there is **no need to escape any special characters** - whether they are curly braces (mustaches), double-mustaches, double-quotes, blackslashes, at-sign, or any other.
You just have to pick the right delimiters and your literal can use any character without escaping.

As a comparison, T4 and all other templating engines they have predefined characters for control blocks (and/or for interpolating variables), and that means that depending on the output you're generating you would have to use special escaping characters:

- If you were using Mustache/Handlebars/Liquid and generating Angular or React JSX you would have to escape the double-mustaches `{{` and `}}`
- If you were using Razor you would have to escape all `@`
- If you were using T4 you would have to escape `@` / `<#` / `#>` (ASPX)
- If you were using standard C# interpolated strings (not the new raw string literals) and generating curly-braces languages (Javascript, Java, C#, etc) you would have to escape all curly braces (`{` and `}`) - that would be much harder to copy/paste/compare between templates and code output.


<br/><br/>  

# Project Components

## Core Library (CodgenCS.Core)

[CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) is the backbone of the toolkit:

- [CodegenTextWriter](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenTextWriter.cs) is the **heart of this library** - some people would say that it's just a custom TextWriter, but we prefer to say it's a **Magic TextWriter** (or a **TextWriter on Steroids**) since it solves lots of code generation issues and provides a lot of helpers. As explained earlier, **CodegenTextWriter supports the interpolation of MANY object types** other than strings, like `Action<>`, `Func<>`, `IEnumerable<>` (lists), and any combination of those, and also manages indentation like magic.  
- [CodegenContext](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenContext.cs) holds multiple `ICodegenTextWriters`, and each one will output to an individual file.

Check out [CodegenCS Core library documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) to learn more about CodegenTextWriter, about how indenting is magically controlled, learn how to write clean and reusable templates using String Interpolation, Raw String Literals and IEnumerables.

Other relevant libraries:

- [CodegenCS.Models](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS.Models): interfaces for models, factories for loading models
- [CodegenCS.Runtime](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS.Runtime): utilities for runtime, like logging, command-line arguments, and template execution context
- [CodegenCS.DotNet](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS.DotNet): special classes for generating .NET code and for manipulating CSPROJ/VBPROJ files.
- [CodegenCS.TemplateBuilder](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs/CodegenCS.TemplateBuilder): builds templates using Roslyn and C# 11
- [CodegenCS.TemplateLauncher](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs/CodegenCS.TemplateLauncher): launches templates (finds entrypoint, injects dependencies, etc) and saves output.
 
Most users won't ever need to download or interact directly with CodegenCS library (or any of the libraries above) - most likely all you need is the **Visual Studio Extension or Command-line Tool**.

  
<hr/>

## Command-line Tool (dotnet-codegencs)

[dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) is a cross-platform .NET tool (command-line tool) that can be used to download, build, and run templates.  
It can also be used to extract models (reverse engineer) from existing sources.  
In other words this is our **"batteries included"** tool.  

<hr/>

## Visual Studio Extension

Our [Visual Studio Extension](https://github.com/CodegenCS/CodegenCS/tree/master/src/VSExtensions/) allows running templates directly from Visual Studio.  
The output files are automatically added to the project (nested under the template item).

<hr/>

## Out-of-the-box Models

Templates can read from any data source (a file, or a database, or anything else) but **Models** are our built-in mechanism for easily providing inputs to templates:

- IModelFactory is a helper class that can be used to load (deserialize) models from JSON files. No need to write boilerplate code (reading from file, deserializing, check if file exists, etc)
- You can write your own model types

- You can rely on our [**out-of-the-box models**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models) for common tasks like generating code based on a [Database Schema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema/DbSchema) or based on a [REST API specification](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSwagAdapter) - so you don't have to reinvent the wheel

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

## Why yet another Code Generator? Why not Mustache, Razor, T4, etc?

In this [blog post](https://rdrizin.com/yet-another-code-generator/) I explain how I've started this project: basically I was searching for a code generator and I had some simple requirements: I wanted debugging support, subtemplates support, and subtemplates should respect the indentation of the parent block (I wanted to avoid mixed-indentation which causes unmaintainable code). I tried many tools and libraries but couldn't find anything meeting those requirements, that's why I've created this project.

Besides that, I've always had some bad experiences with T4 templates (including code difficult to read and maintain, and backward-compatibility issues every time I upgrade my Visual Studio). Last, I believe that templating engines are great for some things (like creating Email templates) due to their sandboxed model, but I think there's nothing better for developers than a full-featured language (and IDE support). 


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

