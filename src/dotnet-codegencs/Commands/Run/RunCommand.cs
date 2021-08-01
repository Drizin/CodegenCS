using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodegenCS.DotNetTool.Commands.Run
{
    public static class RunCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("run");
            command.AddArgument(new Argument<string>("file", description: "CS or CSX file to run. E.g. \"SimplePOCOGenerator.csx\"") { Arity = ArgumentArity.ExactlyOne }); //TODO: validate HERE if file exists

            command.Handler = CommandHandler.Create<ParseResult, RunCommandArgs>(HandleCommand);

            return command;
        }

        public class RunCommandArgs
        {
            public string File { get; set; }
        }

        static string tempFolder = null;
        static Process process = null;
        static private readonly CancellationTokenSource cts = new CancellationTokenSource();
        static ConsoleColor PreviousColor;
        static FileInfo fi;

        async static Task<int> HandleCommand(ParseResult parseResult, RunCommandArgs cliArgs)
        {
            PreviousColor = Console.ForegroundColor;
            if (!(fi = new FileInfo(cliArgs.File)).Exists && !(fi = new FileInfo(cliArgs.File + ".cs")).Exists && !(fi = new FileInfo(cliArgs.File + ".csx")).Exists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Cannot find find {cliArgs.File}");
                Console.ForegroundColor = PreviousColor;
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

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Building '{fi.Name}' (dotnet build)...");
                Console.ForegroundColor = PreviousColor;

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error while building '{fi.Name}'. ExitCode='{exitCode}'");
                    Console.ForegroundColor = PreviousColor;
                    return exitCode;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Running '{fi.Name}' (dotnet run)...");
                Console.ForegroundColor = PreviousColor;

                Console.CancelKeyPress += (s, e) =>
                {
                    cts.Cancel();
                    CleanUp(isCancelling: true);
                    Environment.Exit(-1);
                };

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
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error while executing '{fi.Name}'. ExitCode='{exitCode}'");
                    Console.ForegroundColor = PreviousColor;
                    return exitCode;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Finished executing '{fi.Name}'.");
                Console.ForegroundColor = PreviousColor;

                return exitCode;
            }
            finally
            {
                CleanUp(isCancelling: false);
            }
        }
        private static void CleanUp(bool isCancelling)
        {
            try 
            {
                if (process != null)
                {
                    if (isCancelling)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Killing process '{fi.Name}' (dotnet run)...");
                        Console.ForegroundColor = PreviousColor;
                        process?.Kill();
                    }
                    process = null;
                }
            } 
            catch { };
            try { if (tempFolder != null) Directory.Delete(tempFolder, true); tempFolder = null; } catch { };
        }

    }
}
