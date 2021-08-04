# CodegenCS
C# Library for Code Generation

... or Yet Another Code Generator. Maybe a little better than T4 templates.

This repository contains the [**CodegenCS core library**](#CodegenCS-Core), and the dotnet command-line tool [**dotnet-codegencs**](#dotnet-codegencs) which contains some [utilities](#dotnet-codegencs-utilities) (like extracting MSSQL/PostgreSQL schemas) and some out-of-the-box [templates](#dotnet-codegencs-templates) (like POCO generator).
 

# <a name="CodegenCS-Core"></a> CodegenCS ([Core Library](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS))

CodegenCS is a class library for code generation using pure C#.  
Basically it provides a custom TextWriter tweaked to solve common issues in code generation:
- Preserves indent (keeps track of current Indent level).  
  When you write new lines it will automatically indent the line according to current level. 
- Helpers to concisely write indented blocks (C-style, Java-style or Python-style) using a Fluent API
  (IDisposable context will automatically close blocks)
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code.
- Helpers to keep track of multiple files which can be saved at once in the output folder.
- **IF / ELSE / ENDIF symbols** that can be embedded within the text strings and allow concise syntax for **Control Blocks**

**Sample usage**:

```cs
var w = new CodegenTextWriter();

Action<CodegenTextWriter> generateMyClass = w => w.Write($@"
    void MyClass()
    {{
        void Method1()
        {{
            // ...
        }}
        void Method2()
        {{
            // ...
        }}
    }}");


w.WriteLine($@"
    using System;
    using System.Collections.Generic;
    namespace MyNamespace
    {{
        {generateMyClass}
    }}");

w.SaveToFile("File1.cs"); 
```

Want to learn more? Check out the [full documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS) and the [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests/CoreTests).

# <a name="dotnet-codegencs"></a> dotnet-codegencs (.NET global tool)

**This is a [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0) global tool which is used as entry-point to launch some embedded utilities and or to run the out-of-the-box templates.**

**How to Install**: ```dotnet tool install -g dotnet-codegencs```

**Usage - see all options**: ```codegencs -?``` 

# <a name="dotnet-codegencs-utilities"></a><a name="dotnet-codegencs-extract-dbschema"> DbSchema Extractor

This is a command-line tool which extracts the schema of a MSSQL or PostgreSQL database and save it in a JSON file.  

**Sample usage**:

```codegencs extract-dbschema postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

# <a name="dotnet-codegencs-templates"></a><a name="dotnet-codegencs-simplepocogenerator"> Template: Simple POCO Generator

This is a template that generates POCO classes from a JSON schema extracted with [extract-dbschema](#dotnet-codegencs-extract-dbschema).

**Sample usage**:

```codegencs simplepocogenerator AdventureWorks.json --Namespace=MyProject.POCOs```

```codegencs simplepocogenerator AdventureWorks.json --Namespace=MyProject.POCOs --TargetFolder=OutputFolder --SingleFile=POCOs.generated.cs --CrudExtensions --CrudClassMethods```

**To see all available options use** ```codegencs simplepocogenerator -?``` or check out [Simple POCO documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/)

**It's also easy to customize the template output** - [check out how to do it](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/#customizing)


# Template: Entity Framework Core

This is a template (still in beta) that generates EntityFrameworkCore Entities and DbContext from a JSON schema extracted with [extract-dbschema](#dotnet-codegencs-extract-dbschema).

Sample usage:

```codegencs efcoregenerator AdventureWorks.json --TargetFolder=OutputFolder --Namespace=MyProject.POCOs --DbContextName=AdventureWorksDbContext```


# Contributing

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](https://rdrizin.com/pages/Contact/) to discuss your idea.


Some ideas for new features or templates:
- Port [DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to other database vendors
- Generate Dapper/Petapoco classes from database schema files - check [**Simple POCO Generator**](#dotnet-codegencs-simplepocogenerator)
- Generate EF Core Entities/DBContext
- Generate REST Web API endpoints from OpenAPI YAML
- Generate Nancy endpoints for retrieving/updating business entities
- Generate REST or SOAP web service wrappers (client)
- Generate ASP.NET MVC (Razor Views CSHTML and Controllers) to display and edit business entities
- Data Access Objects from database schema files
- Object caching
- Application-level database journaling


## History
- 2020-07-19: New project/scripts [Simple POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator/) to create POCOs (Dapper or other ORM) based on a Database Schema in JSON file
- 2020-07-12: Fluent API and other major changes
- 2020-07-05: New projects/utilities [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) and [CodegenCS.DbSchema.Extractor](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor) to reverse engineer MSSQL/PostgreSQL databases into JSON schema
- 2020-07-05: [Blog post](https://rdrizin.com/code-generation-in-c-csx-extracting-sql-server-schema/) (and [this](https://rdrizin.com/code-generation-csx-scripts-part1/)) about extracting the schema using Powershell -> CSX (Roslyn) -> CodegenCS
- 2019-10-30: Published Sample Template [EF 6 POCO Generator](https://github.com/Drizin/CodegenCS/tree/master/src/Templates/EF6-POCO-Generator)
- 2019-09-22: Initial public version. See [blog post here](http://rdrizin.com/yet-another-code-generator/)



## License
MIT License
