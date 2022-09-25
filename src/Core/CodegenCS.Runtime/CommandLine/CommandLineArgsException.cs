using System;

namespace CodegenCS.Runtime
{
    /// <summary>
    /// Templates are resolved using dependency injection. If during the template construction (resolving) dotnet-codegencs get an ArgumentException
    /// it will show the error message. CommandLineArgsException extends ArgumentException and allows to write a custom help message (with colors etc).
    /// </summary>
    public class CommandLineArgsException : ArgumentException
    {
        public Action<ILogger> ShowHelp { get; private set; }
        public CommandLineArgsException(string errorMessage, Action<ILogger> showHelp) : base(errorMessage)
        {
            ShowHelp = showHelp;
        }
    }
}
