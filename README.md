[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)  
[![Nuget](https://img.shields.io/nuget/v/CodegenCS?label=CodegenCS)](https://www.nuget.org/packages/CodegenCS)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.svg)](https://www.nuget.org/packages/CodegenCS)


**CodegenCS is a Toolkit for doing Code Generation using plain C#**

This is an alternative to [T4 Templates](https://docs.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates?view=vs-2022) (and to any other code-generator tool based on templating engines like Razor/Handlebars/Liquid).  

In T4 (and most other templating engines) you are basically writing to stdout while mixing control logic within the output literals (like you would do in PHP or Classic ASP).  
  
CodegenCS uses the opposite approach: you get a magical TextWriter and you have to **programmatically** write to it using plain C#.  

The advantage of the programmatic approach is that it gives you more control: You can use the well-established C# syntax (invoking methods, passing parameters, interpolating strings, looping, formatting, etc). **No need to learn a new syntax**. 

The disavantage of the programmatic approach (if you were writing to a regular TextWriter **without this library**) is that you wouldn't be able to do simple things like:
- Iterate through a simple list without having to "leave" the string literal and manually doing a foreach to append the items
- Embed a subtemplate directly within another template
- Embed simple IF conditions directly around your literals, etc.

Additionally, you would have to control indent on your own (each inner block needs to know how many indent spaces should be added to be aligned with the parent block). 

**CodegenCS gives you the best of both worlds:**
- You can write templates using Visual Studio, with full intellisense, syntax highlight, debugging support.
- Templates can leverage .NET and .NET libraries
- You start with the programmatic approach and you can use it whenever it makes sense.  
  No need to learn a new syntax for control flow, iteration, assigning variables, etc.  
  You can get/pass CodegenTextWriter, CodegenContext (multiple files), model/entities, etc.  
- You can write interpolated strings (or plain strings) when it makes sense. No need to learn new syntax for formatting variables.
- Raw String Literals allows cleaner templates by letting us write multiline blocks with controlled-indentation (left padding)
- Raw String Literals allows cleaner templates because we don't need to escape curly braces, double-quotes, blackslashes or other special characters.
- Allows string interpolation of many object types (other than strings): `Action<>`, `Func<>`, `IEnumerable<>` (lists), other templates, if blocks, etc.  
  You don't have to "leave" the string block and go back to the programmatic approach just to render a list or just to add a simple conditional block.
  But yet when you need more control you can leave the string block and go back to the programmatic approach (you just have to embed an Action in your string)
- Implicit indent control (indentation is magically controlled) - you can embed any object in interpolated strings and writer will preserve the parent indentation and/or cursor position.
<!-- - dotnet-codegencs tool lets you download/build/run templates
- dotnet-codegencs can extract database models (reverse engineer from existing MSSQL/PostgreSQL databases into a JSON file) that can be used by your templates.   -->

CodegenCS was written in C# and the templates should be developed using plain C# - but the **templates can generate any text-based output** (you can write C#, Java, Javascript, Python, HTML, SQL Scripts, CSHTML, XML, Markdown, Terraform files, or anything else).


<br/><br/>  

# Project Components

## CodegenCS Core Library

[CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) is a .NET class library for doing code generation using plain C# and it's the backbone of the toolkit.  
  
The heart of the project is [CodegenTextWriter](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenTextWriter.cs). It's just a custom TextWriter but we like to say it's a **TextWriter on steroids** (or a Magic TextWriter) since it solves lots of code generation issues and provides a lot of code generation helpers:
- Like any TextWriter, **you are in control** - you can still write to it manually using any C# constructs (foreach, if, etc).  
  Different from other templating engines, **you don't have to learn a new syntax** - you can still use the language you love (C#).  
  Since it's pure C# **you can edit your templates using the IDE you love (Visual Studio)**, and you can have **debugging** and **intellisense**. (T4 lacks decent intellisense/debugging).
  Since it's pure C# you get the full power of .NET (you can use any libraries).
- Like any TextWriter you can use interpolated strings but **CodegenTextWriter supports the interpolation of many object types** (other than strings):  
  **You can interpolate `Action<>`, `Func<>`, `IEnumerable<>` (lists), other templates, and any combination of those.**.  
  By using those different object types it's easier to **organize complex templates as clean, concise, and reusable templates**.
- CodegenTextWriter has implicit indent control, meaning that **indentation is magically controlled**.  
  Whatever object that you embed in your interpolated strings (even if it's a multiline string or if it's a callback/template that will render into multiple lines) the indentation of the outer block will still be preserved. Inner blocks don't even have to know "how many padding spaces" should be added due to the parent block, it's just all magically captured through the interpolated strings.
- CodegenTextWriter supports the powerful [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals) which allows multiline blocks to be indented anywhere (no more **mixed indentation** issue, which makes templates harder to read when literals and control logic have different paddings).
- With Raw String Literals we **don't need to escape special characters** anymore: If you're rendering C#/Java/Javascript/C (or any other curly-braces language) you can define that your interpolated strings will use double-mustaches standard, so you can just write single-braces without having to escape it. You can also define your interpolated strings to use triple-mustaches to let you use both single-mustaches and double-mustaches together (React JSX, Angular, etc.). No more crazyness about rendering double quotes or backslashes.
- If you want to embed simple control blocks directly inside your interpolated strings you can use symbols like IF/ELSE/ENDIF or IIF.

Another important class is [CodegenContext](https://github.com/CodegenCS/CodegenCS/blob/master/src/Core/CodegenCS/CodegenContext.cs) which basically will let you write to multiple files (multiple TextWriters, all in memory until you decide to save it). T4 Templates have [poor support](https://stackoverflow.com/questions/33575419/how-to-create-multiple-output-files-from-a-single-t4-template-using-tangible-edi) for managing multiple files (T4 basically has a single output stream, and you have to use some hacks/workarounds to save the output buffer into individual files).

Check out [CodegenCS library documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) to learn more about CodegenTextWriter (and how indenting is magically controlled), CodegenContext (and how to write multiple files), learn how to write clean and reusable templates using String Interpolation / Raw String Literals / IEnumerables.
  
## dotnet-codegencs

[dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) is a .NET tool (command-line tool) that can be used to download, build, and run templates.  
It can also be used to extract models (reverse engineer) from existing sources.  
In other words this is our "batteries included" tool.  
**Most users should only need this tool** (won't need to download or interact directly with CodegenCS library).  

dotnet-codegencs builds the templates (using Roslyn) **with C# 11** which means that **you can use Raw String Literals even if your target project is not yet using C# 11** (actually your target project doesn't even have to be .NET, and doesn't even have to be a project - remember, this is an agnostic toolkit - you can write to any kind of text output)

## Out-of-the-box Models

Templates can be based on any data source (data provider), but dotnet-codegencs currently expects models to be a JSON-serialized file.

There are some out-of-the-box models (ready to use in your templates) at the [Models folder](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models):
- [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) represents the **schema of a relational database**.  
  The dotnet-codegencs tool has commands to **extract** a database schema (rever engineer) creating a json model from existing **MSSQL or PostgreSQL** databases
- (Pending - work in progress) **OpenAPI model** (Swagger API) will allow templates to be based on a REST API model.  

We're still working in more models (feel free to collaborate if you want to add new database vendors or any other models), but as explained earlier you can use your own JSON input model in your templates so your data source can be anything, not necessarily a database or an API.

## Sample Templates

[Templates repository (https://github.com/CodegenCS/Templates)](https://github.com/CodegenCS/Templates) contains some templates which are fully-functional samples that you can use, customize, or use as a reference for building your own.  

You can write templates based on any JSON input model (you can use your own model), but all current templates are based on the [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) model (see above)

<br/><br/>

# Quickstart

Check [dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs#quickstart) quickstart.

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

