[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration
)

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


# CodegenCS.Models.DbSchema + nupkg/snupkg
& $msbuild ".\Models\CodegenCS.Models.DbSchema\CodegenCS.Models.DbSchema.csproj"        `
           /t:Restore /t:Build /t:Pack                             `
           /p:PackageOutputPath="..\..\packages-local\"               `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
           /p:Configuration=$configuration                         `
           /p:IncludeSymbols=true                                  `
           /p:SymbolPackageFormat=snupkg                           `
           /verbosity:minimal                                      `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

& $msbuild ".\Models\CodegenCS.Models.DbSchema.Extractor\CodegenCS.Models.DbSchema.Extractor.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }


# CodegenCS.Models.NSwagAdapter + nupkg/snupkg
& $msbuild ".\Models\CodegenCS.Models.NSwagAdapter\CodegenCS.Models.NSwagAdapter.csproj"        `
           /t:Restore /t:Build /t:Pack                             `
           /p:PackageOutputPath="..\..\packages-local\"               `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
           /p:Configuration=$configuration                         `
           /p:IncludeSymbols=true                                  `
           /p:SymbolPackageFormat=snupkg                           `
           /verbosity:minimal                                      `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }


Pop-Location
