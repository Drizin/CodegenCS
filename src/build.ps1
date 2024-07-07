[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Debug"
)

# How to run: .\build.ps1

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow

$dotnetSdk = (& dotnet --list-sdks | %{ $_.Substring(0, $_.IndexOf(".")) } | Sort-Object Descending | Select-Object -First 1)

$hasNet472 = Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse |
    Get-ItemProperty -name Version, Release -EA 0 |
    Where-Object { $_.PSChildName -match '^(?!S)\p{L}'} |
    Select-Object @{name = "NETFramework"; expression = {$_.PSChildName}}, Version, Release |
    Where-Object { $_.NETFramework -eq "Full" -and $_.Release -gt 461814 }

if ($dotnetSdk) { $targetFrameworks = "net" + $dotnetSdk }
elseif ($hasNet472) { $targetFrameworks = "net472" }


if (-not $targetFrameworks) { throw "Can't find .NET or .NET Framework'" }



. .\build-clean.ps1

New-Item -ItemType Directory -Force -Path ".\packages-local"

. .\build-external.ps1 -Configuration $configuration

. .\build-core.ps1 -Configuration $configuration -targetFrameworks $targetFrameworks

. .\build-models.ps1 -Configuration $configuration -targetFrameworks $targetFrameworks

if ($configuration -eq "Release") 
{
	# TODO: clear bin/obj again to enforce that all further projects will use the nugets
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
}

. .\build-tools.ps1 -Configuration $configuration -targetFrameworks $targetFrameworks

if ($configuration -eq "Debug") {
  . .\build-sourcegenerator.ps1
  . .\build-msbuild.ps1
}
# Unit tests # TODO: break this into CORE tests, MODEL tests, CLITESTS 
dotnet build -c -Configuration $configuration .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj
#dotnet test  Core\CodegenCS.Tests\CodegenCS.Tests.csproj

# VSExtension (not working with Release yet - error NU1106: Unable to satisfy conflicting requests)
if ($configuration -eq "Debug" -and $hasNet472) {
    $env:VSToolsPath="C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Microsoft\VisualStudio\v17.0"
. .\build-visualstudio.ps1 -Configuration $configuration
}


