rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\samplesourcegenerator"

if not exist ..\packages-local mkdir ..\packages-local

dotnet clean
rmdir /s /q .\SampleSourceGenerator\bin\
rmdir /s /q .\SampleSourceGenerator\obj\
rmdir /s /q .\SampleSourceGenerator.Test\bin\
rmdir /s /q .\SampleSourceGenerator.Test\obj\

# SampleSourceGenerator
# dotnet build -c release SampleSourceGenerator\SampleSourceGenerator.csproj
# for some reason the nupkg sometimes get all dlls and sometimes gets only one... why's that?
# /p:ContinuousIntegrationBuild=true ?
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Restore SampleSourceGenerator\ /p:Configuration=Release
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Build /t:Pack SampleSourceGenerator\ /p:targetFrameworks="netstandard2.0" /p:Configuration=Release /p:PackageOutputPath=..\..\packages-local\

rmdir /s /q "%HOMEDRIVE%%HOMEPATH%\.nuget\packages\samplesourcegenerator"

# Test Project using SampleSourceGenerator
# "C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" /t:Restore /t:Build SampleSourceGenerator.Test\ /p:Configuration=Release /verbosity:minimal
# .\SampleSourceGenerator.Test\bin\Release\netcoreapp3.1\SampleSourceGenerator.Test.exe
dotnet run --project SampleSourceGenerator.Test

