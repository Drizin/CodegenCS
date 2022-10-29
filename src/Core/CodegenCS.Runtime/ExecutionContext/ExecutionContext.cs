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
        public string TemplatePath { get; set; }
        public ExecutionContext(string templatePath) { TemplatePath = templatePath; }
    }
}
