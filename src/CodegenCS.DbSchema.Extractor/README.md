This page is only about this specific utility - if you're looking for main library, utilities (e.g. database extractors) or templates (e.g. POCOs), please check the [Main Page](https://github.com/Drizin/CodegenCS/).

# CodegenCS.DbSchema.Extractor (DbSchemaExtractor)

**CodegenCS.DbSchema.Extractor** extracts (reverse engineer) the schema of relational databases into a JSON schema.  
Currently it supports MSSQL (Microsoft SQL Server) and PostgreSQL. 

Basically it contains classes to represent the Database Schema (tables, columns, indexes, primary keys, foreign keys), Schema Reader classes (SqlServerSchemaReader or PgsqlSchemaReader) to read the Schema from a MS SQL Server or PostgreSQL database.

# Usage (easy method)

## 1. Ensure you have dotnet-codegencs tool installed

```dotnet tool install -g dotnet-codegencs```

## 2. Extract the Database Schema

This utility can be invoked using [codegencs command-line tool](https://github.com/Drizin/CodegenCS#dotnet-codegencs-extract-dbschema).

**Sample usage**:

```dotnet-codegencs extract-dbschema postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

```dotnet-codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```dotnet-codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

If you need to modify this utility (or port it to another database provider), please check [DbSchema.Extractor source code](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Extractor). 

## 3. Generate the POCOs (or any other Template)

There are many [CodegenCS templates](https://github.com/Drizin/CodegenCS#dotnet-codegencs-templates) available - they will read this JSON schema and will generate code based on your Database Schema.  

A very basic template (to generate [simple POCOs](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator)) can be invoked using [dotnet-codegencs command-line tool](https://github.com/Drizin/CodegenCS#dotnet-codegencs-simplepocogenerator).

**Sample usage**:

```dotnet-codegencs simplepocogenerator AdventureWorks.json --Namespace=MyProject.POCOs```

For more options use ```dotnet-codegencs simplepocogenerator -?``` or check out [Simple POCO documentation](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema.Templates/SimplePOCOGenerator)

# Usage (alternative method using Powershell)

Some developers may prefer to embed this script into their development/build process (without using the precompiled tool and without creating a new project just for that).  
There are some helper Powershell (PS1) files which download/install the required NuGet packages, locate the CSI (C# REPL), and invoke CSX scripts which invoke the CS files.

Basically you'll have to:

- Copy the files from **MSSQL** or **PostgreSQL** subfolders into any folder in your project (set the CS files to NOT be part of your build)
- Edit the connection string and paths in **RefreshSqlServerSchema.csx** or **RefreshPgsqlSchema.csx**
- Execute the PowerShell script **RefreshSqlServerSchema.ps1** or **RefreshPgsqlSchema.ps1**
- The script will automatically install required NuGet packages (Dapper and Newtonsoft), invoke SqlServerSchemaReader/PgsqlSchemaReader to read all your tables, columns, indexes, primary keys, foreign keys.  

You can read more about PS1 invoking CSX scripts [here](https://rdrizin.com/code-generation-csx-scripts-part1/).
