class MyTemplate
{
    Task Main(ICodegenOutputFile writer)
    {
        writer.Write($"My first template");
        return Task.CompletedTask;
    }
}
