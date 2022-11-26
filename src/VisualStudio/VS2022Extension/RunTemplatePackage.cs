using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using RunTemplate.CustomToolGenerator;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace RunTemplate
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(RunTemplatePackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration("CodegenCS Code Generator - Custom Tool", "Executes a CodegenCS Template from Visual Studio", "1.0")] // this is just for package
    [ProvideCodeGenerator(typeof(CodegenCSCodeGenerator), CodegenCSCodeGenerator.CustomToolName, "Executes a CodegenCS Template from Visual Studio", true)]
    //[ProvideAutoLoad(cmdUiContextGuid: Microsoft.VisualStudio.VSConstants.UICONTEXT.SolutionHasSingleProject_string, PackageAutoLoadFlags.BackgroundLoad)]    // This makes the package auto load (always, not depending on any action). Ideally we should NOT force auto load of package (better to only initialize when required - after command is clicked). This also does not guarantee that package is loaded before context menu (and button) are loaded (but if button is clicked it's because command was attached and therefore certainly the package was loaded before)
    //[ProvideAutoLoad(cmdUiContextGuid: Microsoft.VisualStudio.VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, PackageAutoLoadFlags.BackgroundLoad)] // This makes the package auto load (always, not depending on any action). Ideally we should NOT force auto load of package (better to only initialize when required - after command is clicked). This also does not guarantee that package is loaded before context menu (and button) are loaded (but if button is clicked it's because command was attached and therefore certainly the package was loaded before)
    public sealed class RunTemplatePackage : AsyncPackage
    {
        /// <summary>
        /// RunTemplatePackage GUID string.
        /// </summary>
        public const string PackageGuidString = "ceeb4c60-193a-4506-b4e6-773ef8940f1a";

        internal EnvDTE80.DTE2 _dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunTemplatePackage"/> class.
        /// </summary>
        public RunTemplatePackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.            
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            _dte = GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2; // await GetServiceAsync(typeof(DTE)) as DTE;
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await RunTemplateCommand.InitializeAsync(this);
            //TODO: create some command to set a checkbox "Automatically rebuild template on each save", which would update item.Properties.Item("CustomTool").Value = CustomToolGenerator.RunTemplateCustomTool.CustomToolName;
        }

        #endregion
    }
}
