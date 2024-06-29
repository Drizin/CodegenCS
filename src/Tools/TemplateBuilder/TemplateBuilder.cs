using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using static InterpolatedColorConsole.Symbols;
using System.Threading.Tasks;
using System.Collections.Generic;
using CodegenCS.Runtime;

namespace CodegenCS.TemplateBuilder
{
    public class TemplateBuilder : MarshalByRefObject
    {
        protected ILogger _logger;
        protected TemplateBuilderArgs _args;
        protected FileInfo[] inputFiles;

        public TemplateBuilder(ILogger logger, TemplateBuilderArgs args)
        {
            _logger = logger;
            _args = args;
        }

        /// <summary>
        /// Template Builder options.
        /// </summary>
        [Serializable]
        public class TemplateBuilderArgs
        {
            /// <summary>
            /// Path(s) for input CS file(s) that will be compiled
            /// </summary>
            public string[] Template { get; set; }

            /// <summary>
            /// Path for output DLL (Folder and/or Filename)
            /// If folder is not provided then dll is saved in current folder
            /// If filename is not provided then dll is named like the first template input file 
            /// (e.g.MyTemplate.cs will be compiled into MyTemplate.dll)
            /// </summary>
            public string Output { get; set; }

            public bool VerboseMode { get; set; }

            public List<string> ExtraNamespaces { get; set; }
            public List<string> ExtraReferences { get; set; }
        }

        [Serializable]
        public class TemplateBuilderResponse
        {
            public int ReturnCode { get; set; }
            public string TargetFile { get; set; }
            public IEnumerable<CompilationError> CompilationErrors { get; set; }
        }

        [Serializable]
        public class CompilationError
        {
            public string Message { get; set; }
            public int? Line { get; set; }
            public int? Column { get; set; }
        }

        public TemplateBuilderResponse Execute() => ExecuteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<TemplateBuilderResponse> ExecuteAsync()
        {
            inputFiles = new FileInfo[_args.Template.Length];
            for (int i=0; i < _args.Template.Length; i++)
            {
                if (!((inputFiles[i] = new FileInfo(_args.Template[i])).Exists || (inputFiles[i] = new FileInfo(_args.Template[i] + ".cs")).Exists || (inputFiles[i] = new FileInfo(_args.Template[i] + ".csx")).Exists))
                {
                    await _logger?.WriteLineErrorAsync(ConsoleColor.Red, $"Cannot find Template Script {ConsoleColor.Yellow}'{_args.Template[i]}'{PREVIOUS_COLOR}");
                    return new TemplateBuilderResponse() { ReturnCode = -1 };
                }
            }

            string outputFolder = Directory.GetCurrentDirectory();
            string outputFileName = Path.GetFileNameWithoutExtension(inputFiles[0].Name) + ".dll";
            if (!string.IsNullOrWhiteSpace(_args.Output))
            {
                if (_args.Output.Contains(Path.DirectorySeparatorChar) && _args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = Path.GetFullPath(_args.Output);
                else if (_args.Output.Contains(Path.DirectorySeparatorChar) && !_args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFolder = new FileInfo(Path.GetFullPath(_args.Output)).Directory.FullName;
                if (!_args.Output.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    outputFileName = Path.GetFileNameWithoutExtension(_args.Output) + ".dll";
            }

            await _logger?.WriteLineAsync(ConsoleColor.Green, $"Building {ConsoleColor.Yellow}'{string.Join(", ", inputFiles.Select(inp => inp.Name))}'{PREVIOUS_COLOR}...");


            if (_args.VerboseMode)
                await _logger?.WriteLineAsync(ConsoleColor.DarkGray, $"{ConsoleColor.Cyan}Microsoft.CodeAnalysis.CSharp.dll{PREVIOUS_COLOR} version {ConsoleColor.Cyan}{typeof(CSharpParseOptions).Assembly.GetName().Version}{PREVIOUS_COLOR}");

            var compiler = new RoslynCompiler(this, _logger, _args.VerboseMode);

            try
            {
                compiler.AddReferences(_args.ExtraReferences, _args.ExtraNamespaces);
            }
            catch (FileNotFoundException ex)
            {
                return new TemplateBuilderResponse() { ReturnCode = -1, CompilationErrors = new List<CompilationError>() { new CompilationError() { Message = "Assembly reference not found: " + ex.FileName} } };
            }

            var sources = inputFiles.Select(inp => inp.FullName).ToArray();

            var targetFile = Utils.IOUtils.MakeRelativePath(Path.Combine(outputFolder, outputFileName));

            var compilationResult = await compiler.CompileAsync(sources, targetFile);

            if (!compilationResult.Success)
            {
                await _logger?.WriteLineErrorAsync(ConsoleColor.Red, $"Error while building '{string.Join(", ", inputFiles.Select(inp => inp.Name))}'.");
                return new TemplateBuilderResponse() { ReturnCode = -1, CompilationErrors = compilationResult.Errors };
            }

            await _logger?.WriteLineAsync(ConsoleColor.Green, $"\nSuccessfully built template into {ConsoleColor.White}'{targetFile}'{PREVIOUS_COLOR}.");
            return new TemplateBuilderResponse() { ReturnCode = 0, TargetFile = targetFile };
        }


    }
    
}
