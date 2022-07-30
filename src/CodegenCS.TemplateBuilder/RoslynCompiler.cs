using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.TemplateBuilder
{
    internal class RoslynCompiler
    {
        protected readonly HashSet<PortableExecutableReference> _references = new HashSet<PortableExecutableReference>();
        protected readonly HashSet<string> _namespaces = new HashSet<string>();
        protected readonly CSharpCompilationOptions _compilationOptions;        
        protected readonly CSharpParseOptions _parseOptions;
        protected readonly string _dotNetCoreDir;

        public RoslynCompiler()
        {
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
            #endregion

            #region System.Collections
            _namespaces.Add("System.Collections");
            _namespaces.Add("System.Collections.Generic");
            _namespaces.Add("System.Collections.Concurrent");
            AddAssembly(MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location));
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

            AddAssembly("System.Text.RegularExpressions.dll");
            _namespaces.Add("System.Text.RegularExpressions");

            #region TODO: CSharp / Roslyn Analyzers? To allow code generators to be based on Roslyn CodeAnalysis? 
            //AddAssembly(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)); //AddAssembly("Microsoft.CSharp.dll");
            //AddAssembly("Microsoft.CodeAnalysis.dll");
            //AddAssembly("Microsoft.CodeAnalysis.CSharp.dll");
            // add nuget references
            #endregion



            AddAssembly("System.ComponentModel.Primitives.dll");


            // CodegenCS / CodegenCS.DbSchema
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.CodegenContext).GetTypeInfo().Assembly.Location));
            AddAssembly(MetadataReference.CreateFromFile(typeof(CodegenCS.DbSchema.DatabaseSchema).GetTypeInfo().Assembly.Location));
            _namespaces.Add("CodegenCS");
            _namespaces.Add("CodegenCS.DbSchema");

            // Newtonsoft
            _namespaces.Add("Newtonsoft.Json");
            AddAssembly(MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.JsonConvert).GetTypeInfo().Assembly.Location));

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
        public bool Compile(string[] sources, string targetFile)
        {
            var syntaxTrees = sources.Select(source => CSharpSyntaxTree.ParseText(File.ReadAllText(source), _parseOptions)).ToList();
            // ParseText really better? https://stackoverflow.com/questions/16338131/using-roslyn-to-parse-transform-generate-code-am-i-aiming-too-high-or-too-low
            //SyntaxFactory.ParseSyntaxTree(SourceText.From(text, Encoding.UTF8), options, filename);


            //TODO: support for top-level statements?
            CSharpCompilation compilation = CSharpCompilation.Create("assemblyName", syntaxTrees,
                _references,
                _compilationOptions);

            using (var dllStream = new MemoryStream())
            {
                var emitResult = compilation.Emit(dllStream/*, pdbStream*/);

                Action<Diagnostic> writeDiag = (diag) =>
                {
                    var lineStart = diag.Location.GetLineSpan().StartLinePosition.Line;
                    var lineEnd = diag.Location.GetLineSpan().EndLinePosition.Line;
                    Console.WriteLine($"{diag.Id}: Line {lineStart}{(lineStart != lineEnd ? "-" + lineEnd : "")} {diag.GetMessage()}");
                };

                var errors = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
                if (errors.Any())
                {
                    using (Console.WithColor(ConsoleColor.Red))
                    {
                        Console.WriteLine("Errors: ");
                        foreach (var error in errors)
                            writeDiag(error);
                    }
                }

                var warnings = emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning);
                if (warnings.Any())
                {
                    using (Console.WithColor(ConsoleColor.Yellow))
                    {
                        Console.WriteLine("Warnings: ");
                        foreach (var warning in warnings)
                            writeDiag(warning);
                    }
                }

                if (!emitResult.Success)
                {
                    return false;
                }

                dllStream.Seek(0, SeekOrigin.Begin);

                if (!new FileInfo(targetFile).Directory.Exists)
                    new FileInfo(targetFile).Directory.Create();

                using (FileStream fs = new FileStream(targetFile, FileMode.OpenOrCreate))
                {
                    dllStream.CopyTo(fs);
                    fs.Flush();
                }
                return true;
            }

        }
        #endregion

    }
}
