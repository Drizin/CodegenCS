dotnet build -c release CodegenCS\CodegenCS.csproj
dotnet build -c release CodegenCS.DbSchema\CodegenCS.DbSchema.csproj
dotnet build -c release CodegenCS.DbSchema.Extractor\CodegenCS.DbSchema.Extractor.csproj
dotnet build -c release CodegenCS.POCO\CodegenCS.POCO.csproj
dotnet build -c release CodegenCS.EntityFrameworkCore\CodegenCS.EntityFrameworkCore.csproj
dotnet build -c release CodegenCS.Tests\CodegenCS.Tests.csproj
dotnet test  CodegenCS.Tests\CodegenCS.Tests.csproj
dotnet build -c release dotnet-codegencs\dotnet-codegencs.csproj
dotnet tool uninstall -g dotnet-codegencs & dotnet tool install --global --add-source .\packages-local dotnet-codegencs --version 1.0.2
