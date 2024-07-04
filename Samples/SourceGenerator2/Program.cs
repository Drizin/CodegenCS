using System;
using Dapper;
using MyProject.POCOs; // POCOs will be generated (on-the-fly!) under this namespace

namespace SampleSourceGenerator.Demo
{
    partial class Program
    {
        static void Main(string[] args)
        {
            // If you use CodegenCSOutput="Memory" then it looks like magic: POCOs are just generated on the fly!
            // (unfortunately IDE has some bugs and might show errors, but it builds and runs fine! using CodegenCSOutput="File" usually provides better experience)
            // If you debug and step-into the next line you'll see the generated POCOs.

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
