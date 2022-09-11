[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)  
[![Nuget](https://img.shields.io/nuget/v/CodegenCS?label=CodegenCS)](https://www.nuget.org/packages/CodegenCS)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.svg)](https://www.nuget.org/packages/CodegenCS)


**CodegenCS is a Toolkit for doing Code Generation using plain C#**

By using plain C# we can render our outputs using plain strings (and string interpolation) and using well-established and well-known C# constructs (invoking methods, passing parameters, embedding strings, looping, formatting, etc) - no need to learn a new syntax.  
On top of that our templates can leverage .NET Framework and .NET libraries (e.g. read input models from JSON file, read the schema directly from a database, read YAML specs of a REST API, etc).  

CodegenCS is written in C# and templates are developed using plain C# - but the templates can write any text-based output (you can write C#, Java, Javascript, Python, HTML, SQL Scripts, CSHTML, XML, Terraform files, or anything else).


# Components
- [CodegenCS (Core) Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) is the backbone of the toolkit. It's a class library for doing code generation using plain C#.  
  Basically it's a "TextWriter on steroids" that helps with some common code generation tasks and challenges (like indentation control, reusable blocks, multiple output files, and much more).  
  It's fully based on string interpolation and relies on the powerful [C# 11 Raw String Literals](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-11#raw-string-literals).  
  Check out [CodegenCS library documentation](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) to learn more about CodegenTextWriter (and how indenting is magically controlled), CodegenContext (and how to write multiple files), learn how to write clean and reusable templates using String Interpolation / Raw String Literals / IEnumerables.
- [dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) is a .NET tool (command-line tool) that can be used to download, build, and run templates.
  It can also be used to extract models (reverse engineer) from existing sources.  
  **Most users should only need this tool** (won't need to download or interact directly with CodegenCS library).
- [Templates repository (CodegenCS/Templates)](https://github.com/CodegenCS/Templates) contains some templates which are fully-functional samples that you can use, customize, or use as a reference for building your own.  
  Templates can be based on any JSON input model (you can use your own model), but the current templates are all based on the [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) model (see below)
- [Models](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models) folder contains out-of-the-box models ready for use on your templates:
    - [DatabaseSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) represents the schema of a relational database.  
    The dotnet-codegencs tool has commands to extract a database schema (creating a json model) from existing MSSQL or PostgreSQL databases
    - (Work in progress) OpenAPI model (Swagger API) will allow templates to be based on a REST API model.  
    - Currently there are no others out-of-the-box models, but as explained earlier you can use your own JSON input model in your templates so your data source can be anything, not necessarily a database or an API

# Quickstart

Check [dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs#quickstart) quickstart.




<!-- 

## History
- 2020-07-19: New project/scripts [Simple POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/CodegenCS/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)
 -->



<!-- # Stargazers over time

[![Star History Chart](https://api.star-history.com/svg?repos=CodegenCS/CodegenCS&type=Date)](https://star-history.com/#CodegenCS/CodegenCS&Date) -->

# License
MIT License

