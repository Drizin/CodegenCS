class MyTemplate
{
    FormattableString Main() => $$"""
        namespace MyNamespace
        {
            {{GenerateClass("MyFirstClass", "Rick")}}
        }
        """;

    // Subtemplates should ideally be a METHOD that returns the type you need.
    // (in this case the method returns another interpolated string)
    FormattableString GenerateClass(string className, string name) => $$"""
        public class {{className}}()
        {
            public {{className}}()
            {
            }
            public void Hello{{name}}()
            {
              Console.WriteLine("Hello, {{name}}")
            }
        }
        """;
}