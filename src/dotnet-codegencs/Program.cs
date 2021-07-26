using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimplePOCOGenerator = CodegenCS.DbSchema.Templates.SimplePOCOGenerator;
using EFCoreGenerator = CodegenCS.DbSchema.Templates.EFCoreGenerator;

namespace CodegenCS.DotNetTool
{
    class Program
    {
        private readonly string _toolCommandName = "codegencs";
        string tempFolder = null;
        Process process = null;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        static int Main(string[] args)
        {
            try
            {
                return new Program().Run(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
                // use /debug to see full stack trace
                if (args.Any(a => a.ToLower().TrimStart('/').TrimStart('-').TrimStart('-') == "debug"))
                    Console.WriteLine(ex.GetBaseException().ToString());
                else
                    Console.WriteLine(ex.GetBaseException().Message);
                Console.ForegroundColor = previousColor;
                return -1;
            }
        }

        public async Task<int> Run(string[] args)
        {
            Console.CancelKeyPress += (s, e) =>
            {
                cts.Cancel();
                CleanUp();
                Environment.Exit(-1);
            };

            #region Command-Line Arguments
            if (args.Length >= 1)
            {
                switch (args[0].ToLower())
                {
                    case "dbschema-extractor":
                    case "extract-dbschema":
                        return new DbSchema.Extractor.Program("codegencs " + args[0]).Run(args.Skip(1).ToArray());
                    case "poco":
                    case "simplepocogenerator":
                        return new SimplePOCOGenerator.Program("codegencs " + args[0]).Run(args.Skip(1).ToArray());
                    case "efcoregenerator":
                        return new EFCoreGenerator.Program("codegencs " + args[0]).Run(args.Skip(1).ToArray());
                    case "run":
                        if (args.Length >= 2)
                        {
                            FileInfo fi;
                            if (!(fi = new FileInfo(args[1])).Exists && !(fi = new FileInfo(args[1] + ".cs")).Exists && !(fi = new FileInfo(args[1] + ".csx")).Exists)
                            {
                                var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"Cannot find find {args[1]}");
                                Console.ForegroundColor = previousColor;
                                return -1;
                            }
                            string csProj = new StreamReader(typeof(Program).Assembly.GetManifestResourceStream("CodegenCS.DotNetTool.Resources.RunStandaloneCode.csproj")).ReadToEnd();
                            csProj = csProj.Replace("<!-- includes -->", $"<Compile Include=\"{fi.FullName}\" />");
                            tempFolder = Path.Combine(fi.DirectoryName, "." + fi.Name + ".tmp");
                            string csProjFile = Path.Combine(tempFolder, fi.Name + ".csproj");
                            if (!Directory.Exists(tempFolder))
                                Directory.CreateDirectory(tempFolder);
                            try
                            {
                                File.WriteAllText(csProjFile, csProj);

                                var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Building '{fi.Name}' (dotnet build)...");
                                Console.ForegroundColor = previousColor;

                                process = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "dotnet.exe",
                                        Arguments = "build --verbosity quiet " + csProjFile,
                                        UseShellExecute = false,
                                        WorkingDirectory = fi.DirectoryName
                                    }
                                };
                                process.Start();
                                await process.WaitForExitAsync(cts.Token);
                                int exitCode = process.ExitCode;
                                process = null;
                                if (exitCode != 0)
                                {
                                    previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error while building '{fi.Name}'. ExitCode='{exitCode}'");
                                    Console.ForegroundColor = previousColor;
                                    return exitCode;
                                }

                                previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Running '{fi.Name}' (dotnet run)...");
                                Console.ForegroundColor = previousColor;

                                process = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "dotnet.exe",
                                        Arguments = "run --project " + csProjFile,
                                        UseShellExecute = false,
                                        WorkingDirectory = fi.DirectoryName
                                    }
                                };
                                process.Start();
                                await process.WaitForExitAsync(cts.Token);
                                exitCode = process.ExitCode;
                                process = null;

                                if (exitCode != 0)
                                {
                                    previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine($"Error while executing '{fi.Name}'. ExitCode='{exitCode}'");
                                    Console.ForegroundColor = previousColor;
                                    return exitCode;
                                }

                                previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Finished executing '{fi.Name}'.");
                                Console.ForegroundColor = previousColor;

                                return exitCode;
                            }
                            finally
                            {
                                CleanUp();
                            }
                        }
                        return 0;
                }
            }

            ShowUsage();
            return 0;
            #endregion
        }

        private void CleanUp()
        {
            try { process?.Kill(); process = null; } catch { };
            try { if (tempFolder != null) Directory.Delete(tempFolder, true); tempFolder = null; } catch { };
        }

        private void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} extract-dbschema [/parameters | /?]", _toolCommandName));
            Console.WriteLine(string.Format("Usage: {0} simplepocogenerator [/parameters | /?]", _toolCommandName));
            Console.WriteLine(string.Format("Usage: {0} efcoregenerator [/parameters | /?]", _toolCommandName));
            //TODO: Console.WriteLine(string.Format("Usage: {0} <othermodule> [/parameters | /?]", _toolCommandName));
        }
    }
}
