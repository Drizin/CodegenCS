using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Reflection;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using static CodegenCS.DotNetTool.CliCommandParser;
using System.IO;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    public static class CliCommandParser
    {
        internal static readonly Commands.TemplateRunCommand RunTemplate = new Commands.TemplateRunCommand();
        public static readonly RootCommand RootCommand = new RootCommand();

        private static readonly Command _templateCommands = new Command("template");
        private static readonly Command BuildTemplateCommand = Commands.TemplateBuildCommand.GetCommand();
        private static readonly Command RunTemplateCommand = RunTemplate._command;
        private static readonly Command CloneTemplateCommand = Commands.TemplateCloneCommand.GetCommand();

        private static readonly Command _modelCommands = new Command("model");
        private static readonly Command _modelDbSchemaCommands = new Command("dbschema");
        private static readonly Command _modelDbSchemaExtractCommand = DbSchema.Extractor.CliCommand.GetCommand();

        internal static readonly Option HelpOption = new Option<bool>(new[] { "--help", "-?", "/?", "/help", "--?" }, "\nShow Help"); // default lib will also track "-h" 
        internal static readonly Option VerboseOption = new Option<bool>(new[] { "--verbose", "--debug" }, getDefaultValue: () => false, "Verbose mode") { }; // verbose output, detailed exceptions

        private static Command ConfigureCommandLine(Command rootCommand)
        {
            rootCommand.AddCommand(_templateCommands);
            _templateCommands.AddCommand(RunTemplateCommand);
            _templateCommands.AddCommand(BuildTemplateCommand);
            _templateCommands.AddCommand(CloneTemplateCommand);

            _modelDbSchemaCommands.AddCommand(_modelDbSchemaExtractCommand);
            _modelCommands.AddCommand(_modelDbSchemaCommands);
            rootCommand.AddCommand(_modelCommands);

            // Add options
            rootCommand.AddGlobalOption(HelpOption);
            rootCommand.AddGlobalOption(VerboseOption);

            return rootCommand;
        }

        public static System.CommandLine.Parsing.Parser Instance { get; } = new CommandLineBuilder(ConfigureCommandLine(RootCommand))
            .UseExceptionHandler(ExceptionHandler, 1)
            .UseVersionOption()

            .AddMiddleware(async (context, next) =>
            {
                // InvokeAsync middleware:
                // If the parsed command is "template run" (meaning that matched command have a template dll/script to invoke)
                // then we'll anticipate some actions from TemplateRunCommand.HandleCommand before it's invoked (next() step):
                // - If it's not a DLL we first build cs/csx into a dll
                // - We load the dll so we know the number of expected models.
                // - If required we'll parse command-line arguments again
                if (context.ParseResult.CommandResult.Command == CliCommandParser.RunTemplate._command && !context.ParseResult.Errors.Any())
                {
                    var template = context.ParseResult.GetValueForArgument(RunTemplate._templateArg);

                    var buildResult = await RunTemplate.BuildScriptAsync(template);
                    if (buildResult != 0)
                    {
                        context.InvocationResult = new ErrorResult("Could not load or build template: " + template);
                        return;
                    }

                    var loadResult = await RunTemplate.LoadTemplateAsync();
                    if (loadResult != 0)
                    {
                        context.InvocationResult = new ErrorResult("Could not load template: " + template);
                        return;
                    }
                    // Now that we have loaded the DLL we know the number of expected models.

                    // If the number of expected models doesn't match what was initially parsed then we'll parse again
                    // (e.g. something the parser thought was a model might actually be a custom arg that should be forwarded to the template, or the opposite)
                    if (RunTemplate._expectedModels != RunTemplate._initialParsedModels)
                    {
                        var parser = new Parser(context.ParseResult.RootCommandResult.Command);
                        var args = context.ParseResult.Tokens.Select(t=>t.Value).ToArray();
                        context.ParseResult = parser.Parse(args);                        
                    }

                    // Parse() always goes through TemplateRunCommand.ParseModels(), but it never runs TemplateRunCommand.ParseTemplateArgs() unless we force it using GetValueForArgument
                    // GetValueForArgument gets the right results (forcing execution of ParseTemplateArgs that gets the right number of args based on the number of models), but context.ParseResult.CommandResult.Children will still be wrong
                    string[] templateSpecificArgValues = context.ParseResult.GetValueForArgument(RunTemplate._templateSpecificArguments) ?? Array.Empty<string>();

                    CliCommandParser.RunTemplate._launcher.HelpBuilderFactory = (BindingContext bindingContext) => new MyHelpBuilder(bindingContext);

                    CliCommandParser.RunTemplate._launcher.ParseCliUsingCustomCommand = CliCommandParser.RunTemplate.ParseCliUsingCustomCommand;
                }

                await next(context);
            })


            .AddMiddleware(async (context, next) =>
            {
                // Manually intercept help, instead of using .UseHelp
                if (context.ParseResult.HasOption(HelpOption))
                {
                    if (context.ParseResult.CommandResult.Command == CliCommandParser.RunTemplate._command)
                    {
                        // for template run we'll let Invoke() run, and we'll show help there since template may have custom args/options
                    }
                    else
                    {
                        context.ParseResult.ShowHelp(context.BindingContext); // show help and short-circuit invocation pipeline
                        return;
                    }
                }
                await next(context);
            })

            .UseHelpBuilder(context => new MyHelpBuilder(context)) // invoke will use the registered IHelpBuilder (doesn't use ParseResultExtensions.ShowHelp)
            //.UseHelpBuilder<MyHelpBuilder>((context, builder) => { /*builder.Customize(option, descriptor: "-x (eXtreme)");*/ }) // we're capturing HelpOption on our own

            .EnablePosixBundling(false)

            .UseVersionOption()
            .UseParseDirective()
            .UseSuggestDirective()
            .UseParseErrorReporting()

            .Build();

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            if (context.ParseResult.HasOption(VerboseOption))
                Console.WriteLineError(ConsoleColor.Red, "Unhandled exception: " + exception.ToString());
            else
                Console.WriteLineError(ConsoleColor.Red, "Unhandled exception: " + exception.Message);
            //context.ParseResult.ShowHelp();
            context.ExitCode = 999;
        }

        internal class MyHelpBuilder : HelpBuilder
        {
            //TODO: share this HelpBuilder with DbSchema.Extractor
            public MyHelpBuilder(BindingContext context) : base(context.ParseResult.CommandResult.LocalizationResources, maxWidth: GetConsoleWidth()) 
            {
                var command = context.ParseResult.CommandResult.Command;
                //var commandsWithOptions = RootCommand.Children.OfType<Command>().Union(new List<Command>() { RootCommand }).Select(c => new { Command = c, Options = c.Options.Where(o => !o.IsHidden) }).Where(c => c.Options.Any());
                //foreach (var command in commandsWithOptions) // SimplePOCOGeneratorCommand.Options.Where(o => !o.IsHidden)
                {
                    //var helpContext = new HelpContext(this, command.Command, new StringWriter(), context.ParseResult);
                    var helpContext = new HelpContext(this, command, new StringWriter(), context.ParseResult);

                    while (command != null)
                    {
                        foreach (var o in command.Options)
                        {
                            string firstColumnText = Default.GetIdentifierSymbolUsageLabel(o, helpContext);
                            string secondColumnText = Default.GetIdentifierSymbolDescription(o);

                            if (o.ValueType == typeof(bool) && o.Arity.MinimumNumberOfValues == 0)
                                firstColumnText += " [True|False]";
                            else if (o.ValueType == typeof(bool) && o.Arity.MinimumNumberOfValues == 1)
                                firstColumnText += " <True|False>";
                            else if (o.ValueType == typeof(string) && o.Arity.MinimumNumberOfValues == 0 && firstColumnText.Contains("<" + o.ArgumentHelpName + ">")) // if this is optional, why doesn't help show "[name]"  instead of "<name>" ?
                                firstColumnText = firstColumnText.Replace("<" + o.ArgumentHelpName + ">", "[" + o.ArgumentHelpName + "]");

                            // Add small spacers between option groups - a linebreak added before or after the description (right side) should be copied (before or after) the descriptor (left side)
                            // This doesn't work with the standard HelpBuilder (since it's trimming the second column)
                            if (secondColumnText.EndsWith("\n") && !firstColumnText.EndsWith("\n"))
                                firstColumnText += "\n";
                            if (secondColumnText.StartsWith("\n") && !firstColumnText.StartsWith("\n"))
                                firstColumnText = "\n" + firstColumnText;

                            // "-p:OptionName <OptionName>" instead of "-p:OptionName <p:OptionName>"
                            if (o.Name.StartsWith("p:") && string.IsNullOrEmpty(o.ArgumentHelpName) && firstColumnText.Contains("<" + o.Name + ">"))
                            {
                                // we can't change o.ArgumentHelpName at this point.
                                firstColumnText = firstColumnText.Replace("<" + o.Name + ">", "<" + o.Name.Substring(2) + ">");
                            }

                            CustomizeSymbol(o, firstColumnText: ctx => firstColumnText, secondColumnText: ctx => secondColumnText.TrimEnd());

                        }
                        command = command.Parents.FirstOrDefault() as Command;
                    }
                }

                // default descriptor for enum types will show all possible values (e.g. "<MSSQL|PostgreSQL>"), but I think it's better to show possible values in the description and leave descriptor with the 
                CustomizeSymbol(_modelDbSchemaExtractCommand.Arguments.Single(a => a.Name== "dbtype"), firstColumnText: (ctx) => "<dbtype>");
            }

            private static int GetConsoleWidth()
            {
                int windowWidth;
                try
                {
                    windowWidth = System.Console.WindowWidth;
                }
                catch
                {
                    windowWidth = int.MaxValue;
                }
                return windowWidth;
            }

            public override string GetUsage(Command command)
            {
                var usage = base.GetUsage(command);

                // dotnet-codegencs template run? render a friendly format
                
                if (command == RunTemplateCommand)
                {
                    usage = usage.Replace(
                        "[<Models>... [<TemplateArgs>...]]",
                        "[Model [Model2]] [Template args]");
                }
                return usage;
            }
        }
    }
    public static class ParseResultExtensions
    {
        public static void ShowHelp(this ParseResult parseResult, BindingContext context)
        {
            new MyHelpBuilder(context).Write(parseResult.CommandResult.Command, context.Console.Out.CreateTextWriter());
        }
    }

    public class ErrorResult : IInvocationResult
    {
        private readonly string _errorMessage;
        private readonly int _errorExitCode;

        public ErrorResult(string errorMessage, int errorExitCode = 1)
        {
            _errorMessage = errorMessage;
            _errorExitCode = errorExitCode;
        }

        public void Apply(InvocationContext context)
        {
            //context.Console.Error.WriteLine(_errorMessage);
            Console.WriteLineError(ConsoleColor.Red, "ERROR: " + _errorMessage);
            context.ExitCode = _errorExitCode;
        }
    }

}
