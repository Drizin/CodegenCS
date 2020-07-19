/// <summary>
/// This CSX Script will invoke SqlServerSchemaReader, which extracts the schema of SQL database and saves into a JSON file.
/// The easiest way to launch csi.exe (which is shipped with Visual Studio) to run this script is by using PowerShell script RefreshDatabaseSchema.ps1
/// You can do that from Visual Studio (see instructions in RefreshDatabaseSchema.ps1) or you can just execute "Powershell RefreshDatabaseSchema.ps1"
/// </summary>

// System libraries
#r "System.Data.dll"

// Load third-party libraries by their relative paths, relative to "$Env:userprofile\.nuget\packages\"
#r "dapper\2.0.35\lib\netstandard2.0\Dapper.dll"
#r "newtonsoft.json\12.0.3\lib\netstandard2.0\Newtonsoft.Json.dll"

// CS files are better than CSX because Intellisense and Compile-time checks works better. 
#load "DbSchema\Table.cs"
#load "DbSchema\Column.cs"
#load "DbSchema\ForeignKey.cs"
#load "DbSchema\ForeignKeyMember.cs"
#load "DbSchema\DatabaseSchema.cs"
#load "DbSchema\Index.cs"
#load "DbSchema\IndexMember.cs"
#load "SqlServer\SqlServerSchemaReader.cs"

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;
using System.Data.SqlClient;

// Helpers to get the location of the current CSX script
public static string GetScriptPath([CallerFilePath] string path = null) => path;
public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);


// location relative to the CSX script
string outputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), "AdventureWorksSchema.json")); 
string connectionString = @"Data Source=MYDESKTOP\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";

Func<IDbConnection> connectionFactory = () => new SqlConnection(connectionString);
var reader = new SqlServerSchemaReader(connectionFactory);
reader.ExportSchemaToJSON(outputJsonSchema);
