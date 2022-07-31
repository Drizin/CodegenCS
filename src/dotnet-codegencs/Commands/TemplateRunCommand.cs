using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;

namespace CodegenCS.DotNetTool.Commands
{
    internal class TemplateRunCommand
    {
        public static Command GetCommand(string commandName = "run")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string>("Template", description: "Template to run. E.g. \"MyTemplate.dll\" or \"MyTemplate.cs\" or \"MyTemplate\"") { Arity = ArgumentArity.ExactlyOne });

            command.AddArgument(new Argument<string[]>("Models", description: "Input Model(s) E.g. \"DbSchema.json\", \"ApiEndpoints.yaml\", etc. Templates might expect 0, 1 or 2 models") { Arity = new ArgumentArity(0, 2) });

            command.AddOption(new Option<string>(new[] { "--OutputFolder", "-o" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "OutputFolder" });
            command.AddOption(new Option<string>(new[] { "--File", "-f" }, description: "Default Output File [default: \"{MyTemplate}.generated.cs\"]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "DefaultOutputFile" });

            command.Handler = CommandHandler.Create<ParseResult, CommandArgs>(HandleCommand);

            return command;
        }


        protected static async Task<int> HandleCommand(ParseResult parseResult, CommandArgs cliArgs)
        {
            bool verboseMode = (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--verbose"));

            string currentCommand = "dotnet-codegencs template run";
            int statusCode;
            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping '{currentCommand}...'");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };

                var logger = new ColoredConsoleLogger();

                string[] validExtensions = new string[] { ".dll", ".cs", ".csx" };
                FileInfo templateFile = File.Exists(cliArgs.Template) ? new FileInfo(cliArgs.Template) : null;
                for (int i = 0; templateFile == null && i < validExtensions.Length; i++)
                {
                    if (File.Exists(cliArgs.Template + validExtensions[i]))
                        templateFile = new FileInfo(cliArgs.Template + validExtensions[i]);
                }

                if (templateFile == null)
                {
                    if (validExtensions.Contains(Path.GetExtension(cliArgs.Template).ToLower()))
                        Console.WriteLineError(ConsoleColor.Red, $"Cannot find Template {ConsoleColor.Yellow}'{cliArgs.Template}'{PREVIOUS_COLOR}");
                    else
                        Console.WriteLineError(ConsoleColor.Red, $"Cannot find Template {ConsoleColor.Yellow}'{cliArgs.Template}.dll'{PREVIOUS_COLOR} or {ConsoleColor.Yellow}'{cliArgs.Template}.cs'{PREVIOUS_COLOR} or {ConsoleColor.Yellow}'{cliArgs.Template}.csx'{PREVIOUS_COLOR}");
                    return -1;
                }
                else if (!validExtensions.Contains(Path.GetExtension(templateFile.FullName).ToLower()))
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Invalid Template name {ConsoleColor.Yellow}'{templateFile.Name}'{PREVIOUS_COLOR}");
                    Console.WriteLineError(ConsoleColor.Red, $"Valid template extensions are {ConsoleColor.Yellow}{string.Join(", ", validExtensions.Select(ext =>ext.TrimStart('.')))}{PREVIOUS_COLOR}");
                    return -1;
                }

                // If user provided a single CS/CSX file (instead of a DLL file) first we need to build into a dll
                if (Path.GetExtension(cliArgs.Template).ToLower() != ".dll")
                {
                    currentCommand = "dotnet-codegencs template build";

                    var builderArgs = new TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
                    {
                        Template = new string[] { templateFile.FullName },
                        //TODO define folder+filename by hash of template name+contents, to cache results.
                        Output = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(templateFile.FullName)) + ".dll",
                        VerboseMode = verboseMode,
                    };
                    var builder = new TemplateBuilder.TemplateBuilder(logger, builderArgs);

                    statusCode = await builder.ExecuteAsync();

                    if (statusCode != 0)
                    {
                        Console.WriteLineError(ConsoleColor.Red, $"TemplateBuilder ({currentCommand}) Failed.");
                        return -1;
                    }
                    currentCommand = "dotnet-codegencs template run";
                    templateFile = new FileInfo(builderArgs.Output);
                }

                // now we have the DLL to run...
                var launcherArgs = new TemplateLauncher.TemplateLauncher.TemplateLauncherArgs()
                {
                    Template = templateFile.FullName,
                    Models = cliArgs.Models,
                    OutputFolder = cliArgs.OutputFolder,
                    DefaultOutputFile = cliArgs.DefaultOutputFile,
                    VerboseMode = verboseMode
                };


                var ctx = new CodegenContext(); // var ctx = new DotNet.DotNetCodegenContext(); if user passes csproj file, etc.
                var launcher = new TemplateLauncher.TemplateLauncher(logger, new CodegenContext(), launcherArgs);

                statusCode = await launcher.ExecuteAsync();

                if (statusCode != 0)
                {
                    Console.WriteLineError(ConsoleColor.Red, $"TemplateLauncher ({currentCommand}) Failed.");
                    return -1;
                }

                return 0;
            }
        }

        protected class CommandArgs
        {
            /// <see cref="TemplateLauncher.TemplateLauncher.TemplateLauncherArgs.Template"/>
            public string Template { get; set; }

            /// <see cref="TemplateLauncher.TemplateLauncher.TemplateLauncherArgs.Models"/>
            public string[] Models { get; set; }

            /// <see cref="TemplateLauncher.TemplateLauncher.TemplateLauncherArgs.OutputFolder"/>
            public string OutputFolder { get; set; }


            /// <see cref="TemplateLauncher.TemplateLauncher.TemplateLauncherArgs.DefaultOutputFile"/>
            public string DefaultOutputFile { get; set; }

        }

    }
}
