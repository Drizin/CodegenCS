[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)][ValidateSet('Release','Debug')][string]$configuration
)

$ErrorActionPreference="Stop"

$version = "3.5.2"
$nugetPE = "C:\ProgramData\chocolatey\bin\NuGetPackageExplorer.exe"
$7z = "C:\Program Files\7-Zip\7z.exe"

# MSBuild Task (CodegenCS.MSBuild)
# How to run: .\build-msbuildtask.ps1

. $PSScriptRoot\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\codegencs.msbuild"
gci $env:TEMP -r -filter CodegenCS.MSBuild.dll -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore
#gci "$($env:TEMP)\VBCSCompiler\AnalyzerAssemblyLoader" -r -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore
gci .\packages\ -filter CodegenCS.MSBuild.* | Remove-Item -Force -Recurse -ErrorAction Ignore # non-sdk projects will cache packages here

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release"; } else { $configuration = "Debug"; }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow


# Unfortunately Roslyn Analyzers, Source Generators, and MS Build Tasks they all have terrible support for referencing other assemblies without having those added (and conflicting) to the client project
# To get a Nupkg with SourceLink/Deterministic PDB we have to embed the extract the PDBs from their symbol packages so we can embed them into our published package

mkdir .\ExternalSymbolsToEmbed -EA Ignore | Out-Null
$snupkgs = @(
    "interpolatedcolorconsole.1.0.3.snupkg",
    "newtonsoft.json.13.0.3.snupkg",
    "nswag.core.14.0.7.snupkg",
    "nswag.core.yaml.14.0.7.snupkg",
    "njsonschema.11.0.0.snupkg",
    "njsonschema.annotations.11.0.0.snupkg"
)

foreach ($snupkg in $snupkgs){
    Write-Host $snupkg
    if (-not (Test-Path ".\ExternalSymbolsToEmbed\$snupkg")) {
        curl "https://globalcdn.nuget.org/symbol-packages/$snupkg" -o ".\ExternalSymbolsToEmbed\$snupkg"
    }
}
copy .\packages-local\System.CommandLine.2.0.0-codegencs.snupkg .\ExternalSymbolsToEmbed\
copy .\packages-local\System.CommandLine.NamingConventionBinder.2.0.0-codegencs.snupkg .\ExternalSymbolsToEmbed\
$snupkgs = gci .\ExternalSymbolsToEmbed\*.snupkg
foreach ($snupkg in $snupkgs){
    $name = $snupkg.Name
    $name = $name.Substring(0, $name.Length-7)
    $zipContents = (& $7z l -ba -slt "ExternalSymbolsToEmbed\$name.snupkg" | Out-String) -split"`r`n"
    $zipContents | Select-String "Path = " 
    mkdir "ExternalSymbolsToEmbed\$name\" -ea Ignore | out-null
    & $7z x "ExternalSymbolsToEmbed\$name.snupkg" "-oExternalSymbolsToEmbed\$name\" *.pdb -r -aoa
}



dotnet restore .\MSBuild\CodegenCS.MSBuild\CodegenCS.MSBuild.csproj
& $msbuild ".\MSBuild\CodegenCS.MSBuild\CodegenCS.MSBuild.csproj" `
           /t:Restore /t:Build /t:Pack                                          `
           /p:PackageOutputPath="..\..\packages-local\"      `
           /p:Configuration=$configuration                                      `
           /verbosity:minimal                                                   `
           /p:IncludeSymbols=true                                  `
           /p:ContinuousIntegrationBuild=true `

if (! $?) { throw "msbuild failed" }

#if (Test-Path $nugetPE) { & $nugetPE ".\packages-local\CodegenCS.MSBuild.$version.nupkg" }
if (Test-Path $7z) {
    $zipContents = (& $7z l -ba -slt .\packages-local\CodegenCS.MSBuild.$version.nupkg | Out-String) -split"`r`n"
    Write-Host "------------" -ForegroundColor Yellow
    $zipContents|Select-String "Path ="
    sleep 2

    # sanity check: nupkg should have debug-build dlls, pdb files, source files, etc.
    if (-not ($zipContents|Select-String "Path = "|Select-String "CodegenCS.Core.dll")) { throw "msbuild failed" } 
    if (-not ($zipContents|Select-String "Path = "|Select-String "CodegenCS.Core.pdb") -and $configuration -eq "Debug") { throw "msbuild failed" } 
}

# Visual Studio Design-Time-Builds were keeping msbuild running and eventually building (and locking) our templates. I think this is gone, now that Design-Time-Builds were blocked in CodegenCS.MSBuild.targets
# handle codegencs.core.dll
#taskkill /f /im msbuild.exe


# If task fails due to missing dependencies then fusion++ might help to identify what's missing: C:\ProgramData\chocolatey\lib\fusionplusplus\tools\Fusion++.exe

# Test with SDK-Project using msbuild (.NET Framework)
del ..\Samples\MSBuild1\*.g.cs
del ..\Samples\MSBuild1\*.generated.cs
#dotnet clean ..\Samples\MSBuild1\MSBuild1.csproj
dotnet restore ..\Samples\MSBuild1\MSBuild1.csproj
& $msbuild "..\Samples\MSBuild1\MSBuild1.csproj" `
           /t:Restore /t:Rebuild                                           `
           /p:Configuration=$configuration                                      `
           /verbosity:normal
if (! $?) { throw "msbuild failed" }

Write-Host "------------" -ForegroundColor Yellow

if (-not (gci ..\Samples\MSBuild1\*.g.cs)){ throw "Template failed (classes were not added to the compilation)" }

C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe ..\Samples\MSBuild1\bin\$configuration\net8.0\MSBuild1.dll -t MyFirstClass
if (! $?) { throw "Template failed (classes were not added to the compilation)" }
#C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.exe ..\Samples\MSBuild1\bin\$configuration\net8.0\MSBuild1.dll


# Test with SDK-Project using dotnet build (.NET Core)
del ..\Samples\MSBuild1\*.g.cs
del ..\Samples\MSBuild1\*.generated.cs
#dotnet clean ..\Samples\MSBuild1\MSBuild1.csproj
dotnet restore ..\Samples\MSBuild1\MSBuild1.csproj
& dotnet build "..\Samples\MSBuild1\MSBuild1.csproj" `
           /t:Restore /t:Rebuild                                           `
           /p:Configuration=$configuration                                      `
           /verbosity:normal
if (! $?) { throw "msbuild failed" }

Write-Host "------------" -ForegroundColor Yellow

if (-not (gci ..\Samples\MSBuild1\*.g.cs)){ throw "Template failed (classes were not added to the compilation)" }

C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe ..\Samples\MSBuild1\bin\$configuration\net8.0\MSBuild1.dll -t MyFirstClass
if (! $?) { throw "Template failed (classes were not added to the compilation)" }
#C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.exe ..\Samples\MSBuild1\bin\$configuration\net8.0\MSBuild1.dll





# Test with non-SDK-Project (Microsoft Framework Web Application) using msbuild (.NET Framework)
del ..\Samples\MSBuild2\*.g.cs
del ..\Samples\MSBuild2\*.generated.cs
#dotnet clean ..\Samples\MSBuild2\WebApplication.csproj
#dotnet restore ..\Samples\MSBuild2\WebApplication.csproj
& .\nuget.exe restore -PackagesDirectory .\packages ..\Samples\MSBuild2\WebApplication.csproj 
& $msbuild "..\Samples\MSBuild2\WebApplication.csproj" `
           /t:Restore /t:Rebuild                                           `
           /p:Configuration=$configuration                                      `
           /verbosity:normal
if (! $?) { throw "msbuild failed" }

Write-Host "------------" -ForegroundColor Yellow

if (-not (gci ..\Samples\MSBuild2\*.g.cs)){ throw "Template failed (classes were not added to the compilation)" }

C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe ..\Samples\MSBuild2\bin\WebApplication.dll -t MyFirstClass
if (! $?) { throw "Template failed (classes were not added to the compilation)" }
#C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.exe ..\Samples\MSBuild2\bin\WebApplication.dll


# Test with non-SDK-Project (Microsoft Framework Web Application) using dotnet build (.NET Core) - doesnt work


Pop-Location

