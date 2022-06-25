/// <summary>
/// This CSX Script will invoke SqlServerSchemaReader, which extracts the schema of SQL database and saves into a JSON file.
/// The easiest way to launch csi.exe (which is shipped with Visual Studio) to run this script is by using PowerShell script RefreshDatabaseSchema.ps1
/// You can do that from Visual Studio (see instructions in RefreshDatabaseSchema.ps1) or you can just execute "Powershell RefreshDatabaseSchema.ps1"
/// </summary>

// System libraries
#r "System.Data.dll"

// Load third-party libraries by their relative paths, relative to "$Env:userprofile\.nuget\packages\"
#r "dapper\2.0.35\lib\netstandard2.0\Dapper.dll"
#r "npgsql\5.0.3\lib\netstandard2.0\Npgsql.dll"
#r "system.text.json\5.0.0\lib\netstandard2.0\System.Text.Json.dll"
#r "system.threading.channels\4.7.1\lib\netstandard2.0\System.Threading.Channels.dll"
#r "microsoft.bcl.asyncinterfaces\1.0.0\lib\netstandard2.0\Microsoft.Bcl.AsyncInterfaces.dll"
#r "newtonsoft.json\13.0.1\lib\netstandard2.0\Newtonsoft.Json.dll"

// CS files are better than CSX because Intellisense and Compile-time checks works better. 
#load "..\DbSchema\Table.cs"
#load "..\DbSchema\Column.cs"
#load "..\DbSchema\ForeignKey.cs"
#load "..\DbSchema\ForeignKeyMember.cs"
#load "..\DbSchema\DatabaseSchema.cs"
#load "..\DbSchema\Index.cs"
#load "..\DbSchema\IndexMember.cs"
#load "PgsqlSchemaReader.cs"

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Data;
using Npgsql;
using System.Data.SqlClient;

// Helpers to get the location of the current CSX script
public static string GetScriptPath([CallerFilePath] string path = null) => path;
public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);


// location relative to the CSX script
string outputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), @".\AdventureWorksSchema.json")); 
string connectionString = @"Host=localhost;
                            Username=postgres;
                            Password=PUTYOURPASSWORDHERE;
                            Database=Adventureworks;";

Func<IDbConnection> connectionFactory = () => new NpgsqlConnection(connectionString);
var reader = new PgsqlSchemaReader(connectionFactory);
reader.ExportSchemaToJSON(outputJsonSchema);
