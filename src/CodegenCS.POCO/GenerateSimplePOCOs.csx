/// <summary>
/// This CSX Script will invoke SimplePOCOGenerator, which builds simple POCO classes based on a JSON file with schema of SQL database
/// The easiest way to launch csi.exe (which is shipped with Visual Studio) to run this script is by using PowerShell script GenerateSimplePOCOs.ps1
/// You can do that from Visual Studio (see instructions in RefreshDatabaseSchema.ps1) or you can just execute "Powershell GenerateSimplePOCOs.ps1"
/// </summary>

// System libraries
#r "System.Data.dll"

// Load third-party libraries by their relative paths, relative to "$Env:userprofile\.nuget\packages\"
#r "newtonsoft.json\12.0.3\lib\netstandard2.0\Newtonsoft.Json.dll"
#r "codegencs\1.0.5\lib\netstandard2.0\CodegenCS.dll"
#r "codegencs.dbschema\1.0.0\lib\netstandard2.0\CodegenCS.DbSchema.dll"

// CS files are better than CSX because Intellisense and Compile-time checks works better. 
#load "LogicalSchema.cs"
#load "SimplePOCOGenerator.cs"

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;
using System.Data.SqlClient;

// Helpers to get the location of the current CSX script
public static string GetScriptPath([CallerFilePath] string path = null) => path;
public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);


// locations relative to the CSX script
string inputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), "AdventureWorksSchema.json"));
string targetFolder = Path.GetFullPath(Path.Combine(GetScriptFolder(), @".\POCOs\"));


var generator = new SimplePOCOGenerator(inputJsonSchema);
generator.Namespace = "CodegenCS.AdventureWorksPOCOSample";
generator.Generate(targetFolder);
