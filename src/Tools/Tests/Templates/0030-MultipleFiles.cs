class MyTemplate
{
    void Main(ICodegenContext context)
    {
        context["Class1.cs"].WriteLine(GenerateClass("Class1"));
        context["Class2.cs"].WriteLine(GenerateClass("Class2"));
        context["Class3.cs"].WriteLine("public class Class3 {}");
        context.DefaultOutputFile.WriteLine("this goes to standard output"); // e.g. "MyTemplate.generated.cs"
    }

    FormattableString GenerateClass(string className) => $$"""
        public class {{className}}
        {
            public {{className}}()
            {
            }
        }
        """;
}
