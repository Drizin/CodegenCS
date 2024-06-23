// The following #r references will be parsed by RoslynCompiler and the referenced assemblies will be automatically added to the compilation
// Those assemblies can be absolute paths, can be relative to the template, or (most common) can just be looked up inside dotnet assemblies folder
#r "System.Xml.dll"
#r "System.Xml.ReaderWriter.dll"
#r "System.Private.Xml.dll"
using System.IO;
using System.Xml;
using System;

class MyTemplate
{
    async Task<FormattableString> Main(ILogger logger)
    {
        XmlDocument doc = new XmlDocument();
        await logger.WriteLineAsync($"Generating MyTemplate...");
        return $"My first template";
    }
}
