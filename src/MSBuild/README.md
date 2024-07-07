**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/Drizin/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about **CodegenCS Source Generator**:


# CodegenCS MSBuild Task

Our [MSBuild Task](https://nuget.org/packages/CodegenCS.MSBuild) allows running templates on-the-fly during compilation. 

MSBuild Task `CodegenBuildTask` is automatically invoked during `BeforeCompile` target, will search for `*.csx` files in the project folder and will run each one. 
Files are physically saved to disk and will be automatically added to your compilation.


## Quickstart

1. Install nuget `CodegenCS.MSBuild` to your project
   ```xml
   <ItemGroup>
     <PackageReference Include="CodegenCS.MSBuild" Version="3.5.1" PrivateAssets="All" />
   </ItemGroup>
   ```

1. Create a CodegenCS template in your project (name it with `CSX` extension). Example:
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
    See full examples: [SDK-project](/Samples/MSBuild1/) and [non-SDK-project](/Samples/MSBuild2/).
