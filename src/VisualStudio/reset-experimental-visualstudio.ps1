# How to Reset the Visual Studio 2022 Experimental Instance:
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=17.0_defe2a84 /RootSuffix=Exp
& "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VSSDK\VisualStudioIntegration\Tools\Bin\CreateExpInstance.exe" /Reset /VSInstance=16.0_8ff8e53a /RootSuffix=Exp

# delete extensions from %LOCALAPPDATA%\Microsoft\VisualStudio\
# C:\Users\drizi\AppData\Local\Microsoft\VisualStudio\16.0_8ff8e53aExp\Extensions\Rick Drizin
# C:\Users\drizi\AppData\Local\Microsoft\VisualStudio\17.0_defe2a84Exp\Extensions\Rick Drizin
Get-ChildItem ${env:LOCALAPPDATA}\Microsoft\VisualStudio -Recurse | Where{$_.FullName -Match ".*\\Rick Drizin$"} | Remove-Item -Force

# How to install VSIX extension (not in the experimental instance!):
#$vsixinstaller = "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\VSIXInstaller.exe"
$vsixinstaller = "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\VSIXInstaller.exe"
$configuration="Debug"
# & $vsixinstaller /quiet "VisualStudio\VS2019Extension\bin\$configuration\CodegenCS-VS2019Extension.vsix"
# & $vsixinstaller /quiet "VisualStudio\VS2022Extension\bin\$configuration\CodegenCS-VS2022Extension.vsix"
