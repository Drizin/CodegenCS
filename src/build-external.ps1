[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Release"
)

# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug


$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath
Push-Location $dir

New-Item -ItemType Directory -Force -Path ".\packages-local"

git submodule init
git pull --recurse-submodules
git submodule update --remote --recursive

cd External\command-line-api\
git checkout main 
Remove-Item -Recurse ~\.nuget\packages\System.CommandLine* -Force
dotnet clean

dotnet pack  /p:PackageVersion=2.0.0-codegencs -c $configuration
if (! $?) { throw "msbuild failed" }

copy artifacts\packages\$configuration\Shipping\System.CommandLine.2.0.0-codegencs.nupkg ..\..\packages-local\
copy artifacts\packages\$configuration\Shipping\System.CommandLine.NamingConventionBinder.2.0.0-codegencs.nupkg ..\..\packages-local\



Pop-Location
