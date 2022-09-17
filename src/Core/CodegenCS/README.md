**CodegenCS is a Toolkit for doing Code Generation using plain C#**.

Before anything else, don't forget to read the [Main Project Page](https://github.com/CodegenCS/CodegenCS/) to learn the basics (basic idea, basic features, and major components).

This page is only about the **CodegenCS Core Library**:
- If you are **writing a template** (code generator) and want to learn more about CodegenCS features (and internals) this is the right place.
- If you want to **compile and run templates** or **reverse-engineer a database schema** check out the [`Command-line Tool dotnet-codegencs`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) documentation
- If you want to **browse the sample templates** (POCO Generators, DAL generators, etc) check out [https://github.com/CodegenCS/Templates/](https://github.com/CodegenCS/Templates/)
<!-- - If you just want to **download the Visual Studio Extension** check out... (Pending)   -->


# CodegenCS Core Library

[![Nuget](https://img.shields.io/nuget/v/CodegenCS?label=CodegenCS)](https://www.nuget.org/packages/CodegenCS)
[![Downloads](https://img.shields.io/nuget/dt/CodegenCS.svg)](https://www.nuget.org/packages/CodegenCS)

CodegenCS Core Library is a .NET class library for doing code generation using plain C#.  
It's the backbone of the command-line tool [dotnet-codegencs](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs), but you can also use the library without the command-line tool.



Basically this library provides a **"Magic TextWriter"** that helps with common code generation tasks and challenges:
- Supports string interpolation of `Action<>` and `Func<>` delegates - complex templates can be broken into smaller parts
- Supports string interpolation of `IEnumerable<>` lists - items are rendered one by one (both simple items or using powerful delegate callbacks)
- Preserves indentation (when we write new lines it will automatically indent the line according to current level) - indent can be controlled explicitly or implicitly
- Implicit control of indentation means we can embed complex templates inside other templates and their indentation is automatically "captured" by the position where they are embedded
- Helpers to write multi-line blocks without having to worry about different indentations for control logic and output code
- Supports interpolation of "special symbols" that can be used to write simple conditional-blocks without having to "leave" the string literal and use external control logic.
- Supports interpolation of "special instructions" to describe how `IEnumerable` items should be rendered (separated by linebreaks or commas, etc).  

Besides the TextWriter the library also provides a "context" class to manage multiple in-memory text writers, and save them into multiple files (other templating engines have poor support for multiple files).  


<br />
<br />

# Basics

## CodegenTextWriter / ICodegenTextWriter

**`CodegenTextWriter`** is the heart of CodegenCS Library. Basically it's a custom TextWriter (we prefer to say it's a **Magic TextWriter** or a **TextWriter on Steroids**) created to solve common code generation issues (indent control, linebreaks control, mixed-indentation issue):

- Like any regular TextWriter we can manually write to it using plain C#: we can `Write()`, `WriteLine()`, write strings or interpolated strings, write multiline strings, etc
- It's strongly focused on **interpolated strings**, and supports a large number of object types that can just be interpolated (embedded) within strings (this reduces the need for manual writes)
- It has **Indentation Control** - it keeps track of the current Indent level, and when we start writing any new lines they will be indented (padded) according to the current level
- Indentation can be explicitly controlled (we can increase indent, decrease indent, set indentation to be any number of spaces or tabs), and there are some helpers for easily writing **indented blocks** (you create a new scope, and everything you write inside that scope is automatically indented)
- Indentation can be **implicitly controlled** (much easier!) when you use interpolated strings: before the writer renders any interpolated object it will "save the cursor position" (capture the current indentation level even if it was **implicitly** defined by spaces or tabs added before the interpolated variable), and then if the interpolated objects spans into multiple lines those lines will all be indented correctly (**we preserve the indentation** of subsequent lines).  
(In other words you can just embed a class nested under a namespace and it will be indented like magic, you can just embed a method under a class and it will be indented like magic, etc)
- Will automatically adjust multiline blocks (by removing left-padding and removing the first empty line) so that we can just indent-align our multiline blocks wherever they look better - no need to manually worry about whitespace or indenting (since indenting is controlled by the writer context). 
- This same magic (that we've been doing for years) now can also be done by using the new C# 11 [**"Raw String Literals"**](https://github.com/dotnet/csharplang/blob/main/proposals/raw-string-literal.md) - with the advantage that raw string literals let us render regular single-curly-braces without escaping (much easier to write C#/Java/C blocks) and for interpolated expressions we can use double-mustaches
- Supports interpolation of IEnumerables (items are rendered one by one, and between the items we can have separators like line-breaks or any other - it's all configurable using the `.Render()` extension over `IEnumerable<T>`)
- Supports Control Symbols like **IF-ENDIF / IF-ELSE-ENDIF / IIF (Immediate IF)** - it's a concise syntax for conditional blocks (no need to "leave" the text just to add simple logic)
- Supports interpolation of `Action<>`, and `Func<>` delegates

**Writing a line and saving to file**

```cs
var w = new CodegenTextWriter();
w.WriteLine("Line1");
w.SaveToFile("File1.cs");
```

**Writing multiline-block (with string interpolation) and saving to file**

```cs
w.WriteLine($$"""
    public void {{methodName}}()
    {
        Console.WriteLine("Hello World");
    }
    """);
w.SaveToFile("File1.cs");
```

This example above (and most multiline examples in this document) use [**"Raw String Literals"**](https://github.com/dotnet/csharplang/blob/main/proposals/raw-string-literal.md), which is fully supported by CodegenCS and is strongly recommended.  
If you're not familiar with that feature you will learn about it later in this document, but for now all you need to know about Raw String Literals is that:
- Since the string is delimited by 3 double-quotes then it means that we can use 1 or 2 consecutive double-quotes inside the string without having to escape it. Much easier than before.
- Since the first line is an empty line and the last empty is also an empty line, they are ignored - tthe compiler won't add those empty lines to the final string - only the lines inbetween are used.
- Since the ending line is padded by 4 spaces then the whole block (all lines) will be "left-trimmed" by 4 spaces (all lines will have the 4 initial spaces removed).
  This means that we can align our multiline blocks wherever they look better, and leading whitespace is just ignored.
- Since the string starts with two dollar-signs then it's an interpolated string and the interpolated objects should be surrounded by double mustaches (double curly braces).
  This basically means that we can render simple curly-braces without having to escape them. (If you had to write double-mustaches like in a JSX generator you'd want to start the string with 3 dollar-signs)

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

<br/>
<br/>

# Implicit Indent Control

Let's say that your template is complex enough and you want to break it down into smaller methods like this: 

```cs
FormattableString myMethod = $$"""
    public void {{methodName}}()
    {
        Console.WriteLine("Hello World");
    }
    """;

FormattableString myClass = $$"""
    public class {{className}}()
    {
        {{ myMethod }}
    }
    """;

w.WriteLine($$"""
    namespace {{myNamespace}}
    {
        {{ myClass }}
    }
    """);

// PS: Please note that all strings above are using Raw String Literals,
// which mean that the left padding of all blocks above will be left-trimmed.
```

In the example above you are probably expecting to achieve correct indentation (class nested under namespace, methods nested under class), like this:

```cs
namespace MyNamespace
{
    public class MyClass
    {
        public void MyMethod
        {
            Console.WriteLine("Hello World");
        }
    }
}
```

However, **if you were using a regular C# TextWriter** the first line of each inner block would be padded according to the outer block (it would "start after 4 spaces") but all subsequent lines would "go back to column 0".  
This is what you would get:

```cs
namespace MyNamespace
{
    public class MyClass
{
    public void MyMethod
{
    Console.WriteLine("Hello World");
}
}
}
```

In most other templating engines to get the right results you would need a lot of trial-and-error with whitespacing, and in the end of the day each inner block would need to be explicitly adjusted according to the level of indentation of the parent block - which is ugly and counterintuitive.

## CodegenTextWriter Implicit Indent Control

CodegenTextWriter magically controls the indentation of any object embed inside interpolated strings.  

All you have to do is add the right amount of whitespace before the interpolated object (which is intuitive, friendly, and easier to read). 

The same previous examples that wouldn't work with a regular TextWriter **would work like magic if you were using a CodegenTextWriter**:

```cs
// Note that myMethod is padded by 4 spaces
// when compared to parent block 
FormattableString myClass = $$"""
    public class {{className}}()
    {
        {{ myMethod }}
    }
    """;

// Note that myClass is padded by 4 spaces
// when compared to the parent block
w.WriteLine($$"""
    namespace {{myNamespace}}
    {
        {{ myClass }}
    }
    """);

// That's all you need. CodegenTextWriter will handle the rest!
```

To sum, CodegenCS gives you "hassle-free" indentation, and you always get the expected indentation.


## The magic will be revealed

This is how Implicit Indent Control works internally:
- The variable `myClass` is interpolated in the parent block and it's padded by 4 spaces (when compared to the parent block)
- The variable `myMethod` is interpolated in the parent block and it's padded by 4 spaces (when compared to the parent block)
- For each interpolated object CodegenTextWriter will create a **child scope**, and it will **automatically capture** the whitespace before the object.  
  In our example the whitespace was 4 spaces, but [it could be tabs](https://www.youtube.com/watch?v=SsoOG6ZeyUI), or any number of spaces/tabs - it's just captured as is.
- CodegenTextWriter will "intercept" whatever is written under a given child scope (even for multiple nested scope levels), and will ensure that subsequent lines (under that scope) will also padded by the same whitespace as the first line.  
  In other words, whenever the interpolated expression spans into multiple lines (text block) all those lines will be indented correctly (writer **preserves the indentation** of the whole block).
- One way of visualizing this is as if inner blocks are "pasted like a rectangle" ("preserving cursor position"), what some text editors would call "column mode".  
- Any number of levels work. `myMethod` lines will all be padded by 8 spaces (4 defined in the parent `myClass`, and 4 defined in the grandparent `myNamespace` block).

To sum, CodegenTextWriter will **preserve indent / keep cursor position** for any objects that are embedded in interpolated strings (even for complex objects like Actions/Funcs delegates, explained later in this document).


PS: Implicitly controlling indentation using string interpolation is very elegant, works like magic and it's the preferred method. But if for any reason you think you need more control you might prefer to use [Explicit Indent Control](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS/Deprecated.md#ExplicitIndent) (e.g. manually calling `IncreaseIndent()`, `DecreaseIndent()`, etc.)


## Works for any Indented Language (e.g. Python)

Implicit indent control is useful for **any output that needs indenting, it's not only for curly-braces languages**.  
Python is a good example (uses indenting but no curly-braces):

```cs
FormattableString happyMonday = $"""
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
Thanks to our Magic TextWriter you get this:

```python
from datetime import date

# If today is Monday
if date.today().weekday() == 0:
    print("Hello!")
    print("It's great to see you again")
    print("Happy Monday!")
```

... instead of this (which in Python is a serious difference):

```python
from datetime import date

# If today is Monday
if date.today().weekday() == 0:
    print("Hello!")
print("It's great to see you again")
print("Happy Monday!")
```

## Use FormattableString instead of strings

When you write an Interpolated String the C# compiler internally creates the `FormattableString` class, which wraps the message format and the array of embedded objects:

```cs
string firstName = "Rick"; string lastName = "Drizin";
FormattableString msg = $"Hello {firstName} {lastName}";
// This is equivalent to:
FormattableString msg = FormattableStringFactory.Create("Hello {0} {1}", new object[] { firstName, lastName });
```

The magic of implicit indent control (explained earlier) only works because when CodegenTextWriter gets an interpolated string it will prioritize an overload that gets this `FormattableString` type (instead of getting an implicitally-converted string).  
Without this trick it wouldn't be able to do its magic - it wouldn't be able to capture the whitespace that comes before each embedded object (wouldn't even be able to tell apart the outer block from the inner blocks).

The tricky part is that when you're breaking down your template into smaller reusable methods (more on that below) you might be inadvertently converting interpolated strings to strings, breaking our implicit indent control magic.  
In other words, any interpolated string which is not explicitly told to "stay as FormattableString" might be unintentionally converted into a string and then any nested indented-blocks would have their indentation broken.

If it all sounds complex, don't worry - there's a simple rule to avoid any issues - **whenever you write an interpolated string make sure that it's not implicitally converted to `string`**:
- When an interpolated string is assigned to a variable, make sure that you declare the variable as `FormattableString` (instead of `var` or `string`)
- When an interpolated string is returned by a lambda function, make sure that you cast the return to `FormattableString`
- When an interpolated string is returned by a method, make sure that the return type of the method is `FormattableString` (instead of `string`)

Two examples to emphasize it:

```cs
// If the method return was "string" it would break the indentation
FormattableString myClass = $$"""
    public class {{className}}()
    {
        {{ myMethod }}
    }
    """;

// one of the the many object types that you can embed within interpolated strings
// is the `FormattableString` itself (an interpolated string inside another interpolated string)
w.WriteLine($$"""
    namespace {{myNamespace}}
    {
        {{ myClass }}
    }
    """);
```


<br/>
<br/>

# Embedding delegates: Actions and Funcs

Complex templates can be better organized by breaking it down into smaller/reusable methods.

String interpolation doesn't allow the interpolation of methods, but it allows the interpolation of delegates (Actions and Funcs).  

Embedding Actions/Funcs delegates inside interpolated strings is very elegant, can benefit from Implicit Indent Control, and therefore it's the prefered way of organizing templates. However if for any reason you don't like the idea of embedding delegates, or if you think you need more control, you might prefer to [`manually invoke methods`](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS/Deprecated.md#ManuallyInvokingMethods)).

## Embedding Action

```cs
public ICodegenTextWriter writer;

Action myActionDelegate = () => writer.Write($$"""
    public class MyClassName()
    {
        // ...
    }
    """);

void MyGenerator()
{
    writer = new CodegenTextWriter();
    writer.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ myActionDelegate }}
        }
        """);
}
```

## Embedding Func\<FormattableString>

```cs
Func<FormattableString> myFuncDelegate = () => $$"""
    public class MyClassName()
    {
        // ...
    }
    """;

void MyGenerator()
{
    var writer = new CodegenTextWriter();
    writer.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ myFuncDelegate }}
        }
        """);
}
```

## Auto injecting Types

CodegenTextWriter can automatically inject some types into `Action<..>` and `Func<.., FormattableString>` delegates.  Supported types include ICodegenContext, ICodegenTextWriter, and others.  

In the example below we have a CodegenTextWriter writing an interpolated string, and the interpolated string contains an `Action<ICodegenContext>` embedded. CodegenTextWriter will automatically "inject" itself into that Action, according to the required type: 

```cs
// This delegate expects to receive ICodegenTextWriter
Action<ICodegenTextWriter> myActionDelegate = (w) => w.Write($$"""
    public class MyClassName()
    {
        // ...
    }
    """);

// When myActionDelegate is invoked the current ICodegenTextWriter will be
// automatically passed to the delegate.
void MyGenerator()
{
    // writer doesn't need to be global variable anymore
    // it's just passed around using auto injection
    var writer = new CodegenTextWriter();
    writer.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ myActionDelegate }}
        }
        """);
}
```

## Passing other arguments to delegates

If your delegate expect other parameters (other than the standard types that can be automatically injected), you have two options:
- You can use the `WithArguments()` extension which can provide some arguments to any delegate (for any type that you want injected you can just pass null)
- You can convert between delegate types using lambdas.

Let's say you have an `Action<Table>` (a delegate that expects to receive a table object), you can convert that (wrapping) into an `Action` delegate:

```cs
DatabaseSchema _schema;
CodegenTextWriter _writer;

// This delegate can't be interpolated directly because Table type can't be injected automatically
Action<Table> GenerateColumns = (table) =>
{
    foreach(var column in table.Columns)
    {
        _writer.WriteLine($$"""
            public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }
            """);
    }
};

// ..so we create an Action that invokes our Action<Table>
Action GenerateTables = () =>
{
    foreach(var table in _schema.Tables)
    {
        _writer_.WriteLine($$"""
            /// <summary>
            /// POCO for {{ table.TableName }}
            /// </summary>
            public class {{ table.TableName }}
            {
                // THIS! Delegate will be wrapped together with the arguments.
                {{ GenerateColumns.WithArguments(table) }}
            }
            """);

        /* OR if you want to convert Action<Table> into Action:
        _writer_.WriteLine($$"""
            /// <summary>
            /// POCO for {{ table.TableName }}
            /// </summary>
            public class {{ table.TableName }}
            {
                // THIS! This is a new lambda Action that invokes the Action<Table>
                {{ () => GenerateColumns(table) }}
            }
            """);
        */
    }
};

void Generate()
{
    // Requires Newtonsoft.Json
    var _schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));

    _writer = new CodegenTextWriter();
    _writer_.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ GenerateTables }}
        }
        """);
}
```

PS: The examples above (and other examples later) use the [DatabaseSchema Model](https://github.com/CodegenCS/CodegenCS/tree/master/src/Models/CodegenCS.DbSchema), which is one of the out-of-the-box models that our toolkit provides. Basically it represents the schema of a relational database. You can use any model with our library (and with our command-line tool) but for simplicity many examples in the documentation use this specific model. For more information about models please check out the [Main Project Page](https://github.com/CodegenCS/CodegenCS/).

<br />
<br />


# Embedding IEnumerable\<T>

In the previous example we had to **programmatically run loops**: `GenerateTables` does a `foreach` to go through a list of tables, and then `GenerateColumns` does another `foreach` to go through a list of columns.

Iterating through a list is a very common task in code generators, and many templating engines (like Handlebars, Dotliquid, Mustache) have their own syntaxes for doing iteration "inline" (directly inside the text blocks). T4 does not support it (you need to do it programatically).

CodegenTextWriter supports the interpolation of `IEnumerable<T>` (for a lot of `T` types) directly from interpolated strings, which means that you don't have "leave" the text block and programatically run a `foreach` loop when you need to simple iterate through a list of items. It's just an easier alternative instead of explicitly doing a `foreach` and writing each element individually.  

When CodegenTextWriter gets an interpolated `IEnumerable<T>` it will render all items one by one and it will add a separator between the items (by default the separator is a linebreak, but there are many options).

```cs
var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };
var w = new CodegenTextWriter();

w.WriteLine($$"""
    I have to buy:
    {{ groceries.Render() }}
    """);

w.SaveToFile("File1.cs");
// Resulting file is:

// I have to buy:
// Milk
// Eggs
// Diet Coke
```

<!-- For this simple example it would be equivalent of embedding `string.Join(Environment.NewLine, groceries)`, but as you already know using string will break the auto indentation of nested blocks. -->

The `T` can be virtually any type - like `FormattableString`, `Func<FormattableString>`, `string`, `Func<string>`, `Action`, `Action<ICodegenTextWriter>`, etc.

The `Render()` extension is not mandatory (if you just embed the `IEnumerable<T>` list it will also work), and in the example above it's not even making any difference - but we leave it in the example because this `Render()` extension allows customizing the items separator (e.g. using commas instead of linebreaks), as you'll see below.


## `IEnumerable<Func<FormattableString>>`

If we want to manipulate the items before they are rendered (e.g. formatting) we can transform your items using LINQ expressions.  
In the example below convert from `IEnumerable<string>` into `IEnumerable<FormattableString>`:

```cs
var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };
var w = new CodegenTextWriter();

w.WriteLine($$"""
    I have to buy:
    {{ groceries.Select(g => (FormattableString) $"- {g}").Render() }}
    """);

w.SaveToFile("File1.cs");
// Resulting file is:

// I have to buy:
// - Milk
// - Eggs
// - Diet Coke
```

As you already know, to avoid indenting problems you should always prefer `FormattableString` over `string`, but the simple example above (where there is a single level of nested block) would also work with regular strings like this:

```cs 
w.WriteLine($$"""
    I have to buy:
    {{ groceries.Select(g => "- " + g") }}
    """);
```

## Real-world example

Let's use `IEnumerable<FormattableString>` in a more concrete example:


```cs
FormattableString RenderTable(Table table) => $$"""
    /// <summary>
    /// POCO for {{ table.TableName }}
    /// </summary>
    public class {{ table.TableName }}
    {
        // class members...
        {{ table.Columns.Select(column => (FormattableString) $$"""public {{ column.ClrType }} {{ column.ColumnName }} { get; set; }""" ).Render() }}
    }
    """;

void Generate()
{
    var schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
    var w = new CodegenTextWriter();

    w.WriteLine($$"""
        namespace {{myNamespace}}
        {
            {{ schema.Tables.Select(t => RenderTable(t)).Render() }}
        }
        """);
}
```

PS: In the example above there is also a single level of nested block, so removing the `(FormattableString)` cast wouldn't make a difference.

## IEnumerables with Delegates

The embedded `IEnumerable<T>` can have any type of T, including delegates (Actions/Funcs, even with injected parameters).  
If you need to pass arguments to the delegate (even if you want to mix with injected parameters) you can use the `WithArguments()` delegates extension. Like this:


```cs
// return type is IEnumerable<FormattableString>, but for simplicity let's define as object.
Func<Table, object> GenerateColumns = (table) =>
    table.Columns.Select(column => (FormattableString)$$"""
        public {{column.ClrType}} {{column.ColumnName}} { get; set; }
        """);


// This one requires 2 arguments, and the first one will be automatically injected (since arg provided is null)
Func<ICodegenTextWriter, DatabaseSchema, object> GenerateTables = (ICodegenTextWriter w, DatabaseSchema schema) =>
    schema.Tables.Select(table => (FormattableString)$$"""
        /// <summary>
        /// POCO for {{table.TableName}}
        /// </summary>
        public class {{table.TableName}}
        {
            {{ GenerateColumns.WithArguments(table) }}
        }
        """)
        // so far we have IEnumerable<FormattableString>, now we can provide instructions
        // on how each item should be separated from the next one:
        .Render(new RenderEnumerableOptions() { BetweenItemsBehavior = ItemsSeparatorBehavior.WriteLineBreak });

void Generate()
{
    var schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
    var w = new CodegenTextWriter();

    w.Write($$"""
        namespace {{myNamespace}}
        {
            {{ GenerateTables.WithArguments(null, MyDbSchema) }}
        }
        """);
}
```

In this example above we used `Func` but it would also work with `Action`.


## Different separators

`.Render()` extension (after an `IEnumerable<T>`) allows a very detailed customization of line separators (more on that below). But it contains some presets like `RenderEnumerableOptions.SingleLineCSV` and `RenderEnumerableOptions.MultiLineCSV`.

`RenderEnumerableOptions.SingleLineCSV` will make the items be separated by commas (`", "`):

```cs
var groceries = new string[] { "Milk", "Eggs", "Diet Coke" };
var w = new CodegenTextWriter();

w.WriteLine($$"""
    I have to buy: {{ groceries.Render(RenderEnumerableOptions.SingleLineCSV) }}
    """);

// Result is:
// I have to buy: Milk, Eggs, Diet Coke
```

`RenderEnumerableOptions.MultiLineCSV` will make the items be separated by comma-and-linebreak (`",\n"`):

```cs
string[] cols = new string[] { "AddressLine1", "AddressLine2", "City" };

_w.Write($$"""
INSERT INTO [Person].[Address]
(
    {{cols.Select(col => "[" + col + "]").Render(RenderEnumerableOptions.MultiLineCSV)}}
)
VALUES
(
    {{cols.Select(col => "@" + col).Render(RenderEnumerableOptions.MultiLineCSV)}}
)
""");

// If you're wondering why can't you just add a comma after the lambda expression
// that's because you would get a dangling comma (a comma after the last item)
// and that would be invalid SQL statement.
```

<!-- ## Customizing separators

There are extension-methods that can "enhance" `IEnumerable<>` lists with "special instructions" to describe how items should be rendered (separated by linebreaks or commas, etc).  
Those special instructions can be created using method-extensions in a very friendly way. -->



<br/>
<br/>

# <a name="TemplateInterfaces"></a>Template Interfaces

We have some simple **template interfaces** that you can implement in your classes for 2 purposes:
- [`Command-line Tool dotnet-codegencs`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) requires that your entrypoint implements one of these interfaces.  
- If you need to pass parameters then Template interfaces can be easier to invoke than delegates

The 3 most common template interfaces are:
- `ICodegenTemplate<TModel>`: This is the most common - it gets a model (type TModel) and writes output to a ICodegenTextWriter (so it's a **"single-file template"**):
- `ICodegenMultifileTemplate<TModel>`: This is similar to the previous but instead of getting a ICodegenTextWriter (and writing into a single file) it gets a ICodegenContext (and therefore can **write to multiple files**)
- `ICodegenStringTemplate<TModel>`: for templates that just return an interpolated string (saving a few lines when compared to `ICodegenTemplate<TModel>`)


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

## ICodegenMultifileTemplate\<TModel>

This one is similar to `ICodegenTemplate<TModel>` but instead of getting a `ICodegenTextWriter` (and writing into a single file) it gets a `ICodegenContext` (and therefore can write to multiple files):

```cs
interface ICodegenMultifileTemplate<TModel>
{
    void Render(ICodegenContext context, TModel model);
}
```

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

A simple implementation would be like:

```cs
class MyPocoTemplate3 : ICodegenStringTemplate<DatabaseSchema>
{
    public FormattableString Render(DatabaseSchema schema) => $$"""
        /// Auto-Generated by CodegenCS (https://github.com/CodegenCS/CodegenCS)
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

Isn't it cool that we can write a fully functional template without a single explicit `.Write()` statement? It's fully based on interpolated strings and lambdas.






<br />
<br />

# Misc Features


## Control-Flow Symbols


It's possible to interpolate special symbols like `IF/ELSE/ENDIF` or `IIF` to add simple control blocks mixed within your text blocks. No need to "leave" the text block and go back to C# programming for simple stuff.

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

## Fluent API

Most public methods from CodegenTextWriter (like `Write()`, `WriteLine()`, `IncreaseIndent()`, etc) return the object itself (`ICodegenTextWriter`), which means that it's possible to write line-by-line (or block-by-block) using a chained methods (Fluent API):

```cs
var w = new CodegenTextWriter();
w
  .WriteLine($"public void {myMethodName}()")
  .WriteLine("{")
  .WriteLine("    // My method...")
  .WriteLine("}");
```



## <a name="DotNetCodegenContext"></a>DotNetCodegenContext

**Using DotNetCodegenContext to add the generated files to a .NET Framework project (csproj in the old non-SDK format)**

The new csproj format will by default compile all *.cs files under the csproj folder (so it's just about defining where you want the output files to be generated), 
but if you're using .NET Full Framework in the old csproj format you may benefit from automatically adding all outputs to the csproj (or nesting outputs under a parent file):

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



<br />
<br />

# More about **Raw String Literals**

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
  
## How Raw Strings make Multiline Blocks much easier

If the raw string is a **multiline block starting and ending with empty lines** (whitespace allowed) then this multiline block is processed with some cool behavior:
- First line and last lines are removed (as well as the respective linebreaks). By having the "real" block surrounded by empty lines it means that the first "real line" will **always** be aligned with the subsequent lines.
- The **whole block can be indented** (left-padded) with any amount of whitespace. The whitespace preceding the last line should exist in all previous lines and will be removed (in other words the number of spaces or tabs in the last line defines how many spaces or tabs will be removed from the whole block). This means that the multi-line blocks can be indented wherever it fits better - and this avoids having **mixed indentation** (different indentations between literals and control logic, which makes some templating engines like T4 hard to read).

This nice behavior (which our library have been doing for years now, longer before raw string literals were created) fits like a charm with our indent control because multiline blocks never have to be manually indented - they can always be "left-trimmed" and they will just respect the "current indentation level", providing easier maintenance.

## How Raw Strings make String Interpolation easier

**If the raw string starts with 2 dollar signs** (instead of 1) it means that **interpolated expressions** should be surrounded by 2 curly braces instead of 1. This is cool because:
- 2 curly braces (also known as **double mustaches**) is a standard in many other templating engines (handlebars, mustache, etc)
- Since the symbol for interpolating expressions is 2 curly braces we can write single curly-braces as-is (no escaping required) - so if you're generating code for a language that uses a lot of curly-braces (C#, Java, etc) it's much easier.
  PS: If you're generating code that uses a lot of double-mustaches (e.g. generating handlebars templates) you can just use 3 dollar signs, which means that your interpolated expressions would use triple-mustaches instead of double. Isn't that cool?

## Raw String minimum requirements

To use raw string you need Visual Studio 2012 17.2 (or newer) and you need to enable C# 11 features preview (adding `<LangVersion>preview</LangVersion>` to the csproj file).  

CodegenCS has been historically doing something very similar (stripping left-padding and first empty line from multiline blocks) but since C# 11 we believe that raw string literals provides a better syntax (specially because of the double mustaches and being able to render curly-braces easily).  
If you can't use C# 11 you can still use CodegenCS with the old multiline blocks behavior.




<br />
<br />

# Deprecated (or Not Recommended) Features 
For some old features (or alternative syntaxes) which still work but are not recommended (because now we offer better ways of doing it), please refer to [CodegenCS Deprecated Syntax](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS/Deprecated.md). 









<br />
<br />


# <a name="FAQ"></a>FAQ

## How to use CodegenCS Core Library in my project?

If you want to write and run your own templates you probably just need [`dotnet-codegencs`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs).  
Advanced users might prefer to use CodegenCS Core Library in their own project:

<!-- 
The [`dotnet-codegencs template build`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) and [`dotnet-codegencs template run`](https://github.com/CodegenCS/CodegenCS/tree/master/src/dotnet-codegencs) tools automatically add CodegenCS nuget package and reference, so in order to write templates / compile templates / run templates you normally don't need this.  
But for a better development/debugging experience (IDE with intellisense) you should:
-->
- Create a C# Console project
- Install the [NuGet package CodegenCS](https://www.nuget.org/packages/CodegenCS)
- Import namespace: `using CodegenCS`
- Start using like examples below (or check out more examples in [unit tests](https://github.com/CodegenCS/CodegenCS/tree/master/src/Core/CodegenCS.Tests)).



## Why write templates in C# instead of T4, Mustache, Razor Engine, etc?

Templating Engines are usually good for end-users to write their templates (like Email templates), due to their sandboxed model, but what's better for a developer than a full-featured language?

By using a full-featured language (C#) and a full featured IDE (Visual Studio or Visual Studio Code) we can write complex and reusable scripts, with strong-typing, intellisense, debugging support, etc.  
We can use Dapper, Newtonsoft, and other amazing libraries.  
Generating code with C# is much easier (and less ugly) than using T4 templates - easier to read, easier to write, easier to debug, easier to reuse.  

In this [blog post](https://rdrizin.com/yet-another-code-generator/) I've explained why I've created this library, why T4 templates are difficult to use, and how I tried many other tools before deciding to write my own.

## Why should I use Raw String Literals? How does it compare to other approaches?

**Raw String Literals:**

Note how double-mustaches are used for interpolated expressions.

```cs
var w = new CodegenTextWriter();
w.WriteLine($$"""
    public void {{methodName}}()
    {
        // My method...
    }
    """);
```

**Regular String Interpolation using our CodegenTextWriter:**  

Note that CodegenTextWriter adjusts multi-line blocks very similar to raw strings (it removes left-padding and the first empty line).  
Note how C# curly braces have to be escaped, since single-mustaches are used for interpolated expressions.

```cs
var w = new CodegenTextWriter();
w.WriteLine($@"
    public void {methodName}()
    {{
        // My method...
    }}");
```

**Regular String Interpolation using a regular .NET TextWriter:**  

Note how the first line is not aligned with the subsequent lines, and how the whole block can't have leading whitespace.

```cs
var textWriter = new StringWriter();
string methodName = "MyMethod";
textWriter.WriteLine($@"public void {methodName}()
{{
    // My method...
}}");
```

<!-- ## Why can we embed Actions and Funcs but not void methods?
Interpolated expressions must be an object, and voids are not objects. -->


<!-- ## How does `Template.Load<T>` work?  -->



# License
MIT License
