using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using TemplateBuilderArgs = CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;
using CodegenCS.DotNetTool;
using CodegenCS.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using static CodegenCS.TemplateBuilder.TemplateBuilder;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;
using System.Reflection;
using System.Collections.Generic;

namespace CodegenCS.VisualStudio.Tests
{
    internal class InjectionTests 
    {
        protected static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);
        string _tmpFolder;
        string _tmpTemplateFile;
        string _tmpDll;
        ICodegenContext _context;
        TemplateBuilderArgs _builderArgs;
        TemplateLauncherArgs _launcherArgs;
        ILogger _logger = new DebugOutputLogger();
        CliCommandParser _cliCommandParser = new CliCommandParser();
        VSExecutionContext _executionContext;

        [SetUp]
        public void Setup()
        {
        }



        private async Task BuildAsync(FormattableString templateBody)
        {
            FormattableString template = $$"""
                using CodegenCS;
                using CodegenCS.Models.DbSchema;
                using System;
                using System.Collections.Generic;
                using System.IO;
                using System.Linq;
                using System.Runtime.CompilerServices;
                using System.Text.RegularExpressions;
                using Newtonsoft.Json;
                using static CodegenCS.Symbols;
                using static InterpolatedColorConsole.Symbols;
                using System.CommandLine.Binding;
                using System.CommandLine;
                using CodegenCS.Utils;

                {{templateBody}}
                """;

            _tmpFolder = Path.Combine(Path.GetTempPath() ?? Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
            _tmpTemplateFile = Path.Combine(_tmpFolder, Guid.NewGuid().ToString() + ".cs");
            _tmpDll = Path.Combine(_tmpFolder, Guid.NewGuid().ToString() + ".dll");
            new DirectoryInfo(_tmpFolder).Create();
            File.WriteAllText(_tmpTemplateFile, template.ToString());

            _builderArgs = new TemplateBuilderArgs()
            {
                Template = new string[] { _tmpTemplateFile },
                Output = _tmpDll,
                VerboseMode = true,
                ExtraReferences = new List<string>() { typeof(VSExecutionContext).GetTypeInfo().Assembly.Location } // CodegenCS.Runtime.VisualStudio
            };
            var builder = new CodegenCS.TemplateBuilder.TemplateBuilder(_logger, _builderArgs);
            var builderResult = await builder.ExecuteAsync();
            Assert.AreEqual(0, builderResult.ReturnCode);
        }
        private async Task<int> LaunchAsync(string[] models = null, string[] templateArgs = null)
        {
            models ??= new string[0];
            templateArgs ??= new string[0];
            _context = new CodegenContext();

            // Faking TemplateRunCommand, which also provides this context info
            _executionContext = new VSExecutionContext(@"C:\FakeTemplate.csx", @"C:\MyProject\MyProject.csproj", @"C:\MyProject\MySolution.sln");
            var dependencyContainer = new DependencyContainer().AddTestsConsole();
            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _executionContext);

            // VS ONLY
            dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext as VSExecutionContext);


            _launcherArgs = new TemplateLauncherArgs()
            {
                Template = _builderArgs.Output,
                Models = models,
                OutputFolder = _tmpFolder,
                DefaultOutputFile = Path.GetFileName(_tmpTemplateFile) + ".generated.cs",
                TemplateSpecificArguments = templateArgs
            };
            var launcher = new TemplateLauncher.TemplateLauncher(_logger, _context, dependencyContainer, true);

            var loadResult = await launcher.LoadAsync(_builderArgs.Output);

            if (loadResult.ReturnCode != 0)
                return loadResult.ReturnCode;

            _cliCommandParser = new CliCommandParser(); // HACK: this is modified in some places (fake parser) so we should better start fresh
            launcher.ParseCliUsingCustomCommand = _cliCommandParser._runTemplateCommandWrapper.ParseCliUsingCustomCommand;
            var parseResult = _cliCommandParser.Parser.Parse($"testhost template run {_launcherArgs.Template} {string.Join(" ", models?.Any() == true ? models : new string[0])} {string.Join(" ", templateArgs?.Any() == true ? templateArgs : new string[0])}");
            int executeResult = await launcher.LoadAndExecuteAsync(_launcherArgs, parseResult);
            return executeResult;
        }

        [Test]
        public async Task ResolveVSExecutionContext()
        {
            FormattableString template = $$"""
                public class MyTemplate
                {
                    void Main(VSExecutionContext context, ICodegenTextWriter writer)
                    {
                        writer.WriteLine(context.TemplatePath);
                        writer.WriteLine(context.ProjectPath);
                        writer.WriteLine(context.SolutionPath);
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync();
            Assert.AreEqual(0, exitCode);

            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == _executionContext.TemplatePath + "\r\n" + _executionContext.ProjectPath + "\r\n" + _executionContext.SolutionPath + "\r\n");
        }






    }
}
