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
using Assert = NUnit.Framework.Legacy.ClassicAssert;


namespace CodegenCS.Tools.CliTool.Tests
{
    internal class TemplateCliArgsTests : BaseTest
    {
        string _dbschemaModelPath = Path.Combine(GetSourceFileFolder(), @"..\..\Models\CodegenCS.Models.DbSchema.SampleDatabases\AdventureWorksSchema.json");

        [SetUp]
        public void Setup()
        {
            Assert.That(File.Exists(_dbschemaModelPath));
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
            var fakeModel = _dbschemaModelPath; // parser will only accept models if they physically exist.

            //TODO: case insensitive?
            var args = new string[] { "template", "run", "template.cs", "--OutputFolder", @".\folder", "--File", "defaultfile.cs", fakeModel, "--verbose", "now", "comes", "template-specific", "options", "and", "args" };
            var parser = _cliCommandParser.Parser;
            var parseResult = parser.Parse(args);            
            Assert.AreEqual(0, parseResult.Errors.Count);
            Assert.AreEqual(args.Length, parseResult.Tokens.Count);
            Assert.AreEqual("run", parseResult.CommandResult.Command.Name);

            string[] templateSpecificArgs = parseResult.GetValueForArgument<string[]>(_cliCommandParser._runTemplateCommandWrapper._templateSpecificArguments);
            Assert.AreEqual(new string[] { "now", "comes", "template-specific", "options", "and", "args" }, templateSpecificArgs);

            parseResult = _cliCommandParser.RootCommand.Parse(string.Join(" ", args));
            Assert.AreEqual(0, parseResult.Errors.Count);
            Assert.AreEqual(args.Length, parseResult.Tokens.Count);
            Assert.AreEqual("run", parseResult.CommandResult.Command.Name);

            templateSpecificArgs = parseResult.GetValueForArgument<string[]>(_cliCommandParser._runTemplateCommandWrapper._templateSpecificArguments);
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



        [Test]
        public async Task TestMainEntrypoint()
        {
            FormattableString template = $$"""
                public class MyTemplate
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
                    void Main(ICodegenTextWriter writer)
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

        [Test]
        public async Task TestMainEntrypointWithModel()
        {
            FormattableString template = $$"""
                public class MyTemplate
                {
                    MyTemplateArgs _args;
                    public MyTemplate(MyTemplateArgs args)
                    {
                        _args = args;
                    }

                    void Main(ICodegenTextWriter writer, DatabaseSchema schema)
                    {
                        writer.WriteLine(_args.Namespace);
                        writer.WriteLine(schema.Tables.Count());
                    }
                
                    private static Argument<string> argNamespace = new Argument<string>("Namespace", description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne };
                    public static void ConfigureCommand(Command command)
                    {
                        command.AddArgument(argNamespace);
                    }
                    public class MyTemplateArgs : IAutoBindCommandLineArgs
                    {
                        public string Namespace { get; set; }
                    }
                }
                """;

            await BuildAsync(template);
            var exitCode = await LaunchAsync(models: new string[] { _dbschemaModelPath }, templateArgs: new string[] { "MyNamespace" });

            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "MyNamespace" + "\r\n" + "91" + "\r\n");
        }


        [Test]
        public async Task TestMainEntrypointWithCustomInputModel()
        {
            FormattableString template = $$"""
                public class MyModel : IInputModel
                {
                    public string Value1 { get; set; }
                }
                public class MyTemplate
                {
                    void Main(ICodegenTextWriter writer, CommandLineArgs cliArgs, MyModel model)
                    {
                        string ns = cliArgs[0];
                        writer.WriteLine(ns);
                        writer.WriteLine(model.Value1);
                    }
                }
                """;

            await BuildAsync(template);

            var fakeModel = Path.Combine(_tmpFolder, "model.json");
            File.WriteAllText(fakeModel, """
                {
                    Value1: "MyOwnModel"
                }
                """);

            var exitCode = await LaunchAsync(models: new string[] { fakeModel }, templateArgs: new string[] { "MyNamespace" });
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "MyNamespace" + "\r\n" + "MyOwnModel" + "\r\n");
        }

        [Test]
        public async Task TestMainEntrypointWithCustomInputModelAndReturnCode()
        {
            FormattableString template = $$"""
                public class MyModel : IInputModel
                {
                    public string Value1 { get; set; }
                }
                public class MyTemplate
                {
                    int Main(ICodegenTextWriter writer, CommandLineArgs cliArgs, MyModel model)
                    {
                        if (cliArgs.Count() < 1)
                        {
                            Console.WriteLine("Should provide namespace");
                            return -1;
                        }
                        string ns = cliArgs[0];
                        writer.WriteLine(ns);
                        writer.WriteLine(model.Value1);
                        return 0;
                    }
                }
                """;

            await BuildAsync(template);

            var fakeModel = Path.Combine(_tmpFolder, "model.json");
            File.WriteAllText(fakeModel, """
                {
                    Value1: "MyOwnModel"
                }
                """);


            var exitCode = await LaunchAsync(models: new string[] { fakeModel });
            Assert.AreEqual(-1, exitCode); // template returning -1: lack of namespace arg

            exitCode = await LaunchAsync(models: new string[] { fakeModel }, templateArgs: new string[] { "MyNamespace" });
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == "MyNamespace" + "\r\n" + "MyOwnModel" + "\r\n");
        }


        [Test]
        public async Task TestSimpleTemplateWithCliArgs()
        {
            FormattableString template = $$$""""
                public class MyModel : IJsonInputModel
                {
                    public string[] Tables { get; set; }
                }
                public class MyTemplate
                {
                    FormattableString Main(CommandLineArgs cliArgs, MyModel model)
                    {
                        return $$"""
                            namespace {{ cliArgs[0] }}
                            {
                                {{ model.Tables.Select(t => GenerateTable(t)) }}
                            }
                            """;
                    }
                    FormattableString GenerateTable(string tableName)
                    {
                        return $$"""
                            public class {{ tableName }}
                            {
                                // my properties...
                            }
                            """;
                    }
                }
                """";

            await BuildAsync(template);

            var fakeModel = Path.Combine(_tmpFolder, "model.json");
            File.WriteAllText(fakeModel, """
                {
                    "Tables": ["Users", "Products"]
                }
                """);

            var exitCode = await LaunchAsync(models: new string[] { fakeModel }, templateArgs: new string[] { "MyNamespace" });
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            string expected = $$"""
                namespace MyNamespace
                {
                    public class Users
                    {
                        // my properties...
                    }

                    public class Products
                    {
                        // my properties...
                    }
                }
                """;
            Assert.AreEqual(expected, _context.OutputFiles[0].GetContents());
        }

        [Test]
        public async Task TestSimplestTemplateEver()
        {
            FormattableString template = $$$""""
                public class MyModel : IJsonInputModel
                {
                    public string[] Tables { get; set; }
                }
                public class MyTemplate
                {
                    FormattableString Main(MyModel model)
                    {
                        return $$"""
                            namespace MyNamespace
                            {
                                {{ model.Tables.Select(t => GenerateTable(t)) }}
                            }
                            """;
                    }
                    FormattableString GenerateTable(string tableName)
                    {
                        return $$"""
                            public class {{ tableName }}
                            {
                                // my properties...
                            }
                            """;
                    }
                }
                """";

            await BuildAsync(template);

            var fakeModel = Path.Combine(_tmpFolder, "model.json");
            File.WriteAllText(fakeModel, """
                {
                    "Tables": ["Users", "Products"]
                }
                """);

            var exitCode = await LaunchAsync(models: new string[] { fakeModel });
            Assert.AreEqual(0, exitCode);
            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            string expected = $$"""
                namespace MyNamespace
                {
                    public class Users
                    {
                        // my properties...
                    }

                    public class Products
                    {
                        // my properties...
                    }
                }
                """;
            Assert.AreEqual(expected, _context.OutputFiles[0].GetContents());
        }



    }
}
