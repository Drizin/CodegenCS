using System;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;

namespace CodegenCS.Runtime
{
    public class VsOutputWindowPaneOutputLogger : AbstractLogger, ILogger
    {
        protected readonly IVsOutputWindowPane _windowPane;
        protected readonly JoinableTaskFactory _joinableTaskFactory;

        public VsOutputWindowPaneOutputLogger(IVsOutputWindowPane windowPane, JoinableTaskFactory joinableTaskFactory)
        {
            this._windowPane = windowPane;
            this._joinableTaskFactory = joinableTaskFactory;
        }
        public VsOutputWindowPaneOutputLogger(IVsOutputWindowPane windowPane) : this(windowPane, null)
        {
            this._windowPane = windowPane;
        }

        protected override async Task InnerWriteAsync(string message)
        {
            if (this._joinableTaskFactory != null)
                await this._joinableTaskFactory.SwitchToMainThreadAsync();
            this._windowPane.OutputStringThreadSafe(message);
        }

        protected override async Task InnerWriteNewLineAsync()
        {
            if (this._joinableTaskFactory != null)
                await this._joinableTaskFactory.SwitchToMainThreadAsync();
            this._windowPane.OutputStringThreadSafe(Environment.NewLine);
        }

        protected override async Task RefreshUIAsync() => await Task.Delay(1);

        protected override Task SetBackgroundColorAsync(ConsoleColor color) => Task.CompletedTask;

        protected override Task SetForegroundColorAsync(ConsoleColor color) => Task.CompletedTask;

    }
}
