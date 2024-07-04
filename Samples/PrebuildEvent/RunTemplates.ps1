$expectedVersion="3.5.0"

# How to install tool globally:
# dotnet tool install --global dotnet-codegencs --version $expectedVersion

# Let's check if it's already there
#$alreadyInstalled = $(dotnet tool list -g dotnet-codegencs) | out-string -stream | select-string $expectedVersion
$codegencs = "$($env:USERPROFILE)\.dotnet\tools\dotnet-codegencs.exe"

# If not there install globally
if (-not (Test-Path $codegencs)) {
    dotnet tool install --global dotnet-codegencs --version $expectedVersion
    $codegencs = "$($env:USERPROFILE)\.dotnet\tools\dotnet-codegencs.exe"
}

# Or if global installation failed install locally (in a manifest)
if (-not (Test-Path $codegencs)) {
    dotnet new tool-manifest
    dotnet tool install dotnet-codegencs --version $expectedVersion
    $codegencs = "dotnet-codegencs" # all tools added to the manifest will be automatically available
}

if (-not (Test-Path $codegencs)) { throw "can't find dotnet-codegencs" }

$ErrorActionPreference = "Stop"

# Download template if not there
if (-not (Test-Path "DapperExtensionPocos.cs")) {
    & $codegencs template clone https://raw.githubusercontent.com/CodegenCS/Templates/main/DatabaseSchema/DapperExtensionPocos/DapperExtensionPocos.cs 
    # equivalent of & $codegencs template clone DapperExtensionPocos
}

Write-host "Refreshing DB SCHEMA..." -for yellow
#& $codegencs model dbschema extract mssql 'Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword' .\AdventureWorksSchema.json

Write-host "Running DapperExtensionPocos.cs template..." -for yellow
& $codegencs template run DapperExtensionPocos.cs .\AdventureWorksSchema.json "SampleProject.Core.Entities" -o .\GeneratedEntities\ -p:CrudNamespace="SampleProject.Core.Database" -p:CrudFile=".\DapperCrudExtensions.cs" -p:CrudClass="DapperCrudExtensions" -p:TrackPropertiesChange=true
