// Sample template that starts with markup (Main method returns an interpolated string)

// If you look in CSPROJ you'll see this file described as:
// <AdditionalFiles Include="Template2.csx" CodegenCSOutput="Memory" />
// CodegenCSOutput="Memory" means that the output of this file is just rendered into the compilation but it's not saved into a real file

class Template2
{
    void Main(ICodegenOutputFile writer)
    {
        writer.Write($$"""public class MySecondClass {}""");
    }
}
