# CodegenCS CLI


dotnet-codegencs is a command-line tool to build and run code-generator templates (including generating based on a Database Schema or based on a Swagger specification).

It can also be used to clone templates from our online catalog (https://github.com/CodegenCS/Templates/) and to extract the schema of a MSSQL or PostgreSQL database (in case you're generating from a Database Schema) .

**Downloading and running sample template to generate based on a database schema (POCO Generator)**

```dotnet-codegencs template clone https://github.com/CodegenCS/Templates/DatabaseSchema/SimplePocos/SimplePocos.cs```

(or just ```dotnet-codegencs template clone /DatabaseSchema/SimplePocos/SimplePocos.cs```)

```dotnet-codegencs template run SimplePocos.dll AdventureWorks.json MyProject.POCOs```

```dotnet-codegencs template run --OutputFolder=.\OutputFolder\ SimplePocos.dll AdventureWorks.json MyProject.POCOs```

```dotnet-codegencs template run --OutputFolder=.\Somefolder\ --File POCOs.g.cs SimplePocos.dll AdventureWorks.json MyProject.POCOs --p:SingleFile```

**Downloading and running sample template to generate based on a Swagger specification (OpenAPI)**

```dotnet-codegencs template clone https://github.com/CodegenCS/Templates/OpenAPI/NSwagClient/NSwagClient.cs```

```dotnet-codegencs template run NSwagClient.dll petstore-openapi3.json MyProject.RestClients```
```dotnet-codegencs template run --OutputFolder=.\Somefolder\ --File PetstoreClient.g.cs NSwagClient.dll petstore-openapi3.json MyProject.RestClients```


**Extracting schema of a MSSQL or PostgreSQL database**

```dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json```

```dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json```

```dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json```

For more templates or for more information check out the [main project](https://github.com/Drizin/CodegenCS) documentation.

## License
MIT License
