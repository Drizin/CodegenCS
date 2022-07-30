using System;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DbSchema.Extractor
{
    public class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var parser = CliCommand.Instance;
                var parseResult = parser.Parse(args);

                return parseResult.Invoke();
            }
            catch (Exception ex)
            {
                // Should never happen. Most exceptions during Invoke() should be handled by middleware ExceptionHandler
                Console.WriteLineError(ConsoleColor.Red, "Unhandled exception: " + ex.GetBaseException().ToString());
                return -1;
            }
        }
    }
}
