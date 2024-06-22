using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using CliWrap;
using CodegenCS.IO;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using TemplateBuilderArgs = CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs;
using CodegenCS.Runtime;
using CodegenCS.DotNetTool;
using System.Collections.Generic;
using System.Linq;

namespace CodegenCS.Tools.CliTool.Tests;

internal class BaseTest
{
    protected static string GetSourceFileFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

    #region CLI
    private StringBuilder _stdOutBuffer;
    private StringBuilder _stdErrBuffer;
    protected string _stdOut;
    protected string _stdErr;

    [SetUp]
    public async Task Setup()
    {
    }
    protected async Task<CliWrap.CommandResult> Run(string arguments)
    {
        _stdOutBuffer = new StringBuilder();
        _stdErrBuffer = new StringBuilder();

        var result = await Cli.Wrap(Path.Combine(Directory.GetCurrentDirectory(), "dotnet-codegencs.exe"))
            .WithArguments(arguments)
            .WithWorkingDirectory(Directory.GetCurrentDirectory())
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToStringBuilder(_stdOutBuffer))
            .WithStandardErrorPipe(PipeTarget.ToStringBuilder(_stdErrBuffer))
            .ExecuteAsync();
        _stdOut = _stdOutBuffer.ToString();
        _stdErr = _stdErrBuffer.ToString();
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debug.WriteLine(result.ExitCode);
            System.Diagnostics.Debug.WriteLine(_stdOut);
            System.Diagnostics.Debug.WriteLine(_stdErr);
        }
        return result;
    }
    #endregion

    #region Build/Run
    protected string _tmpFolder;
    protected string _tmpTemplateFile;
    protected string _tmpDll;
    protected ICodegenContext _context;
    protected TemplateBuilderArgs _builderArgs;
    protected TemplateLauncherArgs _launcherArgs;
    protected ILogger _logger = new DebugOutputLogger();
    protected CliCommandParser _cliCommandParser = new CliCommandParser();
    #endregion

    protected virtual async Task BuildAsync(FormattableString templateBody, List<string> extraReferences = null)
    {
        FormattableString template = $$"""
                using CodegenCS;
                using CodegenCS.Models.DbSchema;
                using NSwag;
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
        await BuildAsync(_tmpTemplateFile, extraReferences);
    }
    protected async Task BuildAsync(string templateFile, List<string> extraReferences)
    {
        _builderArgs = new TemplateBuilderArgs()
        {
            Template = new string[] { templateFile },
            Output = _tmpDll,
            VerboseMode = true,
            ExtraReferences = extraReferences
        };
        var builder = new CodegenCS.TemplateBuilder.TemplateBuilder(_logger, _builderArgs);
        var builderResult = await builder.ExecuteAsync();
        Assert.AreEqual(0, builderResult.ReturnCode);
    }

    protected async Task<int> LaunchAsync(string[] models = null, string[] templateArgs = null)
    {
        return await LaunchAsync(_builderArgs.Output, models, templateArgs);
    }
    protected async Task<int> LaunchAsync(string templateDll, string[] models = null, string[] templateArgs = null)
    {
        models ??= new string[0];
        templateArgs ??= new string[0];
        _context = new CodegenContext();

        // Faking TemplateRunCommand, which also provides this context info

        var executionContext = new ExecutionContext(_builderArgs.Template[0], null);
        var dependencyContainer = new DependencyContainer().AddTestsConsole();
        dependencyContainer.RegisterSingleton<ExecutionContext>(() => executionContext);

        _launcherArgs = new TemplateLauncherArgs()
        {
            Template = templateDll,
            Models = models,
            OutputFolder = _tmpFolder,
            DefaultOutputFile = Path.GetFileNameWithoutExtension(_tmpTemplateFile ?? _builderArgs.Template[0]) + ".generated.cs", // should be same rule everywhere (VisualStudio RunTemplateWrapper, dotnet-codegencs TemplateRunCommand, and here) // just in-memory, doesn't matter
            TemplateSpecificArguments = templateArgs
        };
        var launcher = new TemplateLauncher.TemplateLauncher(_logger, _context, dependencyContainer, true);

        var loadResult = await launcher.LoadAsync(templateDll);

        if (loadResult.ReturnCode != 0)
            return loadResult.ReturnCode;

        _cliCommandParser = new CliCommandParser(); // HACK: this is modified in some places (fake parser) so we should better start fresh
        launcher.ParseCliUsingCustomCommand = _cliCommandParser._runTemplateCommandWrapper.ParseCliUsingCustomCommand;
        //string cmd = $$"""testhost template run {{_launcherArgs.Template}} {{string.Join(" ", models?.Any() == true ? models : new string[0])}} {{string.Join(" ", templateArgs?.Any() == true ? templateArgs : new string[0])}}""";
        var cmd = new List<string>() { "testhost", "template", "run", _launcherArgs.Template };
        foreach (var model in models)
            cmd.Add(model);
        foreach (var arg in templateArgs)
            cmd.Add(arg);
        var parseResult = _cliCommandParser.Parser.Parse(cmd);
        int executeResult = await launcher.LoadAndExecuteAsync(_launcherArgs, parseResult);
        return executeResult;
    }


    #region For tests that have their own output folder to compare results.
    #region Members
    //protected string _currentFolder;
    //protected string _outputFolder;
    #endregion

    #region ctor
    /// <summary>
    /// Use this for tests that have their own output folder to compare results.
    /// </summary>
    /// <param name="currentFolder"></param>
    //protected BaseTest(/*string currentFolder*/)
    //{
    //    //_currentFolder = currentFolder;
    //    //_outputFolder = currentFolder + "-TestsOutput";
    //}
    #endregion

    #region Asserts
    protected void Assert_That_Content_IsEqual_To_File(ICodegenTextWriter writer, string filePath)
    {
        if (!File.Exists(filePath))
        {
            if (!new FileInfo(filePath).Directory.Exists)
                new FileInfo(filePath).Directory.Create();
            File.WriteAllText(filePath, writer.GetContents());
        }

        string fileContents = File.ReadAllText(filePath);
        Assert.AreEqual(fileContents, writer.GetContents());
    }

    /// <summary>
    /// Compares the Context Outputs with the files in the folder specific for the test outputs (each test have it's own folder)
    /// </summary>
    protected void Assert_That_ContextOutput_IsEqual_To_Folder(ICodegenContext context, string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
            context.SaveToFolder(folder);
        }

        var files = Directory.GetFiles(folder);
        Assert.AreEqual(files.Length, context.OutputFilesPaths.Count);
        foreach (var relativeFilePath in context.OutputFilesPaths)
        {
            string relativePath = Path.Combine(folder, relativeFilePath);
            ICodegenTextWriter writer = context[relativeFilePath];
            Assert_That_Content_IsEqual_To_File(writer, relativePath);
        }
    }
    #endregion


    #endregion

}
