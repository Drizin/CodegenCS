using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;
using CodegenCS.Runtime;

namespace CodegenCS.DotNetTool.Commands
{
    internal class TemplateBuildCommand
    {
        internal static Option<string[]> _referencesArg;
        internal static bool _verboseMode = false;
        public static Command GetCommand(string commandName = "build")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string[]>("template", description: "Template(s) to build. E.g. \"MyTemplate.cs\"") { Arity = ArgumentArity.OneOrMore });

            _referencesArg = new Option<string[]>(new[] { "--reference", "-r" }, parseArgument: ParseAssemblyReferences, description:
                """
                Add dll references
                Can use full path or relative path.
                Relative paths will first search relative to current location,
                and if not found will look relative to dotnet core libraries location
                (e.g. C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.5)
                Examples: -r:System.Xml.dll
                Examples: -r:\"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.5\System.Xml.dll\"
                """
            )
            { Arity = ArgumentArity.ZeroOrMore, ArgumentHelpName = "dll_reference" };
            command.AddOption(_referencesArg);


            command.AddOption(new Option<string>(new[] { "--output", "-o" }, description:
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
            { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "output_name" });

            command.Handler = CommandHandler.Create<ParseResult, CommandArgs>(HandleCommand);

            return command;
        }

        static string[] ParseAssemblyReferences(ArgumentResult result)
        {
            var references = result.Tokens.Select(t => t.Value).ToArray();
            if (_verboseMode)
                Console.WriteLine(ConsoleColor.DarkGray, $"[DEBUG] AssemblyReferences: {ConsoleColor.Yellow}'{String.Join("', '", references)}'{PREVIOUS_COLOR}");
            return references;
        }

        protected static async Task<int> HandleCommand(ParseResult parseResult, CommandArgs cliArgs)
        {
            // Forward Global Options to the Command Options
            _verboseMode = (parseResult.HasOption(CliCommandParser.VerboseOption));

            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping {ConsoleColor.Yellow}'dotnet template build'{PREVIOUS_COLOR}...");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };
                var references = parseResult.GetValueForOption(_referencesArg);

                var args = new TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
                {
                    Template = cliArgs.Template,
                    Output = cliArgs.Output,
                    VerboseMode = _verboseMode,
                    ExtraReferences = references.ToList()
                };
                var builder = new TemplateBuilder.TemplateBuilder(new ColoredConsoleLogger(), args);

                var builderResult = await builder.ExecuteAsync();
                return builderResult.ReturnCode;
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
