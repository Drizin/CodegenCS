[cmdletbinding()]
param(
)

$ErrorActionPreference="Stop"

$version = "3.5.1"
$nugetPE = "C:\ProgramData\chocolatey\bin\NuGetPackageExplorer.exe"
$7z = "C:\Program Files\7-Zip\7z.exe"

# Source Generator(CodegenCS.SourceGenerator)
# How to run: .\build-sourcegenerator.ps1

. $PSScriptRoot\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\codegencs.sourcegenerator"
# CompilerServer: server failed - server rejected the request due to analyzer / generator issues 'analyzer assembly '<source>\CodegenCS\src\SourceGenerator\CodegenCS.SourceGenerator\bin\Release\netstandard2.0\CodegenCS.SourceGenerator.dll' 
# has MVID '<someguid>' but loaded assembly 'C:\Users\<user>\AppData\Local\Temp\VBCSCompiler\AnalyzerAssemblyLoader\<randompath>\CodegenCS.SourceGenerator.dll' has MVID '<otherguid>'' - SampleProjectWithSourceGenerator (netstandard2.0)
gci $env:TEMP -r -filter CodegenCS.SourceGenerator.dll -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore
gci "$($env:TEMP)\VBCSCompiler\AnalyzerAssemblyLoader" -r -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore

$configuration = "Debug"
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



dotnet restore .\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj
& $msbuild ".\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj" `
           /t:Restore /t:Build /t:Pack                                          `
           /p:PackageOutputPath="..\..\packages-local\"      `
           '/p:targetFrameworks="netstandard2.0;"'                 `
           /p:Configuration=$configuration                                      `
           /verbosity:minimal                                                   `
           /p:IncludeSymbols=true                                  `
           /p:ContinuousIntegrationBuild=true `

if (! $?) { throw "msbuild failed" }

if (Test-Path $nugetPE) { & $nugetPE ".\packages-local\CodegenCS.SourceGenerator.$version.nupkg" }
if (Test-Path $7z) {
    $zipContents = (& $7z l -ba -slt .\packages-local\CodegenCS.SourceGenerator.$version.nupkg | Out-String) -split"`r`n"
    Write-Host "------------" -ForegroundColor Yellow
    $zipContents|Select-String "Path ="
    sleep 2

    # sanity check: nupkg should have debug-build dlls, pdb files, source files, etc.
    if (-not ($zipContents|Select-String "Path = "|Select-String "CodegenCS.Core.dll")) { throw "msbuild failed" } 
    if (-not ($zipContents|Select-String "Path = "|Select-String "CodegenCS.Core.pdb")) { throw "msbuild failed" } 
}


#dotnet clean ..\Samples\SourceGenerator1\SourceGenerator1.csproj
dotnet restore ..\Samples\SourceGenerator1\SourceGenerator1.csproj
& $msbuild "..\Samples\SourceGenerator1\SourceGenerator1.csproj" `
           /t:Restore /t:Rebuild                                           `
           /p:Configuration=$configuration                                      `
           /verbosity:normal                                                   
if (! $?) { throw "msbuild failed" }

Write-Host "------------" -ForegroundColor Yellow
C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe ..\Samples\SourceGenerator1\bin\$configuration\netstandard2.0\SourceGenerator1.dll -t MyFirstClass
C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe ..\Samples\SourceGenerator1\bin\$configuration\netstandard2.0\SourceGenerator1.dll -t AnotherSampleClass # should show some methods that were generated on the fly
if (! $?) { throw "Template failed (classed were not added to the compilation)" }

Pop-Location

