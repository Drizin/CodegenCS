[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Debug",
    [switch] $RunTests = $false
)

# How to run: .\build.ps1

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path "Release.snk") { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow

$ErrorActionPreference="Stop"

$dotnetSdk = (& dotnet --list-sdks | %{ $_.Substring(0, $_.IndexOf(".")) } | Sort-Object Descending | Select-Object -First 1)

$hasNet472 = Get-ChildItem 'HKLM:\SOFTWARE\Microsoft\NET Framework Setup\NDP' -Recurse |
    Get-ItemProperty -name Version, Release -EA 0 |
    Where-Object { $_.PSChildName -match '^(?!S)\p{L}'} |
    Select-Object @{name = "NETFramework"; expression = {$_.PSChildName}}, Version, Release |
    Where-Object { $_.NETFramework -eq "Full" -and $_.Release -gt 461814 }

if ($configuration -eq "Release") {
    # dotnet-codegencs is released for multiple targets.
    # other projects are all single-target
    $dotnetcodegencsTargetFrameworks="net6.0;net7.0;net8.0"
    if (-not $dotnetSdk -or -not $hasNet472) { throw "Can't find .NET or .NET Framework'" }
} else {
    # For debug use latest dotnet SDK or (if not found) use net472
    if ($dotnetSdk) { 
        $dotnetcodegencsTargetFrameworks = "net" + $dotnetSdk 
    } elseif ($hasNet472) { 
        $dotnetcodegencsTargetFrameworks = "net472" 
    } else { throw "Can't find .NET or .NET Framework'" }
}

. .\build-clean.ps1

New-Item -ItemType Directory -Force -Path ".\packages-local"

$commandLinePkg1 =".\External\command-line-api\artifacts\packages\$configuration\Shipping\System.CommandLine.2.0.0-codegencs.nupkg"
$commandLinePkg2 =".\External\command-line-api\artifacts\packages\$configuration\Shipping\System.CommandLine.2.0.0-codegencs.snupkg"
$commandLinePkg3 =".\External\command-line-api\artifacts\packages\$configuration\Shipping\System.CommandLine.NamingConventionBinder.2.0.0-codegencs.nupkg"
$commandLinePkg4 =".\External\command-line-api\artifacts\packages\$configuration\Shipping\System.CommandLine.NamingConventionBinder.2.0.0-codegencs.snupkg"

if ((Test-Path $commandLinePkg1) -and (Test-Path $commandLinePkg2) -and (Test-Path $commandLinePkg3) -and (Test-Path $commandLinePkg4))
{
    # no need to rebuild this huge package every time, unless you modify it
    copy $commandLinePkg1 .\packages-local\
    copy $commandLinePkg2 .\packages-local\
    copy $commandLinePkg3 .\packages-local\
    copy $commandLinePkg4 .\packages-local\
} else {
    . .\build-external.ps1 -Configuration $configuration
}

. .\build-core.ps1 -Configuration $configuration

. .\build-models.ps1 -Configuration $configuration

if ($configuration -eq "Release") 
{
	# For release builds we clear bin/obj again to ensure that all further builds will use the locally published nugets
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
}

. .\build-tools.ps1 -Configuration $configuration -dotnetcodegencsTargetFrameworks $dotnetcodegencsTargetFrameworks

if ($configuration -eq "Release") {
  . .\build-sourcegenerator.ps1
  . .\build-msbuild.ps1
}

# Unit tests # TODO: break this into CORE tests, MODEL tests, CLITESTS 
if ($RunTests) {
    dotnet restore .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj
    dotnet build --configuration $configuration .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj
    dotnet test --configuration $configuration .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj

    dotnet restore .\Tools\Tests\CodegenCS.Tools.Tests.csproj
    #dotnet build --configuration $configuration --runtime net8 .\Tools\Tests\CodegenCS.Tools.Tests.csproj # TODO: $dotnetcodegencsTargetFrameworks
    & $msbuild ".\Tools\Tests\CodegenCS.Tools.Tests.csproj" /t:Restore /t:Build /p:Configuration=$configuration /p:targetFrameworks=net8 # TODO: $dotnetcodegencsTargetFrameworks
    dotnet test --configuration $configuration /p:targetFrameworks=net8 .\Tools\Tests\CodegenCS.Tools.Tests.csproj
}

if ($hasNet472) {
    $env:VSToolsPath="C:\Program Files\Microsoft Visual Studio\2022\Professional\Msbuild\Microsoft\VisualStudio\v17.0"
    . .\build-visualstudio.ps1 -Configuration $configuration
}


