#pragma warning disable RS1035 // Do not use APIs banned for analyzers
using CodegenCS.DotNet;
using CodegenCS.Runtime;
using CodegenCS.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CodegenCS.TemplateBuilder.TemplateBuilder;
using static CodegenCS.TemplateLauncher.TemplateLauncher;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;

namespace CodegenCS.CodeGenerator
{
    [Generator]
    public class CodegenGenerator : ISourceGenerator
    {
        //How to Debug Roslyn Code Generator: Project - Properties, Debug, Launch profiles UI, Delete existing one, ADD new and select type ROSLYN COMPONENT
        // If debugger doesn't even launch probably some dependency is missing in the nupkg (fusion++ might help to identify what's missing): C:\ProgramData\chocolatey\lib\fusionplusplus\tools\Fusion++.exe

        #region Errors/Warnings
        private static readonly DiagnosticDescriptor BuildFailure =
            new DiagnosticDescriptor(id: "CODEGENCS001",
                                    title: "Failed to build CodegenCS Template",
                                    messageFormat: "Failed to build CodegenCS Template '{0}': '{1}'",
                                    category: "Design",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor BuildFailureWithLine =
            new DiagnosticDescriptor(id: "CODEGENCS002",
                                    title: "Failed to build CodegenCS Template",
                                    messageFormat: "Failed to build CodegenCS Template '{0}' (line {1} column {2}): '{3}'",
                                    category: "Design",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true);
        private static readonly DiagnosticDescriptor ExecutionFailure =
            new DiagnosticDescriptor(id: "CODEGENCS003",
                                    title: "Failed to run CodegenCS Template",
                                    messageFormat: "Failed to run CodegenCS Template '{0}': '{1}'",
                                    category: "Design",
                                    DiagnosticSeverity.Error,
                                    isEnabledByDefault: true);
        #endregion

        ILogger _logger = new DebugOutputLogger(); // TODO: create logger that can write to DiagnosticDescriptor using Info severity
        DotNetCodegenContext _codegenContext;
        string _executionFolder;
        string _outputFolder;
        ExecutionContext _codegenExecutionContext;
        GeneratorExecutionContext _executionContext;

        public void Initialize(GeneratorInitializationContext initializationContext)
        {
        }

        public void Execute(GeneratorExecutionContext executionContext)
        {
            try
            {
                _executionContext = executionContext;

                string[] validExtensions = new string[] { ".csx", ".cs", ".cgcs" };
                foreach (AdditionalText template in executionContext.AdditionalFiles)
                {
                    executionContext.AnalyzerConfigOptions.GetOptions(template).TryGetValue("build_metadata.AdditionalFiles.CodegenCSOutput", out var outputType);
                    if (string.IsNullOrEmpty(Path.GetExtension(template.Path)) || !validExtensions.Contains(Path.GetExtension(template.Path).ToLower()))
                        continue;
                    if (outputType != null && 
                        !outputType.Equals("File", StringComparison.InvariantCultureIgnoreCase) &&
                        !outputType.Equals("Memory", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, template.Path, $"Invalid Output: {outputType}"));
                        continue;
                    }

                    _codegenContext = new DotNetCodegenContext();

                    // templateDir
                    _executionFolder = new FileInfo(template.Path).Directory.FullName;
                    _outputFolder = _executionFolder;

                    //TODO: CodegenCS.Runtime.VisualStudio should multitarget to netstandard
                    //_codegenExecutionContext = new VSExecutionContext(template.Path, projectPath, solutionPath);
                    _codegenExecutionContext = new ExecutionContext(template.Path, _executionFolder);

                    try
                    {
                        if (executionContext.CancellationToken.IsCancellationRequested) //TODO: pass cancellationToken down the rabbit hole
                            return;
                        var builderResult = BuildTemplateAsync(template.Path).ConfigureAwait(false).GetAwaiter().GetResult();

                        if (builderResult.ReturnCode != 0)
                        {
                            foreach (var error in builderResult.CompilationErrors)
                            {
                                if (error.Line == null && error.Column == null)
                                    _executionContext.ReportDiagnostic(Diagnostic.Create(BuildFailure, Location.None, (object)template.Path, error.Message));
                                else
                                    _executionContext.ReportDiagnostic(Diagnostic.Create(BuildFailureWithLine, Location.None, (object)template.Path, error.Line, error.Column, error.Message));
                            }
                            continue; // template failures are non-fatal // TODO: configuration flag to make them fatal (throw and abort the whole compilation)
                        }

                        //TODO: this class should RegisterForSyntaxNotifications and then forward events to any templates that implements ISyntaxReceiver
                        // (is CodeGenerator singleton? or else we would have to keep a single list of templates)

                        if (executionContext.CancellationToken.IsCancellationRequested)
                            return;
                        string defaultOutputFile = Path.GetFileNameWithoutExtension((string)template.Path) + ".g.cs";
                        string templateDll = builderResult.TargetFile;
                        var runResult = RunTemplateAsync(template.Path, templateDll, defaultOutputFile, outputType).ConfigureAwait(false).GetAwaiter().GetResult(); // TODO: incremental generators have async support

                        if (runResult != 0)
                        {
                            _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, (object)template.Path, "Unknown error"));
                            continue;
                        }
                        // TODO: Info "CodegenCS - run template successfully finished. Files generated: etc..

                        if (executionContext.CancellationToken.IsCancellationRequested)
                            return;

                        // Adds auto-generated sources to the compilation output (adds in-memory, doesn't save to disk!)
                        if (outputType.Equals("Memory", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var o in _codegenContext.OutputFiles)
                            {
                                _executionContext.AddSource($"{Path.GetFileName(o.RelativePath)}", SourceText.From(o.GetContents(), Encoding.UTF8));
                                var path = Path.Combine(_outputFolder, o.RelativePath);
                                // Deleting in case this file was previously generated with "File" and then switch to "Memory".
                                if (File.Exists(path))
                                    File.Delete(path);
                            }
                        }
                        // If not "Memory" then it's "Disk", and RunTemplateAsync will have already saved the output files.
                        // If I remember correctly, at some point saving to disk (under same project) was causing a loop because it was retriggering analyzer (not sure why it's not happening anymore)
                    }
                    catch (Exception ex)
                    {
                        _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, (object)template.Path, ex.Message));
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, "", ex.StackTrace));
            }
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
                    typeof(CodegenGenerator).GetTypeInfo().Assembly.Location, // CodegenCS.SourceGenerator - no idea why I have to pass this down to roslyn, according to fusion looks like somehow Microsoft.CodeAnalysis is trying to load this
                    typeof(CodegenCS.Runtime.AbstractLogger).GetTypeInfo().Assembly.Location, // CodegenCS.Runtime
                    typeof(GeneratorExecutionContext).GetTypeInfo().Assembly.Location, // Microsoft.CodeAnalysis
                    typeof(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax).GetTypeInfo().Assembly.Location, // Microsoft.CodeAnalysis.CSharp
                    typeof(System.Collections.Immutable.ImmutableArray).GetTypeInfo().Assembly.Location, // System.Collections.Immutable
                }
            };

            TemplateBuilderResponse builderResult = null;
            // Latest VS2022 can run TemplateBuilder (Roslyn Microsoft.CodeAnalysis.CSharp 4.2) in same AppDomain without conflict
            //TODO: Does VS2019 also works? Maybe we have to run in isolated process, liks we do in VS2019 Extension
            var builder = new TemplateBuilder.TemplateBuilder(_logger, builderArgs);

            builderResult = await builder.ExecuteAsync();

            return builderResult;
        }

        async Task<int> RunTemplateAsync(string templateItemPath, string templateDll, string defaultOutputFile, string outputType)
        {
            var launcherArgs = new TemplateLauncherArgs()
            {
                Template = templateDll,
                Models = new string[0],
                OutputFolder = _outputFolder,
                ExecutionFolder = _executionFolder,
                DefaultOutputFile = defaultOutputFile,
                SaveOutput = outputType.Equals("File", StringComparison.InvariantCultureIgnoreCase)
            };
            var searchPaths = new string[] { new FileInfo(templateItemPath).Directory.FullName, _executionFolder };
            var dependencyContainer = new DependencyContainer().AddModelFactory(searchPaths);

            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _codegenExecutionContext);
            //dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext as VSExecutionContext);
            dependencyContainer.RegisterSingleton<GeneratorExecutionContext>(() => _executionContext);

            var launcher = new TemplateLauncher.TemplateLauncher(_logger, _codegenContext, dependencyContainer, verboseMode: false);

            int statusCode;
            try
            {
                statusCode = await launcher.LoadAndExecuteAsync(launcherArgs, null);
            }
            catch (Exception ex)
            {
                _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, templateItemPath, $"CodegenCS - error running template: {ex.GetBaseException().Message}"));
                return -3;
            }

            if (_codegenContext?.Errors.Any() == true)
            {
                foreach (var error in _codegenContext.Errors)
                    _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, templateItemPath, $"CodegenCS - error running template: {error}"));
                return statusCode;
            }

            if (statusCode != 0)
            {
                _executionContext.ReportDiagnostic(Diagnostic.Create(ExecutionFailure, Location.None, templateItemPath, $"CodegenCS - error running template"));
            }
            return statusCode;
        }


    }
}
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
