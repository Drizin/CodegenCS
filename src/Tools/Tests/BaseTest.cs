using CliWrap;
using CodegenCS.IO;
using NUnit.Framework;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CodegenCS.Tools.Tests;

internal class BaseTest
{
    protected static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);

    private StringBuilder _stdOutBuffer;
    private StringBuilder _stdErrBuffer;
    protected string _stdOut;
    protected string _stdErr;

    [SetUp]
    public async Task Setup()
    {
        _stdOutBuffer = new StringBuilder();
        _stdErrBuffer = new StringBuilder();
    }
    protected async Task<CliWrap.CommandResult> Run(string arguments)
    {
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

    private string GetTestOutputFolder(string testClassPath, string testName) => Path.Combine(new FileInfo(testClassPath).Directory.FullName + "-TestsOutput", testName);

    #region Asserts
    protected void Assert_That_Content_IsEqual_To_File(ICodegenTextWriter writer, string fileName, [CallerMemberName] string testName = "", [CallerFilePath] string testClassPath = "")
    {
        string outputFolder = GetTestOutputFolder(testClassPath, testName);
        string file = Path.Combine(outputFolder, fileName);
        if (!File.Exists(file))
        {
            if (!new FileInfo(file).Directory.Exists)
                new FileInfo(file).Directory.Create();
            File.WriteAllText(file, writer.GetContents());
        }

        string fileContents = File.ReadAllText(file);
        Assert.AreEqual(fileContents, writer.GetContents());
    }

    /// <summary>
    /// Compares the Context Outputs with the files in the folder specific for the test outputs (each test have it's own folder)
    /// </summary>
    /// <param name="context"></param>
    /// <param name="testName">Do NOT provide any value here. It will get the test name</param>
    protected void Assert_That_ContextOutput_IsEqual_To_TestOutputFolder(ICodegenContext context, [CallerMemberName] string testName = "", [CallerFilePath] string testClassPath = "")
    {
        string outputFolder = GetTestOutputFolder(testClassPath, testName);
        Assert_That_ContextOutput_IsEqual_To_Folder(context, outputFolder);
    }

    private void Assert_That_ContextOutput_IsEqual_To_Folder(ICodegenContext context, string folder)
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
