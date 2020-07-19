# CodegenCS.DbSchema

This is a Plugin (Datasource Provider) to be used with CodegenCS code generator, which allows CodegenCS to generate code based on a Database Schema. 

Basically it contains classes to represent the Database Schema, a Schema Reader class to read the Schema from a MS SQL Server database, 
and PowerShell/CSX scripts to invoke it directly (if you don't want to build it into a .NET Project).

The JSON schema can be used by any application, this is not tied to CodegenCS in any way.

Based on https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/

# Description

This project contains C# code and a CSX (C# Script file) which executes the C# code. There's also a PowerShell Script which helps to launch the CSX script.  
This is cross-platform code and can be embedded into any project (even a class library, there's no need to build an exe since CSX is just invoked by a scripting runtime).  

This code only uses netstandard2.0 libraries, so any project (.NET Framework or .NET Core) can use these scripts.  
Actually the scripts are executed using CSI (C# REPL), which is a scripting engine - the CSPROJ just helps us to test/compile, use NuGet packages, etc.  

## Usage
Just copy these files into your project, tweak connection string, and execute the PowerShell script.

If you want to run in a .NET project you can use like this:

```cs
public void ExtractSchema()
{
	string outputJsonSchema = "AdventureWorksSchema.json");
	string connectionString = @"Data Source=MYDESKTOP\SQLEXPRESS;
					Initial Catalog=AdventureWorks;
					Integrated Security=True;";

	Func<IDbConnection> connectionFactory = () => new SqlConnection(connectionString);
	var reader = new SqlServerSchemaReader(connectionFactory);
	reader.ExportSchemaToJSON(outputJsonSchema);
}
```

## Contributing
This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate or share your own template?**  

Please submit a pull-request or if you prefer you can [contact me](http://drizin.io/pages/Contact/) to discuss your idea.

Some ideas for next steps:
- [Scripts to generate POCO classes](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.POCO) (can be used by Dapper, PetaPoco or other micro-ORMs)
- Scripts to generate EFCore Entities/DbContext


## History
- 2020-07-05: Initial public version. See [blog post here](https://drizin.io/code-generation-in-c-csx-extracting-sql-server-schema/)
- 2020-07-18: Renamed CodegenCS.SqlServer to CodegenCS.DbSchema, in order to support multiple database vendors.

## License
MIT License
