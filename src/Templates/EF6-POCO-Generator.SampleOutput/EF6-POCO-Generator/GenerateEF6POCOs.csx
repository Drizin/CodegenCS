/// <summary>
/// This CSX Script will invoke EFCoreGenerator, which builds EF Entities and DbContext based on a JSON file with schema of SQL database
/// The easiest way to launch csi.exe (which is shipped with Visual Studio) to run this script is by using PowerShell script GenerateEFCore.ps1
/// You can do that from Visual Studio (see instructions in RefreshDatabaseSchema.ps1) or you can just execute "Powershell GenerateEFCore.ps1"
/// </summary>

// System libraries
#r "System.Data.dll"

// NuGet 4.0+ (PackageReference directive in new csproj format) uses at least two global package locations:
// User-specific: %userprofile%\.nuget\packages\
// Machine-wide: %ProgramFiles(x86)%\Microsoft SDKs\NuGetPackages\"
// Load third-party libraries by their relative paths, relative to "$Env:userprofile\.nuget\packages\"
//#r "codegencs\1.0.2\lib\netstandard2.0\CodegenCS.dll"
//#r "humanizer.core\2.8.26\lib\netstandard2.0\Humanizer.dll"

// NuGet < 4.0 saves the packages in "packages" folder under the solution
// Load third-party libraryes by their relative paths, relative to packages folder from solution
#r "CodegenCS.1.0.2\lib\netstandard2.0\CodegenCS.dll"
#r "Humanizer.Core.2.8.26\lib\netstandard2.0\Humanizer.dll"

// CS files are better than CSX because Intellisense and Compile-time checks works better. 
#load "DatabaseObjects.cs"
#load "Enums.cs"
#load "Generator.cs"
#load "HumanizerInflector.cs"
#load "IInflector.cs"
#load "Inflector.cs"
#load "SchemaReader.cs"
#load "Settings.cs"
#load "SqlServerSchemaReader.cs"

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;
using System.Data.SqlClient;
using CodegenCS;
using CodegenCS.DotNet;

// Helpers to get the location of the current CSX script
public static string GetScriptPath([CallerFilePath] string path = null) => path;
public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);


// locations relative to the CSX script
string projectFolder = GetScriptFolder();
string targetFolder = Path.GetFullPath(Path.Combine(GetScriptFolder(), @"..\GeneratedCode\"));
string csproj = Path.GetFullPath(Path.Combine(GetScriptFolder(), @"..\EF6-POCO-Generator.SampleOutput.csproj"));

DotNetCodegenContext context = new DotNetCodegenContext();
Generator generator = new Generator(
    context: context,
    createConnection: () => new System.Data.SqlClient.SqlConnection(@"
                    Data Source=LENOVOFLEX5\SQLEXPRESS;
                    Initial Catalog=AdventureWorks;
                    Integrated Security=True;
                    Application Name=EntityFramework POCO Generator"
    ),
    targetFrameworkVersion: 4.5m
    );

Settings.Namespace = "EF6POCOGenerator.SampleOutput";
Settings.DbContextName = "MyDbContext";


bool multipleFiles = true;

if (multipleFiles)
{
    generator.GenerateMultipleFiles(targetFolder, csproj);
}
else
{
    // Other alternative is to generate a single file
    string targetFile = Path.GetFullPath(Path.Combine(GetScriptFolder(), @$".\{Settings.DbContextName}.cs"));
    generator.GenerateSingleFile(file: targetFile);
}
