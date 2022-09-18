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
    internal class TemplateCloneCommand
    {
        public static Command GetCommand(string commandName = "clone")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string>("origin", description: "Template to download (clone). " 
                + "\nE.g. \"github.com/CodegenCS/Templates/SimplePocos/SimplePocos.cs\""
                + "\nor even simpler: \"SimplePocos/SimplePocos.cs\" or just \"SimplePocos\"") 
            { Arity = ArgumentArity.ExactlyOne });

            command.AddOption(new Option<string>(new[] { "--output", "-o" }, description:
                """
                Folder and/or filename to save output file
                If folder is not provided then file is saved in current folder
                If filename is not provided then file is named like the origin template file 
                Examples: "..\Templates\MyCodeGenerator.cs" (specify a folder AND a file name)
                Examples: "..\Templates\"                    (specify only folder)
                Examples: "MyCodeGenerator.cs"              (specify only filename)
                """
            )
            { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "Output" });

            command.AddOption(new Option<bool>(new[] { "--allow-untrusted-origin"}, description: "Allow downloading templates from untrusted origins.") { Arity = ArgumentArity.ZeroOrOne, IsHidden = true });

            command.Handler = CommandHandler.Create<ParseResult, CommandArgs>(HandleCommand);

            return command;
        }


        protected static async Task<int> HandleCommand(ParseResult parseResult, CommandArgs cliArgs)
        {
            // Forward Global Options to the Command Options
            bool verboseMode = (parseResult.HasOption(CliCommandParser.VerboseOption));

            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping {ConsoleColor.Yellow}'dotnet template build'{PREVIOUS_COLOR}...");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };

                var logger = new ColoredConsoleLogger();

                var downloadArgs = new TemplateDownloader.TemplateDownloader.TemplateDownloaderArgs()
                {
                    Origin = cliArgs.Origin,
                    Output = cliArgs.Output,
                    AllowUntrustedOrigin = cliArgs.AllowUntrustedOrigin,
                    VerboseMode = verboseMode
                };
                var downloader = new TemplateDownloader.TemplateDownloader(logger, downloadArgs);

                var downloadResult = await downloader.ExecuteAsync();

                if (downloadResult.ReturnCode != 0)
                    return downloadResult.ReturnCode;


                var buildArgs = new TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
                {
                    Template = new string[] { downloadResult.DownloadedTemplate },
                    VerboseMode = verboseMode
                };
                var builder = new TemplateBuilder.TemplateBuilder(logger, buildArgs);

                var builderResult = await builder.ExecuteAsync();

                if (builderResult.ReturnCode != 0)
                    return builderResult.ReturnCode;


                var launcher = new TemplateLauncher.TemplateLauncher(logger, new CodegenContext(), verboseMode: false); //TODO: silent mode
                var loadResult = await launcher.LoadAsync(builderResult.TargetFile);

                if (loadResult.ReturnCode != 0)
                    return loadResult.ReturnCode;
                string modelsUsage = "";
                if (loadResult.Model1Type != null)
                    modelsUsage += $" <{loadResult.Model1Type.Name}Model>";
                if (loadResult.Model2Type != null)
                    modelsUsage += $" <{loadResult.Model2Type.Name}Model>";

                if (loadResult.Model1Type == typeof(CodegenCS.DbSchema.DatabaseSchema))
                {
                    await logger.WriteLineAsync(ConsoleColor.Cyan, $"To generate a {ConsoleColor.Yellow}{loadResult.Model1Type.Name} model{PREVIOUS_COLOR} use: '{ConsoleColor.White}dotnet-codegencs model dbschema extract <MSSQL|PostgreSQL> <connectionString> <output>{PREVIOUS_COLOR}'");
                    await logger.WriteLineAsync(ConsoleColor.Cyan, $"For help: '{ConsoleColor.White}dotnet-codegencs model dbschema extract /?{PREVIOUS_COLOR}'");
                    await logger.WriteLineAsync(ConsoleColor.Cyan, $"For a sample schema please check out: '{ConsoleColor.White}https://github.com/CodegenCS/CodegenCS/blob/master/src/Models/CodegenCS.DbSchema.SampleDatabases/AdventureWorksSchema.json{PREVIOUS_COLOR}'");
                }

                await logger.WriteLineAsync(ConsoleColor.Cyan, $"To run this template use: '{ConsoleColor.White}dotnet-codegencs template run {ConsoleColor.Yellow}{builderResult.TargetFile}{PREVIOUS_COLOR}{modelsUsage}{PREVIOUS_COLOR}'");
                await logger.WriteLineAsync(ConsoleColor.Cyan, $"For help: '{ConsoleColor.White}dotnet-codegencs template run /?{PREVIOUS_COLOR}'");


                return 0;
            }
        }

        protected class CommandArgs
        {
            /// <see cref="TemplateDownloader.TemplateDownloader.TemplateDownloaderArgs.Origin"/>
            public string Origin { get; set; }

            /// <see cref="TemplateDownloader.TemplateDownloader.TemplateDownloaderArgs.Output"/>
            public string Output { get; set; }

            public bool AllowUntrustedOrigin { get; set; } = false;
        }

    }
}
