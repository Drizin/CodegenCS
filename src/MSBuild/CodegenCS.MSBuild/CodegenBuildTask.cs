using CodegenCS.DotNet;
using CodegenCS.Runtime;
using CodegenCS.Runtime.Reflection;
using CodegenCS.Utils;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static CodegenCS.TemplateBuilder.TemplateBuilder;
using static CodegenCS.TemplateLauncher.TemplateLauncher;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;
using ILogger = CodegenCS.Runtime.ILogger;

namespace CodegenCS.MSBuild
{
    public class CodegenBuildTask : Microsoft.Build.Utilities.Task
    {
        #region Errors/Warnings
        private static readonly string BuildFailure = "Failed to build CodegenCS Template '{0}': '{1}'";
        private static readonly string BuildFailureWithLine = "Failed to build CodegenCS Template '{0}' (line {1} column {2}): '{3}'";
        private static readonly string ExecutionFailure = "Failed to run CodegenCS Template '{0}': '{1}'";
        #endregion

        private readonly List<ITaskItem> _compileFiles = new List<ITaskItem>();
        private readonly List<ITaskItem> _contentFiles = new List<ITaskItem>();
        private readonly List<ITaskItem> _embeddedResourceFiles = new List<ITaskItem>();
        private readonly List<ITaskItem> _noneFiles = new List<ITaskItem>();
        //TODO: Support "Pages" and other types

        [Output] public ITaskItem[] CompileFiles => _compileFiles.ToArray();
        [Output] public ITaskItem[] ContentFiles => _contentFiles.ToArray();
        [Output] public ITaskItem[] EmbeddedResourceFiles => _embeddedResourceFiles.ToArray();
        [Output] public ITaskItem[] NoneFiles => _noneFiles.ToArray();


        public string ProjectFilePath { get; set; }
        public string SolutionFilePath { get; set; }
        public ITaskItem[] CodegenTemplates { get; set; }

        protected ILogger _logger;

        DotNetCodegenContext _codegenContext;
        string _executionFolder;
        string _outputFolder;
        ExecutionContext _codegenExecutionContext;
        
        public CodegenBuildTask()
        {
            _logger = new MSBuildLogger(this);
        }

        public override bool Execute()
        {
            // If msbuild is invoked directly on the csproj then $(SolutionPath) will be "*Undefined*", in which case you can specify it using /property:SolutionDir="solution directory"
            if (SolutionFilePath == "*Undefined*")
                SolutionFilePath = null;

            if (!string.IsNullOrEmpty(ProjectFilePath))
                Log.LogMessage($"Project: {ProjectFilePath}");
            if (!string.IsNullOrEmpty(SolutionFilePath))
                Log.LogMessage($"Project: {SolutionFilePath}");

            try
            {
                //string[] validExtensions = new string[] { ".csx", ".cs", ".cgcs" };

                if (CodegenTemplates == null || CodegenTemplates.Length == 0)
                    return true;

                var templates = CodegenTemplates
                    .Select(x => x.GetMetadata("FullPath"))
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();

                foreach (var templatePath in templates)
                {
                    //TODO: args.WaitForDebugger
                    //while (!System.Diagnostics.Debugger.IsAttached)
                    //    System.Threading.Thread.Sleep(100);
                    Log.LogMessage(MessageImportance.High, $"Building Template \"{templatePath}\"");
                    _codegenContext = new DotNetCodegenContext();

                    // templateDir
                    _outputFolder = new FileInfo(templatePath).Directory.FullName;

                    try
                    {
                        var builderResult = BuildTemplateAsync(templatePath).ConfigureAwait(false).GetAwaiter().GetResult();

                        if (builderResult.ReturnCode != 0)
                        {
                            foreach (var error in builderResult.CompilationErrors)
                            {
                                if (error.Line == null && error.Column == null)
                                    Log.LogError(BuildFailure, (object) templatePath, error.Message);
                                else
                                    Log.LogError(BuildFailureWithLine, (object)templatePath, error.Line, error.Column, error.Message);
                            }
                            return false; // template build failures are fatal // TODO: configuration flag to make them non-fatal
                        }

                        string defaultOutputFile = Path.GetFileNameWithoutExtension((string)templatePath) + ".g.cs";
                        string templateDll = builderResult.TargetFile;

                        _executionFolder = new FileInfo(templateDll).Directory.FullName;

                        //TODO: CodegenCS.Runtime.VisualStudio should multitarget to netstandard, or even better: hierarchy ProjectExecutionContext, SolutionExecutionContext, etc.
                        //_codegenExecutionContext = new VSExecutionContext(templatePath, projectPath, solutionPath);
                        _codegenExecutionContext = new ExecutionContext(templatePath, _executionFolder);

                        // When this MSBuild task is executed through dotnet build (.NET Core) it should use existing loaded assemblies (if any) to avoid type mismatches among same assembly loaded from different paths (different CodeBase)
                        // When using msbuild (.NET Framework) there's no conflict (even if using Assembly.LoadFrom(path))
                        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                            .Where(asm => !asm.IsDynamic)
                            .Select(asm => new LoadedAssemblyInfo()
                            {
                                Assembly = asm,
                                Name = asm.GetName().Name,
                                Version = asm.GetName().Version
                            }).ToList();
                        List<string> searchPaths = new List<string>() 
                        {
                            Path.GetDirectoryName(Assembly.GetAssembly(typeof(TemplateLauncher.TemplateLauncher)).Location),
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            //TODO: Add to searchPaths locations of all available dlls in loadableAssemblies folders? Or maybe only VS folders?
                        }.Distinct().ToList();
                        
                        var loader = new AssembliesLoader(loadedAssemblies, null, searchPaths);
                        AppDomain.CurrentDomain.AssemblyResolve += loader.AssemblyResolve;


                        Log.LogMessage(MessageImportance.High, $"Running Template \"{templatePath}\"");
                        var runResult = RunTemplateAsync(templatePath, templateDll, defaultOutputFile).ConfigureAwait(false).GetAwaiter().GetResult(); // TODO: incremental generators have async support

                        if (runResult != 0)
                        {
                            Log.LogError(ExecutionFailure, (object)templatePath, "Unknown error");
                            return false; // template execution failures are fatal // TODO: configuration flag to make them non-fatal
                        }
                        Log.LogMessage(MessageImportance.High, $"Successfully executed Template \"{templatePath}\"");

                        string dependentUpon = null;
                        if (!string.IsNullOrEmpty(ProjectFilePath))
                            dependentUpon = new Uri(ProjectFilePath).MakeRelativeUri(new Uri(new FileInfo(templatePath).FullName)).ToString().Replace("/", "\\");

                        foreach (var err in _codegenContext.Errors)
                            Log.LogWarning($"Template reported the following non-fatal error: \"{err}\"");

                        foreach (var o in _codegenContext.OutputFiles)
                        {
                            Log.LogMessage($"Generated file: {Path.Combine(_outputFolder, o.RelativePath)}");
                            TaskItem taskItem = new TaskItem(o.RelativePath);
                            taskItem.SetMetadata("AutoGen", "true");
                            if (dependentUpon != null)
                                taskItem.SetMetadata("DependentUpon", dependentUpon);
                            switch (o.FileType)
                            {
                                case BuildActionType.Compile:
                                    _compileFiles.Add(taskItem); break;
                                case BuildActionType.Content:
                                    _contentFiles.Add(taskItem); break;
                                case BuildActionType.EmbeddedResource:
                                    _embeddedResourceFiles.Add(taskItem); break;
                                case BuildActionType.None:
                                    _noneFiles.Add(taskItem); break;
                                case BuildActionType.NonProjectItem: // generated but not added to the compilation
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogError(ExecutionFailure, (object)templatePath, ex.Message);
                        return false; // template execution failures are fatal // TODO: configuration flag to make them non-fatal
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ExecutionFailure, "", ex.StackTrace);
            }
            return true;
        }

        async Task<TemplateBuilderResponse> BuildTemplateAsync(string itemFullPath)
        {
            var builderArgs = new CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
            {
                Template = new string[] { itemFullPath },
                //TODO: build in-memory?
                Output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(itemFullPath)) + ".dll", //TODO: cache by Template-hash
                VerboseMode = false,
                ExtraReferences = new List<string>() {
                    typeof(CodegenCS.Runtime.AbstractLogger).GetTypeInfo().Assembly.Location, // CodegenCS.Runtime
                    typeof(GeneratorExecutionContext).GetTypeInfo().Assembly.Location, // Microsoft.CodeAnalysis
                    typeof(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax).GetTypeInfo().Assembly.Location, // Microsoft.CodeAnalysis.CSharp (netstandard includes this anyway)
                    typeof(System.Collections.Immutable.ImmutableArray).GetTypeInfo().Assembly.Location, // System.Collections.Immutable
                }
            };

            TemplateBuilderResponse builderResult = null;
            // Latest VS2022 can run TemplateBuilder (Roslyn Microsoft.CodeAnalysis.CSharp 4.2) in same AppDomain without conflict
            //TODO: Does VS2019 also works? Maybe we have to run in isolated process, liks we do in VS2019 Extension
            // or maybe just create a self-hosted webserver providing build services
            var builder = new TemplateBuilder.TemplateBuilder(_logger, builderArgs);

            builderResult = await builder.ExecuteAsync();

            return builderResult;
        }

        async Task<int> RunTemplateAsync(string templateItemPath, string templateDll, string defaultOutputFile)
        {
            var launcherArgs = new TemplateLauncherArgs()
            {
                Template = templateDll,
                Models = new string[0],
                OutputFolder = _outputFolder,
                ExecutionFolder = _executionFolder,
                DefaultOutputFile = defaultOutputFile,
            };
            var searchPaths = new string[] { new FileInfo(templateItemPath).Directory.FullName, _executionFolder };
            var dependencyContainer = new DependencyContainer().AddModelFactory(searchPaths);

            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _codegenExecutionContext);
            //dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext as VSExecutionContext);

            var launcher = new TemplateLauncher.TemplateLauncher(_logger, _codegenContext, dependencyContainer, verboseMode: false);

            int statusCode;
            try
            {
                statusCode = await launcher.LoadAndExecuteAsync(launcherArgs, null);
            }
            catch (Exception ex)
            {
                Log.LogError(ExecutionFailure, templateItemPath, $"CodegenCS - error running template: {ex.GetBaseException().Message}");
                Log.LogError(ExecutionFailure, templateItemPath, $"CodegenCS - error running template: {ex.ToString()}");
                return -3;
            }

            if (_codegenContext?.Errors.Any() == true)
            {
                foreach (var error in _codegenContext.Errors)
                    Log.LogError(ExecutionFailure, templateItemPath, $"CodegenCS - error running template: {error}");
                return statusCode;
            }

            if (statusCode != 0)
            {
                Log.LogError(ExecutionFailure, templateItemPath, $"CodegenCS - error running template");
            }
            return statusCode;
        }

    }
}
