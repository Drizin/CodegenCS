using CodegenCS.Utils;
using System;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DotNetTool
{
    internal class ColoredConsoleLogger : ILogger
    {
        public Task WriteLineAsync()
        {
            Console.WriteLine();
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(FormattableString value)
        {
            Console.WriteLine(value);
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(RawString value)
        {
            Console.WriteLine($"{value}");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            Console.WriteLine(foregroundColor, value);
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            Console.WriteLine(foregroundColor, backgroundColor, value);
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, RawString value)
        {
            Console.WriteLine(foregroundColor, $"{value}");
            return Task.CompletedTask;
        }

        public Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            Console.WriteLine(foregroundColor, backgroundColor, $"{value}");
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync()
        {
            Console.WriteLineError("");
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(FormattableString value)
        {
            Console.WriteLineError(value);
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(RawString value)
        {
            Console.WriteLineError($"{value}");
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, FormattableString value)
        {
            Console.WriteLineError(foregroundColor, value);
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value)
        {
            Console.WriteLineError(foregroundColor, backgroundColor, value);
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, RawString value)
        {
            Console.WriteLineError(foregroundColor, $"{value}");
            return Task.CompletedTask;
        }

        public Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value)
        {
            Console.WriteLineError(foregroundColor, backgroundColor, $"{value}");
            return Task.CompletedTask;
        }
    }
}
