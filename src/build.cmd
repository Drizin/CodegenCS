if not exist packages-local mkdir packages-local

dotnet build -c debug CodegenCS\CodegenCS.csproj
dotnet build -c debug CodegenCS.DbSchema\CodegenCS.DbSchema.csproj
dotnet build -c debug CodegenCS.DbSchema.Extractor\CodegenCS.DbSchema.Extractor.csproj
dotnet build -c debug CodegenCS.DbSchema.Templates\CodegenCS.DbSchema.Templates.csproj 

dotnet build -c debug dotnet-codegencs\dotnet-codegencs.csproj
dotnet tool uninstall -g dotnet-codegencs & dotnet tool install --global --add-source .\packages-local dotnet-codegencs

dotnet build -c debug CodegenCS.Tests\CodegenCS.Tests.csproj
dotnet test  CodegenCS.Tests\CodegenCS.Tests.csproj
