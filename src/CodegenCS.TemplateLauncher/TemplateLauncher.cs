using CodegenCS.___InternalInterfaces___;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;
using static InterpolatedColorConsole.Symbols;
using System.CommandLine.Parsing;

namespace CodegenCS.TemplateLauncher
{
    public class TemplateLauncher
    {
        protected FileInfo inputFile;
        protected FileInfo[] modelFiles;

        public TemplateLauncher()
        {
        }

        public class RunCommandArgs
        {
            public string Template { get; set; }
            public string[] Models { get; set; }
            public string OutputFolder { get; set; }
            public string DefaultOutputFile { get; set; }
        }

        public int HandleCommand(ParseResult parseResult, RunCommandArgs cliArgs)
        {
            bool verboseMode = (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--verbose"));

            if (!((inputFile = new FileInfo(cliArgs.Template)).Exists || (inputFile = new FileInfo(cliArgs.Template + ".dll")).Exists))
            {
                Console.WriteLineError(ConsoleColor.Red, $"Cannot find find Template DLL {cliArgs.Template}");
                return -1;
            }
            
            modelFiles = new FileInfo[cliArgs.Models.Length];
            for (int i = 0; i < cliArgs.Models.Length; i++)
            {
                string model = cliArgs.Models[i];
                if (model != null)
                {
                    if (!((modelFiles[i] = new FileInfo(model)).Exists || (modelFiles[i] = new FileInfo(model + ".json")).Exists || (modelFiles[i] = new FileInfo(model + ".yaml")).Exists))
                    {
                        Console.WriteLineError(ConsoleColor.Red, $"Cannot find find model {model}");
                        return -1;
                    }
                }
            }


            string outputFolder = Directory.GetCurrentDirectory();
            string defaultOutputFile = Path.GetFileNameWithoutExtension(inputFile.Name) + ".cs";
            if (!string.IsNullOrWhiteSpace(cliArgs.OutputFolder))
                outputFolder = Path.GetFullPath(cliArgs.OutputFolder);
            defaultOutputFile = cliArgs.DefaultOutputFile;

            using (var consoleContext = Console.WithColor(ConsoleColor.Cyan))
            {
                System.Console.CancelKeyPress += (s, e) =>
                {
                    Console.WriteLineError(ConsoleColor.Red, $"Stopping 'dotnet template run...'");
                    consoleContext.RestorePreviousColor();
                    //Environment.Exit(-1); CancelKeyPress will do it automatically since we didn't set e.Cancel to true
                };

                Console.WriteLine(ConsoleColor.Green, $"Loading {ConsoleColor.Yellow}'{inputFile.Name}'{PREVIOUS_COLOR}...");


                var asm = Assembly.LoadFile(inputFile.FullName);

                if (asm.GetName().Version?.ToString() != "0.0.0.0")
                    Console.WriteLine($"{ConsoleColor.Cyan}{inputFile.Name}{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{asm.GetName().Version}{PREVIOUS_COLOR}");

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
                for (int i= 0; i < interfacesPriority.Length && entryPointClass == null; i++)
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
                    Console.WriteLineError(ConsoleColor.Red, $"Could not find template entry-point in '{inputFile.Name}'.");
                    return -1;
                }

                MethodInfo entryPointMethod = foundInterface.GetMethod("Render", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public);

                Console.WriteLine(ConsoleColor.Cyan, $"Template entry-point: {ConsoleColor.White}'{entryPointClass.Name}.{entryPointMethod.Name}()'{PREVIOUS_COLOR}...");


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
                        Console.WriteLine(ConsoleColor.Cyan, $"Model{i + 1} type is {ConsoleColor.White}'{modelType.FullName}'{PREVIOUS_COLOR}...");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLineError(ConsoleColor.Red, $"Could not get type for Model{i + 1}: {ex.Message}");
                        return -1;
                    }
                    try
                    {
                        object model = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(modelFiles[i].FullName), modelType);
                        Console.WriteLine(ConsoleColor.Cyan, $"Model{i + 1} successfuly loaded from {ConsoleColor.White}'{modelFiles[i].Name}'{PREVIOUS_COLOR}...");
                        args.Add(model);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLineError(ConsoleColor.Red, $"Could not get type for Model{i + 1}: {ex.Message}");
                        return -1;
                    }
                }


                var instance = Activator.CreateInstance(entryPointClass);

                var ctx = (ICodegenContext)new CodegenContext();
                ctx.DefaultOutputFile.RelativePath = defaultOutputFile;

                if (iTypeTemplate == typeof(IBaseMultifileTemplate)) // pass ICodegenContext
                {
                    args.Insert(0, ctx);
                    entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
                }
                else if (iTypeTemplate == typeof(IBaseSinglefileTemplate)) // pass ICodegenTextWriter
                {
                    args.Insert(0, ctx.DefaultOutputFile);
                    entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
                }
                else if (iTypeTemplate == typeof(IBaseStringTemplate)) // get the FormattableString and write to DefaultOutputFile
                {
                    FormattableString fs = (FormattableString)entryPointClass.GetMethod(entryPointMethod.Name).Invoke(instance, args.ToArray()); //TODO: search by parameters, not only by name
                    ctx.DefaultOutputFile.Write(fs);
                }

                if (ctx.Errors.Any())
                {
                    using (Console.WithColor(ConsoleColor.Red))
                    {
                        Console.WriteLineError($"\nError while building '{inputFile.Name}':");
                        foreach (var error in ctx.Errors)
                            Console.WriteLineError($"{error}");
                        return -1;
                    }
                }

                int savedFiles = ctx.SaveFiles(outputFolder);

                Console.Write($"Generated {ConsoleColor.White}{savedFiles}{PREVIOUS_COLOR} files into folder {ConsoleColor.Yellow}'{outputFolder}'{PREVIOUS_COLOR}").WriteLine(verboseMode ? ":":"");
                if (verboseMode)
                {
                    foreach (var f in ctx.OutputFiles)
                        Console.WriteLine(ConsoleColor.DarkGray, "    " + Path.Combine(outputFolder, f.RelativePath));
                    Console.WriteLine();
                }

                Console.WriteLine(ConsoleColor.Green, $"Successfully executed template {ConsoleColor.Yellow}'{inputFile.Name}'{PREVIOUS_COLOR}.");

                return 0;
            }

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
