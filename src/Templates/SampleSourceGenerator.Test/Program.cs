using System;
using CodegenCS.AdventureWorksPOCOSample;
using Dapper;

namespace SampleSourceGenerator.Test
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // This is the magic. We can use the POCOs even when they are not declared anywhere. They are generated on the fly.
            // If you debug the next line you'll see the generated POCOs.
            Product product = new Product() { Name = "Tesla Model S", ListPrice = 79990 };

            using (var cn = IDbConnectionFactory.CreateConnection())
            {
                var products = cn.Query<Product>("SELECT * FROM Production.Product");
                foreach (var p in products)
                    Console.WriteLine($"I'll buy a {p.Name} for {p.ListPrice:C}");
            }

            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
        }
    }
}
