[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Release"
)

# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


$msbuild = ( 
	"$Env:programfiles\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\msbuild.exe",
	"$Env:programfiles\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\msbuild.exe",
	"$Env:programfiles\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\msbuild.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\msbuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\14.0\Bin\MSBuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1

Remove-Item -Recurse -Force -ErrorAction Ignore ".\packages-local"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\codegencs"
Remove-Item -Recurse -Force -ErrorAction Ignore "$env:HOMEDRIVE$env:HOMEPATH\.nuget\packages\codegencs.*"

New-Item -ItemType Directory -Force -Path ".\packages-local"

# when target frameworks are added/modified dotnet clean might fail and we may need to cleanup the old dependency tree
Get-ChildItem $Path -Recurse | Where{$_.FullName -Match ".*\\obj\\.*project.assets.json$"} | Remove-Item
Get-ChildItem $Path -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
Get-ChildItem $Path -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
Get-ChildItem $Path -Recurse | Where{$_.FullName -Match ".*\.csproj$" -and $_.FullName -NotMatch ".*\\VSExtensions\\" } | ForEach { dotnet clean $_.FullName }
#dotnet clean .\CodegenCS.sln

git submodule init
git pull --recurse-submodules
git submodule update --remote --recursive

cd External\command-line-api\
git checkout main 
Remove-Item -Recurse ~\.nuget\packages\System.CommandLine* -Force
dotnet clean
dotnet pack  /p:PackageVersion=2.0.0-codegencs
copy artifacts\packages\Debug\Shipping\System.CommandLine.2.0.0-codegencs.nupkg ..\..\packages-local
copy artifacts\packages\Debug\Shipping\System.CommandLine.NamingConventionBinder.2.0.0-codegencs.nupkg ..\..\packages-local
cd ..\..\


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


if ($configuration -eq "Release")
{
    # Can clean again since dotnet-codegencs will use Nuget references
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\dotnet-codegencs\bin\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\dotnet-codegencs\obj\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateBuilder\bin\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateBuilder\obj\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateLauncher\bin\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateLauncher\obj\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateDownloader\bin\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\dotnet-codegencs\CodegenCS.TemplateDownloader\obj\
	Remove-Item -Recurse -Force -ErrorAction Ignore  .\VSExtensions\RunTemplate\bin\
	Remove-Item -Recurse -Force -ErrorAction Ignore  .\VSExtensions\RunTemplate\obj\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\Models\CodegenCS.DbSchema.Extractor\bin\ # old name
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\Models\CodegenCS.DbSchema.Extractor\obj\ # old name
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\Models\CodegenCS.Models.DbSchema.Extractor\bin\
    Remove-Item -Recurse -Force -ErrorAction Ignore  .\Models\CodegenCS.Models.DbSchema.Extractor\obj\
    dotnet clean CodegenCS.sln
}


# The following libraries are all part of dotnet-codegencs tool...

& $msbuild ".\Models\CodegenCS.Models.DbSchema.Extractor\CodegenCS.Models.DbSchema.Extractor.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }


          
dotnet restore CodegenCS.TemplateBuilder\CodegenCS.TemplateBuilder.csproj
& $msbuild ".\dotnet-codegencs\CodegenCS.TemplateBuilder\CodegenCS.TemplateBuilder.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

dotnet restore CodegenCS.TemplateLauncher\CodegenCS.TemplateLauncher.csproj
& $msbuild ".\dotnet-codegencs\CodegenCS.TemplateLauncher\CodegenCS.TemplateLauncher.csproj" `
           /t:Restore /t:Build                                                  `
           '/p:targetFrameworks="netstandard2.0;net472;net5.0"'                 `
           /p:Configuration=$configuration                                      `
           /p:IncludeSymbols=true                                               `
           /p:SymbolPackageFormat=snupkg                                        `
           /verbosity:minimal                                                   `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }



# dotnet-codegencs (DotnetTool nupkg/snupkg)
& $msbuild ".\dotnet-codegencs\dotnet-codegencs\dotnet-codegencs.csproj"   `
           /t:Restore /t:Build /t:Pack                             `
           /p:PackageOutputPath="..\..\packages-local\"      `
           '/p:targetFrameworks="net5.0"'                 `
           /p:Configuration=$configuration                `
           /p:IncludeSymbols=true                         `
           /p:SymbolPackageFormat=snupkg                  `
           /verbosity:minimal                             `
           /p:ContinuousIntegrationBuild=true
if (! $?) { throw "msbuild failed" }

# Now all nuget packages (including the global tool) are in .\packages-local\

# uninstall/reinstall global tool from local dotnet-codegencs.*.nupkg:
dotnet tool uninstall -g dotnet-codegencs
dotnet tool install --global --add-source .\packages-local --no-cache dotnet-codegencs
dotnet-codegencs --version


# Unit tests
dotnet build -c $configuration .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj
#dotnet test  Core\CodegenCS.Tests\CodegenCS.Tests.csproj

# VSExtension (not working with Release yet - error NU1106: Unable to satisfy conflicting requests)
& $msbuild ".\VSExtensions\RunTemplate\RunTemplate.csproj"   `
           /t:Restore /t:Build                                     `
           '/p:targetFrameworks="net472"'                 `
           /p:Configuration='Debug'                `
           /verbosity:minimal                             
if (! $?) { throw "msbuild failed" }

# How to Reset the Visual Studio 2022 Experimental Instance:
# & cmd /C "C:\Program Files\Microsoft Visual Studio\2022\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=17.0_defe2a84 /RootSuffix=Exp 

# How to install VSIX extension:
# & "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\VSIXInstaller.exe" /quiet VSExtensions\RunTemplate\bin\Debug\RunTemplate.vsix
