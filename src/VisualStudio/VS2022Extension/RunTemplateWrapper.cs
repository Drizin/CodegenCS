using CodegenCS;
using CodegenCS.DotNet;
using CodegenCS.Runtime;
using TemplateLauncher = CodegenCS.TemplateLauncher.TemplateLauncher;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static CodegenCS.TemplateBuilder.TemplateBuilder;

namespace RunTemplate
{
    internal class RunTemplateWrapper
    {
        private DTE2 _dte { get; set; }
        static internal IVsOutputWindowPane _customPane = null;
        static private VsOutputWindowPaneOutputLogger _logger = null;
        private static ErrorListProvider _errorListProvider;
        private IServiceProvider _serviceProvider;
        private JoinableTaskFactory _joinableTaskFactory;
        private DotNetCodegenContext _codegenContext;
        private ProjectItem _templateItem;
        private string _templateItemPath;
        private string _outputFolder;
        private string _executionFolder;
        private IVsHierarchy _hierarchyItem;
        private VSExecutionContext _executionContext;

        static RunTemplateWrapper()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        public static void Init()
        {
            _customPane ??= CreateCustomPane();
            _logger ??= new VsOutputWindowPaneOutputLogger(_customPane);
        }

        internal RunTemplateWrapper(DTE2 dte, JoinableTaskFactory joinableTaskFactory, IServiceProvider serviceProvider, ProjectItem templateItem, string templateItemPath, string executionFolder, string outputFolder, IVsHierarchy hierarchyItem, VSExecutionContext executionContext)
        {
            _dte = dte;
            _joinableTaskFactory = joinableTaskFactory;
            _serviceProvider = serviceProvider;
            _templateItem = templateItem;
            _templateItemPath = templateItemPath;
            _outputFolder = outputFolder;
            _executionFolder = executionFolder;
            _hierarchyItem = hierarchyItem;
            _codegenContext = new DotNetCodegenContext();
            _executionContext = executionContext;
        }

        private static IVsOutputWindowPane CreateCustomPane()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            //https://stackoverflow.com/questions/1094366/how-do-i-write-to-the-visual-studio-output-window-in-my-custom-tool
            IVsOutputWindow outWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Guid codegenCSOutputPane = new Guid("51946BB0-B39E-4DB6-9995-46D0DAC2FB73"); // for General pane we would look for Microsoft.VisualStudio.VSConstants.GUID_OutWindowGeneralPane;
            string customTitle = "CodegenCS Template";
            outWindow.CreatePane(ref codegenCSOutputPane, customTitle, 1, 1);
            IVsOutputWindowPane customPane;
            outWindow.GetPane(ref codegenCSOutputPane, out customPane);            
            return customPane;
        }

        public async Task RunAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            _errorListProvider = _errorListProvider ?? new ErrorListProvider(_serviceProvider);
            //_errorListProvider.Tasks.Clear();
            // let's clear only errors specific to this file (or any file which was deleted/renamed)
            var tasks = _errorListProvider.Tasks.OfType<ErrorTask>().ToList();
            foreach(var task in tasks)
            {
                if (task.Document == _templateItemPath || !File.Exists(task.Document))
                    _errorListProvider.Tasks.Remove(task);
            }

            // TODO: improve async calls
            // check https://github.com/Microsoft/vs-threading/blob/main/doc/cookbook_vs.md#how-to-write-a-fire-and-forget-method-responsibly
            // check https://stackoverflow.com/questions/50108631/what-is-the-proper-usage-of-joinabletaskfactory-runasync
            _ = _joinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    var builderResult = await BuildTemplateAsync(_templateItemPath);

                    if (builderResult.ReturnCode != 0)
                    {
                        foreach (var error in builderResult.CompilationErrors)
                            AddError(error.Message, TaskCategory.BuildCompile, error.Line ?? 0, error.Column ?? 0);
                        _dte.ToolWindows.ErrorList.ShowErrors = true;
                        _errorListProvider.Show();
                        //_errorListProvider.ForceShowErrors(); ?
                        _errorListProvider.BringToFront();
                        return builderResult.ReturnCode;
                    }

                    string defaultOutputFile = Path.GetFileNameWithoutExtension(_templateItemPath) + ".generated.cs";
                    string templateDll = builderResult.TargetFile;
                    var runResult = await RunTemplateAsync(templateDll, defaultOutputFile);
                    if (runResult != 0)
                    {
                        _customPane.OutputStringThreadSafe($"CodegenCS - error running template\r\n");
                        AddError($"CodegenCS - error running template\r\n", TaskCategory.Misc, -1, -1);
                        _dte.ToolWindows.ErrorList.ShowErrors = true;
                        _errorListProvider.Show();
                        //_errorListProvider.ForceShowErrors(); ?
                        _errorListProvider.BringToFront();
                        return runResult;
                    }
                    _customPane.OutputStringThreadSafe("CodegenCS - run template successfully finished. Updating Solution Explorer tree...\r\n");

                    await AddFilesToSolutionAsync(_outputFolder, _templateItem);
                    _customPane.OutputStringThreadSafe("CodegenCS - finished updating Solution Explorer.\r\n");
                    return 0;
                }
                catch (Exception ex)
                {
                    _customPane.OutputStringThreadSafe($"CodegenCS - error running template: {ex.GetBaseException().Message}\r\n");
                    return -1;
                }
            });
        }

        async Task<TemplateBuilderResponse> BuildTemplateAsync(string itemFullPath)
        {
            var builderArgs = new CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
            {
                Template = new string[] { itemFullPath },
                Output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(itemFullPath)) + ".dll", //TODO: cache by Template-hash
                VerboseMode = false,
            };
            var builder = new CodegenCS.TemplateBuilder.TemplateBuilder(_logger, builderArgs);
            var builderResult = await builder.ExecuteAsync();

            if (builderResult.ReturnCode != 0)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _customPane.OutputStringThreadSafe("TemplateBuilder (dotnet-codegencs template build) Failed.\r\n");
            }
            _customPane.OutputStringThreadSafe("\r\n");
            await Task.Delay(1); // let UI refresh
            return builderResult;
        }
        async Task<int> RunTemplateAsync(string templateDll, string defaultOutputFile)
        {
            var launcherArgs = new CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs()
            {
                Template = templateDll,
                Models = new string[0],
                OutputFolder = _outputFolder,
                ExecutionFolder = _executionFolder,
                DefaultOutputFile = defaultOutputFile,
            };

            var dependencyContainer = new DependencyContainer().AddModelFactory();

            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _executionContext);
            dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext as VSExecutionContext);

            var launcher = new TemplateLauncher(_logger, _codegenContext, dependencyContainer, verboseMode: false);

            int statusCode;
            try
            {
                statusCode = await launcher.LoadAndExecuteAsync(launcherArgs, null);
            }
            catch (Exception ex)
            {
                AddError($"CodegenCS - error running template: {ex.GetBaseException().Message}\r\n", TaskCategory.Misc, -1, -1);
                _dte.ToolWindows.ErrorList.ShowErrors = true;
                _errorListProvider.Show();
                //_errorListProvider.ForceShowErrors(); ?
                _errorListProvider.BringToFront();
                return -3;
            }

            if (_codegenContext?.Errors.Any() == true)
            {
                foreach (var error in _codegenContext.Errors)
                    AddError(error, TaskCategory.Misc, -1, -1);
                _dte.ToolWindows.ErrorList.ShowErrors = true;
                _errorListProvider.Show();
                //_errorListProvider.ForceShowErrors(); ?
                _errorListProvider.BringToFront();
                return statusCode;
            }
            
            if (statusCode != 0)
            {
                if (statusCode != -2) // invalid template args has already shown the help page
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    _customPane.OutputStringThreadSafe("TemplateLauncher (dotnet-codegencs template run) Failed.\r\n");
                }
            }
            return statusCode;
        }

        async Task AddFilesToSolutionAsync(string outputFolder, ProjectItem parentItem)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            parentItem.ExpandView();

            var currentNestedItems = parentItem.ProjectItems.Cast<ProjectItem>().Select(x => new {
                Item = x,
                #pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread
                Path = x.Properties.Item("FullPath")?.Value?.ToString()
                #pragma warning restore VSTHRD010 // Invoke single-threaded types on Main thread
            }).ToList();

            var existingPaths = currentNestedItems.Select(i => i.Path).ToHashSet();
            var generatedPaths = _codegenContext.OutputFiles.Select(f => Path.GetFullPath(Path.Combine(outputFolder, f.RelativePath))).ToHashSet();
            foreach (var item in currentNestedItems)
            {
                if (!generatedPaths.Contains(item.Path))
                {
                    item.Item.Remove();
                }
            }

            foreach (var o in _codegenContext.OutputFiles)
            {
                // DotNetCodegenContext automatically sets FileType based on file extension.
                // BuildActionType.Compile (cs/vb/fs), EmbeddedResource (resx), or BuildActionType.None for everything else.
                // Templates may override those types and may use the "NonProjectItem" which is for outputs that should NOT be automatically added to the csproj

                string fullPath = Path.GetFullPath(Path.Combine(outputFolder, o.RelativePath));

                switch (o.FileType)
                {
                    case BuildActionType.NonProjectItem:
                        // for SDK-style project we may have to explicitly "<Compile Remove>" the files from csproj, depending on the extension?
                        continue;
                    case BuildActionType.Compile:
                    case BuildActionType.EmbeddedResource:
                    case BuildActionType.None:
                    case BuildActionType.Content:
                    default:
                        ProjectItem outputItem = currentNestedItems
                                            .Where(i =>
                                                i.Path == fullPath
                                            ).Select(i => i.Item).FirstOrDefault();
                        if (outputItem == null)
                        {
                            outputItem = parentItem.ProjectItems.AddFromFile(fullPath);
                            await Task.Delay(1); // let UI refresh
                        }
                        if (outputItem.Properties.Item("DependentUpon").Value.ToString() != parentItem.Name)
                            outputItem.Properties.Item("DependentUpon").Value = parentItem.Name; // TODO: check if DependentUpon works for old non-sdk-style. If needed check https://github.com/madskristensen/FileNesting 
                        if (outputItem.Properties.Item("ItemType").Value.ToString() != o.FileType.ToString())
                            outputItem.Properties.Item("ItemType").Value = o.FileType.ToString();
                        break;
                }
            }
        }

        void AddError(string errorMessage, TaskCategory category, int line, int column)
        {
            var newError = new ErrorTask()
            {
                ErrorCategory = TaskErrorCategory.Error,
                Category = category,
                Text = errorMessage,
                Document = _templateItemPath,
                Line = line,
                Column = column,
                HierarchyItem = _hierarchyItem
            };


            newError.Navigate += (s2, e2) =>
            {
                newError.Line++; // TaskProvider.Navigate bug? it goes to PREVIOUS line. And column is ignored.
                _errorListProvider.Navigate(newError, Guid.Parse(EnvDTE.Constants.vsViewKindCode));
                newError.Line--;
            };
            _errorListProvider.Tasks.Add(newError);
        }

        #region VSIX runs .NET Framework - we have to hack the loading .NET Standard Transitive Dependencies
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "CodegenCS.Models.DbSchema")
                return typeof(CodegenCS.Models.DbSchema.DatabaseSchema).Assembly;
            if (name.Name == "CodegenCS.Models")
                return typeof(CodegenCS.Models.IInputModel).Assembly;
            if (name.Name == "CodegenCS.Core")
                return typeof(CodegenCS.ICodegenContext).Assembly;
            if (name.Name == "InterpolatedColorConsole")
                return typeof(InterpolatedColorConsole.ColoredConsole).Assembly;
            if (name.Name.Contains("CoreLib"))
            {
                var asm = Assembly.LoadFrom(@"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\5.0.17\System.Private.CoreLib.dll");
                return asm;
                //return typeof(object).GetTypeInfo().Assembly; // FOR VS Extension (.net framework) this is mscorlib 4.0.0.0
            }
            if (name.Name == "System.CommandLine.NamingConventionBinder")
            {
                return typeof(System.CommandLine.NamingConventionBinder.BindingContextExtensions).Assembly;
            }
            if (name.Name == "System.CommandLine")
            {
                return typeof(System.CommandLine.Argument).Assembly;
            }
            return null;
        }
        #endregion



    }
}
