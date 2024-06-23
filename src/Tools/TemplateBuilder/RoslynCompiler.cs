using CodegenCS.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CompilationError = CodegenCS.TemplateBuilder.TemplateBuilder.CompilationError;

namespace CodegenCS.TemplateBuilder
{
    internal class RoslynCompiler
    {
        protected readonly HashSet<PortableExecutableReference> _references = new HashSet<PortableExecutableReference>();
        protected readonly Dictionary<string, Func<string, bool>> _namespaces = new Dictionary<string, Func<string, bool>>();
        protected readonly CSharpCompilationOptions _compilationOptions;
        protected readonly CSharpParseOptions _parseOptions;
        protected readonly string _dotNetCoreDir;
        protected readonly bool _verboseMode;
        protected ILogger _logger;
        protected TemplateBuilder _builder;

        public RoslynCompiler(TemplateBuilder builder, ILogger logger, bool verboseMode)
        {
            _builder = builder;
            _logger = logger;
            var privateCoreLib = typeof(object).GetTypeInfo().Assembly.Location;
            _dotNetCoreDir = Path.GetDirectoryName(privateCoreLib);

            _compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithOverflowChecks(true)
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    //.WithUsings(_namespaces) // TODO: review why adding namespaces here doesn't make any difference - only AddMissingUsing (applied directly to tree) matters
                    .WithWarningLevel(0);

            // For Microsoft.CodeAnalysis.CSharp 4.2.2 LanguageVersion.Preview means C# 11 preview (which includes raw string literals)
            _parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
            _verboseMode = verboseMode;
        }

        public void AddReferences(List<string> extraReferences, List<string> extraNamespaces)
        {
            // Some namespaces are always added (low risk of conflicting with user classes)
            // but some others will only be added if we detect (using Func) that they are required, to avoid type names conflict
            // Most regex below are checking for non-fully-qualified typename (no leading dot), because if you're using fully-qualified types you don't need "using"

            #region Default Assemblies and Namespaces

            #region Core (System, System.Text, System.Threading.Tasks)
            _namespaces.Add("System", null);
            _namespaces.Add("System.Text", null);
            _namespaces.Add("System.Threading", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bTask\b")); //TODO: always add?
            _namespaces.Add("System.Threading.Tasks", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bTask\b")); //TODO: always add?

            // System.Private.CoreLib: this has all of the main core types in the runtime,
            // including most of the types that are on the System namespace (like mscorlib used to be on .net full framework), including object, List<>, Action<>, etc.
            AddAssembly(MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location)); // AddAssembly("System.Private.CoreLib.dll");

            AddAssembly("netstandard.dll");

            _namespaces.Add("System.Runtime.CompilerServices", null);
            AddAssembly("System.Runtime.dll");
            AddAssembly("System.Threading.dll");
            #endregion

            #region System.Linq
            _namespaces.Add("System.Linq", null);
            AddAssembly("System.Linq.dll");
            AddAssembly("System.Linq.Expressions.dll");
            AddAssembly("System.Core.dll");
            #endregion

            #region System.Collections
            _namespaces.Add("System.Collections", null);
            _namespaces.Add("System.Collections.Generic", null);
            _namespaces.Add("System.Collections.Concurrent", null);
            AddAssembly("System.Collections.dll"); //AddAssembly(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
            AddAssembly("System.Collections.Concurrent.dll");
            AddAssembly("System.Collections.NonGeneric.dll");
            #endregion

            #region System.Net.Http
            _namespaces.Add("System.Net", null);
            _namespaces.Add("System.Net.Http", null);
            AddAssembly("System.Net.Http.dll");
            AddAssembly("System.Net.Primitives.dll");
            AddAssembly("System.Private.Uri.dll");
            #endregion

            #region System.IO, System.Console
            _namespaces.Add("System.IO", null);
            AddAssembly("System.IO.dll");
            AddAssembly(MetadataReference.CreateFromFile(typeof(FileInfo).GetTypeInfo().Assembly.Location)); // System.IO.FileSystem

            // System.Console
            AddAssembly(MetadataReference.CreateFromFile(typeof(System.Console).GetTypeInfo().Assembly.Location)); //AddAssembly("System.Console.dll");

            // InterpolatedColorConsole
            AddAssembly(MetadataReference.CreateFromFile(typeof(InterpolatedColorConsole.ColoredConsole).GetTypeInfo().Assembly.Location));
            _namespaces.Add("InterpolatedColorConsole.Symbols", templateSource => 
                Regex.IsMatch(templateSource, @"(?<!\.)\bPREVIOUS_COLOR\b") || 
                Regex.IsMatch(templateSource, @"(?<!\.)\bPREVIOUS_BACKGROUND_COLOR\b"));

            #endregion


            AddAssembly("System.Reflection.dll");
            _namespaces.Add("System.Reflection", null);

            AddAssembly(typeof(System.Text.RegularExpressions.Regex)); // .net framework
            AddAssembly("System.Text.RegularExpressions.dll");
            _namespaces.Add("System.Text.RegularExpressions", null);

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
            _namespaces.Add("CodegenCS", null);

            _namespaces.Add("CodegenCS.Runtime", templateSource =>
                Regex.IsMatch(templateSource, @"(?<!\.)\bCommandLineArgs\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bIAutoBindCommandLineArgs\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bVSExecutionContext\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bExecutionContext\b") ||
                (Regex.IsMatch(templateSource, @"(?<!\.)\bILogger\b") && Regex.IsMatch(templateSource, @"\bWriteLine(\w*)Async\b")));
            _namespaces.Add("CodegenCS.Models", templateSource =>
                Regex.IsMatch(templateSource, @"(?<!\.)\bIInputModel\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bIJsonInputModel\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bIValidatableJsonInputModel\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bIModelFactory\b"));
            _namespaces.Add("CodegenCS.DotNet", null);
            _namespaces.Add("CodegenCS.Models.DbSchema", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bDatabaseSchema\b"));
            _namespaces.Add("CodegenCS.Symbols", templateSource =>
                Regex.IsMatch(templateSource, @"(?<!\.)\bIF\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bIIF\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bBREAKIF\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bTLW\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bTTW\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bCOMMENT\(\b") ||
                Regex.IsMatch(templateSource, @"(?<!\.)\bRAW\(\b"));

            AddAssembly(MetadataReference.CreateFromFile(typeof(NSwag.OpenApiDocument).GetTypeInfo().Assembly.Location)); // NSwag.Core
            AddAssembly(MetadataReference.CreateFromFile(typeof(NSwag.OpenApiYamlDocument).GetTypeInfo().Assembly.Location)); // NSwag.Core.Yaml
            AddAssembly(MetadataReference.CreateFromFile(typeof(NJsonSchema.JsonSchema).GetTypeInfo().Assembly.Location)); // NJsonSchema
            AddAssembly(MetadataReference.CreateFromFile(typeof(NJsonSchema.Annotations.JsonSchemaAttribute).GetTypeInfo().Assembly.Location)); // NJsonSchema.Annotations
            _namespaces.Add("NSwag", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bOpenApiDocument\b"));

            // Newtonsoft
            _namespaces.Add("Newtonsoft.Json", null); // maybe we should only add namespace if we find references like "JsonConvert"?
            AddAssembly(MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly.Location));

            AddAssembly(MetadataReference.CreateFromFile(typeof(System.CommandLine.Argument).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(System.CommandLine.Binding.BindingContext).GetTypeInfo().Assembly.Location));
            // System.CommandLine.Command, System.CommandLine.ParseResult
            _namespaces.Add("System.CommandLine", templateSource => 
                Regex.IsMatch(templateSource, @"(?<!\.)\bConfigureCommand\b") || 
                Regex.IsMatch(templateSource, @"(?<!\.)\bParseResult\b"));
            _namespaces.Add("System.CommandLine.Binding", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bBindingContext\b"));
            _namespaces.Add("System.CommandLine.Invocation", templateSource => Regex.IsMatch(templateSource, @"(?<!\.)\bInvocationContext\b"));

            #endregion

            // Add this library (TemplateBuilder)? //AddAssembly(typeof(RoslynCompiler));
            // maybe just add AppDomain.CurrentDomain.GetAssemblies() ?

            if (extraNamespaces != null)
                extraNamespaces.ForEach(ns => _namespaces.Add(ns, null));
            if (extraReferences != null)
            {
                extraReferences.ForEach(rfc =>
                {
                    if (!File.Exists(rfc) && File.Exists(Path.Combine(_dotNetCoreDir, rfc)))
                        rfc = Path.Combine(_dotNetCoreDir, rfc);
                    if (!File.Exists(rfc))
                        throw new FileNotFoundException("Can't find " + rfc, rfc);
                    AddAssembly(MetadataReference.CreateFromFile(rfc));
                });
            }
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

        public class CompileResult
        {
            public bool Success { get; set; }
            public IEnumerable<CompilationError> Errors { get; set; }
        }

        #region Compile
        protected static readonly Regex _lineBreaksRegex = new Regex(@"(\r\n|\n|\r)", RegexOptions.Compiled);
        public async Task<CompileResult> CompileAsync(string[] sources, string targetFile)
        {
            var sourcesContents = sources.Select(s => new StringBuilder(File.ReadAllText(s)));
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            foreach (var source in sourcesContents) 
            {
                // ParseText really better? https://stackoverflow.com/questions/16338131/using-roslyn-to-parse-transform-generate-code-am-i-aiming-too-high-or-too-low
                //SyntaxFactory.ParseSyntaxTree(SourceText.From(text, Encoding.UTF8), options, filename);
                var syntaxTree = CSharpSyntaxTree.ParseText(source.ToString(), _parseOptions);

                var references = syntaxTree.GetRoot().DescendantNodes(s => true, true).Where(c => c.Kind() == SyntaxKind.ReferenceDirectiveTrivia);
                if (references.Any())
                {
                    foreach (var reference in references)
                    {
                        var file = ((ReferenceDirectiveTriviaSyntax)reference).File.ValueText;
                        if (!File.Exists(file) && File.Exists(Path.Combine(_dotNetCoreDir, file)))
                            file = Path.Combine(_dotNetCoreDir, file);
                        if (!File.Exists(file))
                            throw new FileNotFoundException("Can't find " + file, file);
                        AddAssembly(MetadataReference.CreateFromFile(file));
                        // can I just remove nodes from SyntaxTree without creating a CSharpSyntaxRewriter?
                        // I guess it's just easier to strip the #r directives and reparse the modified source
                        // (replacing the stripped tokens with whitespace to preserve same line numbers and same token offsets)
                        var text = source.ToString().Substring(reference.SpanStart, reference.Span.Length);
                        var replace = new string(' ', reference.Span.Length);
                        var x = source.Replace(text, replace, reference.SpanStart, reference.Span.Length);
                    }
                    syntaxTree = CSharpSyntaxTree.ParseText(source.ToString(), _parseOptions);
                }
                syntaxTrees.Add(syntaxTree);
            }

            await AddMissingUsings(syntaxTrees);

            //TODO: support for top-level statements?
            CSharpCompilation compilation = CSharpCompilation.Create("assemblyName", syntaxTrees,
                _references,
                _compilationOptions);

            var compilationErrors = new List<CompilationError>();
            using (var dllStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(dllStream, pdbStream); // roslyn throws a lot of exceptions, so "break on all exceptions" is not nice at this line

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
                    return new CompileResult() { Success = false, Errors = compilationErrors };
                }

                dllStream.Seek(0, SeekOrigin.Begin);
                pdbStream.Seek(0, SeekOrigin.Begin);

                if (!new FileInfo(targetFile).Directory.Exists)
                    new FileInfo(targetFile).Directory.Create();

                using (FileStream fs = new FileStream(targetFile, FileMode.OpenOrCreate))
                {
                    await dllStream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
                var targetPdb = targetFile.Substring(0, targetFile.Length - 4) + ".pdb";
                using (FileStream fs = new FileStream(targetPdb, FileMode.OpenOrCreate))
                {
                    await pdbStream.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
                return new CompileResult() { Success = true, Errors = null };
            }

        }
        async Task AddMissingUsings(List<SyntaxTree> trees)
        {
            for (int i = 0; i < trees.Count; i++)
            {
                var rootNode = trees[i].GetRoot() as CompilationUnitSyntax;

                string templateSource = rootNode.ToString(); //TODO: strip strings from CompilationUnitSyntax - we are only interested in checking the template control logic

                foreach (var ns in _namespaces)
                {
                    if (ns.Value == null)
                    {
                        if (_verboseMode)
                            await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"Automatically adding namespace \"{ns.Key}\"");
                        AddMissingUsing(ref rootNode, ns.Key);
                    }
                    else if (ns.Value(templateSource))
                    {
                        if (_verboseMode)
                            await _logger.WriteLineAsync(ConsoleColor.DarkGray, $"Automatically adding namespace \"{ns.Key}\" (due to matching regex)");
                        AddMissingUsing(ref rootNode, ns.Key);
                    }
                }

                trees[i] = SyntaxFactory.SyntaxTree(rootNode, _parseOptions); // rootNode.SyntaxTree (without _parseOptions) would go back to C# 10 (and we need C# 11)
            }
        }
        void AddMissingUsing(ref CompilationUnitSyntax unit, string @namespace)
        {
            bool isStatic = @namespace.Equals("CodegenCS.Symbols") || @namespace.Equals("InterpolatedColorConsole.Symbols"); //TODO: yeah, I know...
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
