using CodegenCS.___InternalInterfaces___;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;
using System.CommandLine.Parsing;

namespace CodegenCS.TemplateBuilder
{
    public class TemplateBuilder
    {
        protected FileInfo[] inputFiles;

        public TemplateBuilder()
        {
        }

        public class RunCommandArgs
        {
            public string[] Template { get; set; }
            public string Output { get; set; }
        }

        public int HandleCommand(ParseResult parseResult, RunCommandArgs cliArgs)
        {
            bool debugMode = (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--debug"));

            inputFiles = new FileInfo[cliArgs.Template.Length];
            for (int i=0; i < cliArgs.Template.Length; i++)
            {
                if (!((inputFiles[i] = new FileInfo(cliArgs.Template[i])).Exists || (inputFiles[i] = new FileInfo(cliArgs.Template[i] + ".cs")).Exists || (inputFiles[i] = new FileInfo(cliArgs.Template[i] + ".csx")).Exists))
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Cannot find find Template Script {cliArgs.Template}");
                    return -1;
                }
            }

            string outputFolder = Directory.GetCurrentDirectory();
            string outputFileName = Path.GetFileNameWithoutExtension(inputFiles[0].Name) + ".dll";
            if (!string.IsNullOrWhiteSpace(cliArgs.Output))
            {
                if (cliArgs.Output.Contains(Path.DirectorySeparatorChar) && cliArgs.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = Path.GetFullPath(cliArgs.Output);
                else if (cliArgs.Output.Contains(Path.DirectorySeparatorChar) && !cliArgs.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = new FileInfo(Path.GetFullPath(cliArgs.Output)).Directory.FullName;
                if (!cliArgs.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFileName = Path.GetFileNameWithoutExtension(new FileInfo(Path.GetFullPath(cliArgs.Output)).Name) + ".dll";
            }

            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping 'dotnet template build...'");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                    Console.ResetColor();
                };

                Console.WriteLine(ConsoleColor.Green, $"Building {ConsoleColor.Yellow}'{string.Join(", ", inputFiles.Select(inp => inp.Name))}'{PREVIOUS_COLOR}...");


                if (debugMode)
                    Console.WriteLine(ConsoleColor.DarkGray, $"{ConsoleColor.Cyan}Microsoft.CodeAnalysis.CSharp.dll{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{typeof(CSharpParseOptions).Assembly.GetName().Version}{PREVIOUS_COLOR}");

                var compiler = new RoslynCompiler();

                var sources = inputFiles.Select(inp => inp.FullName).ToArray();

                var targetFile = Path.Combine(outputFolder, outputFileName);

                bool success = compiler.Compile(sources, targetFile);

                if (!success)
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Error while building '{string.Join(", ", inputFiles.Select(inp => inp.Name))}'.");
                    return -1;
                }

                Console.WriteLine(ConsoleColor.Green, $"\nSuccessfully built template into {ConsoleColor.White}'{targetFile}'{PREVIOUS_COLOR}.");
                return 0;
            }

        }
       

    }
    
}
