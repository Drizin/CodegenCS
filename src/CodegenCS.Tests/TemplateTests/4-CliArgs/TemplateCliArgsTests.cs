using CodegenCS.Utils;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using TemplateBuilderArgs = CodegenCS.TemplateBuilder.TemplateBuilder.TemplateBuilderArgs;
using System.Linq;
using System.CommandLine;

namespace CodegenCS.Tests.TemplateTests
{
    internal class TemplateCliArgsTests : BaseTest
    {
        string _tmpFolder;
        string _tmpTemplateFile;
        string _tmpDll;
        ICodegenContext _context;
        TemplateBuilderArgs _builderArgs;
        TemplateLauncherArgs _launcherArgs;
        ILogger _logger = new DebugOutputLogger();

        private async Task BuildAsync(FormattableString templateBody)
        {
            FormattableString template = $$"""
                using CodegenCS;
                using CodegenCS.DbSchema;
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
                using CodegenCS.Templating;

                {{templateBody}}
                """;

            _tmpFolder = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), Guid.NewGuid().ToString());
            _tmpTemplateFile = Path.Combine(_tmpFolder, Guid.NewGuid().ToString() + ".cs");
            _tmpDll = Path.Combine(_tmpFolder, Guid.NewGuid().ToString() + ".dll");
            new DirectoryInfo(_tmpFolder).Create();
            File.WriteAllText(_tmpTemplateFile, template.ToString());

            _builderArgs = new TemplateBuilderArgs()
            {
                Template = new string[] { _tmpTemplateFile },
                Output = _tmpDll,
                VerboseMode = true,
            };
            var builder = new CodegenCS.TemplateBuilder.TemplateBuilder(_logger, _builderArgs);
            int ret = await builder.ExecuteAsync();
            Assert.AreEqual(0, ret);

        }
        private async Task<int> LaunchAsync(string[] models = null, string[] templateArgs = null)
        {
            models ??= new string[0];
            templateArgs ??= new string[0];
            _context = new CodegenContext();
            _launcherArgs = new TemplateLauncherArgs()
            {
                Template = _builderArgs.Output,
                Models = models,
                OutputFolder = _tmpFolder,
                DefaultOutputFile = Path.GetFileName(_tmpTemplateFile) + ".generated.cs",
                TemplateSpecificArguments = templateArgs
            };
            var launcher = new CodegenCS.TemplateLauncher.TemplateLauncher(_logger, _context, true);

            int exitCode = await launcher.ExecuteAsync(_launcherArgs, null);
            return exitCode;
        }

        [Test]
        public async Task SimpleBuild()
        {
            FormattableString template = $$"""
                public class MyTemplate : ICodegenTemplate
                {
                    public void Render(ICodegenTextWriter writer)
                    {
                        writer.WriteLine("Hello World");
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync();
            Assert.AreEqual(0, exitCode);

            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "Hello World" + "\r\n");
        }



        [Test]
        public void ForwardTemplateArgs()
        {
            var fakeModel = Path.Combine(Environment.GetEnvironmentVariable("TEMP"), "model.json");
            if (!File.Exists(fakeModel))
                File.WriteAllText(fakeModel, ""); // parser will only accept models if they physically exist.

            //TODO: case insensitive?
            var args = new string[] { "template", "run", "template.cs", "--OutputFolder", @".\folder", "--File", "defaultfile.cs", fakeModel, "--verbose", "now", "comes", "template-specific", "options", "and", "args" };
            var parseResult = CodegenCS.DotNetTool.CliCommandParser.Instance.Parse(args);            
            Assert.AreEqual(0, parseResult.Errors.Count);
            Assert.AreEqual(args.Length, parseResult.Tokens.Count);
            Assert.AreEqual("run", parseResult.CommandResult.Command.Name);

            Argument<string[]> s = (Argument<string[]>)parseResult.CommandResult.Command.Children.Where(s => s.Name == "TemplateArgs").Single();
            string[] templateSpecificArgs = parseResult.GetValueForArgument<string[]>(s);
            Assert.AreEqual(new string[] { "now", "comes", "template-specific", "options", "and", "args" }, templateSpecificArgs);

            parseResult = CodegenCS.DotNetTool.CliCommandParser.RootCommand.Parse(string.Join(" ", args));
            Assert.AreEqual(0, parseResult.Errors.Count);
            Assert.AreEqual(args.Length, parseResult.Tokens.Count);
            Assert.AreEqual("run", parseResult.CommandResult.Command.Name);

            s = (Argument<string[]>)parseResult.CommandResult.Command.Children.Where(s => s.Name == "TemplateArgs").Single();
            templateSpecificArgs = parseResult.GetValueForArgument<string[]>(s);
            Assert.AreEqual(new string[] { "now", "comes", "template-specific", "options", "and", "args" }, templateSpecificArgs);

            //TODO: res = GetCommand().Parse("""run Template.cs Model.json --OutputFolder MyOutputFolder lala -lele -Template lili""");
            //TODO: res = GetCommand().Parse("""run Template.cs Model.json --OutputFolder MyOutputFolder lala -lele -Template lili""");

        }


        [Test]
        public async Task TestCommandLineArgs()
        {
            FormattableString template = $$"""
                public class MyTemplate : ICodegenTemplate
                {
                    CommandLineArgs _args;
                    public MyTemplate(CommandLineArgs args)
                    {
                        _args = args;
                    }
                    public void Render(ICodegenTextWriter writer)
                    {
                        writer.WriteLine(_args.Count);
                        writer.WriteLine(_args[0]);
                        writer.WriteLine(_args[1]);
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync(templateArgs: new string[] { "arg1", "arg2" });
            Assert.AreEqual(0, exitCode);

            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "2" + "\r\n" + "arg1" + "\r\n" + "arg2" + "\r\n");
        }


        [Test]
        public async Task TestICommandLineTemplateWithCustomBinder()
        {
            FormattableString template = $$"""
                public class MyTemplate : ICodegenTemplate
                {
                    private static Argument<string> argNamespace = new Argument<string>("Namespace", description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne };
                    public static void ConfigureCommand(Command command)
                    {
                        command.AddArgument(argNamespace);
                    }
                    ParseResult _parseResult;
                    public MyTemplate(ParseResult parseResult)
                    {
                        _parseResult = parseResult;
                    }
                    public void Render(ICodegenTextWriter writer)
                    {
                        var ns = _parseResult.GetValueForArgument(argNamespace);
                        writer.WriteLine(ns);
                    }
                }
                """;

            await BuildAsync(template);
            
            var exitCode = await LaunchAsync(templateArgs: new string[] { "arg1", "arg2" });
            Assert.AreEqual(-2, exitCode); // unrecognized parameters

            exitCode = await LaunchAsync(templateArgs: new string[] {});
            Assert.AreEqual(-2, exitCode); // missing required argument (namespace)

            exitCode = await LaunchAsync(templateArgs: new string[] { "MyNamespace" });
            
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "MyNamespace" + "\r\n");
        }

        [Test]
        public async Task TestICommandLineTemplateWithAutoBinder()
        {
            FormattableString template = $$"""
                public class MyTemplate : ICodegenTemplate
                {
                    private static Argument<string> argNamespace = new Argument<string>("Namespace", description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne };
                    public static void ConfigureCommand(Command command)
                    {
                        command.AddArgument(argNamespace);
                    }
                    public class MyTemplateArgs : IAutoBindCommandLineArgs
                    {
                        public string Namespace { get; set; }
                    }
                    MyTemplateArgs _args;
                    public MyTemplate(MyTemplateArgs args)
                    {
                        _args = args;
                    }
                    public void Render(ICodegenTextWriter writer)
                    {
                        writer.WriteLine(_args.Namespace);
                    }
                }
                """;

            await BuildAsync(template);

            var exitCode = await LaunchAsync(templateArgs: new string[] { "arg1", "arg2" });
            Assert.AreEqual(-2, exitCode); // unrecognized parameters

            exitCode = await LaunchAsync(templateArgs: new string[] { });
            Assert.AreEqual(-2, exitCode); // missing required argument (namespace)

            exitCode = await LaunchAsync(templateArgs: new string[] { "MyNamespace" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "MyNamespace" + "\r\n");
        }


        //TODO: Autobinder!



    }
}
