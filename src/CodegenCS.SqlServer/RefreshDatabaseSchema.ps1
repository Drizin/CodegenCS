# To Execute Powershell Scripts from Visual Studio:
# 1) Right-button PS1 file - "Open With...""
# 2) Configure:
#      Program: Powershell.exe
#      Arguments: -noexit -File %1
#      Friendly Name: Execute PowerShell Script

# To execute CSX scripts you'll need CSI.EXE (C# REPL) which is shipped with Visual Studio
# but can also be installed by using the NuGet package Microsoft.Net.Compilers.Toolset - https://www.nuget.org/packages/Microsoft.Net.Compilers.Toolset/

# For more info about launching CSX scripts from PowerShell or from Visual Studio, check https://drizin.io/code-generation-csx-scripts-part1/

$dir = Split-Path $MyInvocation.MyCommand.Path 
$script = Join-Path $dir ".\RefreshDatabaseSchema.csx"
$requiredLibs = @(
    @{ Name = "Newtonsoft.Json"; Version = "12.0.3" },
    @{ Name = "Dapper"; Version = "2.0.35" }
);

# By default we'll only use NuGet 4 locations. But you can change to 3 if you're hosting 
# your scripts in a project with the old packages.config format and want to rely on existing project packages
$NuGetVersion = 4; 



$ErrorActionPreference = "Stop"

# Locate CSI.EXE by searching common paths
$csi = ( 
    "$Env:userprofile\.nuget\packages\microsoft.net.compilers.toolset\3.6.0\tasks\net472\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Professional\MSBuild\15.0\Bin\Roslyn\csi.exe",
    "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\csi.exe"
) | Where-Object { Test-Path $_ } | Select-Object -first 1

if (!$csi)
{
    Write-Host "---------------------------------------" -for red
    Write-Host "Can't find csi.exe" -for red
    Write-Host "Please fix search paths above, or install NuGet Microsoft.Net.Compilers.Toolset" -for red
    Write-Host "---------------------------------------" -for red
    Exit 1
}
Write-Host "Found csi.exe: $csi" -for cyan

# List of locations to search for assembly references
$libPaths = @()
$libPaths += $dir

if ($NuGetVersion -eq 4)
{
    # New NuGet 4.0+ (PackageReference) saves User-specific packages in %userprofile%\.nuget\packages\
    $libPaths += "${env:userprofile}\.nuget\packages";
    if (Test-Path "${env:userprofile}\.nuget\packages") { $missingNuGetPackagesLocation = "${env:userprofile}\.nuget\packages" }

    # New NuGet 4.0+ (PackageReference) saves Machine-wide packages in %ProgramFiles(x86)%\Microsoft SDKs\NuGetPackages\"
    $libPaths += "${env:ProgramFiles(x86)}\Microsoft SDKs\NuGetPackages";
}

if ($NuGetVersion -eq 3)
{
    # Old NuGet (packages.config) saves packages in "\packages" folder at solution level.
    # Locate by searching a few levels above the script
    $missingNuGetPackagesLocation = ( 
        (Join-Path $dir ".\packages"),
        (Join-Path $dir "..\packages"),
        (Join-Path $dir "..\..\packages"),
        (Join-Path $dir "..\..\..\packages"),
        (Join-Path $dir "..\..\..\..\packages")
    ) | Where-Object { Test-Path $_ } | Select-Object -first 1
    $libPaths += $missingNuGetPackagesLocation
}

# where to download missing NuGet packages
if ((Test-Path $missingNuGetPackagesLocation) -eq $false)
{
    $missingNuGetPackagesLocation = $dir
}


# csi /lib parameter allows multiple paths but does not accept spaces (or quotes) so we have to use short DOS 8.3 paths
$fso = New-Object -ComObject Scripting.FileSystemObject
$libPaths = ($libPaths | Where-Object { Test-Path $_ } | ForEach { $fso.GetFolder($_).shortpath  });


Write-Host "CSI will use the following paths to search for assembly references:`r`n   - $($libPaths -Join "`r`n   - ")" -for cyan


$missingLibs = @()
$requiredLibs | foreach {
    $requiredLib = $_;
    Write-Host "Checking for $($requiredLib.Name) version $($requiredLib.Version)..." -for Cyan -NoNewLine

    if ($NuGetVersion -eq 4)
    {
        # NuGet 4+ format
        $found = $libPaths | 
        ForEach { Join-Path $_ ($requiredLib.Name + '\' + $requiredLib.Version) } | 
        Where-Object { Test-Path $_ } | Select-Object -first 1

        if ($found -eq $null)
        {
            Write-Host "`n$($requiredLib.Name) not found. Will install using NuGet" -for Yellow
            $missingLibs += $requiredLib
        }
        else
        {
             Write-Host "Found: $found" -for Cyan
        }
    }

    if ($NuGetVersion -eq 3)
    {
        # NuGet <=3 format
        $found = $libPaths | 
        ForEach { Join-Path $_ ($requiredLib.Name + '.' + $requiredLib.Version) } | 
        Where-Object { Test-Path $_ } | Select-Object -first 1

        if ($found -eq $null)
        {
            Write-Host "`n$($requiredLib.Name) not found. Will install using NuGet" -for Yellow
            $missingLibs += $requiredLib
        }
        else
        {
             Write-Host "Found: $found4 $found3" -for Cyan
        }
    }


}

if ($missingLibs)
{
    $nuget = Join-Path $env:TEMP "nuget.exe"
    if ((Test-Path $nuget) -eq $False)
    {
        Write-Host "Downloading NuGet.exe into $nuget" -for cyan
        $webClient = New-Object System.Net.WebClient 
        $webClient.DownloadFile("https://dist.nuget.org/win-x86-commandline/latest/nuget.exe", $nuget)
    }

    $missingLibs | foreach {
        $missingLib = $_
        Write-Host "Downloading $missingLib...";
        $libName = $missingLib.Name
        $libVersion = $missingLib.Version
        if ($libVersion -eq $null)
        {
            & $nuget install $libName -OutputDirectory $missingNuGetPackagesLocation
        }
        else
        {
            & $nuget install $libName -Version $libVersion -OutputDirectory $missingNuGetPackagesLocation
        }
        if ($lastExitCode -ne 0)
        {
            Write-host "-------------`nError downloading $missingLib - aborting...`n-------------" -for red
            Exit 1
        }
    }
}


$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
Write-host "Starting csi.exe $script ..." -for yellow
& $csi /lib:$($libPaths -Join ';') $script

$stopwatch.Stop()
Write-Host "Finished in $($stopwatch.Elapsed.TotalMilliSeconds) milliseconds"

# Since I configured "-noexit" parameter in Visual Studio I don't need this
#if ($host.Name -notmatch 'ISE') { Write-Host -NoNewLine "(Just press Enter to exit)" -for cyan; read-host; }  
