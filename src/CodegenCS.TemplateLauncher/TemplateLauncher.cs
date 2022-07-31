using CodegenCS.___InternalInterfaces___;
using CodegenCS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static InterpolatedColorConsole.Symbols;

namespace CodegenCS.TemplateLauncher
{
    public class TemplateLauncher
    {
        protected ILogger _logger;
        protected TemplateLauncherArgs _args;
        ICodegenContext _ctx;

        public TemplateLauncher(ILogger logger, ICodegenContext ctx, TemplateLauncherArgs args)
        {
            _logger = logger;
            _args = args;
            _ctx = ctx;

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
            public string[] Models { get; set; }
            
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
            
            public bool VerboseMode { get; set; }
        }



        public async Task<int> ExecuteAsync()
        {
            FileInfo templateFile;
            if (!((templateFile = new FileInfo(_args.Template)).Exists || (templateFile = new FileInfo(_args.Template + ".dll")).Exists))
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Cannot find Template DLL {_args.Template}");
                return -1;
            }

            FileInfo[] modelFiles = new FileInfo[_args.Models.Length];
            for (int i = 0; i < _args.Models.Length; i++)
            {
                string model = _args.Models[i];
                if (model != null)
                {
                    if (!((modelFiles[i] = new FileInfo(model)).Exists || (modelFiles[i] = new FileInfo(model + ".json")).Exists || (modelFiles[i] = new FileInfo(model + ".yaml")).Exists))
                    {
                        await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Cannot find find model {model}");
                        return -1;
                    }
                }
            }

            string outputFolder = Directory.GetCurrentDirectory();
            string defaultOutputFile = Path.GetFileNameWithoutExtension(templateFile.Name) + ".cs";
            if (!string.IsNullOrWhiteSpace(_args.OutputFolder))
                outputFolder = Path.GetFullPath(_args.OutputFolder);
            if (!string.IsNullOrWhiteSpace(_args.DefaultOutputFile))
                defaultOutputFile = _args.DefaultOutputFile;


            await _logger.WriteLineAsync(ConsoleColor.Green, $"Loading {ConsoleColor.Yellow}'{templateFile.Name}'{PREVIOUS_COLOR}...");


            var asm = Assembly.LoadFile(templateFile.FullName);

            if (asm.GetName().Version?.ToString() != "0.0.0.0")
                await _logger.WriteLineAsync($"{ConsoleColor.Cyan}{templateFile.Name}{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{asm.GetName().Version}{PREVIOUS_COLOR}");

            var types = asm.GetTypes().Where(t => typeof(IBaseTemplate).IsAssignableFrom(t));
            IEnumerable<Type> types2;

            Type entryPointClass = null;

            if (entryPointClass == null && types.Count() == 1)
                entryPointClass = types.Single();

            if (entryPointClass == null && (types2 = types.Where(t => t.Name == "Main")).Count() == 1)
                entryPointClass = types2.Single();

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
            Type iTypeTemplate = null;
            for (int i = 0; i < interfacesPriority.Length && entryPointClass == null; i++)
            {
                if ((types2 = types.Where(t => IsAssignableToType(t, interfacesPriority[i]))).Count() == 1)
                {
                    entryPointClass = types2.Single();
                    foundInterface = interfacesPriority[i];

                    if (IsAssignableToType(entryPointClass, typeof(IBase1ModelTemplate<>)))
                        iBaseXModelTemplate = typeof(IBase1ModelTemplate<>);
                    else if (IsAssignableToType(entryPointClass, typeof(IBase2ModelTemplate<,>)))
                        iBaseXModelTemplate = typeof(IBase2ModelTemplate<,>);
                    else if (IsAssignableToType(entryPointClass, typeof(IBase0ModelTemplate)))
                        iBaseXModelTemplate = typeof(IBase0ModelTemplate);
                    else
                        throw new NotImplementedException();

                    if (IsAssignableToType(entryPointClass, typeof(IBaseMultifileTemplate)))
                        iTypeTemplate = typeof(IBaseMultifileTemplate);
                    else if (IsAssignableToType(entryPointClass, typeof(IBaseSinglefileTemplate)))
                        iTypeTemplate = typeof(IBaseSinglefileTemplate);
                    else if (IsAssignableToType(entryPointClass, typeof(IBaseStringTemplate)))
                        iTypeTemplate = typeof(IBaseStringTemplate);
                    else
                        throw new NotImplementedException();
                    break;
                }
            }

            //TODO: [System.Runtime.InteropServices.DllImportAttribute]

            if (entryPointClass == null)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not find template entry-point in '{templateFile.Name}'.");
                return -1;
            }

            MethodInfo entryPointMethod = foundInterface.GetMethod("Render", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);

            await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Template entry-point: {ConsoleColor.White}'{entryPointClass.Name}.{entryPointMethod.Name}()'{PREVIOUS_COLOR}...");


            int expectedModels;
            if (iBaseXModelTemplate == typeof(IBase2ModelTemplate<,>))
                expectedModels = 2;
            else if (iBaseXModelTemplate == typeof(IBase1ModelTemplate<>))
                expectedModels = 1;
            else
                expectedModels = 0;


            List<object> args = new List<object>();

            for (int i = 0; i < expectedModels; i++)
            {
                Type modelType;
                try
                {
                    modelType = entryPointClass.GetInterfaces().Where(itf => itf.IsGenericType
                        && (itf.GetGenericTypeDefinition() == typeof(IBase1ModelTemplate<>) || itf.GetGenericTypeDefinition() == typeof(IBase2ModelTemplate<,>)))
                        .Select(interf => interf.GetGenericArguments().Skip(i).First()).Distinct().Single();
                    await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Model{(expectedModels > 1 ? (i + 1).ToString():"")} type is {ConsoleColor.White}'{modelType.FullName}'{PREVIOUS_COLOR}...");
                }
                catch (Exception ex)
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not get type for Model{(expectedModels > 1 ? (i + 1).ToString() : "")}: {ex.Message}");
                    return -1;
                }
                try
                {
                    object model = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(modelFiles[i].FullName), modelType);
                    await _logger.WriteLineAsync(ConsoleColor.Cyan, $"Model{(expectedModels > 1 ? (i + 1).ToString() : "")} successfuly loaded from {ConsoleColor.White}'{modelFiles[i].Name}'{PREVIOUS_COLOR}...");
                    args.Add(model);
                }
                catch (Exception ex)
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not get type for Model{(expectedModels > 1 ? (i + 1).ToString() : "")}: {ex.Message}");
                    return -1;
                }
            }


            var instance = Activator.CreateInstance(entryPointClass);

            _ctx.DefaultOutputFile.RelativePath = defaultOutputFile;

            if (iTypeTemplate == typeof(IBaseMultifileTemplate)) // pass ICodegenContext
            {
                args.Insert(0, _ctx);
                entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
            }
            else if (iTypeTemplate == typeof(IBaseSinglefileTemplate)) // pass ICodegenTextWriter
            {
                args.Insert(0, _ctx.DefaultOutputFile);
                entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
            }
            else if (iTypeTemplate == typeof(IBaseStringTemplate)) // get the FormattableString and write to DefaultOutputFile
            {
                FormattableString fs = (FormattableString)entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
                _ctx.DefaultOutputFile.Write(fs);
            }

            if (_ctx.Errors.Any())
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"\nError while building '{templateFile.Name}':");
                foreach (var error in _ctx.Errors)
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"{error}");
                return -1;
            }

            int savedFiles = _ctx.SaveFiles(outputFolder);

            await _logger.WriteLineAsync($"Generated {ConsoleColor.White}{savedFiles}{PREVIOUS_COLOR} files into folder {ConsoleColor.Yellow}'{outputFolder}'{PREVIOUS_COLOR}{(_args.VerboseMode ? ":" : "")}");
            if (_args.VerboseMode)
            {
                foreach (var f in _ctx.OutputFiles)
                    await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"    {Path.Combine(outputFolder, f.RelativePath)}");
                await _logger.WriteLineAsync();
            }

            await _logger.WriteLineAsync(ConsoleColor.Green, $"Successfully executed template {ConsoleColor.Yellow}'{templateFile.Name}'{PREVIOUS_COLOR}.");

            return 0;
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


        #endregion
    }
}
