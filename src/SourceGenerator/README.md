**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/Drizin/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about **CodegenCS Source Generator**:


# CodegenCS Source Generator

Our [Source Generator](https://nuget.org/packages/CodegenCS.SourceGenerator) allows running templates on-the-fly during compilation. We abstract the boilerplate and the hard parts of creating a source generator so you can only focus on your templates.

It's possible to render physical files on disk or just render in-memory (no need to add to source-control or put into ignore lists).


## Quickstart

1. Install nuget `CodegenCS.SourceGenerator` to your project
   ```xml
   <ItemGroup>
     <PackageReference Include="CodegenCS.SourceGenerator" Version="3.5.2" />
   </ItemGroup>
   ```

1. Create a CodegenCS template. Example:
   ```cs
    class Template
    {
        void Main(ICodegenContext context)
        {
            context[$"MyClass1.g.cs"].Write($$"""
                public class MyClas1
                {
                }
                """);
        }
    }
   ```
1. Add this to your csproj:
   ```xml
    <ItemGroup>
      <CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="CodegenCSOutput" />
      <AdditionalFiles Include="Template.csx" CodegenCSOutput="Memory" />
      <!-- Use "Memory" or "File", where "File" will save the output files to disk -->
      <!-- you can include as many templates as you want -->
    </ItemGroup>
   ```
1. If you want to augment on existing classes (using Roslyn Syntax Tree), just inject `GeneratorExecutionContext` like the example below: .

    ```cs
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
                context[$"{className}.g.cs"].Write(GenerateClass(ns, className));
            }
        }
    ```
    See full example [here](/Samples/SourceGenerator1/Template3.csx).