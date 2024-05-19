class MyTemplate
{
    async Task Main(ILogger logger, ICodegenOutputFile writer)
    {
        await logger.WriteLineAsync($"Generating MyTemplate...");
        writer.Write($"My first template");
    }
}
