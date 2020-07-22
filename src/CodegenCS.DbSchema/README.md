# CodegenCS.DbSchema

**CodegenCS.DbSchema** is a Script that extracts the schema of a database (currently only MS SQL Server) and saves it in a JSON file.  

[CodegenCS code generator](https://github.com/Drizin/CodegenCS/) templates may use this JSON schema to generate code based on your Database Schema.  
So in other words, CodegenCS.DbSchema is a Datasource Provider to be used by CodegenCS templates that generate code based on a relational database.

Basically it contains classes to represent the Database Schema, a Schema Reader class to read the Schema from a MS SQL Server database, 
and PowerShell/CSX scripts to invoke it directly (so that you don't need to have a dedicated .NET project for each).


# Usage

## 1. Copy project files into any folder

You can save these files in any folder, it's not necessary to add to your solution or project folder.

## 2. Extract the JSON Schema for your database

- Edit the connection string and paths in [RefreshSqlServerSchema.csx](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.DbSchema/SqlServer/RefreshSqlServerSchema.csx)
- Execute the PowerShell script [RefreshSqlServerSchema.ps1](https://github.com/Drizin/CodegenCS/blob/master/src/CodegenCS.DbSchema/SqlServer/RefreshSqlServerSchema.ps1)  
  This script will automatically install required NuGet packages (Dapper and Newtonsoft), and will invoke SqlServerSchemaReader to read all your tables, columns, indexes, primary keys, foreign keys.  

The idea of using PowerShell scripts (instead of a csproj and an .exe command-line utility) is that you can embed this script into your development/build process wherever you like.

# Architecture

This project contains C# Scripts (CSX files, which invoke C# classes) and use PowerShell scripts (PS1 files) to install the required dependencies (NuGet packages) and invoke the CSX scripts. You don't need to embed this scripts or code into your projects, but if you do it should work both in .NET Framework or .NET Core since this project only uses netstandard2.0 libraries.

The CSX script is very simple (see below) and yet it's all you need to configure:

```cs
string outputJsonSchema = "AdventureWorksSchema.json");
string connectionString = @"Data Source=MYDESKTOP\SQLEXPRESS;
				Initial Catalog=AdventureWorks;
				Integrated Security=True;";

Func<IDbConnection> connectionFactory = () => new SqlConnection(connectionString);
var reader = new SqlServerSchemaReader(connectionFactory);
reader.ExportSchemaToJSON(outputJsonSchema);
```

To learn more about CSX files, check [this post](https://drizin.io/code-generation-csx-scripts-part1/).



# Contributing
This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) or [open an issue](https://github.com/Drizin/CodegenCS/issues) to discuss your idea.

Some ideas for next steps:
- [Scripts to generate POCO classes](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO) (can be used by Dapper, PetaPoco or other micro-ORMs)
- Scripts to generate EFCore Entities/DbContext


# History
- 2020-07-05: Initial public version. See [blog post here](https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/)
- 2020-07-18: Renamed CodegenCS.SqlServer to CodegenCS.DbSchema, in order to support multiple database vendors.

# License
MIT License
