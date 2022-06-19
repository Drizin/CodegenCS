using NUnit.Framework;

namespace CodegenCS.Tests.TemplateTests
{
    public class SinglefileTemplateTests
    {
        /**** Rendering a simple single-file template ***/
        public class MyMethodTemplate : ICodegenSinglefileTemplate
        {
            public void Render(ICodegenTextWriter writer)
            {
                writer.Write($@"
                    public void MyMethod()
                    {{
                        Console.WriteLine(""It works!"");
                    }}");
            }
        }
        [Test]
        public void RenderTest()
        {
            var ctx = new CodegenContext();
            ctx.DefaultOutputFile.RenderSinglefileTemplate<MyMethodTemplate>();
            string expected = @"
public void MyMethod()
{
    Console.WriteLine(""It works!"");
}";
            Assert.AreEqual(expected.TrimStart(), ctx.DefaultOutputFile.GetContents());
        }


        /**** Rendering a more elaborated single-file template ***/
        public class MyComplexTemplate : ICodegenSinglefileTemplate
        {
            public void Render(ICodegenTextWriter writer)
            {
                var tables = new[]
                {
                    new { TableName = "Users", Columns = new string[] { "UserId", "FirstName", "LastName" } },
                    new { TableName = "Products", Columns = new string[] { "ProductId", "Name", "Description" } },
                };

                foreach (var table in tables)
                {
                    writer.WithCurlyBraces($"public class {table.TableName}", (w) =>
                    {
                        foreach (var column in table.Columns)
                        {
                            writer.WriteLine($"public string {column} {{ get; }} {{set;}}");
                        }
                    });
                }
            }
        }
        [Test]
        public void ComplexFileTestTest()
        {
            var ctx = new CodegenContext();
            ctx.DefaultOutputFile.RenderSinglefileTemplate<MyComplexTemplate>();
            string expected = @"
public class Users
{
    public string UserId { get; } {set;}
    public string FirstName { get; } {set;}
    public string LastName { get; } {set;}
}
public class Products
{
    public string ProductId { get; } {set;}
    public string Name { get; } {set;}
    public string Description { get; } {set;}
}
";
            Assert.AreEqual(expected.TrimStart(), ctx.DefaultOutputFile.GetContents());
        }




        /**** Rendering a single-file template which includes another single-file template using typeof() ***/
        public class MyClassTemplate : ICodegenSinglefileTemplate
        {
            public void Render(ICodegenTextWriter writer)
            {
                writer.Write($@"
                    public class MyClass
                    {{
                        {typeof(MyMethodTemplate)}
                    }}");
            }
        }

        [Test]
        public void Subtemplate_With_Typeof()
        {
            var ctx = new CodegenContext();
            ctx.DefaultOutputFile.RenderSinglefileTemplate<MyClassTemplate>();
            string expected = @"
public class MyClass
{
    public void MyMethod()
    {
        Console.WriteLine(""It works!"");
    }
}";
            Assert.AreEqual(expected.TrimStart(), ctx.DefaultOutputFile.GetContents());
        }


        /**** Rendering a single-file template which includes another single-file template using Include.Template<T>() ***/
        // Include will basically wrap the TYPE of the template, but CodegenTextWriter will resolve and render it.
        public class MyClassTemplate2 : ICodegenSinglefileTemplate
        {
            public void Render(ICodegenTextWriter writer)
            {
                writer.Write($@"
                    public class MyClass
                    {{
                        {Include.Template<MyMethodTemplate>()}
                    }}");
            }
        }

        [Test]
        public void Subtemplate_With_IncludeTemplate()
        {
            var ctx = new CodegenContext();
            ctx.DefaultOutputFile.RenderSinglefileTemplate<MyClassTemplate>();
            string expected = @"
public class MyClass
{
    public void MyMethod()
    {
        Console.WriteLine(""It works!"");
    }
}";
            Assert.AreEqual(expected.TrimStart(), ctx.DefaultOutputFile.GetContents());
        }




    }
}
