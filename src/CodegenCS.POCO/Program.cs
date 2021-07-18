using System;
using System.IO;

namespace CodegenCS.POCO
{
    public class Program
    {
        // Helpers to get the location of the current CS file
        public static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;
        public static string GetScriptFolder([System.Runtime.CompilerServices.CallerFilePath] string path = null) => System.Diagnostics.Debugger.IsAttached ? Path.GetDirectoryName(path) : System.IO.Directory.GetCurrentDirectory();

        private string _commandLine { get; set; }
        public Program(string commandLine)
        {
            _commandLine = commandLine;
        }

        static void Main(string[] args)
        {
            new Program(System.AppDomain.CurrentDomain.FriendlyName).Run(args);
        }
        public void Run(string[] args)
        { 
            #region Command-Line Arguments
            var argsParser = new Helpers.CommandLineArgsParser(args);
            if (argsParser["?"] != null || argsParser["help"] != null)
            {
                ShowUsage();
                Environment.Exit(0);
            }
            #endregion

            //string outputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), @".\AdventureWorksSchema.json"));
            var options = new SimplePOCOGeneratorOptions(
                inputJsonSchema: argsParser["input"],
                targetFolder: argsParser["targetFolder"],
                pocosNamespace: argsParser["namespace"]
                );
            SimplePOCOGeneratorConsoleHelper.GetOptions(options); // if args were not provided, ask in Console
            var generator = new SimplePOCOGenerator(options);

            generator.Generate();
        }

        void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} [/input=jsonschema] [/targetfolder=folder] [/namespace=namespace]", _commandLine));
        }

    }
}
