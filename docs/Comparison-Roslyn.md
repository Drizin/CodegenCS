# How does CodegenCS compare to Roslyn?

Roslyn has a **totally** different purpose, so this is like comparing Apples to Oranges.

## What is Roslyn?

- Roslyn **can only read from C# or** **VB.NET** **sources**. It loads one or more source files, builds a compilation **Syntax Tree**, and then it can **analyze** the syntax tree.  
- **Roslyn Analyzers** run during solution build and will analyze this **Syntax Tree** to check for errors (they can show errors or warnings). Creating an Analyzer is not easy and reading the Syntax Tree is not easy either.  
 - **Roslyn Source Generators** are almost identical Roslyn Analyzers (they also run **during build** and also require you to inspect the **Syntax Tree**), with the difference that they are used to generate **temporary files** that are **added to the compilation**.  
 
 To sum, Roslyn Source Generators will augment a .NET project during compilation by reading the source code and generating more source code on-the-fly.

## CodegenCS is totally different:

- It can read from any input model (like a database schema or anything else).
  It doesn't even have a provider to read from C# sources because it's **NOT** for analyzing/augmenting .NET programs
- It can be executed anytime (in your build pipeline like prebuild/postbuild, or just anytime you want)
- It generates static files (that may or may not be part of your compilation, may or may not be in source control).   
- It can be invoked directly with it's CLI tool, no need to be plugged into your build (but you can do it if you want)

To sum, the right comparison (Apples to Apples) would be [comparing CodegenCS vs T4 Templates](Comparison-T4.md).

## "But Roslyn is the right tool to generate code"

Is it? Let's see...

One way of using Roslyn Code Generation is to **generate code programmatically using Syntax Trees**. This is so "cool" and totally [nonsense](https://www.reddit.com/r/dotnet/comments/t3ds4m/why_is_noone_using_roslyn_tokenbased_code/).

If you were to create a simple POCO like this:

```cs
public class MyPOCO  
{  
    public int Name {get;set;}  
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

**The idea of CodegenCS is to just "generate text" - it doesn't care about the output syntax/language that you're generating**.

## "But Roslyn can also generate code by concatenating strings..."

It can - and if you're augmenting on top of an existing C# data source (reading a complex Syntax Tree and generating temporary code that doesn't need to be versioned or even well formatted) then Roslyn might be the right tool.  
(Roslyn doesn't provide anything to help with **indentation** or **tracking multiple files** - and **it doesn't even need to** since all output is just discarded after the build).

However, if you're writing complex templates or if your output needs decent formatting/indenting), then CodegenCS might be a better tool because:
- It has a [TextWriter on Steroids](https://github.com/Drizin/CodegenCS/tree/master/src/Core/CodegenCS) that makes it easy to write clean/concise/reusable templates, with no-hassle indentation control. 
- Simple loops and simple template-composition can be done within an interpolated string, without using multiple statements and control blocks - avoiding a bunch of loops and mixing control-code with generated code. 
- It uses the powerful **C# 11 Raw String Literals** (you can use it even if the target project does not use C# 11) which makes everything even cleaner and doesn't require escaping of curly braces.
- And [much more](https://github.com/Drizin/CodegenCS/tree/master/src/Core/CodegenCS)

## Can I run CodegenCS from a Roslyn Source Generator?

You can, but probably that's not the best way of using CodegenCS:  
CodegenCS main purpose is to generate static sources: it's great at generating well formatted code, managing multiple files, running from command-line tool or from Visual Studio Extension.  
But if you're running from Roslyn you probably don't need any of that...

You could still rely on our models (e.g. generate from database schema), but why would you need Roslyn Source Generator for that? Just use dotnet-codegencs in your build pipeline...

Yet, if you want to run CodegenCS from a Roslyn Source Generator, check out [this Sample](https://github.com/CodegenCS/Samples/tree/main/src/SampleSourceGenerator.SimplePocos)