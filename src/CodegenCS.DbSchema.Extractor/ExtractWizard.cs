using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace CodegenCS.DbSchema.Extractor
{
    public class ExtractWizard
    {
        public DbTypeEnum? DbType { get; set; } = null;
        public string ConnectionString { get; set; } = null;
        public string OutputJsonSchema { get; set; } = null;


        public enum DbTypeEnum
        {
            MSSQL,
            PostgreSQL
        }
        public ExtractWizard()
        {
        }

        public void Run()
        {
            string[] dbTypes = Enum.GetNames(typeof(DbTypeEnum));
            while (DbType == null)
            {
                Console.WriteLine($"[Choose a Database Type]");
                for (int i = 0; i < dbTypes.Length; i++)
                    Console.WriteLine($"{i + 1}. {dbTypes[i]}");
                Console.Write($"Database Type: ");
                string chosen = Console.ReadLine();
                int chosenInt;
                if (int.TryParse(chosen, out chosenInt) && chosenInt >= 1 && chosenInt <= dbTypes.Length)
                    DbType = Enum.Parse<DbTypeEnum>(dbTypes[chosenInt - 1]);
            }
            Console.WriteLine($"Database Type is {DbType}");
            Console.WriteLine($"");

            while(string.IsNullOrEmpty(ConnectionString))
            {

                Console.WriteLine($"[Choose a Connection String]");
                switch (DbType.Value)
                {
                    case DbTypeEnum.MSSQL:
                        Console.Write($"Example: Server=MYSERVER; Database=AdventureWorks; Integrated Security=True;");
                        Console.Write($"Example: Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=myPassword");
                        Console.Write($"Example: Server=MYWORKSTATION\\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True;");
                        break;
                    case DbTypeEnum.PostgreSQL:
                        Console.Write($"Example: Host=localhost; Database=Adventureworks; Username=postgres; Password=myPassword");
                        break;
                }

                Console.Write($"Connection String: ");
                ConnectionString = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(OutputJsonSchema))
            {

                Console.WriteLine($"[Choose an Output File]");
                Console.Write($"Output file: ");
                OutputJsonSchema = Console.ReadLine();
            }

            switch (DbType.Value)
            {
                case DbTypeEnum.MSSQL:
                    {
                        Func<IDbConnection> connectionFactory = () => new System.Data.SqlClient.SqlConnection(ConnectionString);
                        var reader = new PostgreSQL.PgsqlSchemaReader(connectionFactory);
                        reader.ExportSchemaToJSON(OutputJsonSchema);
                    }
                    break;
                case DbTypeEnum.PostgreSQL:
                    {
                        Func<IDbConnection> connectionFactory = () => new Npgsql.NpgsqlConnection(ConnectionString);
                        var reader = new PostgreSQL.PgsqlSchemaReader(connectionFactory);
                        reader.ExportSchemaToJSON(OutputJsonSchema);
                    }
                    break;
            }



            Console.Write($"Press any key to exit...");
            Console.ReadLine();
        }
    }
}
