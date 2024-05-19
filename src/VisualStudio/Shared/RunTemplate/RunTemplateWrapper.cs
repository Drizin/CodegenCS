using CodegenCS.DotNet;
using CodegenCS.Runtime;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static CodegenCS.TemplateBuilder.TemplateBuilder;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using Task = System.Threading.Tasks.Task;
using CodegenCS.VisualStudio.Shared.Utils;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
#if VS2019
using VisualStudioPackage = CodegenCS.VisualStudio.VS2019Extension.VisualStudioPackage;
#else
using VisualStudioPackage = CodegenCS.VisualStudio.VS2022Extension.VisualStudioPackage;
#endif

namespace CodegenCS.VisualStudio.Shared.RunTemplate
{
    internal class RunTemplateWrapper : MarshalByRefObject
    {
        private DTE2 _dte { get; set; }
        static internal IVsOutputWindowPane _customPane = null;
        static private ILogger _logger = null;
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
            // Host AppDomain (VisualStudio) may need to resolve libraries embedded in our extension
            // For compatibility edition we offer to child domain all VS assemblies, and also smart matching for best version or compatible libraries
            AssemblyLoaderInitialization.Initialize();
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

        public void Run() => RunAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task RunAsync()
        {
            await _joinableTaskFactory.SwitchToMainThreadAsync();

            _errorListProvider = _errorListProvider ?? new ErrorListProvider(_serviceProvider);

            // Let's clear only errors specific to this file (or any file which was deleted/renamed), instead of clearing all tasks like errorListProvider.Tasks.Clear()
#if VS2022_OR_NEWER
            ClearPreviousErrors(_errorListProvider);
#elif VS2019_OR_OLDER
            try 
            {
                ClearPreviousErrors(_errorListProvider);
            } 
            catch (TypeLoadException) // Interop versioning hell
            {
                ClearPreviousErrorsDynamic(_errorListProvider);
            }
#endif

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
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
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
                    _customPane.Activate();
                    return -1;
                }
            });
        }

#if VS2022_OR_NEWER
        void ClearPreviousErrors(ErrorListProvider errorListProvider)
        {
            // For VS2022 edition (libraries version >=17.x) the tasks (even ErrorTask) all inherit from Microsoft.VisualStudio.Shell.TaskListItem
            foreach (var task in errorListProvider.Tasks.OfType<TaskListItem>())
            {
                if (task.Document == _templateItemPath || !File.Exists(task.Document))
                    errorListProvider.Tasks.Remove(task);
            }
        }
#elif VS2019_OR_OLDER
        void ClearPreviousErrors(ErrorListProvider errorListProvider)
        {
            // For VS2019 (libraries version 16.x) the tasks inherit from Microsoft.VisualStudio.Shell.Task
            foreach (var task in errorListProvider.Tasks.OfType<ErrorTask>().ToList())
            {
                if (task.Document == _templateItemPath || !File.Exists(task.Document))
                    errorListProvider.Tasks.Remove(task);
            }
        }
        void ClearPreviousErrorsDynamic(ErrorListProvider errorListProvider)
        {
            // However static typing (ClearPreviousErrors above) is not forward-compatible: if someone runs Compatibility Edition under VS2022+
            // then VS would redirect assembly bindings (ErrorTask would be compiled using 16.x but during runtime it would load 17.x)
            // and then the properties from the old base class (Task) wouldn't load (Task doesn't exist anymore in 17.x) and would crash (before loading this method - so we catch-retry outside):
            // "Could not load type 'Microsoft.VisualStudio.Shell.Task' from assembly 'Microsoft.VisualStudio.Shell.15.0, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'."
            // So in order to make our "Compatibility Edition" forward-compatible (work not only in VS2019 but also VS2022+), let's use dynamic typing (no static references to Task class)
            foreach (var task in errorListProvider.Tasks.OfType<ErrorTask>().Cast<dynamic>().ToList())
            {
                if ((string)task.Document == _templateItemPath || !File.Exists((string)task.Document))
                    errorListProvider.Tasks.Remove(task);
            }
        }
#endif

        async Task<TemplateBuilderResponse> BuildTemplateAsync(string itemFullPath)
        {
            var builderArgs = new CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs()
            {
                Template = new string[] { itemFullPath },
                Output = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), Path.GetFileNameWithoutExtension(itemFullPath)) + ".dll", //TODO: cache by Template-hash
                VerboseMode = false,
                ExtraReferences = new List<string>() { typeof(VSExecutionContext).GetTypeInfo().Assembly.Location } // CodegenCS.Runtime.VisualStudio
            };

            TemplateBuilderResponse builderResult = null;

            // Latest VS2022 can run TemplateBuilder (Roslyn Microsoft.CodeAnalysis.CSharp 4.2) in same AppDomain without conflict
#if VS2022_OR_NEWER
            var builder = new TemplateBuilder.TemplateBuilder(_logger, builderArgs);
            builderResult = await builder.ExecuteAsync();

            // Older versions need to run in isolated process
#else



            var isolatedScope = VisualStudioPackage.IsolatedAppDomainWrapper.Value;
            if (isolatedScope.Loader == null)
            {
                // All assemblies loaded by Visual Studio should be available to child AppDomain
                var hostAssemblies = AssemblyLoaderInitialization.GetCurrentAssemblies();

                // Since AssembliesLoader is [Serializable] we just create it here and pass it to child AppDomain:
                var loader = new AssembliesLoader(hostAssemblies);
                isolatedScope.Loader ??= loader;
                // if AssembliesLoader was MarshalByRefObject then we would create it directly in the child AppDomain: var loader = isolatedScope.Create<AssembliesLoader>(hostAssemblies); 
                // else (not [Serializable] nor MarshalByRefObject) then we could use callback AppDomainSetup.AppDomainInitializer to deserialize hostAssemblies and create a new AssembliesLoader
            }

            // Remote Proxies:
            // VsOutputWindowPaneOutputLogger should be a remote proxy. Can't be local (in the calling AppDomain) because FormattableString can't be serialized from the child AppDomain
            var crossDomainLogger = isolatedScope.Create<VsOutputWindowPaneOutputLogger>(_customPane);
                
            // TemplateBuilder should run in isolated AppDomain to avoid conflicting packages (we use recent Roslyn 4.2 version, and old Visual Studio versions conflict with Roslyn and dependencies)
            var builder = isolatedScope.Create<TemplateBuilder.TemplateBuilder>(crossDomainLogger, builderArgs);

#pragma warning disable VSTHRD103 // Call async methods when in an async method
            builderResult = builder.Execute(); // we can't call ExecuteAsync because Task<T> is not serializable (nor inherits MarshalByRefObject), so calling async cross-domains throw SerializationException
#pragma warning restore VSTHRD103 // Call async methods when in an async method
#endif



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
            var launcherArgs = new TemplateLauncherArgs()
            {
                Template = templateDll,
                Models = new string[0],
                OutputFolder = _outputFolder,
                ExecutionFolder = _executionFolder,
                DefaultOutputFile = defaultOutputFile,
            };
            var searchPaths = new string[] { new FileInfo(_templateItemPath).Directory.FullName, _executionFolder };
            var dependencyContainer = new DependencyContainer().AddModelFactory(searchPaths);

            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _executionContext);
            dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext as VSExecutionContext);

            var launcher = new TemplateLauncher.TemplateLauncher(_logger, _codegenContext, dependencyContainer, verboseMode: false);

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
#if VS2022_OR_NEWER
            AddErrorStatic(errorMessage, category, line, column);
#elif VS2019_OR_OLDER
            try
            {
                AddErrorStatic(errorMessage, category, line, column);
            }
            catch (TypeLoadException) // Interop versioning hell
            {
                AddErrorDynamic(errorMessage, category, line, column);
            }
#endif
        }

        void AddErrorStatic(string errorMessage, TaskCategory category, int line, int column)
        {
            var newError = new ErrorTask()
            {
                ErrorCategory = TaskErrorCategory.Error,
                HierarchyItem = _hierarchyItem,
            };

            // in Microsoft.VisualStudio.Shell.15.0 v17 many of the properties below belong to the parent class "TaskListItem"
            // in Microsoft.VisualStudio.Shell.15.0 v16 those properties are in new parent class "Task"
            // (As explained in ClearPreviousErrors(), Task class doesn't exist anymore in 17.x)
            // Even though we don't reference directly type "TaskListItem" or "Task", using those properties with static typing requires those class to exist (breaks forward binary compatibility)

            // VS2022 edition requires VS2022 so it will always have 17.x+ libraries and therefore we don't need dynamic typing
            // VS2019 edition running under VS2019 should also work with this static types (static references to parent class)

            newError.Category = category;
            newError.Text = errorMessage;
            newError.Document = _templateItemPath;
            newError.Line = line;
            newError.Column = column;


            newError.Navigate += (s2, e2) =>
            {
                newError.Line++; // TaskProvider.Navigate bug? it goes to PREVIOUS line. And column is ignored.
                _errorListProvider.Navigate(newError, Guid.Parse(EnvDTE.Constants.vsViewKindCode));
                newError.Line--;
            };
            _errorListProvider.Tasks.Add(newError);
        }
#if VS2019_OR_OLDER
        void AddErrorDynamic(string errorMessage, TaskCategory category, int line, int column)
        {
            var newError = new ErrorTask()
            {
                ErrorCategory = TaskErrorCategory.Error,
                HierarchyItem = _hierarchyItem,
            };

            // VS2019 edition running under VS2022 requires dynamic typing (no static references to Task class, so it's forward-compatible binary-compatible with future versions):
            ((dynamic)newError).Category = category;
            ((dynamic)newError).Text = errorMessage;
            ((dynamic)newError).Document = _templateItemPath;
            ((dynamic)newError).Line = line;
            ((dynamic)newError).Column = column;
            ((dynamic)newError).Navigate += new EventHandler((s2, e2) =>
            {
                ((dynamic)newError).Line++; // TaskProvider.Navigate bug? it goes to PREVIOUS line. And column is ignored.
                _errorListProvider.Navigate(newError, Guid.Parse(EnvDTE.Constants.vsViewKindCode));
                ((dynamic)newError).Line--;
            });

            ((dynamic)_errorListProvider.Tasks).Add(newError); // TaskCollection.Add(TaskListItem) vs TaskCollection.Add(Task)
        }
#endif


    }
}
