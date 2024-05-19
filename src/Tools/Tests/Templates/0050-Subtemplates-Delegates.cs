class MyTemplate
{
    FormattableString Main() => $$"""
        public class MyClass
        {
            {{Subtemplate1}}
            {{Subtemplate2}}
            {{Subtemplate3}}{{Subtemplate4}}
        }
        """;

    void Subtemplate1(ICodegenOutputFile writer)
    {
        // Write instead of WriteLine since the outer template already adds linebreak after this
        writer.Write("This goes to stdout, same stream started by Main()");
    }

    // Same effect as previous, but using explicit delegate (Func) instead of inferring from a regular void
    Func<ICodegenOutputFile, FormattableString> Subtemplate2 = (writer) =>
    {
        writer.WriteLine("This also goes to stdout, same stream started by Main()");
        return $"This will also go to stdout";
    };

    // Same effect, but returning a FormattableString directly so it goes directly into the same output stream
    Func<FormattableString> Subtemplate3 = () => $$"""
      This goes to stdout, same stream started by Main()
      """;

    void Subtemplate4(ICodegenContext context)
    {
        context["Class1.cs"].WriteLine($$"""
            // Class1 is a new stream (different file)
            {{Subtemplate5}}
            """);
    }

    // ICodegenOutputFile (stdout) will be "Class1.cs" since Subtemplate5 was embedded in Subtemplate4
    // (in other words ICodegenOutputFile depends on the current context)
    Action<ICodegenOutputFile> Subtemplate5 = (writer) => writer.Write($$"""
        public class Class1()
        {
            // ...
        }
        """);
}
