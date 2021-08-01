using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
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

namespace CodegenCS.DotNetTool
{
    public static class CliCommandParser
    {
        public static readonly RootCommand RootCommand = new RootCommand();

        private static readonly Command SimplePOCOGeneratorCommand = SimplePOCOGenerator.CliCommand.GetCommand();
        private static readonly Command EFCoreGeneratorCommand = EFCoreGenerator.CliCommand.GetCommand();
        private static readonly Command DbSchemaExtractorCommand = CodegenCS.DbSchema.Extractor.CliCommand.GetCommand();
        private static readonly Command RunCommand = Commands.Run.RunCommand.GetCommand();

        private static readonly Option HelpOption = new Option(new[] { "--help", "-?" }, "Show Help"); // default lib will also track "-h" 
        private static readonly Option DebugOption = new Option(new[] { "--debug" }, "Debug mode"); // verbose output, detailed exceptions

        private static Command ConfigureCommandLine(Command rootCommand)
        {
            // Add subcommands
            rootCommand.AddCommand(SimplePOCOGeneratorCommand);
            rootCommand.AddCommand(EFCoreGeneratorCommand);
            rootCommand.AddCommand(DbSchemaExtractorCommand);
            rootCommand.AddCommand(RunCommand);

            // Add options
            rootCommand.AddGlobalOption(HelpOption);
            rootCommand.AddGlobalOption(DebugOption);

            return rootCommand;
        }

        public static System.CommandLine.Parsing.Parser Instance { get; } = new CommandLineBuilder(ConfigureCommandLine(RootCommand))
            .UseExceptionHandler(ExceptionHandler, 1)
            .UseVersionOption(1)

            .UseMiddleware(async (context, next) =>
            {
                // Manually intercept help, instead of using .UseHelp
                if (context.ParseResult.HasOption(HelpOption))
                {
                    context.ParseResult.ShowHelp(context.Console); // show help and short-circuit invocation pipeline
                    return;
                }
                await next(context);
            })

            .UseHelpBuilder(context => new MyHelpBuilder(context.Console)) // invoke will use the registered IHelpBuilder (doesn't use ParseResultExtensions.ShowHelp)
            //.UseHelp<MyHelpBuilder>(builder => { /*builder.Customize(option, descriptor: "-x (eXtreme)");*/ }) // we're capturing HelpOption on our own

            .UseSuggestDirective()
            .EnablePosixBundling(false)

            .UseVersionOption()
            .UseParseDirective()
            .UseDebugDirective()
            .UseSuggestDirective()
            .UseParseErrorReporting()

            .Build();

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
            if (context.ParseResult.HasOption(DebugOption))
                context.Console.Error.WriteLine("Unhandled exception: " + exception.ToString());
            else
                context.Console.Error.WriteLine("Unhandled exception: " + exception.Message);
            //context.ParseResult.ShowHelp();
            Console.ForegroundColor = previousColor;
            context.ExitCode = 999;
        }

        internal class MyHelpBuilder : HelpBuilder
        {
            //TODO: share this with (or copy to?) DbSchema.Extractor.CommandParser (used on standalone exe)?
            public MyHelpBuilder(IConsole console) : base(console, maxWidth: GetConsoleWidth()) 
            {
                var options = RootCommand.Children.OfType<Command>().Union(new List<Command>() { RootCommand }).SelectMany(c => c.Options).Where(o => !o.IsHidden);
                foreach (var o in options) // SimplePOCOGeneratorCommand.Options.Where(o => !o.IsHidden)
                {
                    //var d = GetArgumentDescriptor(((IOption)o).Argument);
                    var h = GetHelpItem(o);
                    string descriptor = h.Descriptor;
                    if (o.ValueType == typeof(bool) && o.Arity.MinimumNumberOfValues == 0)
                        descriptor += " [True|False]";
                    else if (o.ValueType == typeof(bool) && o.Arity.MinimumNumberOfValues == 1)
                        descriptor += " <True|False>";
                    else if (o.ValueType == typeof(string) && o.Arity.MinimumNumberOfValues == 0) // if this is optional, why doesn't help show "[name]"  instead of "<name>" ?
                        descriptor = descriptor.Replace("<" + o.ArgumentHelpName + ">", "[" + o.ArgumentHelpName + "]");

                    // Add small spacers between option groups
                    // It's possible to add line breaks to Option.Description, but the default value is added AFTER this linebreak, so it's more aesthetic to add line break here in descriptor (left column)
                    if (o.Description.EndsWith("\n"))
                    {
                        o.Description = o.Description.Substring(0, o.Description.Length - 1); // remove line break from right-side (specially because "[default: value]" would be appended AFTER linebreak)
                        descriptor += "\n"; // add to left side
                    }
                    

                    if (h.Descriptor != descriptor)
                        Customize(o, descriptor: () => descriptor);
                }

                // default descriptor for enum types will show all possible values (e.g. "<MSSQL|PostgreSQL>"), but I think it's better to show possible values in the description and leave descriptor with the 
                Customize(DbSchemaExtractorCommand.Arguments.Single(a => a.Name== "dbtype"), descriptor: () => "<dbtype>");
            }

            public MyHelpBuilder() : this(new SystemConsole())
            {
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

            //public override void Write(ICommand command) ... 
        }
    }
    public static class ParseResultExtensions
    {
        public static void ShowHelp(this ParseResult parseResult, IConsole console)
        {
            new MyHelpBuilder(console).Write(parseResult.CommandResult.Command);
        }
    }

}
