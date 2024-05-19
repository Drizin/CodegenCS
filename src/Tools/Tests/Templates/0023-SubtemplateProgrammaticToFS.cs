class MyTemplate
{
    string _ns = "MyNamespace";

    void Main(ICodegenOutputFile writer)
    {
        writer.Write($$"""
            namespace {{_ns}}
            {
                {{WriteClass}}
            }
            """);
    }


    FormattableString WriteClass => $$"""
        public class MyFirstClass {
            public void HelloWorld() {
                // ...
            }
        }
        """;
}
