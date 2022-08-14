using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Linq;
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

                using (Console.WithColor(ConsoleColor.DarkGray))
                {
                    Console.Write($"dotnet-codegencs.exe version {typeof(Program).Assembly.GetName().Version}");
                    Console.WriteLine($" (CodegenCS.dll version {typeof(CodegenCS.ICodegenContext).Assembly.GetName().Version})");
                }
                

                var parser = CliCommandParser.Instance;
                CliCommandParser.RunTemplate._verboseMode = (args?.Any(a=>a.ToLower() == "--verbose" || a.ToLower() == "--debug") ?? false);
                var parseResult = parser.Parse(args);

                bool verboseMode = (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--verbose"));
                if (verboseMode)
                    Console.WriteLine(ConsoleColor.DarkGray, "Verbose mode is on...");

                var invoke = parseResult.InvokeAsync();
                invoke.Wait();
                return invoke.Result;
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
