using NUnit.Framework;

namespace CodegenCS.Tests.TemplateTests
{
    public class MultifileTemplateTests
    {
        /**** Rendering a simple multi-file template ***/
        public class MyMethodTemplate : ICodegenMultifileTemplate
        {
            public void Render(ICodegenContext context)
            {
                context["File1.cs"].Write($@"
                    public void MyMethod()
                    {{
                        Console.WriteLine(""It works!"");
                    }}");
                context["File2.cs"].Write($@"
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
            ctx.RenderMultifileTemplate<MyMethodTemplate>();
            string expected = @"
public void MyMethod()
{
    Console.WriteLine(""It works!"");
}";
            Assert.That(ctx.OutputFiles.Count == 2);
            Assert.AreEqual(expected.TrimStart(), ctx["File1.cs"].GetContents());
            Assert.AreEqual(expected.TrimStart(), ctx["File2.cs"].GetContents());
        }




    }
}
