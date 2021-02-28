# CodegenCS.DbSchema.Extractor (DbSchemaExtractor)

**CodegenCS.DbSchema.Extractor** extracts (reverse engineer) the schema of relational databases into a JSON schema. Currently supports MSSQL (Microsoft SQL Server) and PostgreSQL. 

# Usage (EXE)

```
DbSchemaExtractor.exe /postgresql /cn="Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" /output=AdventureWorks.json
```

```
DbSchemaExtractor.exe /mssql /cn="Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" /output=AdventureWorks.json
```

```
DbSchemaExtractor.exe /mssql /cn="Server=MYSERVER; Database=AdventureWorks; Integrated Security=True" /output=AdventureWorks.json
```


[CodegenCS code generator](https://github.com/Drizin/CodegenCS/) templates may use this JSON schema to generate code based on your Database Schema.  
So in other words, CodegenCS.DbSchema is a Datasource Provider to be used by CodegenCS templates that generate code based on a relational database.

Basically it contains classes to represent the Database Schema, a Schema Reader class to read the Schema from a MS SQL Server database, 
and PowerShell/CSX scripts to invoke it directly (so that you don't need to have a dedicated .NET project for each).


# Usage (Powershell)

Some developers may prefer to run this script directly from Powershell (invoking CSX scripts which compiles and run the CS code).  
This is helpful if you just want to embed the scripts inside an existing project (no need to create a new project for that).

## 1. Copy project files into any folder

Get the files from **MSSQL** or **PostgreSQL** subfolders - copy those files to any folder.  
You don't need to create a project or compile these files inside an existing project. The idea of using PowerShell scripts (instead of a csproj and an .exe command-line utility) is that you can embed this script into your development/build process wherever you like.  

## 2. Extract the JSON Schema for your database

- Edit the connection string and paths in **RefreshSqlServerSchema.csx** or **RefreshPgsqlSchema.csx**
- Execute the PowerShell script **RefreshSqlServerSchema.ps1** or **RefreshPgsqlSchema.ps1**
  This script will automatically install required NuGet packages (Dapper and Newtonsoft), and will invoke SqlServerSchemaReader to read all your tables, columns, indexes, primary keys, foreign keys.  

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

## 3. Use the extracted JSON Schema in a CodegenCS template like [**Simple POCOs** (CodegenCS.POCO)](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO).

```
CodegenCSPOCO.exe /input=AdventureWorks.json /targetFolder=OutputFolder /namespace=MyProject.POCOs
```


## How PS1/CSX works?

PowerShell scripts are used to install the required dependencies (NuGet packages), locate the CSI (C# REPL), and invoke the CSX scripts. The CSX scripts will include and invoke the CS files.

To learn more about CSX files, check [this post](https://drizin.io/code-generation-csx-scripts-part1/).

