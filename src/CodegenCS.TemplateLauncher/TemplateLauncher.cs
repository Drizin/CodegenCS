using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static InterpolatedColorConsole.Symbols;
using System.Threading.Tasks;
using CodegenCS.Utils;
using System.CommandLine;
using System.CommandLine.Binding;
using CodegenCS.Templating;
using System.CommandLine.Invocation;
using System.CommandLine.Help;
using System.CommandLine.IO;

namespace CodegenCS.TemplateLauncher
{
    public class TemplateLauncher
    {
        protected ILogger _logger;
        protected ICodegenContext _ctx;
        protected int _expectedModels;
        protected Type _model1Type = null;
        protected Type _model2Type = null;
        public int ExpectedModels => _expectedModels;
        protected Type _entryPointClass = null;
        protected FileInfo _templateFile;
        internal FileInfo _originallyInvokedTemplateFile;
        public bool ShowTemplateHelp { get; set; }
        protected string _defaultOutputFile = null;
        protected MethodInfo entryPointMethod = null;
        protected FileInfo[] _modelFiles = null;
        protected Type _iTypeTemplate = null;
        protected string _outputFolder = null;
        public bool VerboseMode { get; set; }
        public Func<BindingContext, HelpBuilder> HelpBuilderFactory = null;
        public delegate ParseResult ParseCliUsingCustomCommandDelegate(string filePath, Type model1Type, Type model2Type, DependencyContainer dependencyContainer, MethodInfo configureCommand, ParseResult parseResult);
        public ParseCliUsingCustomCommandDelegate ParseCliUsingCustomCommand = null;

        public TemplateLauncher(ILogger logger, ICodegenContext ctx, bool verboseMode)
        {
            _logger = logger;
            _ctx = ctx;
            VerboseMode = verboseMode;
        }

        /// <summary>
        /// Template Launcher options. For convenience this same model is also used for CLI options parsing.
        /// </summary>
        public class TemplateLauncherArgs
        {
            /// <summary>
            /// Path for Template (DLL that will be executed)
            /// </summary>
            public string Template { get; set; }
            
            /// <summary>
            /// Path for the Models (if any)
            /// </summary>
            public string[] Models { get; set; } = new string[0];

            /// <summary>
            /// Output folder for output files. If not defined will default to Current Directory.
            /// This is just a "base" path but output files may define their relative locations at/under/above this base path.
            /// </summary>
            public string OutputFolder { get; set; }


            /// <summary>
            /// DefaultOutputFile. If not defined will be based on the Template DLL path, adding CS extension.
            /// e.g. for "Template.dll" the default output file will be "Template.cs".
            /// </summary>
            public string DefaultOutputFile { get; set; }

            public string[] TemplateSpecificArguments { get; set; } = new string[0];
        }


        public async Task<int> LoadAndExecuteAsync(string templateDll, TemplateLauncherArgs args, ParseResult parseResult)
        {
            int returnCode = await LoadAsync(templateDll);
            if (returnCode != 0)
                return returnCode;
            return await ExecuteAsync(args, parseResult);
        }

        public async Task<int> LoadAsync(string templateDll)
        {
            if (!((_templateFile = new FileInfo(templateDll)).Exists || (_templateFile = new FileInfo(templateDll + ".dll")).Exists))
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Cannot find Template DLL {templateDll}");
                return -1;
            }

            await _logger.WriteLineAsync(ConsoleColor.Green, $"Loading {ConsoleColor.Yellow}'{_templateFile.Name}'{PREVIOUS_COLOR}...");


            var asm = Assembly.LoadFile(_templateFile.FullName);

            if (asm.GetName().Version?.ToString() != "0.0.0.0")
                await _logger.WriteLineAsync($"{ConsoleColor.Cyan}{_templateFile.Name}{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{asm.GetName().Version}{PREVIOUS_COLOR}");

            var types = asm.GetTypes().Where(t => typeof(IBaseTemplate).IsAssignableFrom(t));
            IEnumerable<Type> types2;

            if (_entryPointClass == null && types.Count() == 1)
                _entryPointClass = types.Single();

            if (_entryPointClass == null && (types2 = types.Where(t => t.Name == "Main")).Count() == 1)
                _entryPointClass = types2.Single();

            var interfacesPriority = new Type[]
            {
                typeof(ICodegenMultifileTemplate<>),
                typeof(ICodegenMultifileTemplate<,>),
                typeof(ICodegenMultifileTemplate),

                typeof(ICodegenTemplate<>),
                typeof(ICodegenTemplate<,>),
                typeof(ICodegenTemplate),

                typeof(ICodegenStringTemplate<>),
                typeof(ICodegenStringTemplate<,>),
                typeof(ICodegenStringTemplate),
            };

            Type foundInterface = null;
            Type iBaseXModelTemplate = null;

            if (_entryPointClass != null)
            {
                for (int i = 0; i < interfacesPriority.Length && foundInterface == null; i++)
                {
                    if (IsAssignableToType(_entryPointClass, interfacesPriority[i]))
                    {
                        foundInterface = interfacesPriority[i];
                    }
                }
            }

            for (int i = 0; i < interfacesPriority.Length && _entryPointClass == null; i++)
            {
                if ((types2 = types.Where(t => IsAssignableToType(t, interfacesPriority[i]))).Count() == 1)
                {
                    _entryPointClass = types2.Single();
                    foundInterface = interfacesPriority[i];
                }
            }

            //TODO: [System.Runtime.InteropServices.DllImportAttribute]

            if (_entryPointClass == null || foundInterface == null)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Could not find template entry-point in '{(_originallyInvokedTemplateFile ?? _templateFile).Name}'.");
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Template should implement ICodegenTemplate, ICodegenMultifileTemplate or ICodegenStringTemplate.");
                return -1;
            }

            if (IsAssignableToType(foundInterface, typeof(IBase1ModelTemplate<>)))
                iBaseXModelTemplate = typeof(IBase1ModelTemplate<>);
            else if (IsAssignableToType(foundInterface, typeof(IBase2ModelTemplate<,>)))
                iBaseXModelTemplate = typeof(IBase2ModelTemplate<,>);
            else if (IsAssignableToType(foundInterface, typeof(IBase0ModelTemplate)))
                iBaseXModelTemplate = typeof(IBase0ModelTemplate);
            else
                throw new NotImplementedException();

            if (IsAssignableToType(foundInterface, typeof(IBaseMultifileTemplate)))
                _iTypeTemplate = typeof(IBaseMultifileTemplate);
            else if (IsAssignableToType(foundInterface, typeof(IBaseSinglefileTemplate)))
                _iTypeTemplate = typeof(IBaseSinglefileTemplate);
            else if (IsAssignableToType(foundInterface, typeof(IBaseStringTemplate)))
                _iTypeTemplate = typeof(IBaseStringTemplate);
            else
                throw new NotImplementedException();

            entryPointMethod = foundInterface.GetMethod("Render", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);

            await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Template entry-point: {ConsoleColor.White}'{_entryPointClass.Name}.{entryPointMethod.Name}()'{PREVIOUS_COLOR}...");


            if (iBaseXModelTemplate == typeof(IBase2ModelTemplate<,>))
            {
                _expectedModels = 2;
                _model1Type = _entryPointClass.GetInterfaces().Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == iBaseXModelTemplate).ToList()[0].GetGenericArguments()[0];
                _model2Type = _entryPointClass.GetInterfaces().Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == iBaseXModelTemplate).ToList()[0].GetGenericArguments()[1];
            }
            else if (iBaseXModelTemplate == typeof(IBase1ModelTemplate<>))
            {
                _expectedModels = 1;
                _model1Type = _entryPointClass.GetInterfaces().Where(it => it.IsGenericType && it.GetGenericTypeDefinition() == iBaseXModelTemplate).ToList()[0].GetGenericArguments()[0];
            }
            else
                _expectedModels = 0;

            return 0;
        }

        public async Task<int> ExecuteAsync(TemplateLauncherArgs _args, ParseResult parseResult)
        {

            _modelFiles = new FileInfo[_args.Models?.Length ?? 0];
            for (int i = 0; i < _modelFiles.Length; i++)
            {
                string model = _args.Models[i];
                if (model != null)
                {
                    if (!((_modelFiles[i] = new FileInfo(model)).Exists || (_modelFiles[i] = new FileInfo(model + ".json")).Exists || (_modelFiles[i] = new FileInfo(model + ".yaml")).Exists))
                    {
                        await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Cannot find find model {model}");
                        return -1;
                    }
                }
            }

            string providedTemplateName = _originallyInvokedTemplateFile?.Name ?? _templateFile?.Name ?? _args.Template;

            _outputFolder = Directory.GetCurrentDirectory();
            _defaultOutputFile = Path.GetFileNameWithoutExtension(providedTemplateName) + ".generated.cs";
            if (!string.IsNullOrWhiteSpace(_args.OutputFolder))
                _outputFolder = Path.GetFullPath(_args.OutputFolder);
            if (!string.IsNullOrWhiteSpace(_args.DefaultOutputFile))
                _defaultOutputFile = _args.DefaultOutputFile;


            _ctx.DefaultOutputFile.RelativePath = _defaultOutputFile;

            var dependencyContainer = new DependencyContainer();
            dependencyContainer.RegisterSingleton<ILogger>(_logger);
            dependencyContainer.RegisterSingleton<ICodegenContext>(_ctx);
            dependencyContainer.RegisterSingleton<ICodegenOutputFile>(_ctx.DefaultOutputFile);
            dependencyContainer.RegisterSingleton<ICodegenTextWriter>(_ctx.DefaultOutputFile);

            // CommandLineArgs can be injected in the template constructor (or in any dependency like "TemplateArgs" nested class), and will bring all command-line arguments that were not captured by dotnet-codegencs tool
            CommandLineArgs cliArgs = new CommandLineArgs(_args.TemplateSpecificArguments);
            dependencyContainer.RegisterSingleton<CommandLineArgs>(cliArgs);



            #region If template has a method "public static void ConfigureCommand(Command)" then we can use it to parse the command-line arguments or to ShowHelp()
            var methods = _entryPointClass.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            MethodInfo configureCommand = methods.Where(m => m.Name == "ConfigureCommand" && m.GetParameters().Count()==1 && m.GetParameters()[0].ParameterType == typeof(Command)).SingleOrDefault();

            if (configureCommand != null)
            {
                if (VerboseMode)
                {
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"{ConsoleColor.Yellow}ConfigureCommand(Command){PREVIOUS_COLOR} method was found.");
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"The following args will be forwarded to the template: {ConsoleColor.Yellow}'{string.Join("', '", _args.TemplateSpecificArguments)}'{PREVIOUS_COLOR}...");
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"These args can be injected into your template using {ConsoleColor.Yellow}CommandLineArgs{PREVIOUS_COLOR} class");
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"or using any custom class that implements {ConsoleColor.Yellow}IAutoBindCommandLineArgs{PREVIOUS_COLOR}");
                }
                if (ParseCliUsingCustomCommand != null)
                {
                    if (VerboseMode)
                        await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"Parsing command-line arguments to check if they match the options/args defined in ConfigureCommand()...");
                    parseResult = ParseCliUsingCustomCommand(providedTemplateName, _model1Type, _model2Type, dependencyContainer, configureCommand, parseResult);
                }
            }
            else
            {
                if (VerboseMode)
                {
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"{ConsoleColor.Yellow}ConfigureCommand(Command){PREVIOUS_COLOR} method was not found.");
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"The following args will be forwarded to the template: {ConsoleColor.Yellow}'{string.Join("', '", _args.TemplateSpecificArguments)}'{PREVIOUS_COLOR}...");
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"These args can be injected into your template using {ConsoleColor.Yellow}CommandLineArgs{PREVIOUS_COLOR} class.");
                }
            }


            var invocationContext = new InvocationContext(parseResult);
            var bindingContext = invocationContext.BindingContext;
            dependencyContainer.RegisterSingleton<ParseResult>(parseResult);
            dependencyContainer.RegisterSingleton<InvocationContext>(invocationContext);
            dependencyContainer.RegisterSingleton<BindingContext>(bindingContext);


            #endregion

            if (_expectedModels != _modelFiles.Count())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Template entry-point {ConsoleColor.White}'{_entryPointClass.Name}.{entryPointMethod.Name}()'{PREVIOUS_COLOR} requires {ConsoleColor.White}{_expectedModels}{PREVIOUS_COLOR} model(s) but got only {ConsoleColor.White}{_modelFiles.Count()}{PREVIOUS_COLOR}.");

                if (parseResult != null)
                    ShowParseResults(bindingContext, parseResult);
                return -2;
            }

            if (parseResult != null && parseResult.Errors.Any())
            {
                foreach (var error in parseResult.Errors)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: {error.Message}");
                ShowParseResults(bindingContext, parseResult);
                return -2;
            }

            if (ShowTemplateHelp)
            {
                ShowParseResults(bindingContext, parseResult);
                return -2;
            }



            #region Loading Required Models
            List<object> modelArgs = new List<object>();

            for (int i = 0; i < _expectedModels; i++)
            {
                Type modelType;
                try
                {
                    modelType = _entryPointClass.GetInterfaces().Where(itf => itf.IsGenericType
                        && (itf.GetGenericTypeDefinition() == typeof(IBase1ModelTemplate<>) || itf.GetGenericTypeDefinition() == typeof(IBase2ModelTemplate<,>)))
                        .Select(interf => interf.GetGenericArguments().Skip(i).First()).Distinct().Single();
                    await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Model{(_expectedModels > 1 ? (i + 1).ToString() : "")} type is {ConsoleColor.White}'{modelType.FullName}'{PREVIOUS_COLOR}...");
                }
                catch (Exception ex)
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not get type for Model{(_expectedModels > 1 ? (i + 1).ToString() : "")}: {ex.Message}");
                    return -1;
                }
                try
                {
                    object model = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(_modelFiles[i].FullName), modelType);
                    await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Model{(_expectedModels > 1 ? (i + 1).ToString() : "")} successfuly loaded from {ConsoleColor.White}'{_modelFiles[i].Name}'{PREVIOUS_COLOR}...");
                    modelArgs.Add(model);
                }
                catch (Exception ex)
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not load Model{(_expectedModels > 1 ? (i + 1).ToString() : "")} (type {ConsoleColor.White}'{modelType.FullName}'{PREVIOUS_COLOR}): {ex.Message}");
                    return -1;
                }
            }
            #endregion



            object instance = null;
            try
            {
                instance = dependencyContainer.Resolve(_entryPointClass);

                //TODO: if _args.TemplateSpecificArguments?.Any() == true && typeof(CommandLineArgs) wasn't injected into the previous Resolve() call:
                // $"ERROR: There are unknown args but they couldn't be forwarded to the template because it doesn't define {ConsoleColor.Yellow}'ConfigureCommand(Command)'{PREVIOUS_COLOR} and doesn't take {ConsoleColor.White}CommandLineArgs{PREVIOUS_COLOR} in the constructor."
            }
            catch (CommandLineArgsException ex)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{ex.Message}");
                if (VerboseMode)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{ex.ToString()}");
                ex.ShowHelp(_logger);
                return -1;
            }
            catch (ArgumentException ex)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{ex.Message}");
                if (VerboseMode)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{ex.ToString()}");
                return -1;
            }

            if (_iTypeTemplate == typeof(IBaseMultifileTemplate)) // pass ICodegenContext
            {
                modelArgs.Insert(0, _ctx);
                (_entryPointClass.GetMethod(entryPointMethod.Name, entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray()) ?? _entryPointClass.GetMethod(entryPointMethod.Name))
                    .Invoke(instance, modelArgs.ToArray());
            }
            else if (_iTypeTemplate == typeof(IBaseSinglefileTemplate)) // pass ICodegenTextWriter
            {
                modelArgs.Insert(0, _ctx.DefaultOutputFile);
                (_entryPointClass.GetMethod(entryPointMethod.Name, entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray()) ?? _entryPointClass.GetMethod(entryPointMethod.Name))
                    .Invoke(instance, modelArgs.ToArray());
            }
            else if (_iTypeTemplate == typeof(IBaseStringTemplate)) // get the FormattableString and write to DefaultOutputFile
            {
                FormattableString fs = (FormattableString) (_entryPointClass.GetMethod(entryPointMethod.Name, entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray()) ?? _entryPointClass.GetMethod(entryPointMethod.Name))
                    .Invoke(instance, modelArgs.ToArray());
                _ctx.DefaultOutputFile.Write(fs);
            }

            if (_ctx.Errors.Any())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"\nError while building '{providedTemplateName}':");
                foreach (var error in _ctx.Errors)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{error}");
                return -1;
            }

            int savedFiles = _ctx.SaveFiles(_outputFolder);

            if (savedFiles == 0)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Yellow, $"No files were generated");
            }
            else if (savedFiles == 1)
            {
                await _logger.WriteLineAsync($"Generated {ConsoleColor.White}{savedFiles}{PREVIOUS_COLOR} file: {ConsoleColor.Yellow}'{_outputFolder.TrimEnd('\\')}\\{_ctx.OutputFilesPaths.Single()}'{PREVIOUS_COLOR}");
            }
            else
            {
                await _logger.WriteLineAsync($"Generated {ConsoleColor.White}{savedFiles}{PREVIOUS_COLOR} files at folder {ConsoleColor.Yellow}'{_outputFolder.TrimEnd('\\')}\\'{PREVIOUS_COLOR}{(VerboseMode ? ":" : "")}");
                if (VerboseMode)
                {
                    foreach (var f in _ctx.OutputFiles)
                        await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"    {Path.Combine(_outputFolder, f.RelativePath)}");
                    await _logger.WriteLineAsync();
                }
            }

            await _logger.WriteLineAsync(ConsoleColor.Green, $"Successfully executed template {ConsoleColor.Yellow}'{providedTemplateName}'{PREVIOUS_COLOR}.");

            return 0;
        }

        void ShowParseResults(BindingContext bindingContext, ParseResult parseResult)
        {
            var helpBuilder = HelpBuilderFactory != null ? HelpBuilderFactory(bindingContext) : new HelpBuilder(parseResult.CommandResult.LocalizationResources);
            var writer = bindingContext.Console.Out.CreateTextWriter();
            helpBuilder.Write(parseResult.CommandResult.Command, writer);
        }


        #region Utils
        protected bool IsInstanceOfGenericType(Type genericType, object instance)
        {
            Type type = instance.GetType();
            return IsAssignableToGenericType(type, genericType);
        }

        /// <summary>
        /// Determines whether the current type can be assigned to a variable of the specified Generic type targetType.
        /// </summary>
        protected bool IsAssignableToGenericType(Type currentType, Type targetType)
        {
            var interfaceTypes = currentType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == targetType)
                    return true;
            }

            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == targetType)
                return true;

            Type baseType = currentType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, targetType);
        }



        /// <summary>
        /// Determines whether the current type can be assigned to a variable of the specified targetType.
        /// </summary>
        protected bool IsAssignableToType(Type currentType, Type targetType)
        {
            if (targetType.IsGenericType)
                return IsAssignableToGenericType(currentType, targetType);

            return targetType.IsAssignableFrom(currentType);
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

        #endregion
    }
}
