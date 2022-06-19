using NUnit.Framework;
using System;

namespace CodegenCS.Tests.TemplateTests
{
    public class TextTemplateTests
    {
        /**** Rendering a text template which includes another text template using Include.Template<T>() ***/
        public class MyMethodTemplate : ICodegenTextTemplate
        {
            public FormattableString GetTemplate()
            {
                return $@"
                    public void MyMethod()
                    {{
                        Console.WriteLine(""It works!"");
                    }}";
            }
        }

        public class MyClassTemplate : ICodegenTextTemplate
        {
            public FormattableString GetTemplate()
            {
                return $@"
                    public class MyClass
                    {{
                        {Include.Template<MyMethodTemplate>()}
                    }}";
            }
        }

        [Test]
        public void Subtemplate_With_Typeof()
        {
            var ctx = new CodegenContext();
            ctx.DefaultOutputFile.RenderTextTemplate<MyClassTemplate>();
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
