using System;
using System.Linq;

namespace CodegenCS.DotNetTool
{
    class Program
    {
        private readonly string _toolCommandName = "codegencs";

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        public void Run(string[] args)
        {
            #region Command-Line Arguments
            var moduleArgs = args.ToList();
            if (args.Any(a => a.ToLower() == "dbschema-extractor"))
            {
                moduleArgs.RemoveAll(a => a.ToLower() == "dbschema-extractor");
                new DbSchema.Extractor.Program("codegencs dbschema-extractor").Run(moduleArgs.ToArray());
                Environment.Exit(0);
            }
            if (args.Any(a => a.ToLower() == "poco"))
            {
                moduleArgs.RemoveAll(a => a.ToLower() == "poco");
                new POCO.Program("codegencs poco").Run(moduleArgs.ToArray());
                Environment.Exit(0);
            }

            ShowUsage();
            #endregion
        }

        private void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} dbschema-extractor [/parameters | /?]", _toolCommandName));
            Console.WriteLine(string.Format("Usage: {0} poco [/parameters | /?]", _toolCommandName));
            Console.WriteLine(string.Format("Usage: {0} <othermodule> [/parameters | /?]", _toolCommandName));
        }
    }
}
