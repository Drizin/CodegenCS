[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)][ValidateSet('Release','Debug')][string]$configuration
)

# How to run:
# .\build-core.ps1
# or
# .\build-core.ps1 -configuration Debug


. .\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow


try {


	# CodegenCS.Core + nupkg/snupkg
	dotnet restore ".\Core\CodegenCS\CodegenCS.Core.csproj"
	& $msbuild ".\Core\CodegenCS\CodegenCS.Core.csproj" `
			   /t:Restore /t:Build /t:Pack `
			   /p:PackageOutputPath="..\..\packages-local\" `
			   /p:Configuration=$configuration `
			   /p:IncludeSymbols=true `
			   /verbosity:minimal `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }
			   

	# CodegenCS.Models + nupkg/snupkg
	dotnet restore ".\Core\CodegenCS.Models\CodegenCS.Models.csproj"
	& $msbuild ".\Core\CodegenCS.Models\CodegenCS.Models.csproj" `
			   /t:Restore /t:Build /t:Pack `
			   /p:PackageOutputPath="..\..\packages-local\" `
			   /p:Configuration=$configuration `
			   /p:IncludeSymbols=true `
			   /verbosity:minimal `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# CodegenCS.Runtime + nupkg/snupkg
	dotnet restore ".\Core\CodegenCS.Runtime\CodegenCS.Runtime.csproj"
	& $msbuild ".\Core\CodegenCS.Runtime\CodegenCS.Runtime.csproj"                          `
			   /t:Restore /t:Build /t:Pack                             `
			   /p:PackageOutputPath="..\..\packages-local\"               `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

	# CodegenCS.DotNet + nupkg/snupkg
	dotnet restore ".\Core\CodegenCS.DotNet\CodegenCS.DotNet.csproj"
	& $msbuild ".\Core\CodegenCS.DotNet\CodegenCS.DotNet.csproj" `
			   /t:Restore /t:Build /t:Pack `
			   /p:PackageOutputPath="..\..\packages-local\" `
			   /p:Configuration=$configuration `
			   /p:IncludeSymbols=true `
			   /verbosity:minimal `
			   /p:ContinuousIntegrationBuild=true
	if (! $?) { throw "msbuild failed" }

} finally {
    Pop-Location
}
