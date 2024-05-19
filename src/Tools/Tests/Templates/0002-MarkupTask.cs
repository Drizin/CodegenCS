class MyTemplate
{
    Task<FormattableString> Main()
    {
        return Task.FromResult((FormattableString)$"My first template");
    }
}