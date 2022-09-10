# CodegenCS

This is a dotnet tool (command-line tool) to extract the schema of a MSSQL or PostgreSQL database (and save it in a JSON file) and to clone/build/run code-generator templates.

**Extracting schema of a MSSQL or PostgreSQL database**

```dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

```dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

**Downloading and running a simple template (POCO Generator)**

```dotnet-codegencs template clone https://github.com/CodegenCS/Templates/SimplePocos/SimplePocos.cs```

(or just ```dotnet-codegencs template clone /SimplePocos/SimplePocos```)

```dotnet-codegencs template run --TargetFolder=.\OutputFolder\ SimplePocos.dll AdventureWorks.json MyProject.POCOs```

```dotnet-codegencs template run --TargetFolder=.\Somefolder\ --File POCOs.generated.cs SimplePocos.dll AdventureWorks.json MyProject.POCOs --SingleFile```


For more templates or for more information check out the [main project](https://github.com/CodegenCS/CodegenCS) documentation.

## License
MIT License
