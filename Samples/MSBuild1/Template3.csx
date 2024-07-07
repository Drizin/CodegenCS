// Sample template that writes programmatically to multiple files

class Template3
{
    void Main(ICodegenContext context)
    {
        var classes = new string[] { "Class1", "Class2", "Class3" };

        foreach (var className in classes)
        {
            context[$"{className}.g.cs"].Write(GenerateClass("MyNamespace", className));
        }
    }

    FormattableString GenerateClass(string ns, string className) => $$"""
        namespace {{ns}}
        {
            partial class {{className}}
            {
                public void Initialize()
                {
                    // This method is generated on the fly!
                }
            }
        }
        """;
}
