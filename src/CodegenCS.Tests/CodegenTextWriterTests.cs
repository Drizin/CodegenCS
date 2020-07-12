using CodegenCS;
using CodegenCS.Utils;
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
        public void ManualIndent()
        {
            _w.WriteLine("Line1");
            _w.IncreaseIndent();
            _w.WriteLine("Line2");
            _w.IncreaseIndent();
            _w.WriteLine("Line3");
            _w.DecreaseIndent();
            _w.WriteLine("Line4");
            string expected =
@"
Line1
    Line2
        Line3
    Line4
";

            Assert.AreEqual(expected.TrimStart(), _w.ToString());
        }

        [Test]
        public void AutoIndent()
        {
            _w.WriteLine("Line1");
            using (_w.WithIndent())
            {
                _w.WriteLine("Line2");
                using (_w.WithIndent())
                {
                    _w.WriteLine("Line3");
                }
                _w.WriteLine("Line4");
            }
            string expected =
@"
Line1
    Line2
        Line3
    Line4
";

            Assert.AreEqual(expected.TrimStart(), _w.ToString());
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
            using (_w.WithCBlock())
            {
                _w.WriteLine($"public class {myClass}");
                using (_w.WithCBlock())
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
            using (_w.WithCBlock($"namespace {myNamespace}"))
            {
                using (_w.WithCBlock($"public class {myClass}"))
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

        [Test]
        public void TestInlineTemplate2()
        {
            List<Property> props = new List<Property>() { new Property() { Name = "Name", Type = "string" }, new Property() { Name = "Age", Type = "int" } };
            string myNamespace = "codegencs";
            string myClass = "Test1";
            int i = 0;
            _w.WithCurlyBraces($"namespace {myNamespace}", (_) => {
                _w.WithCurlyBraces($"public class {myClass}", (_2) => {
                    _w.WriteLine($"// My Properties start here");
                    _w.WriteLine($"Properties.Count={props.Count}");
                    props.Select(prop => (Func<string>)(() => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}")).ToList().ForEach(action => _w.WriteLine(action));
                });
            });
            var c1 = _w.GetContents();
            i = 0;
            _w = new CodegenTextWriter();
            _w.WithCurlyBraces($"namespace {myNamespace}", (_) => {
                _w.WithCurlyBraces($"public class {myClass}", (_2) => {
                    _w.WriteLine($@"
                            // My Properties start here
                            Properties.Count={props.Count}
                            {props.Select(prop => (Func<string>)(() => $"[{i++}]. public {prop.Type} {prop.Name} {{ get; set; }}"))}");
                });
            });
            var c2 = _w.GetContents();

            System.Diagnostics.Debug.WriteLine(c1);
            System.Diagnostics.Debug.WriteLine(c2);

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
}
";

            Assert.AreEqual(c1, expected);
            Assert.AreEqual(c2, expected);
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




    }
}
