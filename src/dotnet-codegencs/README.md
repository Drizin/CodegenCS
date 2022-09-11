**CodegenCS is a Toolkit for doing Code Generation using plain C#**. For an overview of all CodegenCS components and tools check out the [Main Project Page](https://github.com/CodegenCS/CodegenCS/).

[![Nuget](https://img.shields.io/nuget/v/dotnet-codegencs?label=dotnet-codegencs)](https://www.nuget.org/packages/dotnet-codegencs)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-codegencs.svg)](https://www.nuget.org/packages/dotnet-codegencs)

# <a name="dotnet-codegencs"></a>dotnet-codegencs (.NET Tool)

**dotnet-codegencs** is a **[.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools)** that contains utilities to download, build and run templates.  
It can also be used to extract models (reverse engineer) from existing sources, so that those models can be used with the templates.  

# <a name="quickstart"></a>Quickstart

## Installation

- Pre-requisite: .NET
  If you don't have .NET installed you can install it from https://dotnet.microsoft.com/en-us/download
- Run this: ```dotnet tool install -g dotnet-codegencs```  
   If your environment is configured to use private Nuget feeds (instead of the default nuget.org) you may need this option to ignore not-found errors:  
   ```dotnet tool install -g dotnet-codegencs --ignore-failed-sources```


## Extract your Database Model

CodegenCS templates can be based on any JSON input model, but if you want to generate based on a database you can use the out-of-the-box [DatabaseSchema model](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema/DbSchema) which represents the schema of a relational database.  
To extract the schema of a database into a JSON file you can use `model dbschema extract` command like this:  
`dotnet-codegencs model dbschema extract <MSSQL or POSTGRESQL> <connectionString> <output.json>`

Examples:
- `dotnet-codegencs model dbschema extract mssql "Server=MYSERVER; Database=AdventureWorks; User Id=myUsername;Password=MyPassword" AdventureWorks.json` (MSSQL using SQL authentication)
- `dotnet-codegencs model dbschema extract mssql "Server=(local)\SQLEXPRESS; Database=AdventureWorks; Integrated Security=True" AdventureWorks.json` (MSSQL using Windows authentication)
- `dotnet-codegencs model dbschema extract postgresql "Host=localhost; Database=Adventureworks; Username=postgres; Password=MyPassword" AdventureWorks.json` (PostgreSQL)

## Download a Template

The `template clone` command is used to download a copy of any online template to your local folder.  
Let's download a simple template (called SimplePocos) that will generate POCOs for all your database tables:

`dotnet-codegencs template clone https://github.com/CodegenCS/Templates/SimplePocos/SimplePocos.cs`

## Run the Template (Generate POCOs)

The SimplePocos template requires [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) which is the namespace for the generated POCOs, so it should be invoked like `dotnet-codegencs template run SimplePocos.dll <dbSchema.json> <namespace>`. Let's define a namespace and use the model extracted in the previous step:

`dotnet-codegencs template run SimplePocos.dll AdventureWorks.json MyProjectPOCOs`



# Full Documentation

## Get help

Most commands accept `--help`, so you can use `dotnet-codegencs --help` to see all usage options, or you can get help for specific commands like `dotnet-codegencs template --help`, `dotnet-codegencs template clone --help`, `dotnet-codegencs template build --help`, `dotnet-codegencs template run --help`, `dotnet-codegencs model --help`, etc.  

This document will just show tricks but won't describe all options that you can see using `--help`.

## template clone

As shown above, this is the first step and it's used to get a local copy of a template (like git clone, but for downloading a single .cs file).  

You can download templates from any 3rd-party origin by providing the full url: `dotnet-codegencs template clone <template-url>`  
We're not responsible if you're running templates from 3rd-parties - **use them at your own risk** (ensure that you trust the source and review any scripts before running it).  
Downloading from third-party sources will show a warning (but you can skip it with an option flag).  
  
If you're downloading from our own [Templates repository (CodegenCS/Templates)](https://github.com/CodegenCS/Templates) you can just run like this:  
`dotnet-codegencs template clone <templateName>`  
... which is basically a shortcut to `dotnet-codegencs template clone https://github.com/CodegenCS/Templates/<templateName>/<templateName>.cs`.

`template clone` will automatically build the source into a dll after the download (like `template build` would do - see below).

## template run

Templates can be executed from the `.cs` source (they will be compiled on-the-fly into a dll file, equivalent to what would happen if you explicitly run `template build`).  
Templates can also be executed directly from the `.dll` file (which is generated both by `template build` and also by commands like `template clone` or `template run <source.cs>` which will automatically invoke a build).  
If your template is invoked multiple times you can save a few milliseconds (of compilation time) by running from the DLL.

When templates are downloaded (using `template clone`) or when they are executed (using `template run`) they are automatically compiled into a DLL. On the next executions you can just use the `.dll` instead of `.cs`. If you don't provide the extension the tool will by search both for a dll or (if not found) for .cs.

`template run` also accepts some options like `--OutputFolder <outputFolder>` (base folder for all output files), or `--File <defaultOutputFile>` (name of the default output file, useful for templates that write to a single file)

## template build

As explained earlier `template build` is pretty much optional since we can just run `template run` using the `.cs` source file.  
Additionally when you do `template clone` the source will also be compiled into a dll anyway.  
If you're running templates directly from `.dll` and you have modified the `.cs` source you can force a dll rebuild either by deleting the dll (so it gets automatically rebuilt) or by using `dotnet-codegencs template build <template.cs>`.


## Models

There are [3 possible interfaces](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS#template-interfaces) that your template can implement:
- `ICodegenTemplate<TModel>`: The most common template interface - it gets a model (type TModel) and writes output to a ICodegenTextWriter (so it's a "single-file template"):
- `ICodegenMultifileTemplate<TModel>`: This one is similar to `ICodegenTemplate<TModel>` but instead of receiving a ICodegenTextWriter (and writing into a single file) it receives a ICodegenContext (and therefore can write to multiple files)
- `ICodegenStringTemplate<TModel>`: for templates that just return an interpolated string

`template run` command will automatically detect which interface type you're using ([example](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L34)), and will automatically load the model (deserialize into the appropriate type) to pass it to your template.  
All those interfaces may also take 2 models (`<TModel1, TModel2>`), and in this case `template run` will also require/load two models.

## Template-specific Arguments and Options

Templates may define their own options and arguments using [.NET System.CommandLine](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options). Template-specific arguments and options should be passed after the model:  
`dotnet-codegencs template run <template> <model> [template-args]`

SimplePocos template is a good example of how to use custom arguments and options:
- It defines [1 mandatory argument](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) which is the namespace for the generated POCOs
- It defines a [-p:SingleFile option](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L50) which when used will generate all POCOs under a single filename (default output file)

`dotnet-codegencs template run SimplePocos.cs --File MyPOCOS.cs AdventureWorks.json MyProject.POCOs -p:SingleFile`

In the command above, `--File` is an option of `template run` and defines the default output file, while `-p:SingleFile` is a template-specific option (SimplePocos option) and defines that all POCOs should be generated into that single file.  
If we don't specify `-p:SingleFile` then SimplePocos will generate each file on it's own [`<TableName>.generated.poco`](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L313).  
If we specify `-p:SingleFile` but don't specify a `--File` then the default output file would be `SimplePocos.generated.cs` (since we're running `dotnet-codegencs template run SimplePocos ...`).

For any template that [define their own arguments/options](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L49) using [`ConfigureCommand()`](https://github.com/CodegenCS/Templates/blob/main/SimplePocos/SimplePocos.cs#L47) and [.NET System.CommandLine syntax](https://docs.microsoft.com/en-us/dotnet/standard/commandline/define-commands#define-options) we can also get help (see template usage and options):

`dotnet-codegencs template run SimplePocos.cs --help`


# License
MIT License

