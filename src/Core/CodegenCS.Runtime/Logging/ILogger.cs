using System;
using System.Threading.Tasks;

namespace CodegenCS.Runtime
{
    /// <summary>
    /// Reusable tools should write all output to this interface.
    /// Templates can also use this to write to output.
    /// For CLI (dotnet-codegencs) this is implemented by an adapter that writes to InterpolatedColorConsole.ColoredConsole.
    /// For VS Extensions this is implemented by an adapter that writes to custom IVsOutputWindowPane.
    /// The implementations should handle embedded (interpolated) colors (e.g. ColoredConsole supports colors) or discard embedded colors.
    /// </summary>
    public interface ILogger
    {
        Task WriteLineAsync();
        Task WriteLineAsync(FormattableString value);
        Task WriteLineAsync(RawString value);
        Task WriteLineAsync(ConsoleColor foregroundColor, FormattableString value);
        Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value);
        Task WriteLineAsync(ConsoleColor foregroundColor, RawString value);
        Task WriteLineAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value);

        Task WriteLineErrorAsync();
        Task WriteLineErrorAsync(FormattableString value);
        Task WriteLineErrorAsync(RawString value);
        Task WriteLineErrorAsync(ConsoleColor foregroundColor, FormattableString value);
        Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, FormattableString value);
        Task WriteLineErrorAsync(ConsoleColor foregroundColor, RawString value);
        Task WriteLineErrorAsync(ConsoleColor foregroundColor, ConsoleColor backgroundColor, RawString value);
    }
}
