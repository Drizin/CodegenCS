# How does CodegenCS compare to T4 Templates?

I suggest that you check the [documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) of the core library, but I can summarize a few differences:


## Indentation Control

T4 **does not have indentation control** (it's a major headache to adjust indents, spacing, and linebreaks).  

In CodegenCS indenting works like magic - you can embed anything a text block (even multiline blocks or complex callback actions) and indenting is just preserved/consistent.  
CodegenCS TextWriter accepts virtually anything embedded in the interpolated string but yet it **preserves the indentation of the outer blocks**.  
This means that subtemplates don't need to worry (or even know) about parent block indentation level.


## Managing Multiple Files

T4 has **poor support for managing multiple files** (it's a single output stream, requires some hacks/workarounds to save the output buffer in chunks into individual files).  

In CodegenCS it's first-class citizen.

## Coding Paradigm

T4 is **like Classic ASP** in the sense that you're just **writing to stdout while mixing control logic within the output**.  

CodegenCS takes the opposite approach: we start with the control code (since it's just a C# method) and **you have to programmatically write to the outputs.** This provides more control.  
The main magic is that CodegenCS TextWriter will take **interpolated strings** and the **interpolated objects can be virtually anything**: it can be another template, a callback action, an IEnumerable, inline IF/ELSE/ENDIF, IIF, etc.

Even though the library is primarily focused on "manually writing interpolated strings" it's still possible to have **logic and output** (you don't have "**mixed indentation**" like you would have in T4).  
It also makes it much easier to output code **without worrying about escape characters** (think double mustaches or rendering ASP.NET <% %>).  

The library is primarily focused in "manually writing interpolated strings" but yet it's still possible to have **some control logic embedded** in the strings:You don't need to do "foreach" just to render a simple list one per line.You don't need to write multiple strings to do a simple IF/ENDIF.etc.

## Raw String Literals

CodegenCS templates can use **Raw String Literals**, which makes it **much** easier to write multiline blocks, indentation (combined with our implicit indent control), and makes it much easier to copy/paste between templates and output. It's also easier to identify what is control logic and what is output.

It's possible to use C#11 Raw String Literals even if the target project is not using C# 11 (output doesn't even have to be .NET)


## Template Arguments and Options

**CodegenCS templates can define their own arguments and options** (using [.NET System.CommandLine](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options)) and the CLI tools will show/accept/require those arguments.

## Intellisense

T4 has **poor intellisense** and tooling.

CodegenCS templates are plain C# so they can benefit from good C# IDEs intellisense.

## Out-of-the-box Models

CodegenCS has an out-of-the-box model that represents the schema of a relational database (with extractors for reverse engineering MSSQL and PostgreSQL), so it's "batteries included" toolkit (see [quickstart](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs#quickstart))


## Cross Platform

CodegenCS is cross-platform. ~~T4 is not.~~ (is T4 currently cross-platform?)
