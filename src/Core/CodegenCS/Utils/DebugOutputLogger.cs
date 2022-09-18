using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodegenCS.Utils
{
    internal class DebugOutputLogger : ILogger
    {
        public Task WriteLineAsync()
        {
            System.Diagnostics.Debug.WriteLine("");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(FormattableString value)
        {
            WriteInterpolatedString(value);
            System.Diagnostics.Debug.WriteLine("");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(RawString value)
        {
            System.Diagnostics.Debug.WriteLine(value);
            return Task.CompletedTask;
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


        #region Interpolated Strings Parsing and Stripping Embedded colors
        /// <summary>
        /// CodegenCS.TemplateBuilder and CodegenCS.TemplateLauncher write to IOutput which for the CLI is is an adapter to to InterpolatedColorConsole.ColoredConsole.
        /// This adapter (used only for debugging inside Visual Studio) writes to Diagnostics and therefore it should strip embedded colors.
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
                System.Diagnostics.Debug.Write(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];

                InnerWriteFormattableArgument(arg, argFormat);
            }
            string lastPart = value.Format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            System.Diagnostics.Debug.Write(lastPart);
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
            else if (arg.GetType().Name == "RestorePreviousBackgroundColor")
            {
                //if (_previousBackgroundColors.Count > 0)
                //    Console.BackgroundColor = _previousBackgroundColors.Pop();
            }
            else if (arg.GetType().Name == "RestorePreviousColor")
            {
                //if (_previousForegroundColors.Count > 0)
                //    Console.ForegroundColor = _previousForegroundColors.Pop();
            }
            else if (arg is IFormattable)
                System.Diagnostics.Debug.Write(((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
            else
                System.Diagnostics.Debug.Write(arg.ToString());
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
