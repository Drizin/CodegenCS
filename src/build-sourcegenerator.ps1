[cmdletbinding()]
param(
)

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


dotnet restore .\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj
& $msbuild ".\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj" `
           /t:Restore /t:Build /t:Pack                                          `
           /p:PackageOutputPath="..\..\packages-local\"      `
           '/p:targetFrameworks="netstandard2.0;"'                 `
           /p:Configuration=$configuration                                      `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true `

#/t:GetTargetPath /t:GetDependencyTargetPaths 

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

