// Sample template that writes programmatically

class Template2
{
    void Main(ICodegenOutputFile writer)
    {
        writer.Write($$"""public class MySecondClass {}""");
    }
}
