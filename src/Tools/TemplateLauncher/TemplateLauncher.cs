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
using System.CommandLine.Invocation;
using System.CommandLine.Help;
using System.CommandLine.IO;
using static CodegenCS.Utils.TypeUtils;
using CodegenCS.Runtime;
using CodegenCS.Models;
using Newtonsoft.Json;
using CodegenCS.IO;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;

namespace CodegenCS.TemplateLauncher
{
    public class TemplateLauncher
    {
        protected ILogger _logger;
        protected ICodegenContext _ctx;
        protected int _expectedModels;
        protected Type _model1Type = null;
        protected Type _model2Type = null;
        protected Type _entryPointClass = null;
        Type _templatingInterface = null;
        protected ConstructorInfo _ctorInfo = null;
        protected FileInfo _templateFile;
        internal FileInfo _originallyInvokedTemplateFile;
        public bool ShowTemplateHelp { get; set; }
        protected string _defaultOutputFile = null;
        protected MethodInfo _entryPointMethod = null;
        protected FileInfo[] _modelFiles = null;
        protected Type _iTypeTemplate = null;
        protected string _outputFolder = null;
        protected string _executionFolder = null;
        public bool VerboseMode { get; set; }
        public Func<BindingContext, HelpBuilder> HelpBuilderFactory = null;
        public delegate ParseResult ParseCliUsingCustomCommandDelegate(string filePath, Type model1Type, Type model2Type, DependencyContainer dependencyContainer, MethodInfo configureCommand, ParseResult parseResult);
        public ParseCliUsingCustomCommandDelegate ParseCliUsingCustomCommand = null;
        protected IModelFactory _modelFactory;
        protected DependencyContainer _dependencyContainer;

        public TemplateLauncher(ILogger logger, ICodegenContext ctx, DependencyContainer parentDependencyContainer, bool verboseMode)
        {
            _logger = logger;
            _ctx = ctx;
            VerboseMode = verboseMode;
            _dependencyContainer = new DependencyContainer();
            _dependencyContainer.ParentContainer = parentDependencyContainer; //TODO: Autofac, parent-child scopes
            string[] searchPaths = null;
            try
            {
                var exCtx = parentDependencyContainer.Resolve<ExecutionContext>();
                searchPaths = new string[] { new FileInfo(exCtx.TemplatePath).Directory.FullName, exCtx.CurrentDirectory };
            }
            catch (Exception ex) 
            {
                _logger.WriteLineErrorAsync("Can't resolve ExecutionContext");
            }
            _dependencyContainer.AddModelFactory(searchPaths);
            _modelFactory = _dependencyContainer.Resolve<IModelFactory>();
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
            /// Folder where execution will happen. Before template is executed the current folder is changed to this (if defined).
            /// Useful for loading files (e.g. IModelFactory.LoadModelFromFile{TModel}(path)) with relative paths.
            /// </summary>
            public string ExecutionFolder { get; set; }

            /// <summary>
            /// DefaultOutputFile. If not defined will be based on the Template DLL path, adding CS extension.
            /// e.g. for "Template.dll" the default output file will be "Template.generated.cs".
            /// </summary>
            public string DefaultOutputFile { get; set; }

            public string[] TemplateSpecificArguments { get; set; } = new string[0];
        }


        public async Task<int> LoadAndExecuteAsync(TemplateLauncherArgs args, ParseResult parseResult)
        {
            var loadResult = await LoadAsync(args.Template, args.Models.Length);
            if (loadResult.ReturnCode != 0)
                return loadResult.ReturnCode;
            return await ExecuteAsync(args, parseResult);
        }

        public class TemplateLoadResponse
        {
            public int ReturnCode { get; set; }
            public Type Model1Type { get; set; }
            public Type Model2Type { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="templateDll"></param>
        /// <param name="providedModels">If there are multiple constructors or multiple entrypoints it will pick the overload where the number of provided models match the expected models</param>
        /// <returns></returns>
        public async Task<TemplateLoadResponse> LoadAsync(string templateDll, int? providedModels = null)
        {
            if (!((_templateFile = new FileInfo(templateDll)).Exists || (_templateFile = new FileInfo(templateDll + ".dll")).Exists))
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Cannot find Template DLL {templateDll}");
                return new TemplateLoadResponse() { ReturnCode = -1 };
            }

            await _logger.WriteLineAsync(ConsoleColor.Green, $"Loading {ConsoleColor.Yellow}'{_templateFile.Name}'{PREVIOUS_COLOR}...");

            bool success = await FindEntryPoint(providedModels);

            if (!success || _entryPointClass == null || _entryPointMethod == null)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Could not find template entry-point in '{(_originallyInvokedTemplateFile ?? _templateFile).Name}'.");
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Template should have a method called TemplateMain().");
                return new TemplateLoadResponse() { ReturnCode = -1 };
            }

            await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Template entry-point: {ConsoleColor.White}'{_entryPointClass.Name}.{_entryPointMethod.Name}()'{PREVIOUS_COLOR}...");

            if (_templatingInterface != null) // old interfaces already define _expectedModels/_model1Type/_model2Type
            {                
                //TODO: check that constructor doesn't get same models that are expected by Render entrypoint...
                return new TemplateLoadResponse() { ReturnCode = 0, Model1Type = _model1Type, Model2Type = _model2Type };
            }

            // For TemplateMain() or Main() we have to ensure that models are NOT being expected by both, and also ensure set _expectedModels/_model1Type/_model2Type

            // Find the best constructor
            var bestCtor = FindConstructor(providedModels);

            if (bestCtor == null)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Could not find a possible constructor in '{_entryPointMethod.Name}'.");
                return new TemplateLoadResponse() { ReturnCode = -1 };
            }



            // Models required by the entry point method 
            var entrypointModels = _entryPointMethod.GetParameters().Select(p => p.ParameterType).Where(p => _modelFactory.CanCreateModel(p)).ToList();

            // Models required by ctor
            var ctorModels = bestCtor.Parameters.Where(p => p.IsModel).Select(p => p.ParameterType).Where(p => _modelFactory.CanCreateModel(p)).ToList();

            if (ctorModels.Any() && entrypointModels.Any())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Templates with {_entryPointMethod.Name}() entry-point may receive models in the entry-point or in the constructor, but not in both.");
                return new TemplateLoadResponse() { ReturnCode = -1 };
            }

            var expectedModels = ctorModels.Any() ? ctorModels : entrypointModels;

            _expectedModels = expectedModels.Count();
            if (_expectedModels >= 1)
                _model1Type = expectedModels[0];
            if (_expectedModels >= 2)
                _model1Type = expectedModels[1];
            if (_expectedModels >= 3)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Templates invoked from dotnet-codegencs can expect up to 2 models - but this one expects {ConsoleColor.Yellow}'{string.Join("', '", entrypointModels.Select(t => t.Name))}'{PREVIOUS_COLOR}");
                return new TemplateLoadResponse() { ReturnCode = -1 };
            }

            return new TemplateLoadResponse() { ReturnCode = 0, Model1Type = _model1Type, Model2Type = _model2Type };
        }

        async Task<bool> FindEntryPoint(int? providedModels)
        {

            if (_entryPointMethod != null && _entryPointClass != null) // previously calculated
                return true; 

            var asm = Assembly.LoadFile(_templateFile.FullName);

            if (asm.GetName().Version?.ToString() != "0.0.0.0")
                await _logger.WriteLineAsync($"{ConsoleColor.Cyan}{_templateFile.Name}{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{asm.GetName().Version}{PREVIOUS_COLOR}");

            List<MethodInfo> candidates;

            var asmTypes = asm.GetTypes().ToList();
            var asmMethods = asmTypes.SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)).ToList();


            // If providedModels was not provided, we look for any entrypoint (any number of required models)
            // if provided we'll first look for an exact match, then if not found we'll look for a match that doesn't require any model
            // (maybe the models are going to constructor and not required by entrypoint method)
            int?[] searchModels;
            if (providedModels == null)
                searchModels = new int?[] { null };
            else if (providedModels == 0)
                searchModels = new int?[] { 0 };
            else
                searchModels = new int?[] { providedModels, 0 };

            foreach (var searchModel in searchModels)
            {

                // First we search for "TemplateMain" entrypoint method.
                if (_entryPointMethod == null && (candidates = asmMethods.Where(m => m.Name == "TemplateMain").ToList()).Any())
                {

                    var evaluatedCandidates = candidates.Select(method => new MethodOverload<MethodInfo>(method, method.GetParameters().Select(p => p.ParameterType).ToList())).ToList();
                    var best = FindBestOverload(evaluatedCandidates, _modelFactory, searchModel);
                    if (best != null)
                    {
                        _entryPointMethod = best.CtorOrMethod;
                        _entryPointClass = _entryPointMethod.DeclaringType;
                        return true;
                    }
                }
                // Then we search for a single "Main" entrypoint method //TODO: deprecate?
                if (_entryPointMethod == null && (candidates = asmMethods.Where(m => m.Name == "Main").ToList()).Any())
                {
                    var evaluatedCandidates = candidates.Select(method => new MethodOverload<MethodInfo>(method, method.GetParameters().Select(p => p.ParameterType).ToList())).ToList();
                    var best = FindBestOverload(evaluatedCandidates, _modelFactory, searchModel);

                    if (best != null)
                    {
                        _entryPointMethod = best.CtorOrMethod;
                        _entryPointClass = _entryPointMethod.DeclaringType;
                        return true;
                    }
                    else
                    {
                        // TODO: we can't find an exact match with the number of provided models... show error message and show the expected overloads/models?
                    }
                }
            }


            #region LEGACY Templating interfaces (prefer using TemplateMain() or Main() entrypoints)
            //TODO: deprecate
            var interfacesPriority = new Type[]
            {
                typeof(ICodegenMultifileTemplate<,>),
                typeof(ICodegenMultifileTemplate<>),
                typeof(ICodegenMultifileTemplate),

                typeof(ICodegenTemplate<,>),
                typeof(ICodegenTemplate<>),
                typeof(ICodegenTemplate),

                typeof(ICodegenStringTemplate<,>),
                typeof(ICodegenStringTemplate<>),
                typeof(ICodegenStringTemplate),
            };
            // Then we search for templating interfaces (they all inherit from IBaseTemplate)
            if (_entryPointClass == null)
            {
                var templateInterfaceTypes = asmTypes.Where(t => typeof(IBaseTemplate).IsAssignableFrom(t)).ToList();

                // If there's a single class implementing IBaseTemplate
                if (_entryPointClass == null && templateInterfaceTypes.Count() == 1)
                {
                    _entryPointClass = templateInterfaceTypes.Single();
                    // If a class implements multiple templating interfaces (or if different classes implement different interfaces) we'll pick the most elaborated interface
                    for (int i = 0; i < interfacesPriority.Length && _templatingInterface == null; i++)
                    {
                        if (IsAssignableToType(_entryPointClass, interfacesPriority[i]))
                            _templatingInterface = interfacesPriority[i];
                    }
                }
                else if (templateInterfaceTypes.Any()) // if multiple classes, find the best one by the best interface
                {
                    for (int i = 0; i < interfacesPriority.Length && _entryPointClass == null; i++)
                    {
                        IEnumerable<Type> types2;
                        if ((types2 = templateInterfaceTypes.Where(t => IsAssignableToType(t, interfacesPriority[i]))).Count() == 1)
                        {
                            _entryPointClass = types2.Single();
                            _templatingInterface = interfacesPriority[i];
                        }
                    }
                }
            }
            if (_templatingInterface != null)
            {
                Type iBaseXModelTemplate = null;

                if (IsAssignableToType(_templatingInterface, typeof(IBase1ModelTemplate<>)))
                    iBaseXModelTemplate = typeof(IBase1ModelTemplate<>);
                else if (IsAssignableToType(_templatingInterface, typeof(IBase2ModelTemplate<,>)))
                    iBaseXModelTemplate = typeof(IBase2ModelTemplate<,>);
                else if (IsAssignableToType(_templatingInterface, typeof(IBase0ModelTemplate)))
                    iBaseXModelTemplate = typeof(IBase0ModelTemplate);
                else
                    throw new NotImplementedException();

                if (IsAssignableToType(_templatingInterface, typeof(IBaseMultifileTemplate)))
                    _iTypeTemplate = typeof(IBaseMultifileTemplate);
                else if (IsAssignableToType(_templatingInterface, typeof(IBaseSinglefileTemplate)))
                    _iTypeTemplate = typeof(IBaseSinglefileTemplate);
                else if (IsAssignableToType(_templatingInterface, typeof(IBaseStringTemplate)))
                    _iTypeTemplate = typeof(IBaseStringTemplate);
                else
                    throw new NotImplementedException();


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

                if (_entryPointMethod == null)
                    _entryPointMethod = _templatingInterface.GetMethod("Render", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);
                await _logger.WriteLineAsync(ConsoleColor.Yellow, $"WARNING: Templating interfaces ICodegenTemplate/ICodegenMultifileTemplate/ICodegenStringTemplate are deprecated and should be replaced by TemplateMain() entrypoint.");
                await Task.Delay(2000);
                return true;
            }

            #endregion

            return false;

        }
        private MethodOverload<ConstructorInfo> FindConstructor(int? providedModels)
        {
            var ctors = _entryPointClass.GetConstructors().Select(ctor => new MethodOverload<ConstructorInfo>(ctor, ctor.GetParameters().Select(p => p.ParameterType).ToList())).ToList();
            var bestCtor = FindBestOverload(ctors, _modelFactory, providedModels);
            if (bestCtor == null && providedModels > 0) // maybe the models are going to entrypoint method and not required by constructor
                bestCtor = FindBestOverload(ctors, _modelFactory, 0);

            if (bestCtor == null)
            {
                // TODO: we can't find an exact match with the number of provided models... show error message and show the expected overloads/models?
            }

            _ctorInfo = bestCtor.CtorOrMethod;
            return bestCtor;
        }

        class MethodOverload<T>
        {
            public T CtorOrMethod { get; set; }
            public List<MethodParameter> Parameters { get; set; }
            public string ErrorMessage { get; set; }
            public MethodOverload(T ctorOrMethod, List<Type> parameters)
            {
                CtorOrMethod = ctorOrMethod;
                Parameters = parameters.Select(p => new MethodParameter(p)).ToList();
            }
        }
        class MethodParameter
        {
            public Type ParameterType { get; set; }
            public bool IsModel { get; set; }
            public MethodParameter(Type type)
            {
                ParameterType = type;
            }
        }
        
        /// <summary>
        /// Given a list of overloads (for method or constructor) will find the best match
        /// </summary>
        /// <param name="providedModels">Null means "any" number of models. Other values will look for an exact match</param>
        /// <returns></returns>
        private MethodOverload<T> FindBestOverload<T>(List<MethodOverload<T>> overloads, IModelFactory modelFactory, int? providedModels)
        {
            foreach (var overload in overloads)
            {
                foreach (var p in overload.Parameters)
                    p.IsModel = modelFactory.CanCreateModel(p.ParameterType);

                if (overload.Parameters.Where(p => p.IsModel).GroupBy(t => t.ParameterType).Any(g => g.Count() > 1))
                {
                    //TODO: print error message
                    overload.ErrorMessage = "Templates with Main() entry-point cannot receive 2 models of the same type.";
                    continue;
                }
                //TODO: check if other non-Model parameters can be resolved using DependencyContainer? (probably requires receiving parent dependencyContainer)
            }

            var bestOverloads = overloads
                .Where(o => o.ErrorMessage == null)
                .OrderByDescending(ctor => ctor.Parameters.Where(p => p.IsModel).Count()) // prefer the overload with the largest number of models
                .ThenByDescending(ctor => ctor.Parameters.Count()); // then the largest number of total parameters

            if (providedModels != null && bestOverloads.Any(o => o.Parameters.Where(p => p.IsModel).Count() == providedModels))
                return bestOverloads.First(o => o.Parameters.Where(p => p.IsModel).Count() == providedModels);

            return bestOverloads.FirstOrDefault();
        }

        public async Task<int> ExecuteAsync(TemplateLauncherArgs _args, ParseResult parseResult)
        {
            if (_entryPointClass == null)
                throw new InvalidOperationException("Should call LoadAsync() before ExecuteAsync(). Or use LoadAndExecuteAsync()");

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

            _outputFolder = _executionFolder = Directory.GetCurrentDirectory();
            _defaultOutputFile = Path.GetFileNameWithoutExtension(providedTemplateName) + ".generated.cs"; //TODO: attribute to define default Suffix/Extension (while keeping names auto-inferred)
            if (!string.IsNullOrWhiteSpace(_args.OutputFolder))
                _outputFolder = Path.GetFullPath(_args.OutputFolder);
            if (!string.IsNullOrWhiteSpace(_args.ExecutionFolder))
                _executionFolder = Path.GetFullPath(_args.ExecutionFolder);
            if (!string.IsNullOrWhiteSpace(_args.DefaultOutputFile))
                _defaultOutputFile = _args.DefaultOutputFile;


            _ctx.DefaultOutputFile.RelativePath = _defaultOutputFile;

            _ctx.DependencyContainer.ParentContainer = _dependencyContainer;
            
            _dependencyContainer.RegisterSingleton<ILogger>(_logger);

            // CommandLineArgs can be injected in the template constructor (or in any dependency like "TemplateArgs" nested class), and will bring all command-line arguments that were not captured by dotnet-codegencs tool
            CommandLineArgs cliArgs = new CommandLineArgs(_args.TemplateSpecificArguments);
            _dependencyContainer.RegisterSingleton<CommandLineArgs>(cliArgs);



            #region If template has a method "public static void ConfigureCommand(Command)" then we can use it to parse the command-line arguments or to ShowHelp()
            // If template defines a method "public static void ConfigureCommand(Command command)", then this method can be used to configure (describe) the template custom Arguments and Options.
            // In this case dotnet-codegencs will pass an empty command to this configuration method, will create a Parser for the Command definition,
            // will parse the command line to extract/validate those extra arguments/options, and if there's any parse error it will invoke the regular ShowHelp() for the Command definition.
            // If there are no errors it will create and register (in the Dependency Injection container) ParseResult, BindingContext and InvocationContext.
            // Those objects (ParseResult/BindingContext/InvocationContext) can be used to get the args/options that the template needs.
            // Example: TemplateOptions constructor can take ParseResult and extract it's values using parseResult.CommandResult.GetValueForArgument and parseResult.CommandResult.GetValueForOption.

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
                    parseResult = ParseCliUsingCustomCommand(providedTemplateName, _model1Type, _model2Type, _ctx.DependencyContainer, configureCommand, parseResult);
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


            BindingContext bindingContext = null;
            if (parseResult != null)
            {
                var invocationContext = new InvocationContext(parseResult);
                bindingContext = invocationContext.BindingContext;
                _dependencyContainer.RegisterSingleton<ParseResult>(parseResult);
                _dependencyContainer.RegisterSingleton<InvocationContext>(invocationContext);
                _dependencyContainer.RegisterSingleton<BindingContext>(bindingContext);
            }


            #endregion

            if (_expectedModels > _modelFiles.Count())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Template entry-point {ConsoleColor.White}'{_entryPointClass.Name}.{_entryPointMethod.Name}()'{PREVIOUS_COLOR} requires {ConsoleColor.White}{_expectedModels}{PREVIOUS_COLOR} model(s) but got only {ConsoleColor.White}{_modelFiles.Count()}{PREVIOUS_COLOR}.");

                if (parseResult != null)
                    ShowParseResults(bindingContext, parseResult);
                return -2;
            }
            if (_expectedModels < _modelFiles.Count())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: Template entry-point {ConsoleColor.White}'{_entryPointClass.Name}.{_entryPointMethod.Name}()'{PREVIOUS_COLOR} requires {ConsoleColor.White}{_expectedModels}{PREVIOUS_COLOR} model(s) and got {ConsoleColor.White}{_modelFiles.Count()}{PREVIOUS_COLOR}.");
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Maybe your template expects a model class and you forgot to use IInputModel interface? Or maybe you have provided an extra arg which is not expected?");

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
                Type modelType = null;
                try
                {
                    if (i == 0)
                        modelType = _model1Type;
                    else if (i == 1)
                        modelType = _model2Type;
                    modelType = modelType ?? _entryPointClass.GetInterfaces().Where(itf => itf.IsGenericType
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
                    object model;
                    if (_iTypeTemplate != null) // old templating interfaces - always expect a JSON model
                        model = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(_modelFiles[i].FullName), modelType);
                    else if (_modelFactory.CanCreateModel(modelType))
                    {
                        model = await _modelFactory.LoadModelFromFileAsync(modelType, _modelFiles[i].FullName);
                    }
                    else
                    {
                        await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not load Model{(_expectedModels > 1 ? (i + 1).ToString() : "")}: Unknown type. Maybe your model should implement IJsonInputModel?");
                        return -1;
                    }
                    await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Model{(_expectedModels > 1 ? (i + 1).ToString() : "")} successfuly loaded from {ConsoleColor.White}'{_modelFiles[i].Name}'{PREVIOUS_COLOR}...");
                    modelArgs.Add(model);
                    _dependencyContainer.RegisterSingleton(modelType, model); // might be injected both in _entryPointClass ctor or _entryPointMethod args
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
                //TODO: pass preferred _ctorInfo since we already calculated that. And use the Resolve() logic EARLIER, in FindEntryPoint / FindConstructor
                instance = _ctx.DependencyContainer.Resolve(_entryPointClass);

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

            string previousFolder = Directory.GetCurrentDirectory();
            try
            {
                if (_executionFolder != null)
                    Directory.SetCurrentDirectory(_executionFolder);

                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Newtonsoft.Json.Formatting.Indented,
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                #region Invoking template
                if (_iTypeTemplate == typeof(IBaseMultifileTemplate)) // pass ICodegenContext
                {
                    modelArgs.Insert(0, _ctx);
                    (_entryPointClass.GetMethod(_entryPointMethod.Name, _entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray()) ?? _entryPointClass.GetMethod(_entryPointMethod.Name))
                        .Invoke(instance, modelArgs.ToArray());
                }
                else if (_iTypeTemplate == typeof(IBaseSinglefileTemplate)) // pass ICodegenTextWriter
                {
                    modelArgs.Insert(0, _ctx.DefaultOutputFile);
                    var method = _entryPointClass.GetMethod(_entryPointMethod.Name, _entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray());
                    method = method ?? _entryPointClass.GetMethod(_entryPointMethod.Name);
                    method.Invoke(instance, modelArgs.ToArray());
                }
                else if (_iTypeTemplate == typeof(IBaseStringTemplate)) // get the FormattableString and write to DefaultOutputFile
                {
                    FormattableString fs = (FormattableString)(_entryPointClass.GetMethod(_entryPointMethod.Name, _entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray()) ?? _entryPointClass.GetMethod(_entryPointMethod.Name))
                        .Invoke(instance, modelArgs.ToArray());
                    _ctx.DefaultOutputFile.Write(fs);
                }
                else if (_iTypeTemplate == null && _entryPointMethod != null) // Main() entrypoint
                {
                    var argTypes = _entryPointMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                    object[] entryPointMethodArgs = new object[argTypes.Length];
                    for (int i = 0; i < argTypes.Length; i++)
                        entryPointMethodArgs[i] = _ctx.DependencyContainer.Resolve(argTypes[i]);

                    Type returnType = _entryPointMethod.ReturnType;
                    object result = _entryPointMethod.Invoke(instance, entryPointMethodArgs);
                    Type[] genericTypes;
                    
                    // Tasks should be executed/awaited
                    if (IsAssignableToType(returnType, typeof(Task)))
                    {
                        var task = (Task)result;
                        await task;
                        
                        // and result should be unwrapped
                        if (IsAssignableToGenericType(returnType, typeof(Task<>)) && (genericTypes = returnType.GetGenericArguments()) != null)
                        {
                            if (genericTypes[0]==typeof(FormattableString))
                            {
                                returnType = typeof(FormattableString);
                                result = ((Task<FormattableString>)result).Result;
                            }
                            else if (genericTypes[0]==typeof(int))
                            {
                                returnType = typeof(int);
                                result = ((Task<int>)result).Result;
                            }
                        }
                        else
                        {
                            returnType = typeof(void);
                        }
                    }

                    if (returnType == typeof(FormattableString))
                    {
                        var fs = (FormattableString)result;
                        _ctx.DefaultOutputFile.Write(fs);
                    }
                    else if (returnType == typeof(string))
                    {
                        var s = (string)result;
                        _ctx.DefaultOutputFile.Write(s);
                    }
                    else if (returnType == typeof(int))
                    {
                    	if (((int)result) != 0)
                    	{
                            await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"\nExiting with non-zero result code from template ({(int)result}). Nothing saved.");
                            return (int)result;
                        }
                    }
                    else if (returnType == typeof(void)) 
                    {
                        // a void method should write directly to output streams
                    }
                    else
                        throw new NotImplementedException($"Unsupported Template Return Type {returnType.Name}");

                }
                #endregion
            }
            finally
            {
                if (_executionFolder != null)
                    Directory.SetCurrentDirectory(previousFolder);
            }

            if (_ctx.Errors.Any())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"\nError while building '{providedTemplateName}':");
                foreach (var error in _ctx.Errors)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{error}");
                return -1;
            }

            var savedFiles = _ctx.SaveToFolder(_outputFolder).SavedFiles;

            if (savedFiles.Count == 0)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Yellow, $"No files were generated");
            }
            else if (savedFiles.Count == 1)
            {
                await _logger.WriteLineAsync($"Generated {ConsoleColor.White}{savedFiles.Count}{PREVIOUS_COLOR} file: {ConsoleColor.Yellow}'{_outputFolder.TrimEnd('\\')}\\{_ctx.OutputFilesPaths.Single()}'{PREVIOUS_COLOR}");
            }
            else
            {
                await _logger.WriteLineAsync($"Generated {ConsoleColor.White}{savedFiles.Count}{PREVIOUS_COLOR} files at folder {ConsoleColor.Yellow}'{_outputFolder.TrimEnd('\\')}\\'{PREVIOUS_COLOR}{(VerboseMode ? ":" : "")}");
                if (VerboseMode)
                {
                    foreach (var f in savedFiles)
                        await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"    {f}");
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
