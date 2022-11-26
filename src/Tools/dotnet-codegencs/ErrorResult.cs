using System;
using System.CommandLine.Invocation;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    public class ErrorResult : IInvocationResult
    {
        private readonly string _errorMessage;
        private readonly int _errorExitCode;

        public ErrorResult(string errorMessage, int errorExitCode = 1)
        {
            _errorMessage = errorMessage;
            _errorExitCode = errorExitCode;
        }

        public void Apply(InvocationContext context)
        {
            //context.Console.Error.WriteLine(_errorMessage);
            Console.WriteLineError(ConsoleColor.Red, "ERROR: " + _errorMessage);
            context.ExitCode = _errorExitCode;
        }
    }
}
