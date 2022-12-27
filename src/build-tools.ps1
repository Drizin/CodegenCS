[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration
)

# CLI tool (dotnet-codegencs)
# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


. .\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow


dotnet restore .\Tools\TemplateBuilder\CodegenCS.Tools.TemplateBuilder.csproj
& $msbuild ".\Tools\TemplateBuilder\CodegenCS.Tools.TemplateBuilder.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

dotnet restore .\Tools\TemplateLauncher\CodegenCS.Tools.TemplateLauncher.csproj
& $msbuild ".\Tools\TemplateLauncher\CodegenCS.Tools.TemplateLauncher.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

dotnet restore .\Tools\TemplateDownloader\CodegenCS.Tools.TemplateDownloader.csproj
& $msbuild ".\Tools\TemplateDownloader\CodegenCS.Tools.TemplateDownloader.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }



# dotnet-codegencs (DotnetTool nupkg/snupkg)
& $msbuild ".\Tools\dotnet-codegencs\dotnet-codegencs.csproj"   `
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
