# CodegenCS

**DbSchema Extractor**

This is a dotnet tool (command-line tool) that extracts the schema of a MSSQL or PostgreSQL database and saves it in a JSON file.  It also contains a simple template to generate POCOs based on the JSON schema.

Sample usage:

```codegencs extract-dbschema postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```codegencs extract-dbschema mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

**Simple POCO Generator**

This is a template that generates POCO classes from a JSON schema extracted with dbschema-extractor.

Sample usage:

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=OutputFolder --Namespace=MyProject.POCOs```

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=. --Namespace=MyProject.POCOs --SingleFile=POCOs.generated.cs --CrudExtensions```

```codegencs simplepocogenerator AdventureWorks.json --TargetFolder=. --Namespace=MyProject.POCOs --CrudClassMethods```

For more templates or for more information check out the [main project](https://github.com/Drizin/CodegenCS) documentation.

## License
MIT License
