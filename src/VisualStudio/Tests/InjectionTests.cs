using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading.Tasks;
using TemplateLauncherArgs = CodegenCS.TemplateLauncher.TemplateLauncher.TemplateLauncherArgs;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Parsing;
using CodegenCS.Runtime;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.CodeAnalysis;
using static CodegenCS.TemplateBuilder.TemplateBuilder;
using ExecutionContext = CodegenCS.Runtime.ExecutionContext;
using System.Reflection;
using System.Collections.Generic;
using Assert = NUnit.Framework.Legacy.ClassicAssert;
using System.Windows.Media.Effects;


namespace CodegenCS.VisualStudio.Tests
{
    internal class InjectionTests : CodegenCS.Tools.Tests.BaseTest
    {
        protected static string GetCurrentFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);
        VSExecutionContext _executionContext;

        protected override void RegisterDependencies(DependencyContainer dependencyContainer)
        {
            // VS ONLY
            _executionContext = new VSExecutionContext(_builderArgs.Template[0], @"C:\MyProject\MyProject.csproj", @"C:\MyProject\MySolution.sln");
            dependencyContainer.RegisterSingleton<VSExecutionContext>(() => _executionContext);
            dependencyContainer.RegisterSingleton<ExecutionContext>(() => _executionContext);
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

            var extraReferences = new List<string>() { typeof(VSExecutionContext).GetTypeInfo().Assembly.Location }; // CodegenCS.Runtime.VisualStudio
            await BuildAsync(template, extraReferences);
            var exitCode = await LaunchAsync(); // TODO: VS Extension passing models/arguments
            Assert.AreEqual(0, exitCode);

            Assert.AreEqual(1, _context.OutputFiles.Count);
            Assert.That(_context.OutputFilesPaths.Contains(_launcherArgs.DefaultOutputFile));
            Assert.That(_context.OutputFiles[0].GetContents() == _executionContext.TemplatePath + "\r\n" + _executionContext.ProjectPath + "\r\n" + _executionContext.SolutionPath + "\r\n");
        }






    }
}
