using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CodegenCS.Utils
{
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
