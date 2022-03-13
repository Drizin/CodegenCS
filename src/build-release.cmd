rmdir /s /q ".\packages-local"
rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\codegencs"
rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\codegencs.schema"

if not exist packages-local mkdir packages-local

dotnet clean

dotnet build -c release CodegenCS\CodegenCS.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack CodegenCS\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true

dotnet build -c release CodegenCS.DbSchema\CodegenCS.DbSchema.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack CodegenCS.DbSchema\ /p:targetFrameworks="netstandard2.0;net472" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true


dotnet build -c release CodegenCS.DbSchema.Extractor\CodegenCS.DbSchema.Extractor.csproj
dotnet build -c release CodegenCS.DbSchema.Templates\CodegenCS.DbSchema.Templates.csproj

dotnet build -c release dotnet-codegencs\dotnet-codegencs.csproj
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Pack dotnet-codegencs\ /p:targetFrameworks="net5.0" /p:Configuration=Release /p:IncludeSymbols=true /p:SymbolPackageFormat=snupkg /p:PackageOutputPath=..\packages-local\ /verbosity:minimal /p:ContinuousIntegrationBuild=true
dotnet tool uninstall -g dotnet-codegencs & dotnet tool install --global --add-source .\packages-local dotnet-codegencs

dotnet build -c release CodegenCS.Tests\CodegenCS.Tests.csproj
dotnet test  CodegenCS.Tests\CodegenCS.Tests.csproj
