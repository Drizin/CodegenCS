[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Release"
)

# How to run:
# .\build-core.ps1
# or
# .\build-core.ps1 -configuration Debug


. .\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

try {


	# CodegenCS.Core + nupkg/snupkg
	& $msbuild ".\Core\CodegenCS\CodegenCS.Core.csproj"                          `
			   /t:Restore /t:Build /t:Pack                             `
			   /p:PackageOutputPath="..\..\packages-local\"               `
			   '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /p:SymbolPackageFormat=snupkg                           `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }
			   

	# CodegenCS.Models + nupkg/snupkg
	& $msbuild ".\Core\CodegenCS.Models\CodegenCS.Models.csproj"                          `
			   /t:Restore /t:Build /t:Pack                             `
			   /p:PackageOutputPath="..\..\packages-local\"               `
			   '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /p:SymbolPackageFormat=snupkg                           `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# CodegenCS.Runtime + nupkg/snupkg
	& $msbuild ".\Core\CodegenCS.Runtime\CodegenCS.Runtime.csproj"                          `
			   /t:Restore /t:Build /t:Pack                             `
			   /p:PackageOutputPath="..\..\packages-local\"               `
			   '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /p:SymbolPackageFormat=snupkg                           `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true

	# CodegenCS.DotNet + nupkg/snupkg
	& $msbuild ".\Core\CodegenCS.DotNet\CodegenCS.DotNet.csproj"                          `
			   /t:Restore /t:Build /t:Pack                             `
			   /p:PackageOutputPath="..\..\packages-local\"               `
			   '/p:targetFrameworks="netstandard2.0;net472;net5.0"'    `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /p:SymbolPackageFormat=snupkg                           `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

} finally {
    Pop-Location
}
