using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using EnvDTE;
using EnvDTE80;

namespace CodegenCS.VisualStudio.Shared.Utils
{
    internal class Utils
    {
        private const string DefaultTitle = "CodegenCS";

        public static int ShowMessage(IServiceProvider serviceProvider, string message, string title = DefaultTitle, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON button = OLEMSGBUTTON.OLEMSGBUTTON_OK)
        {
            return VsShellUtilities.ShowMessageBox(serviceProvider, message, title, icon, button, OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        public static int ShowError(IServiceProvider serviceProvider, string message, string title = "Error") => ShowMessage(serviceProvider, message, title, OLEMSGICON.OLEMSGICON_CRITICAL);

        public static Project GetActiveProject(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Array activeSolutionProjects = dte.ActiveSolutionProjects as Array;

            if (activeSolutionProjects != null && activeSolutionProjects.Length > 0)
                return activeSolutionProjects.GetValue(0) as Project;
            return null;
        }

        public static IEnumerable<ProjectItem> GetSelectedItems(DTE2 dte)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var items = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                ProjectItem item = selItem.Object as ProjectItem;

                if (item != null)
                    yield return item;
            }

            // same as dte.SelectedItems.Item(x).ProjectItem ? 
        }

        public static string GetItemPath(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return item?.Properties?.Item("FullPath")?.Value.ToString();
        }

        public static IEnumerable<string> GetSelectedItemPaths(DTE2 dte)
        {
            foreach (ProjectItem item in GetSelectedItems(dte))
                yield return GetItemPath(item);
        }

    }
}
