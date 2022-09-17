**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/CodegenCS/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about the **dotnet-codegencs tool**:
- If you are **writing a template** (code generator) and want to learn more about CodegenCS features (and internals) then check out the [CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) documentation.
- If you want to **compile and run templates** or **reverse-engineer a database schema** this is the right place.
- If you want to **browse the sample templates** (POCO Generators, DAL generators, etc) check out [https://github.com/CodegenCS/Templates/](https://github.com/CodegenCS/Templates/)
<!-- - If you just want to **download the Visual Studio Extension** check out... (Pending)   -->


# <a name="dotnet-codegencs"></a>CodegenCS Command-Line Tool (dotnet-codegencs)

[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)

**dotnet-codegencs** is a **[.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)** that contains utilities to download, build and run templates.  
It can also be used to extract models (reverse engineer) from existing sources, so that those models can be used with the templates.  

# <a name="quickstart"></a>Quickstart

## Installation

- Pre-requisite: .NET
  If you don't have .NET installed you can install it from https://dotnet.microsoft.com/en-us/download
- Install running this command: ```dotnet tool install -g dotnet-codegencs```  
   If your environment is configured to use private Nuget feeds (in addition to nuget.org) you may need `--ignore-failed-sources` option to ignore not-found errors.


## Extract your Database Model

CodegenCS templates can be based on any JSON input model, but **if you want to generate a model based on a database** (which is probably the most popular use of code generators) you can use the out-of-the-box [DatabaseSchema model](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) which represents the schema of a relational database.  

To **extract the schema of a database** into a JSON file you can use the command `dotnet-codegencs model dbschema extract`, like this:  
`dotnet-codegencs model dbschema extract <MSSQL or POSTGRESQL> <connectionString> <output.json>`

Examples:
- `dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json` (MSSQL using SQL authentication)
- `dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json` (MSSQL using Windows authentication)
- `dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json` (PostgreSQL)

If you don't have a database and want a sample schema you can download AdventureWorks schema [here](https://raw.githubusercontent.com/CodegenCS/CodegenCS/master/src/Models/CodegenCS.DbSchema.SampleDatabases/AdventureWorksSchema.json).

## Download a Template

The `template clone` command is used to download a copy of any online template to your local folder.  
Let's download a simple template called **SimplePocos** that can generate POCOs for all our database tables:

`dotnet-codegencs template clone https://github.com/CodegenCS/Templates/SimplePocos/SimplePocos.cs`

(You can browser other [sample templates here](https://github.com/CodegenCS/Templates/)).

## Run the Template (Generate POCOs)

SimplePocos template requires [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) which is the namespace for the generated POCOs, so it should be invoked like `dotnet-codegencs template run SimplePocos.dll <dbSchema.json> <namespace>`. Let's use the model extracted in the previous step and let's define the namespace as "MyEntities":

`dotnet-codegencs template run SimplePocos.dll AdventureWorks.json MyEntities`

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


# <a name="writing-templates"></a>How to Write Templates

When you run `dotnet-codegencs template run` it expects that your template implements one of the [possible templating interfaces](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS#template-interfaces):
- `ICodegenTemplate<TModel>`: This is the most common template interface - it gets a model (type TModel) and writes output to a ICodegenTextWriter (so it's a "single-file template"):
- `ICodegenMultifileTemplate<TModel>`: This is similar to the previous but instead of getting a ICodegenTextWriter (and writing into a single file) it gets a ICodegenContext (and therefore can write to multiple files)
- `ICodegenStringTemplate<TModel>`: for templates that just return an interpolated string

So basically `template run` load your template and detect which interface you have implemented ([example](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L34)), and then it will automatically load the model (deserialize it into the appropriate type) and pass it to your template.  

There are other interfaces (e.g. you don't need to get a model, or you may expect two different models) but the 3 above are the most common. As an example, if your template uses 2 models (`SomeInterface<TModel1, TModel2>`) then `template run` would expect (and load) two input files.

<br/>
<br/>

# Advanced 

## Adding Arguments and Options to your Template

Templates may define their own options and arguments using [.NET System.CommandLine](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options). Template-specific arguments and options should be passed after the model:  
`dotnet-codegencs template run <template> <model> [template-args]`

SimplePocos template is a good example of how to use custom arguments and options:
- It defines [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) which is the namespace for the generated POCOs
- It defines a [-p:SingleFile option](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L50) which when used will generate all POCOs under a single filename (default output file)

`dotnet-codegencs template run SimplePocos.cs --File MyPOCOS.cs AdventureWorks.json MyProject.POCOs -p:SingleFile`

In the command above, `--File` is an option of `template run` and defines the default output file, while `-p:SingleFile` is a template-specific option (SimplePocos option) and defines that all POCOs should be generated into that single file.  
If we don't specify `-p:SingleFile` then SimplePocos will generate each file on it's own [`<TableName>.generated.poco`](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L313).  
If we specify `-p:SingleFile` but don't specify a `--File` then the default output file would be `SimplePocos.generated.cs` (since we're running `dotnet-codegencs template run SimplePocos ...`).  
(P.S. we suggest this `-p:` as an [alternative prefix](https://github.com/CodegenCS/command-line-api/commit/b78690b47a68e9a9aca419c0c053df1acb9317b5) to avoid conflicting options with the main tool, but it's also possible to use  standard formats like `--youroption` or `/youroption`)

For any template that [define their own arguments/options](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) using [`ConfigureCommand()`](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L47) and [.NET System.CommandLine syntax](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options) we can also get help (see template usage and options):

`dotnet-codegencs template run SimplePocos.cs --help`

## Using third-party libraries

Currently dotnet-codegencs will automatically [add some libraries](https://github.com/CodegenCS/CodegenCS/blob/master/src/dotnet-codegencs/CodegenCS.TemplateBuilder/RoslynCompiler.cs) including Generics, System.Net.Http, System.IO, and Newtonsoft JSON. In the future dotnet-codegencs should allow dynamic nuget references, for for now if you want to use other libraries please use [CodegenCS Core Library](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS) directly.


<br/>

# License
MIT License

