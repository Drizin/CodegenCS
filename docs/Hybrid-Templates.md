# CodegenCS Hybrid Templates

## How Markup-based Templates work
Most templating engines (including T4/Mustache/Handlebars/Liquid/Razor) are **"markup-based"** which works like this:  
- You get a **single output stream**. 
- Your template is just a **text-block** (literal) that by default gets rendered directly into this output stream.
- There are escape characters to embed variables within the text block
- and other escape characters (or other markup) to **mix control logic within the text block**  
(like adding an `if` around HTML tags).

This approach **provides simplicity** but it has a lot of problems that lead to **ugly/unmaintainable code**:
- Complex logic (when needed) is still mixed within the literals and is hard to read.
- Weak support for breaking down templates into smaller reusable pieces (subtemplates)  
  That's why templates frequently are a single file with huge text-block.
- You have to learn a new syntax (instead of using well-established C#) - not only for conditional blocks and loops, but also for things like `Substring()`, `ToUpper()`, etc.
- No support for writing to multiple files (some might have ugly workarounds)
- Weak IDE support for debugging and auto-completion (intellisense)
- Subtemplates (when available) should write their own indentation and it should be correctly aligned to the parent template
- Control logic (like `if` or `foreach`) is usually misaligned (hard to read) because aligning them correctly would affect the indentation of the text blocks (in other words there's **mixed indentation** between control logic and literals)  
Indentation is very fragile and requires a lot of trial-and-error


All those problems are gone when we use programmatic templates.

## How Programmatic Templates work

Code-generation using programmatic approach works like this:
- You have a `System.Text.StringBuilder` (or a `System.IO.TextWriter`) and Template methods should **programmatically** write to it using plain C# (`AppendLine()` or `WriteLine()`, etc.)
- You can obviously **use string interpolation** (using the syntax and formatting functions that you already know) and you can obviously write **multiline strings** (no need to write line-by-line)

Programmatic approach have some obvious advantages:
- You get **more control**, more flexibility, and complex templates are more maintainable.
- **You don't have to learn a new syntax**: you can use your favorite language (C#) for control flow (`foreach` loops, `if`, etc), interpolating strings, formatting, LINQ expressions, invoking methods and passing parameters, etc.
- You can **write templates using your favorite IDE (Visual Studio)** - with **intellisense**, **syntax highlighting**, and full **debugging** capabilities
- Templates can **leverage the power of .NET** and any **.NET library**  
  (think LINQ, Dapper, Newtonsoft, Swashbuckle, RestSharp, Humanizer, etc.)

This approach **provides control** but it has some problems that also lead to **ugly/unmaintainable code**:
- String Interpolation by default can only understand types that can be directly converted to string (`ToString()`).  
This means that you can't interpolate another function (think of it as a subtemplate) inside a text block.
- String Interpolation by default can't evaluate any logic - **even simple conditions and loops** should be done outside the text literals, which means that text blocks are sliced into very small pieces and code gets really ugly.
- `TextWriter`/`StringBuilder` do not keep any "context" about indentation so methods that write to them should write their own indentation and it should be correctly aligned to the parent template (in other words they should "know and honor" the parent indentation)

## Our Hybrid Approach

**CodegenCS combines the best of both worlds** - it has all the power of programmatic code generation but the simplicity of text-oriented templating engines, and you can switch between those two approaches at any moment:
- Templates are regular C# classes which have an entry-point method and may have a constructor.
- You can get types automatically injected either into the class entry-point or into the constructor
- If you inject `ICodegenTextWriter` that's the standard output (single file output).  
If you inject `ICodegenContext` you can write to multiple files (it manages multiple instances of `ICodegenTextWriter`).  
That's first-class support for writing to multiple files.
- You can also inject models, like a JSON that represents a MSSQL schema, or a YAML that represents a Swagger definition (OpenAPI), or any custom Model

## What's the secret?

A standard `TextWriter` has methods that expect `string` type (`Write(string)` or `WriteLine(string)`) so they will **dumbly** convert interpolated strings (or anything else) to a plain string by calling `ToString()`.  
The same happens for `StringBuilder` methods (like `Append(string)` or `AppendLine(string)`).  

But [CodegenTextWriter](https://github.com/CodegenCS/CodegenCS/tree/master/docs/CodegenTextWriter.md) is much smarter and has methods that actually accept interpolated strings (like `Write(FormattableString)` or `WriteLine(FormattableString)`) - and those interpolated strings will be parsed and processed block by block, which enables some magic:
- [**Supports the interpolation of MANY object types**](https://github.com/CodegenCS/CodegenCS/tree/master/docs/CodegenTextWriter.md) other than `string`/`FormattableString`, like delegates (`Action<>`/`Func<>`), lists (`IEnumerable<>` where elements are rendered one by one), and any combination of those.  
By embedding a delegate (`Action<>`, `Func<>`) you can seamlessly switch between from markup-mode to programmatic-mode.  
While in programmatic-mode you can just call `.WriteLine(FormattableString)` to switch to markup-mode.
- [**Implicit Indentation Control**](https://github.com/CodegenCS/CodegenCS/tree/master/docs/Indent-Control.md): before any interpolated object is rendered the writer will capture the current indentation (capture how many spaces or tabs or any other character were written before the interpolated object), and if the interpolated object renders into multiple lines then all subsequent lines will get the same indentation as the first line.
