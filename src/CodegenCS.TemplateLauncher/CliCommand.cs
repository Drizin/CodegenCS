using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace CodegenCS.TemplateLauncher
{
    public static class CliCommand
    {
        public static Command GetCommand(string commandName = "run")
        {
            var command = new Command(commandName);

            command.AddArgument(new Argument<string>("Template", description: "Template dll to run. E.g. \"MyTemplate.dll\" or \"MyTemplate\"") { Arity = ArgumentArity.ExactlyOne });

            command.AddArgument(new Argument<string[]>("Models", description: "Input Model(s) E.g. \"DbSchema.json\", \"ApiEndpoints.yaml\", etc. Templates might expect 0, 1 or 2 models") { Arity = new ArgumentArity(0, 2) });

            command.AddOption(new Option<string>(new[] { "--OutputFolder", "-o" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "OutputFolder" });
            command.AddOption(new Option<string>(new[] { "--File", "-f" }, description: "Default Output File [default: \"{Template}.generated.cs\"]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "DefaultOutputFile" });

            command.Handler = CommandHandler.Create<ParseResult, TemplateLauncher.RunCommandArgs>(new TemplateLauncher().HandleCommand);

            return command;
        }
    }
}
