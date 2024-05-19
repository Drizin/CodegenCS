class MyTemplate
{
    string _ns = "MyNamespace";

    FormattableString Main() => $$"""
        namespace {{_ns}}
        {
            {{WriteClass}}
        }
        """;

    FormattableString WriteClass() => $$"""
        public class MyFirstClass {
            public void HelloWorld() {
                // ...
            }
        }
        """;
}
