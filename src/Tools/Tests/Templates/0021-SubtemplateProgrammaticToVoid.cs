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
