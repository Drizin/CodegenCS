using System;
using System.IO;
using System.Linq;
using static InterpolatedColorConsole.Symbols;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using CodegenCS.Runtime;
using System.Collections.Generic;
using CodegenCS.Tools.TemplateDownloader;

namespace CodegenCS.TemplateDownloader
{
    public class TemplateDownloader
    {
        protected ILogger _logger;
        protected TemplateDownloaderArgs _args;

        public TemplateDownloader(ILogger logger, TemplateDownloaderArgs args)
        {
            _logger = logger;
            _args = args;
        }

        /// <summary>
        /// Template Builder options.
        /// </summary>
        public class TemplateDownloaderArgs
        {
            /// <summary>
            /// Path(s) for input CS file(s) that will be downloaded
            /// </summary>
            public string Origin { get; set; }

            /// <summary>
            /// Path for output file (Folder and/or Filename)
            /// If folder is not provided then file is saved in current folder
            /// If filename is not provided then file is named like the origin file 
            /// (e.g.MyTemplate.cs will be cloned locally with same name MyTemplate.cs)
            /// </summary>
            public string Output { get; set; }

            public bool AllowUntrustedOrigin { get; set; } = false;

            public bool VerboseMode { get; set; }
        }

        public class TemplateDownloaderResponse
        {
            public int ReturnCode { get; set; }
            public string DownloadedTemplate { get; set; }
        }

        private static Regex _githubRegex = new Regex(@"(?<Scheme>http[s]?://)?github.com/(?<User>[^/]*)/(?<Project>[^/]*)(/(?<Path>.*))?",
            RegexOptions.IgnoreCase
            | RegexOptions.Singleline
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        public async Task<TemplateDownloaderResponse> ExecuteAsync()
        {
            string path;
            try
            {
                path = await GetDownloadUrl(_args.Origin);
                path = await FixGithubUrl(path);
            }
            catch (UntrustedTemplateOriginException)
            {
                return new TemplateDownloaderResponse() { ReturnCode = -1 };
            }

            string outputFolder = Directory.GetCurrentDirectory();
            string outputFileName = path.ToString().Substring(path.ToString().LastIndexOf("/") + 1);
            if (!string.IsNullOrWhiteSpace(_args.Output))
            {
                if (_args.Output.Contains(Path.DirectorySeparatorChar) && _args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = Path.GetFullPath(_args.Output);
                else if (_args.Output.Contains(Path.DirectorySeparatorChar) && !_args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = new FileInfo(Path.GetFullPath(_args.Output)).Directory.FullName;
                if (!_args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFileName = Path.GetFileName(_args.Output);
            }
            var targetFile = Utils.IOUtils.MakeRelativePath(Path.Combine(outputFolder, outputFileName));
            if (_args.VerboseMode)
                await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"Output file '{ConsoleColor.Cyan}{targetFile}{PREVIOUS_COLOR}'");

            if (_args.VerboseMode)
                await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"Downloading from '{ConsoleColor.Cyan}{path.ToString()}{PREVIOUS_COLOR}'");


            var wc = new WebClient();
            try
            {
                wc.DownloadFile(path.ToString(), targetFile);
                await _logger.WriteLineAsync(ConsoleColor.Green, $"Template {ConsoleColor.White}'{path.ToString()}'{PREVIOUS_COLOR} was successfully saved into {ConsoleColor.White}'{targetFile}'{PREVIOUS_COLOR}");
            }
            catch (Exception ex)
            {
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not download Template Script {ConsoleColor.Yellow}'{path.ToString()}'{PREVIOUS_COLOR}");
                await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: {ex.Message}");
                return new TemplateDownloaderResponse() { ReturnCode = -1 };
            }


            return new TemplateDownloaderResponse() { ReturnCode = 0, DownloadedTemplate = targetFile };

        }

        private async Task<string> GetDownloadUrl(string origin)
        {
            if (origin.ToLower().StartsWith("http://") || origin.ToLower().StartsWith("https://"))
            {
                // Rule #1: accept fully qualified urls like
                // http(s)://domain.com/folder/file.cs or
                // https://github.com/user/project/file.cs
                return origin;
            }

            if (!(origin.Contains(".") || origin.Contains(":") || origin.Contains(@"\") || origin.Contains("/")))
            {
                // if it's a single word without any symbols, check the templates index
                var wc = new WebClient();
                var catalogUrl = "https://raw.githubusercontent.com/CodegenCS/Templates/main/templates-index.json";
                try
                {
                    string indexContents = wc.DownloadString(catalogUrl);
                    var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, TemplateMetadata>>(indexContents);
                    dic = new Dictionary<string, TemplateMetadata>(dic, StringComparer.OrdinalIgnoreCase);
                    if (!dic.ContainsKey(origin))
                    {
                        await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not find {ConsoleColor.Yellow}'{origin}'{PREVIOUS_COLOR} in Templates Catalog");
                        throw new Exception();
                    }
                    return $"https://github.com/CodegenCS/Templates/{dic[origin].Uri.TrimStart('/')}";
                }
                catch (Exception ex)
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Could not download look up Templates Catalog {ConsoleColor.Yellow}'{catalogUrl}'{PREVIOUS_COLOR}");
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"ERROR: {ex.Message}");
                    throw new Exception();
                }
                //var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, TemplateMetadata>>(File.ReadAllText(@"D:\Repositories\CodegenCS.Templates\templates-index.json"));
                //dic = new Dictionary<string, TemplateMetadata>(index, StringComparer.OrdinalIgnoreCase);
            }

            StringBuilder urlBuilder = new StringBuilder();
            // Rule #2: ignore missing schema - accept like:
            // github.com/user/project/file.cs or 
            // domain.com/folder/file.cs
            urlBuilder.Append("https://");

            // If if starts with forward-slash or if there's no dot until the first slash, then there's no domain.
            if (origin.StartsWith("/") || !origin.Contains("/") || (origin.Contains("/") && !origin.Substring(0, origin.IndexOf("/")).Contains(".")))
            {
                // Rule #3: accept "GithubProject/Template.cs" as a shortcut to github.com/CodegenCS/Templates/GithubProject/Template.cs

                urlBuilder.Append("github.com/CodegenCS/Templates/");
            }

            urlBuilder.Append(origin.TrimStart('/'));

            return urlBuilder.ToString();
        }
        private async Task<string> FixGithubUrl(string path)
        {
            Match match;
            if ((match = _githubRegex.Match(path)) != null && match.Success)
            {
                string githubUser = match.Groups["User"].Value;
                string githubProject = match.Groups["Project"].Value;
                string githubPath = match.Groups["Path"].Value;


                // Rule #4: if it's specified a folder (or project) in Github but it's not specified a file (no extension) we treat it as a shortcut to the cs file with the same name as the folder.
                // e.g. accept github.com/GithubUser/GithubProject as a shortcut to github.com/GithubUser/GithubProject/GithubProject.cs
                // e.g. accept                       GithubProject as a shortcut to github.com/CodegenCS/Templates/GithubProject/GithubProject.cs

                string lastPart;
                if (string.IsNullOrWhiteSpace(githubPath))
                    lastPart = githubProject;
                else if (githubPath.Contains("/"))
                    lastPart = githubPath.Substring(githubPath.LastIndexOf("/") + 1);
                else
                    lastPart = githubPath.TrimStart('/').TrimEnd('/');
                if (!lastPart.ToLower().EndsWith(".cs") && !lastPart.ToLower().EndsWith(".csx"))
                {
                    githubPath = githubPath.TrimEnd('/') + "/" + lastPart + ".cs";
                }

                path = $"https://raw.githubusercontent.com/{githubUser}/{githubProject}/main/{githubPath}";

                if (!_args.AllowUntrustedOrigin && !githubUser.Equals("CodegenCS", StringComparison.OrdinalIgnoreCase))
                {
                    await _logger.WriteLineAsync(ConsoleColor.Yellow, $"Warning: third-party template origin '{ConsoleColor.Cyan}https://github.com/{githubUser}{PREVIOUS_COLOR}' is unknown / untrusted.");
                    await _logger.WriteLineAsync(ConsoleColor.Yellow, $"You can skip this question using the option \"--allow-untrusted-origin\"");
                    ConsoleKey response;
                    do
                    {
                        await _logger.WriteLineAsync(ConsoleColor.Yellow, $"Are you sure you want to download this template? Press \"y\" for Yes or \"n\" for No.");
                        response = Console.ReadKey(false).Key;   // true is intercept key (dont show), false is show
                        if (response != ConsoleKey.Enter)
                            Console.WriteLine();

                    } while (response != ConsoleKey.Y && response != ConsoleKey.N);
                    if (response != ConsoleKey.Y)
                        throw new UntrustedTemplateOriginException();
                }
            }
            return path;
        }

    }
    internal class UntrustedTemplateOriginException : Exception
    {
    }
    
}
