[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Release"
)

# CLI tool (dotnet-codegencs)
# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


. .\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir


dotnet restore CodegenCS.TemplateBuilder\CodegenCS.TemplateBuilder.csproj
& $msbuild ".\dotnet-codegencs\CodegenCS.TemplateBuilder\CodegenCS.TemplateBuilder.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

dotnet restore CodegenCS.TemplateLauncher\CodegenCS.TemplateLauncher.csproj
& $msbuild ".\dotnet-codegencs\CodegenCS.TemplateLauncher\CodegenCS.TemplateLauncher.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }



# dotnet-codegencs (DotnetTool nupkg/snupkg)
& $msbuild ".\dotnet-codegencs\dotnet-codegencs\dotnet-codegencs.csproj"   `
           /t:Restore /t:Build /t:Pack                             `
           /p:PackageOutputPath="..\..\packages-local\"      `
           '/p:targetFrameworks="net5.0"'                 `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

# Global tool (like all other nuget packages) will be in .\packages-local\

# uninstall/reinstall global tool from local dotnet-codegencs.*.nupkg:
dotnet tool uninstall -g dotnet-codegencs
dotnet tool install --global --add-source .\packages-local --no-cache dotnet-codegencs
dotnet-codegencs --version


Pop-Location
