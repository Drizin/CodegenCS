using CodegenCS.Runtime;
using CodegenCS.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CodegenCS.TemplateBuilder.TemplateBuilder;

namespace CodegenCS.TemplateBuilder
{
    internal class RoslynCompiler
    {
        protected readonly HashSet<PortableExecutableReference> _references = new HashSet<PortableExecutableReference>();
        protected readonly HashSet<string> _namespaces = new HashSet<string>();
        protected readonly CSharpCompilationOptions _compilationOptions;
        protected readonly CSharpParseOptions _parseOptions;
        protected readonly string _dotNetCoreDir;
        protected ILogger _logger;

        public RoslynCompiler(ILogger logger)
        {
            _logger = logger;
            var privateCoreLib = typeof(object).GetTypeInfo().Assembly.Location;
            _dotNetCoreDir = Path.GetDirectoryName(privateCoreLib);

            #region Default Assemblies and Namespaces

            #region Core (System, System.Text, System.Threading.Tasks)
            _namespaces.Add("System");
            _namespaces.Add("System.Text");
            _namespaces.Add("System.Threading");
            _namespaces.Add("System.Threading.Tasks");

            // System.Private.CoreLib: this has all of the main core types in the runtime,
            // including most of the types that are on the System namespace (like mscorlib used to be on .net full framework), including object, List<>, Action<>, etc.
            AddAssembly(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)); // AddAssembly("System.Private.CoreLib.dll");

            AddAssembly("netstandard.dll");

            AddAssembly("System.Runtime.dll");
            #endregion

            #region System.Linq
            _namespaces.Add("System.Linq");
            AddAssembly("System.Linq.dll");
            AddAssembly("System.Linq.Expressions.dll");
            AddAssembly("System.Core.dll");
            #endregion

            #region System.Collections
            _namespaces.Add("System.Collections");
            _namespaces.Add("System.Collections.Generic");
            _namespaces.Add("System.Collections.Concurrent");
            AddAssembly("System.Collections.dll"); //AddAssembly(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
            AddAssembly("System.Collections.Concurrent.dll");
            AddAssembly("System.Collections.NonGeneric.dll");
            #endregion

            #region System.Net.Http
            _namespaces.Add("System.Net");
            _namespaces.Add("System.Net.Http");
            AddAssembly("System.Net.Http.dll");
            AddAssembly("System.Net.Primitives.dll");
            AddAssembly("System.Private.Uri.dll");
            #endregion

            #region System.IO, System.Console
            _namespaces.Add("System.IO");
            AddAssembly("System.IO.dll");
            AddAssembly(MetadataReference.CreateFromFile(typeof(FileInfo).GetTypeInfo().Assembly.Location)); // System.IO.FileSystem

            // System.Console
            AddAssembly(MetadataReference.CreateFromFile(typeof(System.Console).GetTypeInfo().Assembly.Location)); //AddAssembly("System.Console.dll");

            // InterpolatedColorConsole
            AddAssembly(MetadataReference.CreateFromFile(typeof(InterpolatedColorConsole.ColoredConsole).GetTypeInfo().Assembly.Location));
            #endregion


            AddAssembly("System.Reflection.dll");
            _namespaces.Add("System.Reflection");

            AddAssembly(typeof(System.Text.RegularExpressions.Regex)); // .net framework
            AddAssembly("System.Text.RegularExpressions.dll");
            _namespaces.Add("System.Text.RegularExpressions");

            #region TODO: CSharp / Roslyn Analyzers? To allow code generators to be based on Roslyn CodeAnalysis? 
            //AddAssembly(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)); //AddAssembly("Microsoft.CSharp.dll");
            //AddAssembly("Microsoft.CodeAnalysis.dll");
            //AddAssembly("Microsoft.CodeAnalysis.CSharp.dll");
            // add nuget references
            #endregion



            AddAssembly("System.ComponentModel.Primitives.dll");


            // CodegenCS / CodegenCS.Runtime / CodegenCS.Models.DbSchema
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.CodegenContext).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.Runtime.ExecutionContext).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.Models.IInputModel).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.DotNet.DotNetCodegenContext).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.Models.DbSchema.DatabaseSchema).GetTypeInfo().Assembly.Location));
            _namespaces.Add("CodegenCS");
            _namespaces.Add("CodegenCS.Runtime");
            _namespaces.Add("CodegenCS.Models");
            _namespaces.Add("CodegenCS.DotNet");
            _namespaces.Add("CodegenCS.Models.DbSchema");

            AddAssembly(MetadataReference.CreateFromFile(typeof(NSwag.OpenApiDocument).GetTypeInfo().Assembly.Location)); // NSwag.Core
            AddAssembly(MetadataReference.CreateFromFile(typeof(NSwag.OpenApiYamlDocument).GetTypeInfo().Assembly.Location)); // NSwag.Core.Yaml
            AddAssembly(MetadataReference.CreateFromFile(typeof(NJsonSchema.JsonSchema).GetTypeInfo().Assembly.Location)); // NJsonSchema

            // Newtonsoft
            _namespaces.Add("Newtonsoft.Json");
            AddAssembly(MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly.Location));

            AddAssembly(MetadataReference.CreateFromFile(typeof(System.CommandLine.Argument).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(System.CommandLine.Binding.BindingContext).GetTypeInfo().Assembly.Location));

            #endregion

            // Add this library? //AddAssembly(typeof(RoslynCompiler));
            // maybe just add AppDomain.CurrentDomain.GetAssemblies() ?


            _compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithUsings(_namespaces)
                    .WithWarningLevel(0);

            // For Microsoft.CodeAnalysis.CSharp 4.2.2 LanguageVersion.Preview means C# 11 preview (which includes raw string literals)
            _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
        }


        #region AddAssembly
        protected bool AddAssembly(PortableExecutableReference reference)
        {
            if (_references.Any(r => r.FilePath == reference.FilePath))
                return true;

            _references.Add(reference);
            return true;
        }

        protected bool AddAssembly(Type type)
        {
            try
            {
                if (_references.Any(r => r.FilePath == type.Assembly.Location))
                    return true;

                var systemReference = MetadataReference.CreateFromFile(type.Assembly.Location);
                _references.Add(systemReference);
            }
            catch
            {
                return false;
            }

            return true;
        }

        protected bool AddAssembly(string path)
        {
            var file = Path.GetFullPath(path);

            if (!File.Exists(file))
            {
                file = Path.Combine(_dotNetCoreDir, path);
                if (!File.Exists(file))
                    return false;
            }

            if (_references.Any(r => r.FilePath == file)) return true;

            try
            {
                var reference = MetadataReference.CreateFromFile(file);
                _references.Add(reference);
            }
            catch
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Compile
        public async Task<(bool success, IEnumerable<CompilationError> errors)> Compile(string[] sources, string targetFile)
        {
            var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source), _parseOptions)).ToList();
            // ParseText really better? https://stackoverflow.com/questions/16338131/using-roslyn-to-parse-transform-generate-code-am-i-aiming-too-high-or-too-low
            //SyntaxFactory.ParseSyntaxTree(SourceText.From(text, Encoding.UTF8), options, filename);

            AddMissingUsings(syntaxTrees);


            //TODO: support for top-level statements?
            CSharpCompilation compilation = CSharpCompilation.Create("assemblyName", syntaxTrees,
                _references,
                _compilationOptions);

            var compilationErrors = new List<CompilationError>();
            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(dllStream, pdbStream);

                //TODO: combine all errors into a single statement, 
                // since Powershell ISE stops at first stderr message when $ErrorActionPreference = "Stop"
                Func<ConsoleColor, Diagnostic, Task> writeErrorAsync = async (color, diag) =>
                {
                    var lineStart = diag.Location.GetLineSpan().StartLinePosition.Line;
                    var lineEnd = diag.Location.GetLineSpan().EndLinePosition.Line;
                    await _logger.WriteLineErrorAsync(color, $"  {diag.Id}: Line {lineStart}{(lineStart != lineEnd ? "-" + lineEnd : "")} {diag.GetMessage()}");
                    compilationErrors.Add(new CompilationError() { Message = diag.GetMessage(), Line = lineStart, Column = diag.Location.GetLineSpan().StartLinePosition.Character });
                };
                var errors = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                if (errors.Any())
                {
                    await _logger.WriteLineErrorAsync(ConsoleColor.Red, $"Errors: ");
                    foreach (var error in errors)
                        await writeErrorAsync(ConsoleColor.Red, error);
                }

                Func<ConsoleColor, Diagnostic, Task> writeWarningAsync = async (color, diag) =>
                {
                    var lineStart = diag.Location.GetLineSpan().StartLinePosition.Line;
                    var lineEnd = diag.Location.GetLineSpan().EndLinePosition.Line;
                    await _logger.WriteLineAsync(color, $"  {diag.Id}: Line {lineStart}{(lineStart != lineEnd ? "-" + lineEnd : "")} {diag.GetMessage()}");
                };

                var warnings = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
                if (warnings.Any())
                {
                    await _logger.WriteLineAsync(ConsoleColor.Yellow, $"Warnings: ");
                    foreach (var warning in warnings)
                        await writeWarningAsync(ConsoleColor.Yellow, warning);
                }

                if (!emitResult.Success)
                {
                    return (false, compilationErrors);
                }

                dllStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);

                if (!new FileInfo(targetFile).Directory.Exists)
                    new FileInfo(targetFile).Directory.Create();

                using (FileStream fs = new FileStream(targetFile, FileMode.OpenOrCreate))
                {
                    dllStream.CopyTo(fs);
                    fs.Flush();
                }
                var targetPdb = targetFile.Substring(0, targetFile.Length - 4) + ".pdb";
                using (FileStream fs = new FileStream(targetPdb, FileMode.OpenOrCreate))
                {
                    pdbStream.CopyTo(fs);
                    fs.Flush();
                }
                return (true, null);
            }

        }
        void AddMissingUsings(List<SyntaxTree> trees)
        {
            for (int i = 0; i < trees.Count; i++)
            {
                var rootNode = trees[i].GetRoot() as CompilationUnitSyntax;
                AddMissingUsing(ref rootNode, "CodegenCS");


                string templateSource = rootNode.ToString(); //TODO: strip strings from CompilationUnitSyntax - we are only interested in checking the template control logic

                // These namespaces probably won't conflict with anything
                AddMissingUsing(ref rootNode, "System");
                AddMissingUsing(ref rootNode, "System.Collections.Generic");
                AddMissingUsing(ref rootNode, "System.Linq");
                AddMissingUsing(ref rootNode, "System.IO");
                AddMissingUsing(ref rootNode, "System.Runtime.CompilerServices");
                AddMissingUsing(ref rootNode, "System.Text.RegularExpressions");
                AddMissingUsing(ref rootNode, "Newtonsoft.Json"); // I doubt this might conflict with anything. Maybe should search for JsonConvert and some other classes

                // To avoid type names conflict we only add some usings if we detect as required
                // Most regex below are checking for non-fully-qualified typename (no leading dot).
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bDatabaseSchema\b"))
                    AddMissingUsing(ref rootNode, "CodegenCS.Models.DbSchema");
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bOpenApiDocument\b"))
                    AddMissingUsing(ref rootNode, "NSwag");
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bCommandLineArgs\b")
                    || Regex.IsMatch(templateSource, @"(?<!\.)\bIAutoBindCommandLineArgs\b"))
                    AddMissingUsing(ref rootNode, "CodegenCS.Runtime");
                else if (Regex.IsMatch(templateSource, @"(?<!\.)\bILogger\b") && Regex.IsMatch(templateSource, @"\bWriteLine(\w*)Async\b"))
                    AddMissingUsing(ref rootNode, "CodegenCS.Utils");
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bIInputModel\b") 
                    || Regex.IsMatch(templateSource, @"(?<!\.)\bIJsonInputModel\b") 
                    || Regex.IsMatch(templateSource, @"(?<!\.)\bIValidatableJsonInputModel\b")
                    || Regex.IsMatch(templateSource, @"(?<!\.)\bIModelFactory\b")
                    )
                    AddMissingUsing(ref rootNode, "CodegenCS.Models");

                if (Regex.IsMatch(templateSource, @"(?<!\.)\bConfigureCommand\b") || Regex.IsMatch(templateSource, @"(?<!\.)\bParseResult\b"))
                    AddMissingUsing(ref rootNode, "System.CommandLine"); // System.CommandLine.Command, System.CommandLine.ParseResult
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bBindingContext\b"))
                    AddMissingUsing(ref rootNode, "System.CommandLine.Binding");
                if (Regex.IsMatch(templateSource, @"(?<!\.)\bInvocationContext\b"))
                    AddMissingUsing(ref rootNode, "System.CommandLine.Invocation");

                if (Regex.IsMatch(templateSource, @"(?<!\.)\bIF\(\b") || Regex.IsMatch(templateSource, @"(?<!\.)\bIIF\(\b"))
                    AddMissingUsing(ref rootNode, "CodegenCS.Symbols", true);

                if (Regex.IsMatch(templateSource, @"(?<!\.)\bPREVIOUS_COLOR\b") || Regex.IsMatch(templateSource, @"(?<!\.)\bPREVIOUS_BACKGROUND_COLOR\b"))
                    AddMissingUsing(ref rootNode, "InterpolatedColorConsole.Symbols", true);

                trees[i] = SyntaxFactory.SyntaxTree(rootNode, _parseOptions); // rootNode.SyntaxTree (without _parseOptions) would go back to C# 10 (and we need C# 11 preview)
            }
        }
        void AddMissingUsing(ref CompilationUnitSyntax unit, string @namespace, bool isStatic = false)
        {
            var qualifiedName = SyntaxFactory.ParseName(@namespace);
            if (!unit.Usings.Select(d => d.Name.ToString()).Any(u => u == qualifiedName.ToString()))
            {
                UsingDirectiveSyntax usingDirective;
                if (isStatic)
                    usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.Token(SyntaxKind.StaticKeyword), null, qualifiedName).NormalizeWhitespace();
                else
                    usingDirective = SyntaxFactory.UsingDirective(qualifiedName).NormalizeWhitespace();
                unit = unit.AddUsings(usingDirective);
           }
        }
        #endregion

    }
}
