class MyTemplate
{
    string _ns = "MyNamespace";

    FormattableString Main() => $$"""
        namespace {{_ns}}
        {
            {{WriteClass}}
        }
        """;

    void WriteClass(ICodegenOutputFile writer)
    {
        writer.Write($$"""
            public class MyFirstClass {
                public void HelloWorld() {
                  // ...
                }
            }
            """);
    }
}
