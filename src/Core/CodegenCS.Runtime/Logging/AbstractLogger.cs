using InterpolatedColorConsole.SpecialSymbols;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Task = System.Threading.Tasks.Task;


namespace CodegenCS.Runtime
{
    /// <summary>
    /// This is an abstract implementation of ILogger which requires just a few overrides (like writing a message to output, or changing colors).
    /// Loggers without color-support (like Visual Studio Output Window) can just ignore the colors part.
    /// </summary>
    public abstract class AbstractLogger : MarshalByRefObject, ILogger
    {
        #region Abstract Methods
        protected abstract Task InnerWriteAsync(string message);
        protected abstract Task InnerWriteNewLineAsync();
        protected abstract Task SetForegroundColorAsync(ConsoleColor color);
        protected abstract Task SetBackgroundColorAsync(ConsoleColor color);
        protected abstract Task RefreshUIAsync();
        #endregion

        #region Tracking of Previous Colors written
        internal Stack<ConsoleColor> _previousForegroundColors = new Stack<ConsoleColor>();
        internal Stack<ConsoleColor> _previousBackgroundColors = new Stack<ConsoleColor>();
        #endregion

        public AbstractLogger()
        {
        }

        #region ILogger
        public async Task WriteLineAsync()
        {
            await InnerWriteNewLineAsync();
            await RefreshUIAsync();
        }

        public async Task WriteLineAsync(FormattableString value)
        {
            await WriteInterpolatedStringAsync(value);
            await InnerWriteNewLineAsync();
            await RefreshUIAsync();
        }

        public async Task WriteLineAsync(RawString value)
        {
            await WriteLineAsync(value);
        }


        public async Task WriteLineAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineAsync(ConsoleColor foregroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }


        public async Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
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

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            await WriteLineAsync(value);
        }

        public async Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            await WriteLineAsync(value);
        }
        #endregion

        #region Interpolated Strings Parsing and Stripping Embedded colors
        private async Task WriteInterpolatedStringAsync(FormattableString value)
        {
            var arguments = value.GetArguments();
            var matches = _formattableArgumentRegex.Matches(value.Format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string literal = value.Format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                lastPos = matches[i].Index + matches[i].Length;
                await InnerWriteAsync(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];

                await InnerWriteFormattableArgumentAsync(arg, argFormat);
            }
            string lastPart = value.Format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            await InnerWriteAsync(lastPart);
        }

        private async Task InnerWriteFormattableArgumentAsync(object arg, string format)
        {
            if (arg is ConsoleColor && (format == "background" || format == "bg"))
            {
                _previousBackgroundColors.Push(Console.BackgroundColor);
                await SetBackgroundColorAsync((ConsoleColor)arg);
            }
            else if (arg is ConsoleColor)
            {
                _previousForegroundColors.Push(Console.ForegroundColor);
                await SetForegroundColorAsync((ConsoleColor)arg);
            }
            else if (arg is RestorePreviousBackgroundColor)
            {
                if (_previousBackgroundColors.Count > 0)
                {
                    await SetBackgroundColorAsync(_previousBackgroundColors.Pop());
                }
            }
            else if (arg is RestorePreviousColor)
            {
                if (_previousForegroundColors.Count > 0)
                {
                    await SetForegroundColorAsync(_previousForegroundColors.Pop());
                }
            }
            else if (arg is IFormattable)
                await InnerWriteAsync(((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
            else
                await InnerWriteAsync(arg.ToString());
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
