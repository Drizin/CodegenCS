using CodegenCS;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        CodegenTextWriter _w = null;

        [SetUp]
        public void Setup()
        {
            _w = new CodegenTextWriter();
        }

        [Test]
        public void TestMultiline()
        {
            _w.WriteLine(@"
                namespace codegencs
                {
                    public class Test1
                    {
                        // My Properties start here
                    }
                }");
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";

            Assert.AreEqual(_w.ToString(), expected);
        }


        [Test]
        public void TestCStyleBlock()
        {
            string myNamespace = "codegencs";
            string myClass = "Test1";
            _w.WriteLine($"namespace {myNamespace}");
            using (_w.WithCStyleBlock())
            {
                _w.WriteLine($"public class {myClass}");
                using (_w.WithCStyleBlock())
                {
                    _w.WriteLine("// My Properties start here");
                }
            }
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";

            Assert.AreEqual(_w.ToString(), expected);
        }

        [Test]
        public void TestCStyleBlock2()
        {
            string myNamespace = "codegencs";
            string myClass = "Test1";
            using (_w.WithCStyleBlock($"namespace {myNamespace}"))
            {
                using (_w.WithCStyleBlock($"public class {myClass}"))
                {
                    _w.WriteLine("// My Properties start here");
                }
            }
            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
    }
}
";
            Assert.AreEqual(_w.ToString(), expected);
        }

        [Test]
        public void TestInlineTemplate()
        {
            List<Property> props = new List<Property>() { new Property() { Name = "Name", Type = "string" }, new Property() { Name = "Age", Type = "int" } };
            string myNamespace = "codegencs";
            string myClass = "Test1";
            //_w.Write($@"
            //    namespace {myNamespace}
            //    {{
            //        public class {myClass}
            //        {{
            //            // My Properties start here
            //            { RenderProperties(props) }
            //        }}
            //    }}");

            int i = 0;

            //_w.Write($@"
            //    namespace {myNamespace}
            //    {{
            //        public class {myClass}
            //        {{
            //            // My Properties start here
            //            Properties.Count={props.Count}
            //            {props.Select(prop => (Func<FormattableString>)(() => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}"))}
            //        }}
            //    }}");

            _w.Write($@"
                namespace {myNamespace}
                {{
                    public class {myClass}
                    {{
                        // My Properties start here
                        Properties.Count={props.Count}
                        {props.Select(prop => (Func<string>)(() => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}"))}
                    }}
                }}");

            System.Diagnostics.Debug.WriteLine(_w.ToString());
            string expected =
@"namespace codegencs
{
    public class Test1
    {
        // My Properties start here
        Properties.Count=2
        [0]. public string Name { get; set; }
        [1]. public int Age { get; set; }
    }
}";

            Assert.AreEqual(_w.ToString(), expected);
        }
        public class Property
        {
            public string Name;
            public string Type;
        }
        Func<FormattableString> RenderProperties(List<Property> props)
        {
            int i = 0;
            return () => $@"
                Properties.Count={props.Count}
                {string.Join(Environment.NewLine, props.Select(prop => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}"))}"
            ;
        }



        public class TodoItem
        {
            public string Description { get; private set; }
            public IList<TodoItem> SubTasks { get; private set; }

            public TodoItem(string description)
            {
                Description = description;
                SubTasks = new List<TodoItem>();
            }
        }
        TodoItem[] todoList =
        {
            new TodoItem("Get milk"),
            new TodoItem("Clean the house")
            {
                SubTasks =
                {
                    new TodoItem("Living room"),
                    new TodoItem("Bathrooms")
                    {
                        SubTasks =
                        {
                            new TodoItem("Guest bathroom"),
                            new TodoItem("Family bathroom")
                        }
                    },
                    new TodoItem("Bedroom")
                }
            },
            new TodoItem("Mow the lawn")
        };

        private static void WriteToDoList(IEnumerable<TodoItem> todoItems, CodegenTextWriter writer)
        {
            foreach (var item in todoItems)
            {
                writer.WriteLine("- {0}", item.Description);

                if (item.SubTasks.Any())
                {
                    writer.IncreaseIndent();
                    WriteToDoList(item.SubTasks, writer);
                    writer.DecreaseIndent();
                }
            }
        }
        [Test]
        public void TestRecursiveFunction()
        {
            // https://mariusschulz.com/blog/using-the-indentedtextwriter-class-to-output-hierarchically-structured-data
            WriteToDoList(todoList, _w);
            string expected = @"- Get milk
- Clean the house
    - Living room
    - Bathrooms
        - Guest bathroom
        - Family bathroom
    - Bedroom
- Mow the lawn
";
            Assert.AreEqual(_w.ToString(), expected);
        }







        public class Product
        {
            public string Name { get; set; }
            public decimal Price { get; set; }
            public string Description { get; set; }
        }
        public List<Product> products = new List<Product>()
        {
            new Product() { Name = "Router TP-Link", Price=290.90m, Description="TP-Link AC5400 Tri Band Gaming Router" },
            new Product() { Name = "LG 27\" monitor", Price=449.99m, Description="LG 27UK850-W 27\" 4K UHD IPS Monitor" },
            new Product() { Name = "ASUS ZenBook 13", Price=999.99m, Description="ASUS ZenBook 13 Ultra-Slim Durable Laptop 13.3" },
        };

        [Test]
        public void TestForeach()
        {
            //_w.WriteLine($@"
            //        {(Action<CodegenTextWriter>)((w) =>
            //        {
            //            foreach (var product in products)
            //            {
            //                w.WriteLine($@"
            //                <li>
            //                    <h2>{product.Name}</h2>
            //                    Only {product.Price.ToString("c")}

            //                    {product.Description}
            //                </li>");
            //            }
            //        })}
            //    </ul>");

            //_w.WriteLine($@"
            //    <ul id=""products"">
            //        {products.Select<Product,FormattableString>(product => $@"
            //        <li>
            //            <h2>{product.Name}</h2>
            //            Only {product.Price.ToString("c")}

            //            {product.Description}
            //        </li>")}
            //    </ul>");

            //_w.WriteLine($@"
            //    <ul id=""products"">
            //        {products.Select(product => $@"
            //        <li>
            //            <h2>{product.Name}</h2>
            //            Only {product.Price.ToString("c")}

            //            {product.Description}
            //        </li>")}
            //    </ul>");

            //_w.WriteLine($@"
            //    <ul id=""products"">
            //        {products.Select(product => $@"<b>{product.Name}</b>")}
            //    </ul>");

            _w.WriteLine($@"
                <ul id=""products"">
                    {string.Join(",", products.Select(product => $@"<b>{product.Name}</b>"))}
                </ul>");


            System.Diagnostics.Debug.WriteLine(_w.ToString());
            // {string.Join(_w.NewLine, products.Select(p=>p.Name))}

            /*
            // https://github.com/dotliquid/dotliquid

            <ul id=""products"">
              {% for product in products %}
                <li>
                  <h2>{{product.name}}</h2>
                  Only {{product.price | price }}

                  {{product.description | prettyprint | paragraph }}
                </li>
              {% endfor %}
            </ul>
            */

        }


        [Test]
        public void TestMSProj()
        {
            MSBuildProjectEditor editor = //new MSBuildProjectEditor(@"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\CodegenCS.Tests.csproj");
                new MSBuildProjectEditor(@"D:\Repositories\EntityFramework-Scripty-Templates4\src\Test.csproj");
            editor.AddItem(itemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\lala\Test.cs", parentItemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\GenerateEFModel.csx");
            editor.RemoveUnusedDependentItems(parentItemPath: @"D:\Repositories\EntityFramework-Scripty-Templates4\src\GenerateEFModel.csx");
            editor.Save();
        }

        [Test]
        public void TestContextMSProj()
        {
            MSBuildProjectEditor editor = new MSBuildProjectEditor(@"D:\Repositories\EntityFramework-Scripty-Templates4\src\Test.csproj");
            CodegenContext context = new CodegenContext(@"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\");
            var file1 = context.GetTextWriter("File1.cs");
            var file2 = context.GetTextWriter("Path2\\File2.cs");
            file1.WriteLine("// Helloooooo");
            file2.WriteLine("// Hello from File2");
            string masterFile = @"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\CodegenTextWriterTests.cs";
            context.SaveFiles();
            editor.AddItem(masterFile);
            foreach (var o in context.OutputFilesAbsolute)
                editor.AddItem(itemPath: o.Key, parentItemPath: masterFile, itemType: o.Value.ItemType);
            editor.Save();
        }

        [Test]
        public void TestDatabase()
        {
            MSBuildProjectEditor editor = new MSBuildProjectEditor(@"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\CodegenCS.Tests.csproj");
            CodegenContext context = new CodegenContext(@"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\");

            string templateFile = @"D:\Repositories\CodegenCS-public\src\CodegenCS.Tests\CodegenTextWriterTests.cs";
            editor.AddItem(templateFile);

            Database db = Database.CreateSQLServerConnection(@"Data Source=LENOVOFLEX5\SQLEXPRESS;Initial Catalog=northwind;Integrated Security=True;Application Name=CodegenCS");
            var tables = db.Query("SELECT Name FROM sys.tables");
            foreach (var table in tables)
            {
                var file = context.GetTextWriter($"{table.Name.ToString()}.cs");
                file.WriteLine("// Helloooooo");
                foreach (var o in context.OutputFilesAbsolute)
                    editor.AddItem(itemPath: o.Key, parentItemPath: templateFile, itemType: o.Value.ItemType);
            }

            context.SaveFiles();
            editor.Save();
        }


    }
}
