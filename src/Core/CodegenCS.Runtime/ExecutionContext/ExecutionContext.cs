namespace CodegenCS.Runtime
{
    /// <summary>
    /// Provides information about the template being executed
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Full path of the Template being executed
        /// </summary>
        public string TemplatePath { get; set; } // DLL or CS? Why not have both?
        public string CurrentDirectory { get; set; }
        public ExecutionContext(string templatePath, string currentDirectory) 
        { 
            TemplatePath = templatePath;
            CurrentDirectory = currentDirectory;
        }
    }
}
