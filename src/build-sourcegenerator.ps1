[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration
)

# CLI tool (dotnet-codegencs)
# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


. $PSScriptRoot\build-include.ps1

$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\codegencs.sourcegenerator"
# CompilerServer: server failed - server rejected the request due to analyzer / generator issues 'analyzer assembly '<source>\CodegenCS\src\SourceGenerator\CodegenCS.SourceGenerator\bin\Release\netstandard2.0\CodegenCS.SourceGenerator.dll' 
# has MVID '<someguid>' but loaded assembly 'C:\Users\<user>\AppData\Local\Temp\VBCSCompiler\AnalyzerAssemblyLoader\<randompath>\CodegenCS.SourceGenerator.dll' has MVID '<otherguid>'' - SampleProjectWithSourceGenerator (netstandard2.0)
gci $env:TEMP -r -filter CodegenCS.SourceGenerator.dll -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore
gci "$($env:TEMP)\VBCSCompiler\AnalyzerAssemblyLoader" -r -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore

if (-not $PSBoundParameters.ContainsKey('configuration'))
{
	if (Test-Path Release.snk) { $configuration = "Release" } else { $configuration = "Debug" }
}
Write-Host "Using configuration $configuration..." -ForegroundColor Yellow


dotnet restore .\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj
& $msbuild ".\SourceGenerator\CodegenCS.SourceGenerator\CodegenCS.SourceGenerator.csproj" `
           /t:Restore /t:Build /t:Pack                                          `
           /p:PackageOutputPath="..\..\packages-local\"      `
           '/p:targetFrameworks="netstandard2.0;"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                  `
          /verbosity:minimal                                                   `
           /p:IncludeSymbols=true                         `
           /p:ContinuousIntegrationBuild=true `

#/t:GetTargetPath /t:GetDependencyTargetPaths 

if (! $?) { throw "msbuild failed" }

#C:\ProgramData\chocolatey\bin\NuGetPackageExplorer.exe D:\Repositories\CodegenCS\src\packages-local\CodegenCS.SourceGenerator.3.5.0.nupkg
Write-Host "------------" -ForegroundColor Yellow
(&"C:\Program Files\7-Zip\7z.exe" l -ba -slt .\packages-local\CodegenCS.SourceGenerator.3.5.0.nupkg)|Select-String Path
sleep 2

#dotnet clean .\SourceGenerator\SampleProjectWithSourceGenerator\SampleProjectWithSourceGenerator.csproj
dotnet restore .\SourceGenerator\SampleProjectWithSourceGenerator\SampleProjectWithSourceGenerator.csproj
& $msbuild ".\SourceGenerator\SampleProjectWithSourceGenerator\SampleProjectWithSourceGenerator.csproj" `
           /t:Restore /t:Rebuild                                           `
           /p:Configuration=$configuration                                      `
           /verbosity:normal                                                   
if (! $?) { throw "msbuild failed" }

Write-Host "------------" -ForegroundColor Yellow
C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe .\SourceGenerator\SampleProjectWithSourceGenerator\bin\$configuration\netstandard2.0\SampleProjectWithSourceGenerator.dll -t MyFirstClass
C:\ProgramData\chocolatey\lib\dnspyex\tools\dnSpy.Console.exe .\SourceGenerator\SampleProjectWithSourceGenerator\bin\$configuration\netstandard2.0\SampleProjectWithSourceGenerator.dll -t AnotherSampleClass # should show some methods that were generated on the fly
if (! $?) { throw "Template failed (classed were not added to the compilation)" }

Pop-Location

