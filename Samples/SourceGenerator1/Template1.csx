// Sample template that starts with markup (Main method returns an interpolated string)

// If you look in CSPROJ you'll see this file described as:
// <AdditionalFiles Include="Template1.csx" CodegenCSOutput="File" />
// CodegenCSOutput="File" means that the output of this file is saved into a real file

class Template1
{
    FormattableString Main() => $$"""public class MyFirstClass {}""";
}