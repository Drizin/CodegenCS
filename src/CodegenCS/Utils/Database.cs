using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace CodegenCS.Utils
{
    public class Database
    {
        protected string _connectionString;
        protected Database(string connectionString)
        {
            this._connectionString = connectionString;
        }
        public static Database CreateSQLServerConnection(string connectionString)
        {
            Database db = new Database(connectionString)
            {
                CreateConnection = () =>
                {
                    var conn = new System.Data.SqlClient.SqlConnection(connectionString);
                    conn.Open();
                    return conn;
                }
            };
            return db;
        }

        public Func<IDbConnection> CreateConnection;

        /// <summary>
        /// Creates a raw SqlConnection to the database. This is for using Dapper or raw ADO.NET.
        /// Don't forget to wrap your connection inside a "using" statement, to automatically close/dispose connection at end:
        /// using (var conn = DB.CreateConnection())
        /// {
        ///    ...
        /// }
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// Execute parameterized SQL
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public int Execute(string sql, object parms = null, IDbTransaction transaction = null, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            using (var conn = CreateConnection())
            {
                int affected = -1;
                affected = conn.Execute(sql, parms, transaction, commandTimeout, commandType);
                return affected;
            }
        }

        /// <summary>
        /// Execute parameterized SQL, returning the data typed as per T
        /// </summary>
        /// <returns>List of Entities of type T</returns>
        public List<T> Execute<T>(string sql, object parms = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            using (var conn = CreateConnection())
            {
                return conn.Query<T>(sql, parms, transaction, buffered, commandTimeout, commandType).ToList();
            }
        }

        /// <summary>
        ///  Executes a query, returning the data typed as per T
        /// </summary>
        /// <returns>
        /// A sequence of data of the supplied type; if a basic type (int, string, etc) is
        ///  queried then the data from the first column in assumed, otherwise an instance
        ///  is created per row, and a direct column-name===member-name mapping is assumed
        ///  (case insensitive).
        /// </returns>
        public List<T> Query<T>(string sql, object parms = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            using (var conn = CreateConnection())
            {
                return conn.Query<T>(sql, parms, transaction, buffered, commandTimeout, commandType).ToList();
            }
        }

        /// <summary>
        ///  Return a sequence of dynamic objects      <para /> 
        ///  Example:
        ///  var tables = DB.Query("SELECT * FROM sys.tables");
        ///  foreach(var table in tables) Response.WriteLine(table.Name);  <para /> 
        /// </summary>
        public List<dynamic> Query(string sql, object parms = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = default(int?), CommandType? commandType = default(CommandType?))
        {
            using (var conn = CreateConnection())
            {
                return conn.Query(sql, parms, transaction, buffered, commandTimeout, commandType).ToList();
            }
        }



    }
}
