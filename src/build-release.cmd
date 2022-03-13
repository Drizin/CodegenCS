rmdir /s /q ".\packages-local"
rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\codegencs"
rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\codegencs.schema"

if not exist packages-local mkdir packages-local

dotnet clean

# CodegenCS + nupkg/snupkg
dotnet build -c release CodegenCS\CodegenCS.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack CodegenCS\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true

# CodegenCS.DbSchema + nupkg/snupkg
dotnet build -c release CodegenCS.DbSchema\CodegenCS.DbSchema.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack CodegenCS.DbSchema\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true


# Can clean again since dotnet-codegencs will use Nuget references
dotnet clean
rmdir /s /q .\dotnet-codegencs\bin\
rmdir /s /q .\dotnet-codegencs\obj\
rmdir /s /q .\CodegenCS.DbSchema.Extractor\bin\
rmdir /s /q .\CodegenCS.DbSchema.Extractor\obj\


# This DLL is packed inside dotnet-codegencs tool
dotnet build -c release CodegenCS.DbSchema.Extractor\CodegenCS.DbSchema.Extractor.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Build CodegenCS.DbSchema.Extractor\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /verbosity:minimal /p:ContinuousIntegrationBuild=true

# This DLL is packed inside dotnet-codegencs tool
dotnet build -c release CodegenCS.DbSchema.Templates\CodegenCS.DbSchema.Templates.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Build CodegenCS.DbSchema.Templates\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /verbosity:minimal /p:ContinuousIntegrationBuild=true


# dotnet-codegencs (DotnetTool nupkg/snupkg)
dotnet build -c release dotnet-codegencs\dotnet-codegencs.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack dotnet-codegencs\ /p:targetFrameworks="net5.0" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true
dotnet tool uninstall -g dotnet-codegencs & dotnet tool install --global --add-source .\packages-local dotnet-codegencs


# Unit tests
dotnet build -c release CodegenCS.Tests\CodegenCS.Tests.csproj
dotnet test  CodegenCS.Tests\CodegenCS.Tests.csproj
