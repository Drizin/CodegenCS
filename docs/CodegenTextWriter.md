# CodegenTextWriter

CodegenTextWriter is the **heart of CodegenCS toolkit**.  

We like to say it's a **TextWriter on Steroids** or a **Magic TextWriter** - but if you don't believe in magic you can say it's just a custom TextWriter that leverages string interpolation to automatically control indentation, and enriches string interpolation to allow interpolation of many types (like `Action<>`, `Func<>`, `IEnumerable<>`) and special symbols (like `IF`/`ELSE`/`ENDIF`).  
This enriched string interpolation is what allows pure C# string interpolation to be used as a markup-language.

## How it works inside

Before going into the features it's helpful to understand how CodegenTextWriter works (and why it exists).

A standard `TextWriter` has methods that expect `string` type (like `Write(string)` or `WriteLine(string)`) so they will **dumbly** convert interpolated strings (or anything else) to a plain string by calling `ToString()`.  
The same happens for `StringBuilder` methods (like `Append(string)` or `AppendLine(string)`).  

But [CodegenTextWriter](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenTextWriter.cs) is much smarter and has methods that actually accept interpolated strings (like `Write(FormattableString)` or `WriteLine(FormattableString)`) - and those interpolated strings will be parsed and processed block by block, and interpolated objects are evaluated (one by one) in smart way, not using `ToString()`. All important features are described below (and in the [Main Project Documentation](https://github.com/CodegenCS/CodegenCS/)) and many of them are only possible because of this enhanced string interpolation.

# Basics

## Indentation Control

CodegenTextWriter has a clever [Indent Control](https://github.com/CodegenCS/CodegenCS/tree/master/docs/Indent-Control.md) that automatically captures the current indent (whatever number of spaces or tabs you have in current line) and will preserve it when you interpolate a large object (multiline) or when you interpolate a subtemplate.  

This means that methods don't need to ever know about parent indentation.

(as an example - you can just embed a class nested under a namespace and it will be indented like magic, you can just embed a method under a class and it will be indented like magic, etc)

Generating code with the correct indentation (even if there are multiple nested levels) has never been so easy, and it works for both curly-bracket languages (C#/Javascript/Java/Golang/etc) and also for indentation-based languages (like Python).  

Explicit indent control is also supported: we can increase indent, decrease indent, set indentation to be any number of spaces or tabs, and there are some helpers for easily writing **indented blocks** (you create a new scope, and everything you write inside that scope is automatically indented)


## Standard String Interpolation

Like any regular TextWriter we can manually write to it using plain C# using `Write()` / `WriteLine()`.
You can obviously write **multiline strings** (large blocks, no need to write line-by-line) using raw string literals (or verbatim string literals) and **you can (and should!) use string interpolation** (`FormattableString`) (to mix variables within text blocks).

Since interpolated objects are processed by an advanced parser you can also interpolate many other object types...

Since templates are compiled using C# 11 the preferred method for writing strings is using the new (and more powerful) [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals). 

<!--This is a new way of doing string interpolation (that has many advantages over the previous way):

This new syntax should be prefered over the old ways of writing interpolated strings (`$"my string {myVariable}`), multiline/verbatim strings (`@"my string"`) or interpolated verbatim strings (`$@"my string {myVariable}"`).  -->


## Interpolation of delegates

Embedding delegates is very easy and powerful:
- Supports interpolation of `Func<>` and `Action<>` delegates (they act as "subtemplates")
- Delegates can receive arguments through dependency injection of types like `ICodegenTextWriter`, `ICodegenContext`, `ICodegenOutputFile`, input model, etc.
- Delegates can also receive explicitly passed parameters to delegates (like passing a `foreach` variable)
 or by explicitly passing/binding variables.
- `Func<>` delegates may return `FormattableString`, `string`, or many other supported types.
- `Action<>` delegates may get (injected) `ICodegenTextWriter` (or `ICodegenContext`) and may write to it

## Interpolation of Lists

Embedding lists is possible by interpolating `IEnumerable<>` types:  
- Items are rendered one by one
- Items can be simple items (like `FormattableString` or `string`) or any other supported types (like delegates, or even other `IEnumerables`)
- Between the items we can have separators like commas, line-breaks or any other.
  Smart defaults will make things beautiful "out-of-the-box", but it's all configurable using the `IEnumerable<T>.Render()` extension.

It's much cleaner and more concise than doing `foreach` programmatically.


## Easy to switch between text mode and programmatic mode

By embedding delegates (`Action<>`, `Func<>`) you can seamlessly switch between from markup-mode to programmatic-mode.  
While in programmatic-mode you can just call `.WriteLine(FormattableString)` to switch to markup-mode.
**It's your choice!**


This means we can rely on large text blocks whenever possible (or whenever it makes sense), and yet we can gain more control whenever we need it (by invoking delegates), so your code can be as concise as a text-oriented template would be and as powerful as a C# program can be. So you have the **best balance between simplicity/maintenability and power**.

This hybrid approach enables the development of templates that are **clean, concise, and maintanable**.

**That makes CodegenCS the only code-generator where ["Simple things are simple, and Complex things are possible"](https://en.wikiquote.org/wiki/Alan_Kay).**


## Control Blocks (IF/ELSE/ENDIF)

Supports interpolation of special Control Symbols like **IF-ENDIF / IF-ELSE-ENDIF / IIF (Immediate IF)** that can be used to write simple conditional-blocks without having to "leave" the string literal and use external control logic.

It's a concise syntax for conditional blocks (no need to "leave" the text just to add simple logic)

Some other useful symbols include:

- `COMMENT(string)`: can be used to write comments that won't be rendered
- `RAW(string)`: can be used to write multiline strings without having them automatically indented by implicit indent control.

## Familiar C# Syntax

By using plain C# (instead of templating engines like Liquid/Handlebars) there's no need to learn new syntax for simple things like concatenating strings, converting strings to uppercase/lowercase, or formatting dates.

<!-- 
<details>
  <summary>Sample usage</summary>

  ```cs
  // When using our tools to run a template you would just get CodegenTextWriter injected (no need to create it)
  var writer = new CodegenTextWriter();

  writer.WriteLine($$"""
      public class {{_myClassName}}
      {
          public {{_myClassName}}()
          {
              {{DelegateThatRendersMyContructor}}
          }
      }
      """);
  ```
</details>
-->