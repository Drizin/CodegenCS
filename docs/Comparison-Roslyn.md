# How does CodegenCS compare to Roslyn Source Generators?

## What is Roslyn?

**Roslyn** is just the codename for the .NET Compiler Platform, which includes compilers (CodegenCS actually uses Roslyn to compile templates) and code analysis APIs.

## What are Roslyn Analyzers?

**Roslyn Analyzers** are basically plugins that run during compilation (and also during real time typing), read source files, build a compilation **Syntax Tree**, and then custom code can be used to analyze the syntax tree to provide suggestions/warnings/errors.

## What are Roslyn Source Generators?

**Roslyn Source Generators** are almost identical to Roslyn Analyzers but they can generate **temporary files** that are **added to the compilation**:

**Usually the new generated code is based on the Compilation Syntax Trees** (because if you don't need to read the Syntax Tree then you don't need a Source Generator - you can just generate your code during a prebuild event).

[Most source generators render code using plain text](https://www.reddit.com/r/dotnet/comments/t3ds4m/why_is_noone_using_roslyn_tokenbased_code/) (e.g. using StringBuilders) because it's just **much much easier** than manually building roslyn tokens. 
Generating code using roslyn tokens and syntax trees is both painful and nonsense. Writing human-readable code and letting compilers transform that into syntax trees is the exact reason why compilers exist.  
Yet, some developers love hard challenges so instead of getting the job done and writing maintainable code they prefer spending weeks learning how syntax trees work generating code as if they were a compiler.

[Even MSFT says](https://www.reddit.com/r/dotnet/comments/t3ds4m/comment/hyudrtq/?utm_source=share&utm_medium=web3x&utm_name=web3xcss&utm_term=1&utm_content=share_button) that syntax trees are supposed to be used for analyzing code, and not for generating code.  

To sum, Roslyn Source Generators will augment a .NET project during compilation by reading the source code and generating more source code on-the-fly. But again: the fact that you have to **read** existing code using syntax trees does not imply that you have to **write** the new code using syntax trees.

## Do I need a Source Generator? Or is it better to use script/prebuild/dotnet-codegencs?

If you don't need to generate code based on existing code then you don't need syntax trees or Source Generators - you can just use prebuild events to invoke our command-line tool `dotnet-codegencs`: see [this example](/Samples/PrebuildEvent/RunTemplates.ps1) of a prebuild script that will install dotnet-codegencs, refresh a database schema, and run a template that generates POCOs. 

Frequent claims people do when they think they need a Source Generator:
- "I need it to run during my build" -> you can use prebuild events
- "I don't want to add to source control" -> you can just ignore patterns like `*.generated.cs` or `*.g.cs`
- "I need to read the Syntax Tree" -> now you can with [**CodegenCS.SourceGenerator**](https://www.nuget.org/packages/CodegenCS.SourceGenerator)

Other advantages of using scripts instead of source generators:
- Templates can be executed anytime (no need to rebuild the project). 
- You can easily see the generated files (and even save into source control if you want)

## CodegenCS Source Generator

CodegenCS now has a [**Source Generator plugin**](https://www.nuget.org/packages/CodegenCS.SourceGenerator) that you can just add to your project and use it to run templates.

So if you need to generate code based on existing code (or if you have any other reason to use source generators),  now you don't need to learn how source generators work, you don't need to write your own (which is hard) - you can just use our generator and build friendly CodegenCS templates, and leverage our sample templates.

By injecting `GeneratorExecutionContext` in your templates you can get access to the full syntax tree (exactly like you were writing your own source generator) so you can augment on existing classes - see [Template3.csx](/Samples/SourceGenerator1/Template3.csx) for an example.
  
Check out [this Sample](https://github.com/CodegenCS/tree/main/Samples/SourceGenerator1) and [this Sample](https://github.com/CodegenCS/tree/main/Samples/SourceGenerator2)


## "Roslyn is obviously the correct way of generating code"

Using Roslyn to **generate code programmatically using Syntax Trees** is so "cool" (is it?) but totally [nonsense](https://www.reddit.com/r/dotnet/comments/t3ds4m/why_is_noone_using_roslyn_tokenbased_code/). 

If you were to create a simple POCO like this:

```cs
public class MyPOCO  
{  
    public int Name { get; set; }
}
```

... you would need nonsense code like this:
```cs
// How to generate it using Roslyn Syntax Trees:
CompilationUnit()
.WithMembers(
    SingletonList<MemberDeclarationSyntax>(
        ClassDeclaration("MyPOCO")
        .WithModifiers(
            TokenList(
                Token(SyntaxKind.PublicKeyword)))
        .WithMembers(
            SingletonList<MemberDeclarationSyntax>(
                PropertyDeclaration(
                    PredefinedType(
                        Token(SyntaxKind.IntKeyword)),
                    Identifier("Name"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword)))
                .WithAccessorList(
                    AccessorList(
                        List<AccessorDeclarationSyntax>(
                            new AccessorDeclarationSyntax[]{
                                AccessorDeclaration(
                                    SyntaxKind.GetAccessorDeclaration)
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken)),
                                AccessorDeclaration(
                                    SyntaxKind.SetAccessorDeclaration)
                                .WithSemicolonToken(
                                    Token(SyntaxKind.SemicolonToken))})))))))
```

I'm sure that's not the best way to generate a POCO class and not the best use of our time.

To sum: why would we have to worry about valid Syntax Trees if all we want to do is to render some simple output code?   

**The idea of CodegenCS is to just "generate text" - it doesn't care about the output syntax/language that you're generating**. And most developers writing their own source generators also prefer this method (generate text).

## "But Roslyn can also generate code by concatenating strings..."

It can, but it doesn't provide many nice features that we do: Smart **indentation**, **concise syntax** (subtemplates), **tracking multiple active files**, decent debugging support, and [much more](https://github.com/Drizin/CodegenCS/)

