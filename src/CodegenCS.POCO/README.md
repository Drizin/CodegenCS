# CodegenCS.POCO

**CodegenCS.POCO** is a Code-generation template which reads a JSON database schema and generates [POCOS](https://stackoverflow.com/a/250006/3606250) for your tables. (See [example POCO](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/POCOs/Product.cs)).

These POCOs can be used by Dapper, PetaPoco or any other micro-ORMs or full ORMs.

The generator will optionally create code for `override bool Equals()`, `override int GetHashCode()`, and can also create **ActiveRecord** CRUD queries (Insert/Update).

To use this template you first need to use [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema), which is a Script that extracts the schema of a MS SQL Server database and saves it in a JSON file.

# Usage

## 1. Download project files into any folder

You'll only need CSX files, PS1 files, and CS files. 

You can save these files in any folder, it's not necessary to add to your solution or project folder.

## 2. Extract the JSON Schema for your database

- Edit the connection string and paths in [RefreshSqlServerSchema.csx](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/RefreshSqlServerSchema.csx)
- Execute the PowerShell script [RefreshSqlServerSchema.ps1](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/RefreshSqlServerSchema.ps1)  
  This script will automatically install required NuGet packages (CodegenCS.DbSchema, Dapper and Newtonsoft), and will read all your tables, columns, indexes, primary keys, foreign keys.  
  This script invokes SqlServerSchemaReader from CodegenCS.DbSchema.dll, but if you need to modify the code you can find [CodegenCS.DbSchema sources here](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema).
  
## 3. Generate POCOs 
 
- Edit the paths and the POCOs Namespace in [GenerateSimplePOCOs.csx](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/GenerateSimplePOCOs.csx)
- Execute the PowerShell script [GenerateSimplePOCOs.ps1](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/GenerateSimplePOCOs.ps1)  
  This script will automatically install required NuGet packages (CodegenCS and Newtonsoft), will read the JSON file and generates POCOs.
  Optionally you can specify a csproj file and all POCOs will be added to your csproj file.

The [generator script](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.POCO/SimplePOCOGenerator.cs) is very simple to understand and customize.

# Architecture

This project contains C# Scripts (CSX files, which invoke C# classes) and use PowerShell scripts (PS1 files) to install the required dependencies (NuGet packages) and invoke the CSX scripts. You don't need to embed this scripts or code into your projects, but if you do it should work both in .NET Framework or .NET Core since this project only uses netstandard2.0 libraries.

To learn more about CSX files, check [this post](https://drizin.io/code-generation-csx-scripts-part1/).

This generator uses [CodegenCS](https://github.com/Drizin/CodegenCS) library for writing text-files without going crazy about indentation.  

# Contributing

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) or [open an issue](https://github.com/Drizin/CodegenCS/issues) to discuss your idea.

Some ideas for next steps:
- Scripts to generate EFCore Entities/DbContext


# History
- 2020-07-05: Initial public version. See [blog post here](https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/)

# License
MIT License
