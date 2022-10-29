using CodegenCS;
using CodegenCS.Runtime;
using CodegenCS.Utils;
using InterpolatedColorConsole.SpecialSymbols;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RunTemplate
{
    internal class VsOutputWindowPaneOutputLogger : ILogger
    {
        protected IVsOutputWindowPane _windowPane;
        internal VsOutputWindowPaneOutputLogger(IVsOutputWindowPane windowPane)
        {
            _windowPane = windowPane;
        }

        public async Task WriteLineAsync()
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _windowPane.OutputStringThreadSafe(Environment.NewLine);
            await RefreshUIAsync();
        }

        public async Task WriteLineAsync(FormattableString value)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            WriteInterpolatedString(value);
            _windowPane.OutputStringThreadSafe(Environment.NewLine);
            await RefreshUIAsync();
        }

        public async Task WriteLineAsync(RawString value)
        {
            //ThreadHelper.ThrowIfNotOnUIThread();
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            _windowPane.OutputStringThreadSafe(value);
            _windowPane.OutputStringThreadSafe(Environment.NewLine);
            await RefreshUIAsync();
        }

        public async Task WriteLineAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineAsync(ConsoleColor foregroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync()
        {
            await WriteLineAsync();
        }

        public async Task WriteLineErrorAsync(FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(RawString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }

        private async Task RefreshUIAsync()
        {
            await Task.Delay(1);
        }


        #region Interpolated Strings Parsing and Stripping Embedded colors
        /// <summary>
        /// <see cref="CodegenCS.TemplateBuilder"/> and <see cref="CodegenCS.TemplateLauncher"/> write to IOutput which for the CLI is is an adapter to to <see cref="InterpolatedColorConsole.ColoredConsole"/> .
        /// This adapter (used only in VS Extensions) write to <see cref="IVsOutputWindowPane"/> and therefore it should strip embedded colors.
        /// </summary>
        private void WriteInterpolatedString(FormattableString value)
        {
            var arguments = value.GetArguments();
            var matches = _formattableArgumentRegex.Matches(value.Format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string literal = value.Format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                lastPos = matches[i].Index + matches[i].Length;
                _windowPane.OutputStringThreadSafe(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];

                InnerWriteFormattableArgument(arg, argFormat);
            }
            string lastPart = value.Format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            _windowPane.OutputStringThreadSafe(lastPart);
        }

        private void InnerWriteFormattableArgument(object arg, string format)
        {
            if (arg is ConsoleColor && (format == "background" || format == "bg"))
            {
                //_previousBackgroundColors.Push(Console.BackgroundColor);
                //Console.BackgroundColor = (ConsoleColor)arg;
            }
            else if (arg is ConsoleColor)
            {
                //_previousForegroundColors.Push(Console.ForegroundColor);
                //Console.ForegroundColor = (ConsoleColor)arg;
            }
            else if (arg is RestorePreviousBackgroundColor)
            {
                //if (_previousBackgroundColors.Count > 0)
                //    Console.BackgroundColor = _previousBackgroundColors.Pop();
            }
            else if (arg is RestorePreviousColor)
            {
                //if (_previousForegroundColors.Count > 0)
                //    Console.ForegroundColor = _previousForegroundColors.Pop();
            }
            else if (arg is IFormattable)
                _windowPane.OutputStringThreadSafe(((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
            else
                _windowPane.OutputStringThreadSafe(arg.ToString());
        }

        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d+)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

        #endregion

    }
}
