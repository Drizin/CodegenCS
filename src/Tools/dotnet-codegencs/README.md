**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/CodegenCS/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about **CodegenCS Command-line Tool (dotnet-codegencs)**:
- If you are **writing a template** (code generator) and want to learn more about CodegenCS features (and internals) then check out the [CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) documentation.
- If you want to **compile and run templates** or **reverse-engineer a database schema** this is the right place.
- If you want to **browse the sample templates** (POCO Generators, DAL generators, etc) check out [https://github.com/CodegenCS/Templates/](https://github.com/CodegenCS/Templates/)
- If you just want to **download the Visual Studio Extension** check out the [Visual Studio Extension](https://github.com/CodegenCS/CodegenCS/tree/master/src/VisualStudio/)


# <a name="dotnet-codegencs"></a>CodegenCS Command-Line Tool (dotnet-codegencs)

[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)

**dotnet-codegencs** is a **[.NET Command-line Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)** that contains utilities to download, build and run templates.  
It can also be used to extract models (reverse engineer) from existing sources, so that those models can be used with the templates.  

## Installation

- Pre-requisite: .NET
  If you don't have .NET installed you can install it from https://dotnet.microsoft.com/en-us/download
- Install running this command: ```dotnet tool install -g dotnet-codegencs```  
   If your environment is configured to use private Nuget feeds (in addition to nuget.org) you may need `--ignore-failed-sources` option to ignore not-found errors.

<br/>
<br/>







# How it works

When you run `dotnet-codegencs template run`:
- It will search for an entrypoint method called `Main()` (in any class in the cs file)
- It will automatically create an instance of that class and invoke that `Main()` method.
- Both the class constructor and the `Main()` method may get automatically injected into them some important types: `ICodegenContext` (if you want to write to multiple files), `ICodegenTextWriter` (if you want to write to a single file), `CliArgs` (if your template needs to read custom command-line arguments), etc.
- Both the class constructor and the `Main()` method may also get automatically injected into them the Input Models: any class that implements  `IJsonInputModel`. Any class that implements that interface will be automatically deserialized from a JSON file (so it must be provided in the command-line, after the template name) and injected as necessary.
- `Main()` return type can be `void` - use this if you're manually writing to `ICodegenContext` or `ICodegenTextWriter`. Outputs are automatically saved.
- `Main()` return type can also be `int` - it's exactly like `void` but the templates can return a nonzero result to indicate an error (and outputs wouldn't be saved).
- `Main()` return type can also be `FormattableString` or `string` - it's a more "functional" approach - in this case that return is automatically written to the default output file (`ICodegenTextWriter`) - no need to call `writer.WriteLine()`
- If multiple files were written (into a `ICodegenContext`) then they are all saved under current folder  
  Different folder can be specified using option `--OutputFolder [OutputFolder]`
- If a single file was written (into a `ICodegenTextWriter`) then it's saved under current folder as `<TemplateName>.generated.cs`  
  Different file can be specified using option `--File [DefaultOutputFile]`
- Using statements are automatically added to the script (if not there) to let templates be as simple as possible
 
In the templates repository you'll find templates using the [legacy syntax](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L35) which requires templates to implements one of the [templating interfaces](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS#template-interfaces) (`ICodegenTemplate<TModel>`, `ICodegenMultifileTemplate<TModel>`, `ICodegenStringTemplate<TModel>`). That's legacy, now that it's possible (and easier) to just use a `Main()` method and inject whatever object you need.




<br/>
<br/>

# Commands

The purpose of this document is only to show the basic commands, it does NOT describe all different options - but you can see all available commands and options using `--help`:
- `dotnet-codegencs --help` (shows all available commands)
- `dotnet-codegencs template --help` (shows template subcommands)
- `dotnet-codegencs template clone --help` (shows template clone usage/options)
- `dotnet-codegencs template build --help` (shows template build usage/options)
- `dotnet-codegencs template run --help` (shows template run usage/options)
- `dotnet-codegencs model dbschema extract --help` (shows how to extract a dbschema model)
- Etc.


## Template Clone

As shown in the Quickstart, `template clone` is the first step and is used to get a local copy of a template (like `git clone`, but for downloading a single .cs file - so it's more like `wget`).  

You can download templates from any 3rd-party origin by providing the full url: `dotnet-codegencs template clone <template-url>`  
We're not responsible for 3rd-party templates - **use them at your own risk** (ensure that you trust the source and review any scripts before running it).  
Downloading from third-party sources will show a warning (but you can skip it with option `--allow-untrusted-origin`).  
  
If you're downloading from our own [Templates repository (https://github.com/CodegenCS/Templates)](https://github.com/CodegenCS/Templates) you can use this shortcut:  
`dotnet-codegencs template clone <templateName>`  
(equivalent to:  `dotnet-codegencs template clone https://github.com/CodegenCS/Templates/<templateName>/<templateName>.cs`).

`template clone` will automatically build the source into a dll after the download (equivalent to running `template build`).

## Template Run

Templates can be executed from the `.cs` source (they will be compiled on-the-fly into a dll file) or they can be executed from the `.dll` file (which is generated using `template build`).  
If you need to invoke the template multiple times in a row then running from the `.dll` might be the preferred method (faster), but for most cases running from the `.cs` should be fine.

When templates are downloaded (using `template clone`) they are automatically compiled into a DLL (no need to run `template build`), so immediately after cloning you can run using the `.dll` instead of `.cs` (and if you don't provide an extension `template run` will first try to find with the `dll` extension before trying `.cs`).

`template run` also accepts some options like `--OutputFolder <outputFolder>` (base folder for all output files), or `--File <defaultOutputFile>` (name of the default output file, useful for templates that write to a single file)

## Template Build

This compiles the template (e.g. `template.cs`)  into a DLL (e.g. `template.dll`).  
As mentioned earlier, `template build` is totally optional since `template run` can use the `.cs` source file.  
If you're running templates directly from `.dll` and you have modified the `.cs` source then you can rebuild the dll using `dotnet-codegencs template build <template.cs>`.


<!-- # <a name="writing-templates"></a>How to Write Templates

When you run `dotnet-codegencs template run` it expects that your template implements one of the [possible templating interfaces](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS#template-interfaces):
- `ICodegenTemplate<TModel>`: This is the most common template interface - it gets a model (type TModel) and writes output to a ICodegenTextWriter (so it's a "single-file template"):
- `ICodegenMultifileTemplate<TModel>`: This is similar to the previous but instead of getting a ICodegenTextWriter (and writing into a single file) it gets a ICodegenContext (and therefore can write to multiple files)
- `ICodegenStringTemplate<TModel>`: for templates that just return an interpolated string

So basically `template run` load your template and detect which interface you have implemented ([example](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L35)), and then it will automatically load the model (deserialize it into the appropriate type) and pass it to your template.  

There are other interfaces (e.g. you don't need to get a model, or you may expect two different models) but the 3 above are the most common. As an example, if your template uses 2 models (`SomeInterface<TModel1, TModel2>`) then `template run` would expect (and load) two input files. -->

<br/>
<br/>

# Advanced 

## Adding Arguments and Options to your Template

Templates may define their own options and arguments using [.NET System.CommandLine](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options). Template-specific arguments and options should be passed after the model:  
`dotnet-codegencs template run <template> <model> [template-args]`

SimplePocos template is a good example of how to use custom arguments and options:
- It defines [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L52) which is the namespace for the generated POCOs
- It defines a [-p:SingleFile option](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L53) which when used will generate all POCOs under a single filename (default output file)

`dotnet-codegencs template run SimplePocos.cs --File MyPOCOS.cs AdventureWorks.json MyProject.POCOs -p:SingleFile`

In the command above, `--File` is an option of `template run` and defines the default output file, while `-p:SingleFile` is a template-specific option (SimplePocos option) and defines that all POCOs should be generated into that single file.  
If we don't specify `-p:SingleFile` then SimplePocos will generate each file on it's own [`<TableName>.generated.poco`](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L314).  
If we specify `-p:SingleFile` but don't specify a `--File` then the default output file would be `SimplePocos.generated.cs` (since we're running `dotnet-codegencs template run SimplePocos ...`).  
(P.S. we suggest this `-p:` as an [alternative prefix](https://github.com/CodegenCS/command-line-api/commit/b78690b47a68e9a9aca419c0c053df1acb9317b5) to avoid conflicting options with the main tool, but it's also possible to use  standard formats like `--youroption` or `/youroption`)

For any template that [define their own arguments/options](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L52) using [`ConfigureCommand()`](https://github.com/CodegenCS/Templates/blob/main/DatabaseSchema/SimplePocos/SimplePocos.cs#L47) and [.NET System.CommandLine syntax](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options) we can also get help (see template usage and options):

`dotnet-codegencs template run SimplePocos.cs --help`

## Using third-party libraries

Currently dotnet-codegencs will automatically [add some libraries](https://github.com/CodegenCS/CodegenCS/blob/master/src/Tools/TemplateBuilder/RoslynCompiler.cs) including Generics, System.Net.Http, System.IO, and Newtonsoft JSON. In the future dotnet-codegencs should allow dynamic nuget references, for for now if you want to use other libraries please use [CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) directly.


<br/>

# License
MIT License

