using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodegenCS.Runtime
{
    /// <summary>
    /// CommandLineArgs class can be injected in Templates (or any other class) and provide an array of strings all other arguments/options that were not recognized/captured by dotnet-codegencs.
    /// Template constructor can validate those arguments and throw ArgumentException for invalid arguments, so that dotnet-codegencs shows the error message.
    /// If besides the error message (exception message shown in red) you want to show a custom help message (e.g. show all command-line options, use colors, etc)
    /// you can use <see cref="CommandLineArgsException" />
    /// 
    /// Other options for passing command-line arguments to templates are <see cref="IAutoBindCommandLineArgs"/> (see <see cref="AutoBindCommandLineArgsTypeResolver"/>)
    /// or specifying a "public static void ConfigureCommand(Command command)" (see TemplateLauncher)
    /// </summary>
    public class CommandLineArgs : IReadOnlyList<string>
    {
        private readonly ReadOnlyCollection<string> _args;
        public CommandLineArgs(IEnumerable<string> args)
        {
            _args = new ReadOnlyCollection<string>(args.ToList());
        }
        public string this[int index] => _args[index];

        public int Count => _args.Count;

        public IEnumerator<string> GetEnumerator() => _args.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _args.GetEnumerator();
    }
}
