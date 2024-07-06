using DependencyContainer = CodegenCS.Utils.DependencyContainer;
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
using System.Collections.Generic;

namespace CodegenCS.Tools.Tests;

public class BaseTest
{
    protected static string GetSourceFileFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);
    protected static string TemplatesFolder = Path.Combine(GetSourceFileFolder(), "Templates");

    [SetUp]
    public virtual Task Setup()
    {
        return Task.CompletedTask;
    }

    #region Build/Run
    protected string _tmpFolder;
    protected string _tmpTemplateFile;
    protected string _templateFileName; // only if there is a real file
    protected string _tmpDll;
    protected ICodegenContext _context;
    protected TemplateBuilderArgs _builderArgs;
    protected TemplateLauncherArgs _launcherArgs;
    protected ILogger _logger = new DebugOutputLogger();
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
    protected async Task BuildAsync(string templateFile, List<string> extraReferences = null)
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
        models ??= new string[0];
        templateArgs ??= new string[0];
        _context = new CodegenContext();

        var dependencyContainer = new DependencyContainer().AddTestsConsole();
        RegisterDependencies(dependencyContainer);

        // Faking TemplateRunCommand, which also provides this context info
        _launcherArgs = new TemplateLauncherArgs()
        {
            Template = _builderArgs.Output,
            Models = models,
            OutputFolder = _tmpFolder,
            DefaultOutputFile = Path.GetFileNameWithoutExtension(_templateFileName  ?? _tmpTemplateFile) + ".g.cs",
            TemplateSpecificArguments = templateArgs
        };
        var launcher = new TemplateLauncher.TemplateLauncher(_logger, _context, dependencyContainer, true);

        var loadResult = await launcher.LoadAsync(_builderArgs.Output);

        if (loadResult.ReturnCode != 0)
            return loadResult.ReturnCode;

        int executeResult = await launcher.LoadAndExecuteAsync(_launcherArgs, null);
        return executeResult;
    }

    protected virtual void RegisterDependencies(DependencyContainer dependencyContainer) 
    {
        var executionContext = new ExecutionContext(_builderArgs.Template[0], null);
        dependencyContainer.RegisterSingleton<ExecutionContext>(() => executionContext);
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
