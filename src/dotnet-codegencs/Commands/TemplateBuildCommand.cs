using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;

namespace CodegenCS.DotNetTool.Commands
{
    internal class TemplateBuildCommand
    {
        public static Command GetCommand(string commandName = "build")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string[]>("Template", description: "Template(s) to build. E.g. \"MyTemplate.cs\"") { Arity = ArgumentArity.OneOrMore });

            command.AddOption(new Option<string>(new[] { "--Output", "-o" }, description:
                """
                Folder and/or filename to save output dll
                If folder is not provided then dll is saved in current folder
                If filename is not provided then dll is named like the input template file 
                   (e.g. MyTemplate.cs will be compiled into MyTemplate.dll)
                Examples: "..\Templates\MyCodeGenerator.dll" (specify a folder AND a file name)
                Examples: "..\Templates\"                    (specify only folder)
                Examples: "MyCodeGenerator.dll"              (specify only filename)
                """
            )
            { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Output" });

            command.Handler = CommandHandler.Create<ParseResult, CommandArgs>(HandleCommand);

            return command;
        }


        protected static async Task<int> HandleCommand(ParseResult parseResult, CommandArgs cliArgs)
        {
            // Forward Global Options to the Command Options
            bool verboseMode = (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--verbose"));

            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping {ConsoleColor.Yellow}'dotnet template build'{PREVIOUS_COLOR}...");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };

                var args = new TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
                {
                    Template = cliArgs.Template,
                    Output = cliArgs.Output,
                    VerboseMode = verboseMode
                };
                var builder = new TemplateBuilder.TemplateBuilder(new ColoredConsoleLogger(), args);

                return await builder.ExecuteAsync();
            }
        }

        protected class CommandArgs
        {
            /// <see cref="TemplateBuilder.TemplateBuilder.TemplateBuilderArgs.Template"/>
            public string[] Template { get; set; }

            /// <see cref="TemplateBuilder.TemplateBuilder.TemplateBuilderArgs.Output"/>
            public string Output { get; set; }
        }

    }
}
