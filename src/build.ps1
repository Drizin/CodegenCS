[cmdletbinding()]
param(
    [Parameter(Mandatory=$False)]
    [ValidateSet('Release','Debug')]
    [string]$configuration="Release"
)

# How to run: .\build.ps1   or   .\build.ps1 -configuration Debug



. .\build-clean.ps1

New-Item -ItemType Directory -Force -Path ".\packages-local"

. .\build-external.ps1 $configuration

. .\build-core.ps1 $configuration

. .\build-models.ps1 $configuration

if ($configuration -eq "Release") 
{
	# TODO: clear bin/obj again to enforce that all further projects will use the nugets
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Core\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\bin$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
    Get-ChildItem .\Models\ -Recurse | Where{$_.FullName -CMatch ".*\\obj$" -and $_.PSIsContainer} | Remove-Item -Recurse -Force -ErrorAction Ignore
}

. .\build-tools.ps1 $configuration


# Unit tests # TODO: break this into CORE tests, MODEL tests, CLITESTS 
dotnet build -c $configuration .\Core\CodegenCS.Tests\CodegenCS.Tests.csproj
#dotnet test  Core\CodegenCS.Tests\CodegenCS.Tests.csproj

# VSExtension (not working with Release yet - error NU1106: Unable to satisfy conflicting requests)
#. .\build-visualstudio.ps1 $configuration


