using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Text;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DbSchema.Extractor
{
    public static class CliCommand
    {
        public static Command GetCommand(string commandName = "extract-dbschema")
        {
            var command = new Command(commandName);
            //command.AddAlias("dbschema-extractor");

            command.AddArgument(new Argument<ExtractWizard.DbTypeEnum>("dbtype", description: $"Database type ({string.Join("|", Enum.GetNames(typeof(ExtractWizard.DbTypeEnum)))})", parse: argResult =>
            {
                try
                {
                    return (ExtractWizard.DbTypeEnum)Enum.Parse(typeof(ExtractWizard.DbTypeEnum), argResult.Tokens[0].Value, ignoreCase: true);
                }
                catch (Exception ex)
                {
                    argResult.ErrorMessage = "Invalid dbtype: " + argResult.Tokens[0].Value; 
                    throw;
                }
            })
            { Arity = ArgumentArity.ExactlyOne });

            command.AddArgument(new Argument<string>("connectionString", 
                description: $"Connection String\n" + 
                "MSSQL example:\"Server=MYWORKSTATION\\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True;\"\n" + 
                "MSSQL example:\"Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=myPassword;\"\n" + 
                "PostgreSQL example: \"Host=localhost; Database=Adventureworks; Username=postgres; Password=myPassword\"") { Arity = ArgumentArity.ExactlyOne });

            command.AddArgument(new Argument<string>("output", description: "Output JSON schema. E.g. \"Schema.json\"") { Arity = ArgumentArity.ExactlyOne });

            //TODO: There's a bug in command-line-api - if we call dotnet-codegencs.exe  extract-dbschema postgresql (missing 2 parameters), we get System.ArgumentOutOfRangeException

            command.Handler = CommandHandler.Create<ParseResult, DbSchemaExtractorArgs>(HandleCommand);

            return command;
        }

        static int HandleCommand(ParseResult parseResult, DbSchemaExtractorArgs cliArgs)
        {
            Console.WriteLine(ConsoleColor.Green, $"Executing '{typeof(ExtractWizard).Name}' template...");

            var wizard = new ExtractWizard();
            wizard.DbType = cliArgs.DbType;
            wizard.OutputJsonSchema = cliArgs.Output;
            wizard.ConnectionString = cliArgs.ConnectionString;

            wizard.Run(); // if mandatory args were not provided, will ask in Console

            Console.WriteLine(ConsoleColor.Green, $"Finished executing '{typeof(ExtractWizard).Name}' template.");

            return 0;
        }

        public class DbSchemaExtractorArgs
        {
            public ExtractWizard.DbTypeEnum DbType { get; set; }
            public string ConnectionString { get; set; }
            public string Output { get; set; }
        }

        #region Self-contained exe
        private static Command ConfigureCommandLine(Command command)
        {
            return command;
        }

        public static System.CommandLine.Parsing.Parser Instance { get; } = new CommandLineBuilder(ConfigureCommandLine(GetCommand(commandName: System.AppDomain.CurrentDomain.FriendlyName)))
            .UseExceptionHandler(ExceptionHandler, 1)
            .UseVersionOption()

            //.UseHelpBuilder(context => new MyHelpBuilder(context.Console)) 
            .UseHelp()

            .UseSuggestDirective()
            .EnablePosixBundling(false)

            .UseVersionOption()
            .UseParseDirective()
            //.UseDebugDirective()
            .UseSuggestDirective()
            .UseParseErrorReporting()

            .Build();

        private static void ExceptionHandler(Exception exception, InvocationContext context)
        {
            if (exception is TargetInvocationException)
            {
                exception = exception.InnerException;
            }
            Console.WriteLineError(ConsoleColor.Red, "Unhandled exception: " + exception.ToString());
            //context.ParseResult.ShowHelp();
            context.ExitCode = 999;
        }

        #endregion
    }
}
