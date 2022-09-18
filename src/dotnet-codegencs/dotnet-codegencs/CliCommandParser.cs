using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;
using System.CommandLine.Parsing;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    internal class CliCommandParser
    {
        private readonly Parser _parser;
        internal readonly RootCommand RootCommand = new RootCommand();

        private readonly Command _templateCommands = new Command("template");
        internal readonly Commands.TemplateRunCommand _runTemplateCommandWrapper;
        internal Command _runTemplateCommand => _runTemplateCommandWrapper._command;

        private readonly Command _buildTemplateCommand = Commands.TemplateBuildCommand.GetCommand();
        private readonly Command _cloneTemplateCommand = Commands.TemplateCloneCommand.GetCommand();

        private readonly Command _modelCommands = new Command("model");
        private readonly Command _modelDbSchemaCommands = new Command("dbschema");
        internal readonly Command _modelDbSchemaExtractCommand = DbSchema.Extractor.CliCommand.GetCommand();

        internal static readonly Option HelpOption = new Option<bool>(new[] { "--help", "-?", "/?", "/help", "--?" }, "\nShow Help"); // default lib will also track "-h" 
        internal static readonly Option VerboseOption = new Option<bool>(new[] { "--verbose", "--debug" }, getDefaultValue: () => false, "Verbose mode") { }; // verbose output, detailed exceptions

        internal CliCommandParser()
        {
            _runTemplateCommandWrapper = new Commands.TemplateRunCommand();
            _parser = BuildParser();
        }
        internal CliCommandParser(Commands.TemplateRunCommand runTemplateCommandWrapper)
        {
            _runTemplateCommandWrapper = runTemplateCommandWrapper;
            _parser = BuildParser();
        }


        private Command ConfigureCommandLine(Command rootCommand)
        {
            rootCommand.AddCommand(_templateCommands);
            _templateCommands.AddCommand(_runTemplateCommand);
            _templateCommands.AddCommand(_buildTemplateCommand);
            _templateCommands.AddCommand(_cloneTemplateCommand);

            _modelDbSchemaCommands.AddCommand(_modelDbSchemaExtractCommand);
            _modelCommands.AddCommand(_modelDbSchemaCommands);
            rootCommand.AddCommand(_modelCommands);

            // Add options
            rootCommand.AddGlobalOption(HelpOption);
            rootCommand.AddGlobalOption(VerboseOption);

            return rootCommand;
        }

        public Parser Parser => _parser;
        private Parser BuildParser()
        {
            return new CommandLineBuilder(ConfigureCommandLine(RootCommand))
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
                    if (context.ParseResult.CommandResult.Command == _runTemplateCommand && !context.ParseResult.Errors.Any())
                    {
                        var template = context.ParseResult.GetValueForArgument(_runTemplateCommandWrapper._templateArg);

                        var buildResult = await _runTemplateCommandWrapper.BuildScriptAsync(template);
                        if (buildResult != 0)
                        {
                            context.InvocationResult = new ErrorResult("Could not load or build template: " + template);
                            return;
                        }

                        var loadResult = await _runTemplateCommandWrapper.LoadTemplateAsync();
                        if (loadResult != 0)
                        {
                            context.InvocationResult = new ErrorResult("Could not load template: " + template);
                            return;
                        }
                        // Now that we have loaded the DLL we know the number of expected models.

                        // If the number of expected models doesn't match what was initially parsed then we'll parse again
                        // (e.g. something the parser thought was a model might actually be a custom arg that should be forwarded to the template, or the opposite)
                        if (_runTemplateCommandWrapper._expectedModels != _runTemplateCommandWrapper._initialParsedModels)
                        {
                            var parser = new Parser(context.ParseResult.RootCommandResult.Command);
                            var args = context.ParseResult.Tokens.Select(t => t.Value).ToArray();
                            context.ParseResult = parser.Parse(args);
                        }

                        // Parse() always goes through TemplateRunCommand.ParseModels(), but it never runs TemplateRunCommand.ParseTemplateArgs() unless we force it using GetValueForArgument
                        // GetValueForArgument gets the right results (forcing execution of ParseTemplateArgs that gets the right number of args based on the number of models), but context.ParseResult.CommandResult.Children will still be wrong
                        string[] templateSpecificArgValues = context.ParseResult.GetValueForArgument(_runTemplateCommandWrapper._templateSpecificArguments) ?? Array.Empty<string>();

                        _runTemplateCommandWrapper._launcher.HelpBuilderFactory = (BindingContext bindingContext) => new CliHelpBuilder(this, bindingContext);

                        _runTemplateCommandWrapper._launcher.ParseCliUsingCustomCommand = _runTemplateCommandWrapper.ParseCliUsingCustomCommand;
                    }

                    await next(context);
                })


                .AddMiddleware(async (context, next) =>
                {
                    // Manually intercept help, instead of using .UseHelp
                    if (context.ParseResult.HasOption(HelpOption))
                    {
                        if (context.ParseResult.CommandResult.Command == _runTemplateCommand)
                        {
                            // for template run we'll let Invoke() run, and we'll show help there since template may have custom args/options
                        }
                        else
                        {
                            context.ParseResult.ShowHelp(this, context.BindingContext); // show help and short-circuit invocation pipeline
                            return;
                        }
                    }
                    await next(context);
                })

                .UseHelpBuilder(context => new CliHelpBuilder(this, context)) // invoke will use the registered IHelpBuilder (doesn't use ParseResultExtensions.ShowHelp)
                                                                        //.UseHelpBuilder<MyHelpBuilder>((context, builder) => { /*builder.Customize(option, descriptor: "-x (eXtreme)");*/ }) // we're capturing HelpOption on our own

                .EnablePosixBundling(false)

                .UseVersionOption()
                .UseParseDirective()
                .UseSuggestDirective()
                .UseParseErrorReporting()

                .Build();
        }

        private void ExceptionHandler(Exception exception, InvocationContext context)
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

        
    }

}
