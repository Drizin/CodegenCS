using CodegenCS;
using CodegenCS.DotNet;
using CodegenCS.TemplateBuilder;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace RunTemplate
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RunTemplateCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("a4ef19db-b911-48fd-a04e-b6de7215ccfd");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private DTE2 _dte { get { return ((RunTemplatePackage)package)._dte;  }  }


        /// <summary>
        /// Initializes a new instance of the <see cref="RunTemplateCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private RunTemplateCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RunTemplateCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in RunTemplateCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new RunTemplateCommand(package, commandService);
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {

                var solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));


                var selectedItems = Utils.GetSelectedItems(_dte).ToList();
                if (selectedItems.Count() == 0)
                {
                    Utils.ShowError(package, "Should select at least one template");
                    return;
                }
                var selectedItemsPaths = selectedItems.Select(f => Utils.GetItemPath(f)).ToList();
                var extensions = selectedItemsPaths.Select(p => Path.GetExtension(p)).ToList();

                string[] validExtensions = new string[] { ".csx", ".cs", ".cgcs" };
                var invalidFiles = selectedItemsPaths.Where(p => string.IsNullOrEmpty(Path.GetExtension(p)) || !validExtensions.Contains(Path.GetExtension(p).ToLower()));
                if (invalidFiles.Any())
                {
                    Utils.ShowError(package, $"Invalid file extension ({Path.GetFileName(invalidFiles.First())}). Valid extensions are {string.Join(", ", validExtensions)}");
                    return;
                }

                RunTemplateWrapper.Init();
                RunTemplateWrapper._customPane.Clear();
                RunTemplateWrapper._customPane.Activate();
                _dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Activate();


                foreach (var templateItem in selectedItems)
                {
                    string templateItemPath = templateItem.FileNames[0]; // (string)templateItem.Properties.Item("FullPath").Value;
                    Project project = templateItem.ContainingProject; // _dte.Solution.Projects.Item(1);
                    string templateDir = new FileInfo(templateItemPath).Directory.FullName; // Path.GetDirectoryName(project.FullName) + "\\";
                    //string itemRelativePath = CodegenCS.Utils.IOUtils.MakeRelativePath(projectDir, itemFullPath);
                    //Window solutionExplorer = _dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer);
                    //UIHierarchy hierarchy = (UIHierarchy)solutionExplorer.Object;
                    var projectUniqueName = project.FileName;
                    IVsHierarchy hierarchyItem; // error tasks need to be associated to a project 
                    solution.GetProjectOfUniqueName(projectUniqueName, out hierarchyItem);

                    var runTemplateWrapper = new RunTemplateWrapper(_dte, package.JoinableTaskFactory, package, templateItem, templateItemPath, templateDir, hierarchyItem);

                    _ = package.JoinableTaskFactory.RunAsync(runTemplateWrapper.RunAsync);
                }

            }
            catch (Exception ex)
            {
                Utils.ShowError(package, (System.Diagnostics.Debugger.IsAttached ? ex.ToString() : ex.Message));
            }

        }

    }
}
