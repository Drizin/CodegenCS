using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodegenCS.DotNetTool
{
    class Program
    {
        private readonly string _toolCommandName = "codegencs";
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        static int Main(string[] args)
        {
            return new Program().Run(args).GetAwaiter().GetResult();
        }

        public async Task<int> Run(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
            };

            #region Command-Line Arguments
            var moduleArgs = args.ToList();
            if (args.Any(a => a.ToLower() == "dbschema-extractor"))
            {
                moduleArgs.RemoveAll(a => a.ToLower() == "dbschema-extractor");
                new DbSchema.Extractor.Program("codegencs dbschema-extractor").Run(moduleArgs.ToArray());
                return 0;
            }
            if (args.Any(a => a.ToLower() == "poco"))
            {
                moduleArgs.RemoveAll(a => a.ToLower() == "poco");
                new POCO.Program("codegencs poco").Run(moduleArgs.ToArray());
                return 0;
            }

            ShowUsage();
            return 0;
            #endregion
        }

        private void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} dbschema-extractor [/parameters | /?]", _toolCommandName));
            Console.WriteLine(string.Format("Usage: {0} poco [/parameters | /?]", _toolCommandName));
            //TODO: Console.WriteLine(string.Format("Usage: {0} <othermodule> [/parameters | /?]", _toolCommandName));
        }
    }
}
