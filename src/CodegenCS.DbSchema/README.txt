CodegenCS.DbSchema 

This is a Plugin (Datasource Provider) to be used with CodegenCS code generator.
This plugin described the structure of a Database Schema, and can be used by Templates which generate code based on a JSON Database Schema file.

If you want to extract the Schema of a MS SQL Server Database you can run this:

using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using CodegenCS.DbSchema.SqlServer;

namespace MyNamespace
{
    public class MySchemaReader
    {
        public void ExtractSchema()
        {
            string outputJsonSchema = "AdventureWorksSchema.json");
            string connectionString = @"Data Source=MYDESKTOP\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";

            Func<IDbConnection> connectionFactory = () => new SqlConnection(connectionString);
            var reader = new SqlServerSchemaReader(connectionFactory);
            reader.ExportSchemaToJSON(outputJsonSchema);
        }
    }
}

If you want to run this a CSX/PowerShell, please refer to the sources here: https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema
