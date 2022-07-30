using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimplePOCOGenerator = CodegenCS.DbSchema.Templates.SimplePOCOGenerator;
using EFCoreGenerator = CodegenCS.DbSchema.Templates.EFCoreGenerator;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using static CodegenCS.DotNetTool.CliCommandParser;
using System.IO;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    public static class CliCommandParser
    {
        public static readonly RootCommand RootCommand = new RootCommand();

        private static readonly Command BuildTemplateCommand = CodegenCS.TemplateBuilder.CliCommand.GetCommand();
        private static readonly Command RunTemplateCommand = CodegenCS.TemplateLauncher.CliCommand.GetCommand();
        private static readonly Command SimplePOCOGeneratorCommand = SimplePOCOGenerator.CliCommand.GetCommand();
        private static readonly Command EFCoreGeneratorCommand = EFCoreGenerator.CliCommand.GetCommand();
        private static readonly Command DbSchemaExtractorCommand = CodegenCS.DbSchema.Extractor.CliCommand.GetCommand();

        private static readonly Option HelpOption = new Option<bool>(new[] { "--help", "-?" }, "\nShow Help"); // default lib will also track "-h" 
        private static readonly Option VerboseOption = new Option<bool>(new[] { "--verbose", "--debug" }, getDefaultValue: () => false, "Verbose mode") { }; // verbose output, detailed exceptions

        private static Command ConfigureCommandLine(Command rootCommand)
        {
            var templateCommands = new Command("template");
            rootCommand.AddCommand(templateCommands);
            templateCommands.AddCommand(BuildTemplateCommand);
            templateCommands.AddCommand(RunTemplateCommand);

            rootCommand.AddCommand(SimplePOCOGeneratorCommand);
            rootCommand.AddCommand(EFCoreGeneratorCommand);
            rootCommand.AddCommand(DbSchemaExtractorCommand);

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
                // Manually intercept help, instead of using .UseHelp
                if (context.ParseResult.HasOption(HelpOption))
                {
                    context.ParseResult.ShowHelp(context.BindingContext); // show help and short-circuit invocation pipeline
                    return;
                }
                await next(context);
            })

            .UseHelpBuilder(context => new MyHelpBuilder(context)) // invoke will use the registered IHelpBuilder (doesn't use ParseResultExtensions.ShowHelp)
            //.UseHelpBuilder<MyHelpBuilder>((context, builder) => { /*builder.Customize(option, descriptor: "-x (eXtreme)");*/ }) // we're capturing HelpOption on our own

            .UseSuggestDirective()
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
            //TODO: share this with (or copy to?) DbSchema.Extractor.CommandParser (used on standalone exe)?
            public MyHelpBuilder(BindingContext context) : base(context.ParseResult.CommandResult.LocalizationResources, maxWidth: GetConsoleWidth()) 
            {
                var command = context.ParseResult.CommandResult.Command;
                //var commandsWithOptions = RootCommand.Children.OfType<Command>().Union(new List<Command>() { RootCommand }).Select(c => new { Command = c, Options = c.Options.Where(o => !o.IsHidden) }).Where(c => c.Options.Any());
                //foreach (var command in commandsWithOptions) // SimplePOCOGeneratorCommand.Options.Where(o => !o.IsHidden)
                {
                    //var helpContext = new HelpContext(this, command.Command, new StringWriter(), context.ParseResult);
                    var helpContext = new HelpContext(this, command, new StringWriter(), context.ParseResult);

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

                        //TODO: current HelpBuilder is trimming the second column - so this doesn't work anymore
                        // Add small spacers between option groups - a linebreak added before or after the description (right side) should be copied (before or after) the descriptor (left side)
                        //if (secondColumnText.EndsWith("\n") && !firstColumnText.EndsWith("\n"))
                        //    firstColumnText += "\n";
                        //if (secondColumnText.StartsWith("\n") && !firstColumnText.StartsWith("\n"))
                        //    firstColumnText = "\n" + firstColumnText;

                        CustomizeSymbol(o, firstColumnText: ctx => firstColumnText, secondColumnText: ctx => secondColumnText.TrimEnd());
                    }
                }

                // default descriptor for enum types will show all possible values (e.g. "<MSSQL|PostgreSQL>"), but I think it's better to show possible values in the description and leave descriptor with the 
                CustomizeSymbol(DbSchemaExtractorCommand.Arguments.Single(a => a.Name== "dbtype"), firstColumnText: (ctx) => "<dbtype>");
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
        }
    }
    public static class ParseResultExtensions
    {
        public static void ShowHelp(this ParseResult parseResult, BindingContext context)
        {
            new MyHelpBuilder(context).Write(parseResult.CommandResult.Command, context.Console.Out.CreateTextWriter());
        }
    }

}
