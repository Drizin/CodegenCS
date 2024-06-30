// This template is a little more elaborated: 
// it gets injected Microsoft.CodeAnalysis.GeneratorExecutionContext
// so it can lookup for some syntax nodes from the compilation tree and can build code based on those elements

//#r "Microsoft.CodeAnalysis.dll"
//#r "Microsoft.CodeAnalysis.CSharp.dll"
//#r "System.Collections.Immutable.dll"
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp; // TryGetInferredMemberName extension

class Template3
{
    void Main(ICodegenContext context, GeneratorExecutionContext generatorContext)
    {
        var classes = generatorContext.Compilation.SyntaxTrees
            .SelectMany(syntaxTree => syntaxTree.GetRoot().DescendantNodes())
            .Where(x => x is ClassDeclarationSyntax)
            .Cast<ClassDeclarationSyntax>()
            //.Where(c => c.Identifier.ValueText.EndsWith("Controller", StringComparison.OrdinalIgnoreCase)) // just an example
            .ToList();

        foreach (var c in classes)
        {
            var typeSymbol = generatorContext.Compilation.GetSemanticModel(c.SyntaxTree).GetDeclaredSymbol(c);
            var fqn = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            if (fqn.StartsWith("global::"))
                fqn = fqn.Substring("global::".Length);
            int pos = fqn.LastIndexOf(".");
            if (pos == -1)
                continue;
            var className = fqn.Substring(pos + 1);
            var ns = fqn.Substring(0, pos);
            if (string.IsNullOrEmpty(className))
                continue;
            context[$"{className}.g.cs"].Write(GenerateClass(ns, className)); // ".g.cs" to avoid deleting the other part of the partial class (the real file)
        }
    }

    FormattableString GenerateClass(string ns, string className) => $$"""
        namespace {{ns}}
        {
            partial class {{className}}
            {
                public void Initialize()
                {
                    // This method is generated on the fly!
                }
            }
        }
        """;
}
