using CodegenCS;
using CodegenCS.AdventureWorksPOCOSample;
using Dapper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Tests
{
    public class POCOTests
    {
        IDbConnection cn;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = @"Data Source=LENOVOFLEX5\SQLEXPRESS;
                            Initial Catalog=AdventureWorks;
                            Integrated Security=True;";
            cn = new SqlConnection(connectionString);
        }
        #endregion

        int businessEntityID = 1;
        string nationalIDNumber = "295847284";
        DateTime birthDate = new DateTime(1969, 01, 29);
        string maritalStatus = "S"; // single
        string gender = "M";


        /// <summary>
        /// Tests a full insert (all columns) and full update (all columns)
        /// </summary>
        [Test]
        public void TestInsertUpdate()
        {
            var product = new Product() { 
                Name = "ProductName", 
                ProductNumber = "1234", 
                SellStartDate = DateTime.Now, 
                ModifiedDate = DateTime.Now, 
                SafetyStockLevel = 5, 
                ReorderPoint = 700 
            };

            int deleted = cn.Execute($"DELETE [Production].[Product] WHERE ([ProductNumber]='1234' OR [ProductNumber]='12345')");


            cn.Save(product);

            product.Name = "Name2";
            product.ProductNumber = "12345";

            cn.Update(product);
        }

    }
}
