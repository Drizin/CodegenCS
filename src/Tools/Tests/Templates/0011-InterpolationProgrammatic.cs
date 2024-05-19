class MyTemplate
{
    string _name = "Rick";

    void Main(ICodegenOutputFile writer)
    {
        writer.WriteLine($$"""
            public class MyFirstClass {
                public void Hello{{_name}}() {
                  Console.WriteLine("Hello, {{_name}}")
                }
            }
            """);
    }
}