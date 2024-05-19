class MyTemplate
{
    async Task<FormattableString> Main(ILogger logger)
    {
        await logger.WriteLineAsync($"Generating MyTemplate...");
        return $"My first template";
    }
}