# CodegenCS.Models.DbSchema

**CodegenCS.Models.DbSchema** represents the schema of a relational database.

# <a name="quickstart"></a>Quickstart

## Install Command-line Tool (dotnet-codegencs)

- Pre-requisite: .NET
  If you don't have .NET installed you can install it from https://dotnet.microsoft.com/en-us/download
- Install running this command: ```dotnet tool install -g dotnet-codegencs```  
   If your environment is configured to use private Nuget feeds (in addition to nuget.org) you may need `--ignore-failed-sources` option to ignore not-found errors.


## Extract your Database Schema

To **extract the schema of a database** into a JSON file you can use the command `dotnet-codegencs model dbschema extract`, like this:  
`dotnet-codegencs model dbschema extract <MSSQL or POSTGRESQL> <connectionString> <output.json>`

Examples:
- `dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json` (MSSQL using SQL authentication)
- `dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json` (MSSQL using Windows authentication)
- `dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json` (PostgreSQL)

[**Currently**](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema.Extractor) it only supports MSSQL (Microsoft SQL Server) and PostgreSQL. Feel free to collaborate if you want to add support for a new database vendor.

If you don't have a database and want a sample schema you can download AdventureWorks schema [here](https://raw.githubusercontent.com/CodegenCS/CodegenCS/master/src/Models/CodegenCS.DbSchema.SampleDatabases/AdventureWorksSchema.json).

## Download a Template

The `template clone` command is used to download a copy of any online template to your local folder.  
Let's download a simple template called **SimplePocos** that can generate POCOs for all our database tables:

`dotnet-codegencs template clone https://github.com/CodegenCS/Templates/SimplePocos/SimplePocos.cs`

(You can browser other [sample templates here](https://github.com/CodegenCS/Templates/)).

## Run the Template

SimplePocos template requires [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) which is the namespace for the generated POCOs, so it should be invoked like `dotnet-codegencs template run SimplePocos.dll <dbSchema.json> <namespace>`. Let's use the model extracted in the previous step and let's define the namespace as "MyEntities":

`dotnet-codegencs template run SimplePocos.dll AdventureWorks.json MyEntities`

<!-- TODO: Run the template using VS Extension -->