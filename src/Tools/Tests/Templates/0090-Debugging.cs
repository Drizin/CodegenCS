// If you run TemplateBuildTests this is the only unit test that should break into the debugger
// It's possible to break both inside MARKUP mode (interpolating a symbol) or in PROGRAMMATIC mode
class MyTemplate
{
    string[] groceries = new string[] { "Milk", "Eggs", "Diet Coke" };

    // If you run this template using Visual Studio Unit Tests 
    // it will break into the debugger when it reaches the BREAKIF(true)
    FormattableString Main() => $$"""
        I have to buy:
            {{BREAKIF(true)}}{{groceries}}
            Will write some more stuff
            {{MethodWithBreak}}
        """;

    // If you want to break from inside a method in your template (programmatic mode)
    // then you can just use System.Diagnostics.Debugger.Break() and make sure you
    // disable "Tools - Debugging - General - Enable Just My Code", in order to break and see the template source code (because it's invoked using reflection so it's considered external code)
    void MethodWithBreak(ICodegenOutputFile file)
    {
        file.WriteLine("writing something here");
        System.Diagnostics.Debugger.Break();
        file.WriteLine("last part");
    }
}