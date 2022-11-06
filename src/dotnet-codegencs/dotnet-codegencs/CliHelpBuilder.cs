using System.CommandLine.Binding;
using System.CommandLine.Help;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.CommandLine.IO;
using System.CommandLine.Parsing;

namespace CodegenCS.DotNetTool
{
    internal class CliHelpBuilder : HelpBuilder
    {
        CliCommandParser _cliCommandParser;
        //TODO: share this HelpBuilder with standalone utilities (like DbSchema.Extractor) if we keep them as standalone
        public CliHelpBuilder(CliCommandParser cliCommandParser, BindingContext context) : base(context.ParseResult.CommandResult.LocalizationResources, maxWidth: GetConsoleWidth())
        {
            _cliCommandParser = cliCommandParser;
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
            CustomizeSymbol(cliCommandParser._modelDbSchemaExtractCommand.Arguments.Single(a => a.Name == "dbtype"), firstColumnText: (ctx) => "<dbtype>");
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

            if (command == _cliCommandParser._runTemplateCommand)
            {
                usage = usage.Replace(
                    "[<Models>... [<TemplateArgs>...]]",
                    "[Model [Model2]] [Template args]");
            }
            return usage;
        }
    }

    public static class ParseResultExtensions
    {
        internal static void ShowHelp(this ParseResult parseResult, CliCommandParser cliCommandParser, BindingContext context)
        {
            new CliHelpBuilder(cliCommandParser, context).Write(parseResult.CommandResult.Command, context.Console.Out.CreateTextWriter());
        }
    }

}
