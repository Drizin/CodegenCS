class MyTemplate
{
    string _name = "Rick";

    FormattableString Main() => $$"""
        public class MyFirstClass {
            public void Hello{{_name}}() {
              Console.WriteLine("Hello, {{_name}}")
            }
        }
        """;
}
