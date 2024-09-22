using InterpolatedColorConsole.SpecialSymbols;
using Microsoft.Build.Framework;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodegenCS.MSBuild
{
    public class MSBuildLogger : CodegenCS.Runtime.ILogger
    {
        Microsoft.Build.Utilities.Task _task;
        public MSBuildLogger(Microsoft.Build.Utilities.Task task) 
        {
            _task = task;
        }
        
        public Task WriteLineAsync()
        {
            _task.Log.LogMessage("");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(FormattableString value)
        {
            _task.Log.LogMessage(MessageImportance.High, RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(RawString value)
        {
            _task.Log.LogMessage(MessageImportance.High, value.ToString());
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            _task.Log.LogMessage(MessageImportance.High, RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            _task.Log.LogMessage(MessageImportance.High, RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, RawString value)
        {
            _task.Log.LogMessage(MessageImportance.High, value.ToString());
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            _task.Log.LogMessage(MessageImportance.High, value.ToString());
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync()
        {
            _task.Log.LogError("");
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(FormattableString value)
        {
            _task.Log.LogError(RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(RawString value)
        {
            _task.Log.LogError(value.ToString());
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            _task.Log.LogError(RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            _task.Log.LogError(RemoveColors(value));
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, RawString value)
        {
            _task.Log.LogError(value.ToString());
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            _task.Log.LogError(value.ToString());
            return Task.CompletedTask;
        }

        protected string RemoveColors(FormattableString value)
        {
            StringBuilder writer = new StringBuilder();
            var arguments = value.GetArguments();
            var matches = _formattableArgumentRegex.Matches(value.Format);
            int lastPos = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                // unescape escaped curly braces
                string literal = value.Format.Substring(lastPos, matches[i].Index - lastPos).Replace("{{", "{").Replace("}}", "}");
                lastPos = matches[i].Index + matches[i].Length;
                writer.Append(literal);
                // arguments[i] may not work because same argument can be used multiple times
                int argPos = int.Parse(matches[i].Groups["ArgPos"].Value);
                string argFormat = matches[i].Groups["Format"].Value;
                object arg = arguments[argPos];

                InnerWriteFormattableArgument(writer, arg, argFormat);
            }
            string lastPart = value.Format.Substring(lastPos).Replace("{{", "{").Replace("}}", "}");
            writer.Append(lastPart);
            return writer.ToString();
        }
        
        // CodegenCS currently uses InterpolatedColorConsole - this is just to strip colors, as MSBuild Tasks can't write coloors 
        //TODO: replace by Spectre.Console
        private void InnerWriteFormattableArgument(StringBuilder writer, object arg, string format)
        {
            if (arg == null)
                return;
            if (arg is ConsoleColor && (format == "background" || format == "bg"))
            {
                Console.BackgroundColor = (ConsoleColor)arg;
            }
            else if (arg is ConsoleColor)
            {
                Console.ForegroundColor = (ConsoleColor)arg;
            }
            else if (arg is RestorePreviousBackgroundColor)
            {
            }
            else if (arg is RestorePreviousColor)
            {
            }
            else if (arg is IFormattable)
                writer.Append(((IFormattable)arg).ToString(format, System.Globalization.CultureInfo.InvariantCulture));
            else
                writer.Append(arg.ToString());
        }


        private static Regex _formattableArgumentRegex = new Regex(
              "{(?<ArgPos>\\d*)(:(?<Format>[^}]*))?}",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

    }
}
