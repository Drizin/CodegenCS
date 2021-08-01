using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimplePOCOGenerator = CodegenCS.DbSchema.Templates.SimplePOCOGenerator;
using EFCoreGenerator = CodegenCS.DbSchema.Templates.EFCoreGenerator;
using System.CommandLine;
using System.CommandLine.Parsing;

namespace CodegenCS.DotNetTool
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                var parser = CliCommandParser.Instance;
                var parseResult = parser.Parse(args);

                return parseResult.Invoke(); //return CommandParser.RootCommand.Invoke(args);
            }
            catch (Exception ex)
            {
                // Should never happen. Most exceptions during Invoke() should be handled by middleware ExceptionHandler
                var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unhandled exception: "  + ex.GetBaseException().ToString());
                Console.ForegroundColor = previousColor;
                return -1;
            }
        }

    }
}
