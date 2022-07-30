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
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {

                Console.WriteLine(ConsoleColor.Blue, $"""
                       ______          __                      ___________
                      / ____/___  ____/ /__  ____ ____  ____  / ____/ ___/
                     / /   / __ \/ __  / _ \/ __ `/ _ \/ __ \/ /    \__ \ 
                    / /___/ /_/ / /_/ /  __/ /_/ /  __/ / / / /___ ___/ / 
                    \____/\____/\__,_/\___/\__, /\___/_/ /_/\____//____/  
                                          /____/                    
                    """);

                var parser = CliCommandParser.Instance;
                var parseResult = parser.Parse(args);

                return parseResult.Invoke(); //return CommandParser.RootCommand.Invoke(args);
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
