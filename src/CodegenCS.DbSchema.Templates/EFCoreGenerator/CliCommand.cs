using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DbSchema.Templates.EFCoreGenerator
{
    public static class CliCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("efcoregenerator");

            command.AddArgument(new Argument<string>("input", description: "Input JSON schema. E.g. \"Schema.json\"") { Arity = ArgumentArity.ExactlyOne }); //TODO: validate HERE if file exists
            command.AddOption(new Option<string>(new[] { "--TargetFolder", "-t" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Folder" });
            command.AddOption(new Option<string>(new[] { "--Namespace", "-n" }, description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });
            command.AddOption(new Option<string>(new[] { "--DbContextName", "-c" }, description: "DbContext Class Name") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });

            command.Handler = CommandHandler.Create<ParseResult, EFCoreGeneratorArgs>(HandleCommand);

            return command;
        }

        static int HandleCommand(ParseResult parseResult, EFCoreGeneratorArgs cliArgs)
        {
            var options = cliArgs.ToOptions();
            EFCoreGeneratorConsoleHelper.GetOptions(options); // if mandatory args were not provided, ask in Console

            Console.WriteLine(ConsoleColor.Green, $"Executing '{typeof(EFCoreGenerator).Name}' template...");

            var generator = new EFCoreGenerator(options);
            generator.Generate();
            generator.AddCSX();
            generator.Save();

            using (Console.WithColor(ConsoleColor.Green))
            {
                Console.WriteLine($"Finished executing '{typeof(EFCoreGenerator).Name}' template.");
                Console.WriteLine($"A copy of the original template (with the options used) was saved as '{typeof(EFCoreGenerator).Name}.csx'.");
                Console.WriteLine($"To customize the template outputs you can modify the csx file and regenerate using 'codegencs run {typeof(EFCoreGenerator).Name}.csx'.");
            }
            return 0;
        }

        public class EFCoreGeneratorArgs
        {
            public string Input { get; set; }
            public string TargetFolder { get; set; }
            public string Namespace { get; set; }
            public string DbContextName { get; set; }

            public EFCoreGeneratorOptions ToOptions()
            {
                var options = new EFCoreGeneratorOptions(this.Input); // if not provided EFCoreGeneratorConsoleHelper.GetOptions will ask for it
                
                if (!string.IsNullOrEmpty(this.TargetFolder))
                    options.TargetFolder = this.TargetFolder;
                else
                    options.TargetFolder = Directory.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(this.Namespace))
                    options.EntitiesNamespace = this.Namespace;
                if (!string.IsNullOrEmpty(this.DbContextName))
                    options.ContextName = this.DbContextName;
                return options;
            }
        }
    }
}
