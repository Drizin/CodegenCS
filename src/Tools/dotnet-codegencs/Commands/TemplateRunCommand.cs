using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;
using System.CommandLine.Invocation;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System.Reflection;
using CodegenCS.Runtime;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;

namespace CodegenCS.DotNetTool.Commands
{
    internal class TemplateRunCommand
    {
        internal readonly Argument<string> _templateArg;
        internal readonly Argument<string[]> _modelsArg;
        internal readonly Argument<string[]> _templateSpecificArguments;

        internal bool _verboseMode = false;
        private bool _showTemplateHelp = false;
        private readonly ILogger _logger;
        private FileInfo _templateFile = null;
        private FileInfo _originallyInvokedTemplateFile;
        internal TemplateLauncher.TemplateLauncher _launcher = null;
        private CodegenContext _ctx = new CodegenContext(); // _ctx = new DotNet.DotNetCodegenContext(); if user passes csproj file, etc.
        internal int? _expectedModels = null;
        internal int? _initialParsedModels = null;
        internal Command _command;
        internal int? buildResult = null;
        internal int? loadResult = null;
        DependencyContainer _dependencyContainer;


        public TemplateRunCommand(Command fakeTemplateCommand = null)
        {
            _templateArg = new Argument<string>("template", description: "Template to run. E.g. \"MyTemplate.dll\" or \"MyTemplate.cs\" or \"MyTemplate\"") { Arity = ArgumentArity.ExactlyOne };
            _modelsArg = new Argument<string[]>("models", description: "Input Model(s) E.g. \"DbSchema.json\", \"ApiEndpoints.yaml\", etc. Templates might expect 0, 1 or 2 models", parse: ParseModels)
            {
                //Arity = new ArgumentArity(0, 2)
                // If we have limited arity (e.g. 0 to 2 args) using OnlyTake() when parsing this argument (Models) won't work:
                // even if we OnlyTake(1) the next argument (TemplateArgs parsed by ParseTemplateArgs()) will still miss one token (ParseResultVisitor misses the PassedOver arguments)

                // So we have to be unlimited:
                Arity = ArgumentArity.ZeroOrMore
            };
            _templateSpecificArguments = new Argument<string[]>("TemplateArgs", parse: ParseTemplateArgs)
            {
                Description = "Template-specific arguments/options (if template requires/accepts it)",
                Arity = new ArgumentArity(0, 999), 
                //Arity = ArgumentArity.ZeroOrMore,
                HelpName ="template_args"
            };
            _logger = new ColoredConsoleLogger();
            
            if (fakeTemplateCommand == null)
                _command = GetCommand();
            else
            {
                _command = GetFakeRunCommand();
                _command.AddCommand(fakeTemplateCommand);
            }
            _dependencyContainer = new DependencyContainer().AddConsole().AddModelFactory();
        }

        public Command GetCommand(string commandName = "run")
        {
            var command = new Command(commandName);

            AddGlobalOptions(command);

            command.AddArgument(_templateArg);

            command.AddArgument(_modelsArg);

            command.AddArgument(_templateSpecificArguments);

            command.Handler = CommandHandler.Create<InvocationContext, ParseResult, CommandArgs>(HandleCommand);
            // a Custom Binder (inheriting from BinderBase<CommandArgs>) could be used to create CommandArgs (defining which arguments are Models and which ones are TemplateArgs):
            //command.SetHandler((args) => HandleCommand(args), new CustomBinder());

            return command;
        }
        protected void AddGlobalOptions(Command command)
        {
            command.AddGlobalOption(new Option<string>(new[] { "--OutputFolder", "-o" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "OutputFolder" });
            command.AddGlobalOption(new Option<string>(new[] { "--File", "-f" }, description: "Default Output File [default: \"{MyTemplate}.generated.cs\"]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "DefaultOutputFile" });
        }
        protected internal Command GetFakeRunCommand()
        {
            var command = new Command("run");
            AddGlobalOptions(command);
            command.IsHidden = true;
            return command;
        }

        string[] ParseModels(ArgumentResult result)
        {
            string[] models = new string[_expectedModels ?? 2];
            int foundModels = 0;
            int maxModels = Math.Min(Math.Min(result.Tokens.Count, 2), _expectedModels ?? 2);
            for (int i = 0; i < maxModels; i++)
            {
                FileInfo fi;
                if ((fi = new FileInfo(result.Tokens[i].Value)).Exists || (fi = new FileInfo(result.Tokens[i].Value + ".json")).Exists || (fi = new FileInfo(result.Tokens[i].Value + ".yaml")).Exists)
                {
                    foundModels++;
                    models[i] = fi.FullName;
                }
                else if (_expectedModels != null) // if we already know the number of expected models we can be strict, else we should be lenient and ignore non-models
                {
                    result.ErrorMessage = "ERROR: Cannot find model: " + result.Tokens[i].Value; // automatically handled UseParseErrorReporting() middleware
                    return null;
                }
            }
            _initialParsedModels ??= foundModels;
            if (_verboseMode)
                Console.WriteLine(ConsoleColor.DarkGray, "[DEBUG] Models: " + (models.ToList().Take(foundModels).Any() ? String.Join(", ", models.ToList().Take(foundModels)) : "<none>"));
            if (result.Tokens.Count != foundModels)
                result.OnlyTake(foundModels);
            if (foundModels == models.Length)
                return models;
            return models.ToList().Take(foundModels).ToArray();
        }

        string[] ParseTemplateArgs(ArgumentResult result)
        {
            // Since Models arg is unlimited (ArgumentArity.ZeroOrMore) this subsequent argument will get the same arguments, and we have to skip the number of tokens which were matched to models
            int models = result.Parent.GetValueForArgument(_modelsArg)?.Length ?? 0;
            var arr = result.Tokens.Skip(models).Select(t => t.Value).ToArray();
            if (_verboseMode && arr.Any())
                Console.WriteLine(ConsoleColor.DarkGray, $"[DEBUG] TemplateArgs: {ConsoleColor.Yellow}'{String.Join("', '", arr)}'{PREVIOUS_COLOR}");
            else
                Console.WriteLine(ConsoleColor.DarkGray, $"[DEBUG] TemplateArgs: <none>");
            return arr;
        }


        /// <summary>
        /// If template is not yet built (DLL), builds CS or CSX into a DLL
        /// </summary>
        public async Task<int> BuildScriptAsync(string template)
        {
            string currentCommand = "dotnet-codegencs template run";
            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping {ConsoleColor.Yellow}'{currentCommand}'{PREVIOUS_COLOR}...");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };
            }

            string[] validExtensions = new string[] { ".dll", ".cs", ".csx" };
            _templateFile = File.Exists(template) ? new FileInfo(template) : null;
            for (int i = 0; _templateFile == null && i < validExtensions.Length; i++)
            {
                if (File.Exists(template + validExtensions[i]))
                    _templateFile = new FileInfo(template + validExtensions[i]);
            }

            if (_templateFile == null)
            {
                if (validExtensions.Contains(Path.GetExtension(template).ToLower()))
                    Console.WriteLineError(ConsoleColor.Red, $"Cannot find Template {ConsoleColor.Yellow}'{template}'{PREVIOUS_COLOR}");
                else
                    Console.WriteLineError(ConsoleColor.Red, $"Cannot find Template {ConsoleColor.Yellow}'{template}.dll'{PREVIOUS_COLOR} or {ConsoleColor.Yellow}'{template}.cs'{PREVIOUS_COLOR} or {ConsoleColor.Yellow}'{template}.csx'{PREVIOUS_COLOR}");
                return -1;
            }
            else if (!validExtensions.Contains(Path.GetExtension(_templateFile.FullName).ToLower()))
            {
                Console.WriteLineError(ConsoleColor.Red, $"Invalid Template name {ConsoleColor.Yellow}'{_templateFile.Name}'{PREVIOUS_COLOR}");
                Console.WriteLineError(ConsoleColor.Red, $"Valid template extensions are {ConsoleColor.Yellow}{string.Join(", ", validExtensions.Select(ext => ext.TrimStart('.')))}{PREVIOUS_COLOR}");
                return -1;
            }

            // If user provided a single CS/CSX file (instead of a DLL file) first we need to build into a dll
            if (Path.GetExtension(template).ToLower() != ".dll")
            {
                currentCommand = "dotnet-codegencs template build";
                string tmpFolder = Path.Combine(Path.GetTempPath() ?? Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
                var builderArgs = new TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
                {
                    Template = new string[] { _templateFile.FullName },
                    //TODO define folder+filename by hash of template name+contents, to cache results.
                    Output = Path.Combine(tmpFolder, Path.GetFileNameWithoutExtension(_templateFile.FullName)) + ".dll",
                    VerboseMode = _verboseMode,
                };
                var builder = new TemplateBuilder.TemplateBuilder(_logger, builderArgs);

                var builderResult = await builder.ExecuteAsync();

                if (builderResult.ReturnCode != 0)
                {
                    Console.WriteLineError(ConsoleColor.Red, $"TemplateBuilder ({ConsoleColor.Yellow}'{currentCommand}'{PREVIOUS_COLOR}) Failed.");
                    return -1;
                }
                currentCommand = "dotnet-codegencs template run";
                _originallyInvokedTemplateFile = _templateFile;
                _templateFile = new FileInfo(builderArgs.Output);
            }
            return 0;
        }

        public async Task<int> LoadTemplateAsync()
        {
            _launcher ??= new TemplateLauncher.TemplateLauncher(_logger, _ctx, _dependencyContainer, _verboseMode) { _originallyInvokedTemplateFile = _originallyInvokedTemplateFile };
            var loadResult = await _launcher.LoadAsync(_templateFile.FullName);
            _expectedModels = (loadResult.Model1Type != null ? 1 : 0) + (loadResult.Model2Type != null ? 1 : 0);
            return loadResult.ReturnCode;
        }

        protected async Task<int> HandleCommand(InvocationContext context, ParseResult parseResult, CommandArgs cliArgs)
        {
            _verboseMode |= (parseResult.HasOption(CliCommandParser.VerboseOption));
            _showTemplateHelp |= (parseResult.HasOption(CliCommandParser.HelpOption));

            string currentCommand = "dotnet-codegencs template run";
            int statusCode;
            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping {ConsoleColor.Yellow}'{currentCommand}'{PREVIOUS_COLOR}...");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };


                if (_launcher == null) // is this possible? arriving here without LoadTemplateAsync
                {
                    _launcher ??= new TemplateLauncher.TemplateLauncher(_logger, _ctx, _dependencyContainer, _verboseMode) { _originallyInvokedTemplateFile = _originallyInvokedTemplateFile};
                    var loadResult = await _launcher.LoadAsync(_templateFile.FullName);
                    return loadResult.ReturnCode;
                }

                _launcher.ShowTemplateHelp = _showTemplateHelp;

                // now we have the DLL to run...
                var launcherArgs = new TemplateLauncher.TemplateLauncher.TemplateLauncherArgs()
                {
                    Template = _templateFile.FullName,
                    Models = cliArgs.Models,
                    OutputFolder = cliArgs.OutputFolder,
                    DefaultOutputFile = cliArgs.File,
                    TemplateSpecificArguments = cliArgs.TemplateArgs,
                };


                var executionContext = new ExecutionContext(_templateFile.FullName);
                _dependencyContainer.RegisterSingleton<ExecutionContext>(() => executionContext);

                statusCode = await _launcher.ExecuteAsync(launcherArgs, parseResult);

                if (statusCode != 0)
                {
                    if (statusCode != -2) // invalid template args has already shown the help page
                        Console.WriteLineError(ConsoleColor.Red, $"TemplateLauncher ({ConsoleColor.Yellow}'{currentCommand}'{PREVIOUS_COLOR}) Failed.");
                    return -1;
                }

                return 0;
            }
        }

        /// <summary>
        /// Creates a custom Command for the template, configures it (using ConfigureCommand() defined in template)
        /// </summary>
        Command CreateCustomCommand(string filePath, Type model1Type, Type model2Type, MethodInfo configureCommand)
        {
            // Create a subcommand for the invoked template, add model arguments, add custom options/arguments
            var customTemplateCommand = new Command(filePath.Replace(" ", "_"));

            if (model1Type != null && model2Type != null)
            {
                customTemplateCommand.AddArgument(new Argument<string>("Model1", $"Model of type {model1Type.FullName}") { Arity = ArgumentArity.ExactlyOne });
                customTemplateCommand.AddArgument(new Argument<string>("Model2", $"Model of type {model2Type.FullName}") { Arity = ArgumentArity.ExactlyOne });
            }
            else if (model1Type != null)
            {
                customTemplateCommand.AddArgument(new Argument<string>("Model", $"Model of type {model1Type.FullName}") { Arity = ArgumentArity.ExactlyOne });
            }

            //configure custom options/arguments
            configureCommand.Invoke(null, new object[] { customTemplateCommand });

            return customTemplateCommand;
        }

        public ParseResult ParseCliUsingCustomCommand(string filePath, Type model1Type, Type model2Type, DependencyContainer dependencyContainer, MethodInfo configureCommand, ParseResult parseResult)
        {
            // If template have a static ConfigureCommand(Command), let's create a fake command and reparse the command-line arguments using the arguments/options defined in ConfigureCommand()

            // This dummy (temporary) command is just to parse (validate) the Arguments/Options defined in "public static void ConfigureCommand(Command)"
            var templateCommand = CreateCustomCommand(filePath, model1Type, model2Type, configureCommand);
            dependencyContainer.RegisterSingleton<Command>(templateCommand);

            // Command is registered under a new (fake) RootCommand structure which has fake "template run" (with a single subcommand specific for running this command)
            var fakeRootCommand = new CliCommandParser(new TemplateRunCommand(templateCommand)).RootCommand;

            // Parse again from root, but now with fake run command
            var parser = new Parser(fakeRootCommand);

            var allArgs = parseResult.Tokens.Select(t => t.Value).ToList();
            var templateArg = parseResult.CommandResult.GetValueForArgument(_templateArg);
            int templatePos = allArgs.IndexOf(templateArg);
            if (templatePos > 0)
                allArgs[templatePos] = filePath;

            parseResult = parser.Parse(allArgs);
            return parseResult;
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
            public string File { get; set; }

            public string[] TemplateArgs { get; set; }

        }

    }
}
