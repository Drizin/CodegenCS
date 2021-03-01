using System;
using System.IO;

namespace CodegenCS.DbSchema.Extractor
{
    class Program
    {
        // Helpers to get the location of the current CS file
        public static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;
        public static string GetScriptFolder([System.Runtime.CompilerServices.CallerFilePath] string path = null) => System.Diagnostics.Debugger.IsAttached ? Path.GetDirectoryName(path) : System.IO.Directory.GetCurrentDirectory();

        static void Main(string[] args)
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
            var wizard = new ExtractWizard();
            if (argsParser["postgresql"] != null)
                wizard.DbType = ExtractWizard.DbTypeEnum.PostgreSQL;
            else if (argsParser["mssql"] != null)
                wizard.DbType = ExtractWizard.DbTypeEnum.MSSQL;
            wizard.OutputJsonSchema = argsParser["output"];
            wizard.ConnectionString = argsParser["cn"];


            wizard.Run();
        }

        static void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} [/postgresql | /mssql] [/cn=connectionString] [/output=path]", System.AppDomain.CurrentDomain.FriendlyName));
            Console.WriteLine(string.Format(""));
            Console.WriteLine(string.Format("Examples: {0} /postgresql /cn=\"Host=localhost; Database=Adventureworks; Username=postgres; Password=myPassword\" /output=AdventureWorks.json", System.AppDomain.CurrentDomain.FriendlyName));
            Console.WriteLine(string.Format("Examples: {0} /mssql /cn=\"Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=myPassword;\" /output=AdventureWorks.json", System.AppDomain.CurrentDomain.FriendlyName));
        }

    }
}
