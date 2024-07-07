
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
    "${Env:ProgramFiles(x86)}\MSBuild\13.0\Bin\MSBuild.exe",
    "${Env:ProgramFiles(x86)}\MSBuild\12.0\Bin\MSBuild.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1


$targetNugetExe = "$PSScriptRoot\nuget.exe"
if (-not (Test-Path $targetNugetExe)) {
	$sourceNugetExe = "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe"
	Invoke-WebRequest $sourceNugetExe -OutFile $targetNugetExe
	Set-Alias nuget $targetNugetExe -Scope Global -Verbose
}
