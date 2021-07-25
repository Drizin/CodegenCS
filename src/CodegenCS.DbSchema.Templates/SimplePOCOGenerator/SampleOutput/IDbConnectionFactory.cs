using System;
using System.Data;
using System.Data.SqlClient;

namespace CodegenCS.AdventureWorksPOCOSample
{
    public class IDbConnectionFactory
    {
        public static IDbConnection CreateConnection()
        {
            string connectionString = @"Data Source=MYWORKSTATION\\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";

            return new SqlConnection(connectionString);
        }
    }
}

