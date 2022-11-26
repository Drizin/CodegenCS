[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration
)

# Visual Studio Extensions
# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


. .\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	#$configuration = (Test-Path Release.snk) ? "Release" : "Debug"
	$configuration = "Debug"
}
elseif ($configuration -eq "Release") {
	# error NU1106: Unable to satisfy conflicting requests
	Write-Host "ERROR: Cannot build this project in Release mode" -ForegroundColor Red
	exit
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow

try {

	# This component is hard to debug (fragile dependencies) so it's better to clean on each build
	Get-ChildItem .\VisualStudio\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
	Get-ChildItem .\VisualStudio\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
	Get-ChildItem .\VisualStudio\ -Recurse | Where{$_.FullName -Match ".*\\obj\\.*project.assets.json$"} | Remove-Item


	# CodegenCS.Runtime.VisualStudio
	& $msbuild ".\VisualStudio\CodegenCS.Runtime.VisualStudio\CodegenCS.Runtime.VisualStudio.csproj"                          `
			   /t:Restore /t:Build                                     `
			   '/p:targetFrameworks="net472"'    `
			   /p:Configuration=$configuration                         `
			   /p:IncludeSymbols=true                                  `
			   /verbosity:minimal                                      `
			   /p:ContinuousIntegrationBuild=true

	& $msbuild ".\VisualStudio\VS2022Extension\VS2022Extension.csproj"   `
			   /t:Restore /t:Build                                     `
			   '/p:targetFrameworks="net472"'                 `
			   /p:Configuration=$configuration
	if (! $?) { throw "msbuild failed" }
	
	& $msbuild ".\VisualStudio\VS2019Extension\VS2019Extension.csproj"   `
			   /t:Restore /t:Build                                     `
			   '/p:targetFrameworks="net472"'                 `
			   /p:Configuration=$configuration                        		   
	if (! $?) { throw "msbuild failed" }

	# The secret to VSIX painless-troubleshooting is inspecting the VSIX package:
	# & "C:\Program Files\7-Zip\7zFM.exe" .\VisualStudio\VS2022Extension\bin\Debug\CodegenCS.VSExtensions.VisualStudio2022.vsix



} finally {
    Pop-Location
}
