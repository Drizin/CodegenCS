# CodegenCS (Core Library)

**C# Library for Code Generation**

# Description

CodegenCS Core Library is a Class Library for Code Generation that allows us to generate code using plain C#.  

By using plain C# we can render our outputs using plain strings (and string interpolation) and using well-established and well-known C# constructs (invoking methods, passing parameters, embedding strings, looping, formatting, etc) - no need to learn a new syntax.  

On top of that our templates can leverage .NET Framework and .NET libraries for anything that they need (e.g. reading input models from JSON file, or reading directly the the schema from a database, or reading the YAML specs of a REST API).  

CodegenCS is written in C# and templates are developed using plain C# - but the templates can write any text-based output (not only C# code - you can write Javascript, Python, Java, HTML, SQL Scripts, CSHTML, XML, Terraform files, or anything else)

For a general overview of all CodegenCS components and tools check out the [Main Page](https://github.com/Drizin/CodegenCS/).

## Quick Start / NuGet Package

- Create a C# Console project
- Install the [NuGet package CodegenCS](https://www.nuget.org/packages/CodegenCS)
- Import namespace: `using CodegenCS`
- Start using like examples below (or check out more examples in [unit tests](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Tests)).


# Basics

## CodegenTextWriter / ICodegenTextWriter

**`CodegenTextWriter`** is the heart of CodegenCS Library. Basically it's a custom TextWriter (a **TextWriter on steroids**) created to solve common code generation issues (indent control, linebreaks control, mixed-indentation issue):

- Like any regular TextWriter we can manually write to it using plain C#: we can `Write()`, `WriteLine()`, write strings or interpolated strings, write multiline strings, etc
- It's strongly focused on **interpolated strings**, and supports a large number of object types that can just be interpolated (embedded) within strings (this reduces the need for manual writes)
- It has **Indentation Control** - it keeps track of the current Indent level, and when we start writing any new lines they will be indented (padded) according to the current level
    - Indentation can be explicitly controlled (we can increase indent, decrease indent, set indentation to be any number of spaces or tabs), and there are some helpers for easily writing **indented blocks** (you create a new scope, and everything you write inside that scope is automatically indented)
    - Indentation can be **implicitly controlled** (much easier!) when you use interpolated strings: before the writer renders any interpolated object it will "save the cursor position" (capture the current indentation level even if it was **implicitly** defined by spaces or tabs added before the interpolated variable), and then if the interpolated objects spans into multiple lines those lines will all be indented correctly (**we preserve the indentation** of subsequent lines).  
    (In other words you can just embed a class nested under a namespace and it will be indented like magic, you can just embed a method under a class and it will be indented like magic, etc)
- Will automatically adjust multiline blocks (by removing left-padding and removing the first empty line) so that we can just indent-align our multiline blocks wherever they look better - no need to manually worry about whitespace or indenting (since indenting is controlled by the writer context). 
- This same magic (that we've been doing for years) now can also be done by using the new C# 11 [**"Raw String Literals"**](https://github.com/dotnet/csharplang/blob/main/proposals/raw-string-literal.md) - with the advantage that raw string literals let us render regular single-curly-braces without escaping (much easier to write C#/Java/C blocks) and for interpolated expressions we can use double-mustaches
- Supports string interpolation of IEnumerables (items are rendered one by one, and between the items we can have separators like line-breaks or any other - it's all configurable using the `.Render()` extension over `IEnumerable<T>`)
- Supports Control Symbols like IF-ENDIF / IF-ELSE-ENDIF / IIF (Immediate IF) - it's a concise syntax for conditional blocks
- Supports string interpolation of Actions, Functions or Templating interfaces (prefered)

**Writing lines to a CodegenTextWriter and saving to file**

```cs
var w = new CodegenTextWriter();
w.WriteLine("Line1");
w.SaveToFile("File1.cs");
```

**Fluent API**

Most public methods (like `Write()`, `WriteLine()`, `IncreaseIndent()`, etc) return the object itself (`ICodegenTextWriter`), which mean that it's possible to write line-by-line (or block-by-block) using a chained methods (Fluent API):

```cs
var w = new CodegenTextWriter();
w
  .WriteLine($"public void {myMethodName}()")
  .WriteLine("{")
  .WriteLine("    // My method...")
  .WriteLine("}");
```

## CodegenContext / ICodegenContext

**`CodegenContext`** is a Context class that can manage multiple output files (keeps everything in-memory until the files are saved to disk).  

**Creating a Context to keep track of multiple files, and save all files at once**

```cs
var ctx = new CodegenContext();

var f1 = ctx["File1.cs"];
var f2 = ctx["File2.cs"];

f1.WriteLine("..."); f2.WriteLine("...");

ctx.SaveFiles(outputFolder);
```


## **Raw String Literals** to write Multi-line Blocks and Interpolated Strings

C# 11 (currently in preview) has a new feature called [**"Raw String Literals"**](https://github.com/dotnet/csharplang/blob/main/proposals/raw-string-literal.md) that helps a lot both for writing multi-line blocks and for writing regular curly-braces when we're using string interpolation.

Any string starting with 3 (or more) double-quotes (and ending with the same number of double-quotes) is considered by the compiler to be raw string literal. If it starts with one or more dollar-signs it's also an interpolated string.  
Raw strings by default can span into multiple lines - there's no need to use `@` character anymore:

```cs
w.WriteLine($$"""
    public void {{methodName}}()
    {
        // My method...
    }
    """);
```

## Embedding FormattableStrings (or other types) inside other Interpolated Strings

CodegenTextWriter supports (can understand and render) a large number of object types that can just be interpolated (embedded) within strings, including `FormattableString` (interpolated strings inside another interpolated strings).

`FormattableString` is the .NET class that implements Interpolated Strings (when we write an interpolated string the compiler creates a FormattableString for us), and for code generation `FormattableString` is preferable over strings because it preserves the individual location of each embedded element (more details in the FAQ below). 

```cs
FormattableString RenderTable(Table table) => $$"""
            /// <summary>
            /// POCO for {{ table.TableName }}
            /// </summary>
            public class {{ table.TableName }}
            {
                // class members...
            }
            """;

void Generate()
{
    var schema = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
    var usersTable = schema.Tables.Single(t => t.TableName=="Users");
    var productsTable = schema.Tables.Single(t => t.TableName=="Products");

    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ RenderTable(usersTable) }}
            {{ RenderTable(productsTable) }}
        }
        """);
}
```

## Read-to-Use Input Models

In the previous example we are using DatabaseSchema from [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) - basically this represents the database schema of a MSSQL database or a PostgreSQL database.  
This is one of the ready-to-use input models that you can use in your templates.

## Implicit Control of Indent Level

CodegenTextWriter supports [`Explicit Indent Control`](#ExplicitIndent), but the preferred method is to implicitly control the indentation by using string interpolation and by positioning the interpolated expressions in the right place (adding whitespace before interpolated expressions).  
In the previous example (repeated below for clarity) there's the `WriteLine` block (which as explaid above will be "left-trimmed" since it's a raw string), which means that `namespace ...` will start at `column 0` (no leading whitespace).  
There's also the inner method `RenderTable(Table table)` which returns a multiline block (which is also a raw string and therefore its leading whitespace will also be left-trimmed, so no leading whitespace either).  

```cs
FormattableString RenderTable(Table table) => $$"""
            /// <summary>
            /// POCO for {{ table.TableName }}
            /// </summary>
            public class {{ table.TableName }}
            {
                // class members...
            }
            """;
//...
w.WriteLine($$"""
    namespace {{myNamespace}}
    {
        {{ RenderTable(usersTable) }}
        {{ RenderTable(productsTable) }}
    }
    """);
```



**The magic of Implicit Indent Control:**
- `RenderTable(usersTable)` and `RenderTable(productsTable)` are interpolated after 4 spaces, so CodegenTextWriter will **automatically capture** that they start at `column 4` and that the leading whitespace consists of 4 spaces
  (It could be a tab, or any number of spaces/tabs/etc)
- Whenever the inner block (the interpolated expression) spans into multiple lines those lines will all be indented correctly (**we preserve the indentation** of subsequent lines).
- To sum, each line will be padded with 4 spaces. As if the inner expression was "pasted like a rectangle" ("preserving cursor position").  
- Any number of levels work. When `RenderTable()` renders more indentation (e.g. the line that says `// class members...`) the result is that this line will be 8 spaces ahead of the outer block.  

To sum, CodegenTextWriter can understand different object types interpolated in Interpolated strings while **keeping cursor position** of embedded arguments.


Implicit indent control also works for other indented languages that don't use curly-braces (e.g. Python):

```cs
FormattableString HappyMonday() => $"""
    print("Hello!")
    print("It's great to see you again")
    print("Happy Monday!")
    """;

var w = new CodegenTextWriter();
w.WriteLine($$"""
    from datetime import date

    # If today is Monday
    if date.today().weekday() == 0:
        {{ HappyMonday }}
    """);
```
Indented blocks can have any number of spaces - CodegenTextWriter doesn't care - it will just preserve whatever indent-whitespace you use.

## Embedding IEnumerable\<T>

One common task in code generators is to repeat a block (or a template or even an inline expression) for all items in a collection.  

CodegenTextWriter supports the interpolation of `IEnumerable<T>` - it's much easier than explicitly doing a `foreach` and writing each element individually (e.g. `writer.Write(element)`).  
When CodegenTextWriter finds an interpolated `IEnumerable<T>` it will render all items one by one (whatever type they are - they can be string, FormattableString, Func<string>, etc.) and between the items it will add a separator (which by default is a linebreak):

```cs
void RenderGroceryList()
{
    var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };

    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        I have to buy:
        {{ groceries.Render() }}
        """);
    // Result is:
    // I have to buy:
    // Milk
    // Eggs
    // Diet Coke
}
```

We can use LINQ expressions to make it more elaborated:

```cs
void RenderGroceryList()
{
    var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };

    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        I have to buy:
        {{ groceries.Select(g => "- " + g).Render() }}
        """);
    // Result is:
    // I have to buy:
    // - Milk
    // - Eggs
    // - Diet Coke
}
```


```cs
FormattableString RenderTable(Table table) => $$"""
        /// <summary>
        /// POCO for {{ table.TableName }}
        /// </summary>
        public class {{ table.TableName }}
        {
            // class members...
            {{ table.Columns.Select(column => $$"""public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }""" ).Render() }}
        }
        """;
void Generate()
{
    var schema = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));

    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ schema.Tables.Select(t => RenderTable(t)).Render() }}
        }
        """);
}
```

The `Render()` extension allows to customize the line separators and has some presets (e.g. `Render(RenderEnumerableOptions.SingleLineCSV)` will make the items be separated by `", "`):

```cs
void RenderGroceryList()
{
    var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };

    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        I have to buy: {{ groceries.Render(RenderEnumerableOptions.SingleLineCSV) }}
        """);
    // Result is:
    // I have to buy: Milk, Eggs, Diet Coke
}
```



## Control-Flow Symbols


**IF-ENDIF statements**

```cs
using CodegenCS;                 // besides this...
using static CodegenCS.Symbols;  // you also need this

void RenderMyApiClient(bool injectHttpClient)
{
    w.WriteLine($$"""
        public class MyApiClient
        {
            public MyApiClient({{ IF(injectHttpClient) }}HttpClient httpClient{{ ENDIF }})
            { {{ IF(injectHttpClient) }}
                _httpClient = httpClient; {{ENDIF}}
            }
        }
        """);
}
```

If we call `RenderMyApiClient(injectHttpClient: false)` we would get this output:

```cs
public class MyApiClient
{
    public MyApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
}
```

**IF-ELSE-ENDIF**

```cs
w.WriteLine($$"""
    public class MyApiClient
    {
        public void InvokeApi()
        {
            try
            {
                restApi.Invoke();
            }
            catch (Exception ex)
            { {{IF(settings.swallowExceptions) }}
                Log.Error(ex); {{ ELSE }}
                throw; {{ ENDIF }}
            }
        }
    }
    """);
```

**Nested IF statements**
```cs
w.WriteLine($$"""
    {{ IF(generateConstructor) }}public class MyApiClient
    {
        public MyApiClient({{ IF(injectHttpClient) }}HttpClient httpClient{{ ENDIF }})
        { {{IF(injectHttpClient) }} 
            _httpClient = httpClient; {{ ENDIF }}
        }}
    } {{ ENDIF }}
    """);
```

**IIF (Immediate IF):**

```cs
w.WriteLine($$"""
    public class User
    {
        {{ IIF(isVisibilityPublic, $"public ") }}string FirstName { get; set; }
        {{ IIF(isVisibilityPublic, $"public ", $"protected ") }}string FirstName { get; set; }
    }
    """);
```


# Breaking Large Templates into Smaller Blocks

Embedding subtemplates inside outer templates (using string interpolation and implicit Indent) is very elegant and it's the prefered method (unless you need complex control logic, in which case you'd have to [`manually invoke methods`](#ManuallyInvokingMethods)).

## Embedding a `FormattableString`

As shown earlier, embedding a `FormattableString` (either as variable, property or function) inside another `FormattableString` is the simplest way of invoking a subtemplate:


```cs
FormattableString generateClass = $@"
    void MyClass()
    {{
        void Method1() { /* ... */ }
        void Method2() { /* ... */ }
    }}";

void Generate()
{
    var w = new CodegenTextWriter();
    w.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ generateClass }}
        }
        """);
}
```

If for any reason you need lazy evaluation you can also use `Func<FormattableString>`. Other options are `string` or `Func<string>` (but our [FAQ](#FAQ) explains why you should prefer `FormattableString` over plain `string`).

## Embedding Action\<ICodegenTextWriter>

**Embedding (interpolating) an `Action<ICodegenTextWriter>`**

If you need a little more flexibility you can embed an `Action<ICodegenTextWriter>` and ICodegenTextWriter will be automatically passed to the action.  


```cs
Action<ICodegenTextWriter> GenerateClass = w => {
    w.Write($@"
        void MyClass()
        {{
            void Method1() { /* ... */ }
            void Method2() { /* ... */ }
        }}");
};

Action<ICodegenTextWriter> GenerateFile = w => w.WriteLine($$"""
    using System;
    using System.Collections.Generic;
    namespace {{ ns }}
    {
        {{ GenerateClass }}
    }
    """);

var w = new CodegenTextWriter();
GenerateFile(w);
```

If your action can write to the text writer without getting it as an argument (e.g. writer could be a static variable) you can also use `Action`.  
`Action<ICodegenTextWriter>` and `Action` behave like `Func<FormattableString>` in the sense that they will be evaluated "on demand" (only by the moment that we are rendering the full template).

```cs
public static CodegenTextWriter w = new CodegenTextWriter();

Action generateClass = () => w.Write($@"
    void MyClass()
    {{
        void Method1() { /* ... */ }
        void Method2() { /* ... */ }
    }}");

Action generateFile = ()) => w.WriteLine($$"""
    using System;
    using System.Collections.Generic;
    namespace {{ ns }}
    {
        {{ generateClass }}
    }
    """);

generateFile(w);
```


# Template Interfaces

As explained above, CodegenTextWriter can render embedded Actions and Functions, but that can get a little ugly when we need to [`pass parameters`](#Action-ICodegenTextWriter-Args).  
To solve that issue we have some simple **template interfaces** that can be used to make templates easier to use and invoke.  

## ICodegenTemplate\<TModel>

The most common template interface is `ICodegenTemplate<TModel>` - it gets a model (type TModel) and writes output to a ICodegenTextWriter (so it's a "single-file template"):
```cs
interface ICodegenTemplate<TModel>
{
    void Render(ICodegenTextWriter writer, TModel model);
}
```

A simple implementation would be like:
```cs
using CodegenCS;
using CodegenCS.DbSchema;
using System.Linq;

class MyPocoTemplate : ICodegenTemplate<Table>
{
    public void Render(ICodegenTextWriter writer, Table model)
    {
        writer.Write($$"""
            /// <summary>
            /// POCO for {model.TableName}
            /// </summary>
            public class {model.TableName}
            {
                {{ model.Columns.Select(column => $$"""public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }""" ).Render() }}
            }
            """);
    }
}
```

The template above could be used in different ways...

## Programatically Invoking Templates

```cs
using CodegenCS;
using CodegenCS.DbSchema;
using System.Linq;

class MyGenerator
{
    static void Main()
    {
        var schema = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
        var writer = new CodegenTextWriter(); // writing all POCOs in a single file.
        
        // We can load and render the template directly from the writer (no string interpolation)
        foreach (var table in schema.Tables)
            writer.LoadTemplate<MyPocoTemplate>().Render(table);

        File.WriteAllText("MyPocos.cs", writer.GetContents());
    }
}
```

If we want each POCO in a different file we could use ICodegenContext:

```cs
    static void Main()
    {
        var schema = Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
        var context = new CodegenContext();
        
        foreach (var table in schema.Tables)
            context[table.TableName + ".cs"].LoadTemplate<MyPocoTemplate>().Render(table);
            
        context.SaveFiles(outputFolder: ".");
    }
```


## Embedding a Template inside an Interpolated String

```cs
static void Main()
{
    // ...
    // Or we can use string interpolation to load and render directly inside string
    writer.WriteLine($$"""
        // My first POCO:
        {{ Template.Load<MyPocoTemplate>().Render(tables[0]) }}

        // My second POCO:
        {{ Template.Load<MyPocoTemplate>().Render(tables[1]) }}
        """);
    // ...
    // Note that the Render() above is mandatory because it's providing the model to the template
}
```

## Embedding an IEnumerable of Templates (one template is rendered for each element) inside an Interpolated String

```cs
static void Main()
{
    // ...
   
    // Or we can avoid the outer foreach and just directly embed and Render() an IEnumerable:
    writer.WriteLine($$"""
        // All my POCOs, rendered one by one.
        {{ tables.Select(table => Template.Load<MyPocoTemplate>().Render(table)).Render() }}
        """);
    // ...
    // Note that the second Render() above is optional - it allows to specify details like the separator 
    // between the items, but CodegenTextWriter is lenient and will render the items even if you forget the Render()
}
```

## ICodegenMultifileTemplate\<TModel>

This one is similar to `ICodegenTemplate<TModel>` but instead of receiving a `ICodegenTextWriter` (and writing into a single file) it receives a `ICodegenContext` (and therefore can write to multiple files):

```cs
interface ICodegenMultifileTemplate<TModel>
{
    void Render(ICodegenContext context, TModel model);
}
```

Example:


A simple implementation would be like:
```cs
using CodegenCS;
using CodegenCS.DbSchema;
using System.Linq;

class MyPocoTemplate : ICodegenMultifileTemplate<DatabaseSchema>
{
    public void Render(ICodegenContext context, DatabaseSchema schema)
    {
        // Multifile template gets context, and for each table it will
        // load and render another template
        // and will output each table in its own file
        foreach (var table in schema.Tables)
            context[table.TableName + ".cs"].LoadTemplate<MyPocoTemplate>().Render(table);
            
        context.SaveFiles(outputFolder: ".");
    }
}
```


## ICodegenStringTemplate\<TModel>

ICodegenStringTemplate is for templates that just return an interpolated string:
```cs
interface ICodegenStringTemplate<TModel>
{
    FormattableString Render(TModel model);
}
```

Example:
```cs
class MyPocoTemplate3 : ICodegenStringTemplate<DatabaseSchema>
{
    public FormattableString Render(DatabaseSchema schema) => $$"""
        /// Auto-Generated by CodegenCS (https://github.com/Drizin/CodegenCS)
        /// Copyright Rick Drizin (just kidding - this is MIT license - use however you like it!)
            
        namespace MyNamespace
        {
            {{ schema.Tables.Select(t => RenderTable(t)) }}
        }
        """;

    FormattableString RenderTable(Table table) => $$"""
        /// <summary>
        /// POCO for Users
        /// </summary>
        public class {{ table.TableName }}
        {
            {{ table.Columns.Select(c => RenderColumn(table, c)) }}
        }
        """;

    FormattableString RenderColumn(Table table, Column column) => $$"""
        /// <summary>
        /// [dbo].[{{ table.TableName }}][{{ column.ColumnName }}] ({{ column.SqlDataType }})
        /// </summary>
        public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }
        """;
}
```

## Templates Summary

So basically:

- There are interfaces for single-file templates (those that get a ICodegenTextWriter and render to it) or multiple-file templates (those that get a ICodegenContext and may render multiple files)
- There are interfaces for writing the template using imperative programming (using pure C# to write to the received ICodegenTextWriter or ICodegenContext) and simpler interfaces just expect you to return an interpolated string.
- There is an extension `Load<T>()` to Load any template (by the Class Type `T` - we can load any type that implements templating interfaces). `Load<T>()` has Dependency Injection support (it can inject into the template constructor any required dependency)
- After a template is loaded there is an extension `Render(TModel model)` (or `Render()`)  to invoke (render) that template (providing the required input models, if any).
- Templates can be loaded and rendered directly from ICodegenTextWriter or from ICodegenContext using `textWriter.LoadTemplate<T>().Render(TModel model)`
- Templates can be loaded and rendered directly from interpolated strings (`{{ Template.Load<T>().Render(TModel model) }}`)
- Everything will be strongly typed, with intellisense/autocomplete and type-checking (e.g. `Render()` will expect a type depending on the template that was loaded)
- We can embed subtemplates inside other templates, and they can receive/pass models, meaning complex templates can be well organized (instead of a single huge/ugly template)
- Templates can rely on some **Ready to Use Input Models** like [CodegenCS.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.DbSchema) - this model represents the database schema of a MSSQL database or a PostgreSQL database, and can be used by templates that generate POCOs or even complete data access layers. [dotnet-codegencs dbschema extract](https://github.com/Drizin/CodegenCS/tree/master/src/dotnet-codegencs) is a tool that extract the schema of those databases into a JSON file.
- Another input model (under development) is [CodegenCS.Models.OpenAPI](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS.Models.OpenAPI) - this model represents an OpenAPI (Swagger) specification, and can be used by templates that generate REST API clients or servers.
- You can use any other structured data source (that can be read using C#) as an input model (so you can read from JSON, YAML, XML, schema of other database vendors, etc)




















# Misc Features

## DotNetCodegenContext

**Using DotNetCodegenContext to add the generated files to a .NET Framework project (csproj in the old non-SDK format)**

The new csproj format will by default compile all *.cs files under the csproj folder (so it's just about defining where you want the output files to be generated), 
but if you're using .NET Full Framework in the old csproj format you may benefit from automatically adding all outputs to the csproj:

```cs
// DotNetCodegenContext is just a specialized version of CodegenContext
var ctx = new DotNetCodegenContext();

var f1 = ctx["File1.cs"]; 
f1.WriteLine("Line1");

ctx.SaveFiles(outputFolder);

// by default DotNetCodegenContext will set .cs/.vb files to be of type "Compile", 
// but we could also specify files to be added as Content or EmbeddedResource
ctx.AddToProject(csProj, outputFolder);
```
<!--
Do we have helpers for nesting files (in Visual Studio)?
- Adding generated files to old csproj (non-SDK style), with the option to nest the generated files under a single file
- Adding generated files to new csproj (SDK style) nested under a single file
-->





See full documentation [here](https://github.com/Drizin/CodegenCS/tree/master/src/CodegenCS/).