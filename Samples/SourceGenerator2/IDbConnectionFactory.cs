using System.Data;
using System.Data.SqlClient;

namespace MyProject.POCOs
{
    public class IDbConnectionFactory
    {
        public static IDbConnection CreateConnection()
        {
            return new SqlConnection(@"Data Source=(local); Initial Catalog=AdventureWorks2019; Integrated Security=True;");
            // Implement your own factory... e.g. new NpgsqlConnection (...)
        }
    }
}

