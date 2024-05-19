class MyTemplate
{
    string[] groceries = new string[] { "Milk", "Eggs", "Diet Coke" };

    FormattableString Main() => $$"""
    I have to buy:
        {{groceries}}
    """;
}