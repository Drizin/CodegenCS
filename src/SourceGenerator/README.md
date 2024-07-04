**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/Drizin/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about **CodegenCS Source Generator**:


# CodegenCS Source Generator

Our [Source Generator](https://nuget.org/packages/CodegenCS.SourceGenerator) allows running templates on-the-fly during compilation.  
It's possible to render physical files on disk or just render in-memory (no need to add to source-control or put into ignore lists).


## Quickstart

1. Install nuget CodegenCS.SourceGenerator to your project
1. Add this to your project
   ```
    <ItemGroup>
      <CompilerVisibleItemMetadata Include="AdditionalFiles" MetadataName="CodegenCSOutput" />
      <AdditionalFiles Include="Template.csx" CodegenCSOutput="Memory" />
      <!-- Use "Memory" or "File", where "File" will save the output files to disk -->
      <!-- you can include as many templates as you want -->
    </ItemGroup>
   ```
1. If you want to augment on existing classes (using Roslyn Syntax Tree), just inject `GeneratorExecutionContext` (see [Template3.csx](/Samples/SourceGenerator1/Template3.csx) example).

