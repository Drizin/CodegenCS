using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodegenCS.Runtime
{
    public class DebugOutputLogger : AbstractLogger, ILogger
    {
        protected override Task InnerWriteAsync(string message)
        {
            System.Diagnostics.Debug.Write(message);
            return Task.CompletedTask;
        }

        protected override Task InnerWriteNewLineAsync()
        {
            System.Diagnostics.Debug.WriteLine("");
            return Task.CompletedTask;
        }

        protected override Task RefreshUIAsync() => Task.CompletedTask;

        protected override Task SetBackgroundColorAsync(ConsoleColor color) => Task.CompletedTask;

        protected override Task SetForegroundColorAsync(ConsoleColor color) => Task.CompletedTask;

    }
}
