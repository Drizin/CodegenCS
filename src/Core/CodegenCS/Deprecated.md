# Deprecated (or Not Recommended) Features 

The features below still work but are not recommended anymore. For recommended features/syntax please refer to [CodegenCS Core Library](https://github.com/Drizin/CodegenCS/tree/master/src/Core/CodegenCS).

## Writing to ICodegenTextWriter WITHOUT Raw String Literals (Not Recommended)

The prefered method of writing interpolated strings or multiline blocks is using Raw String Literals, but if you can't use it you can still use regular string interpolation and verbatim string literals:

**Writing interpolated strings** (line by line) using `$`:

```cs
var w = new CodegenTextWriter();
w.WriteLine($"public void {myMethodName}()");
w.WriteLine("{");
w.WriteLine("    // My method...");
w.WriteLine("}");
```

**Writing a multiline block with interpolated strings** (using) `$@`):


```cs
var w = new CodegenTextWriter();
w.WriteLine($@"
    public void {myMethodName}()
    {{
        // My method...
    }}");
```

CodegenTextWriter will automatically adjust multi-line blocks very similar to what raw strings do:
- Multiline blocks will automatically ignore (strip) an empty line at the beginning.
- Multiline blocks will automatically strip left-padding whitespace (they will "dock the whole block to the left", limited by the longest line).


## <a name="ExplicitIndent"></a>Explicit Indent Level Control 
(Not recommended - prefer implicit indent control)

With the Fluent API (method chaining) we can manually control the indentation:

```cs
void RenderTable(ICodegenTextWriter w, Table table)
{
    w.WriteLine($"public class {table.TableName}").WriteLine("{").IncreaseIndent();
    foreach (var column in table.Columns.OrderBy(c => c.ColumnName))
        w.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(column)} {propertyName} {{ get; set; }}");
    w.DecreaseIndent().WriteLine("}"); // end of class
}
void Generate()
{
    var w = new CodegenTextWriter();
    w
      .WriteLine("// Testing FluentAPI")
      .WriteLine($"namespace {myNamespace}").WriteLine("{").IncreaseIndent();

    foreach (var table in schema.Tables.OrderBy(t => t.TableName))
        RenderTable(w, table);

    w.DecreaseIndent().WriteLine("}"); // end of namespace

    w.SaveToFile("File1.cs");
}
```

All the lines written between `IncreaseIndent()` and `DecreaseIndent()` will be indented one level more than the parent.  
By default each indent level will be 4 spaces, which means that `public class ...` will be indented with 4 spaces, and `public {type} {propertyName} { get; set; }` will be indented with 8 spaces.


## Explicit Indented Blocks using "With" IDisposable Helpers 
(Not recommended - prefer using implicit indent control)

In the previous example the curly braces and indentation were manually controlled. But CodegenTextWriter has some `With*()` helpers that can automatically handle that for us:  
We just define what should be written before the block starts and the helper will automatically write the curly braces, the line break, will increase indentation, and when the block ends (IDisposable is disposed) it adds linebreak (if missing), decreases indentation, and write the closing curly braces. The result is much cleaner than previous example:

```cs
var w = new CodegenTextWriter();
using (w.WithCBlock("namespace MyNamespace"))
{
    using (w.WithCBlock("public class MyClass"))
    {
        w.WriteLine(@"
            /// <summary>
            /// MyMethod does some cool stuff
            /// </summary>");
        using (w.WithCBlock("void MyMethod()"))
        {
            w.WriteLine(@"
                // Method body...
                // Method body...");
        });
    });
}
```

Helper above was `WithCBlock()` which indents using [Allman style](https://en.wikipedia.org/wiki/Indentation_style#Allman_style) (braces go on the next line), but there's also `WithJavaBlock()` which is the most common style for Java/Javascript (a variation of [C/C# Kernighan & Ritchie Style](https://en.wikipedia.org/wiki/Indentation_style#K&R_style) where opening brace goes at the end of the previous line before the indented block) and `WithPythonBlock()` (python style blocks have colons and indentation but no curly braces):

```cs
var w = new CodegenTextWriter();
// WithPythonBlock will automatically add the colon (:) and do the indentation
using (_w.WithPythonBlock("if date.today().weekday() == 0"))
{
    _w.WriteLine($"""
        print("Hello!")
        print("It's great to see you again")
        print("Happy Monday!")
        """);
}
```

## Explicit Indented Blocks using "Lambda-Style" Helpers
(Not recommended - prefer using implicit indent control)

```cs
string ns = "myNamespace";
string className = "myClass";
string method = "MyMethod";

w.WithCurlyBraces($"namespace {ns}", () =>
{
  w.WithCurlyBraces($"public class {className}", () => {
    w.WithCurlyBraces($"public void {method}()", () =>
    {
      w.WriteLine(@"test");
    });
  });
});
```

This is pretty much like using the `With*()` helpers - the only difference is that lambda callbacks can be `Action` or `Action<ICodegenTextWriter>` - so if the lambdas are in other method (different scope) they can just "receive" the ICodegenTextWriter (it doesn't have to be "shared" like a public instance).


## <a name="ManuallyInvokingMethods"></a>Manually Invoking Methods (without using interpolation)

**Manually invoking C# methods that write to the CodegenTextWriter**  (Not recommended - prefer implicit indent control and embedded templates)

One obvious way of breaking complex templates into smaller blocks is to explicit invoke one method from another and pass around CodegenTextWriter and the required models:

```cs
void GenerateTable(ICodegenTextWriter w, Table table)
{
    w.WriteLine($$"""
      public class {{ table.TableName }}
      {
          void Method1() { /* ... */ }
          void Method2() { /* ... */ }
      }""");
}

void GenerateFile(ICodegenTextWriter w)
{
    w.WriteLine("""
      using System;
      using System.Collections.Generic;
      """);

    // WithCBlock is a helper method described later 
    using (w.WithCBlock("public namespace MyNamespace"))
    {
        foreach(var table in schema.Tables)
            GenerateTable(w, table);
    }
}
```



## <a name="Action-ICodegenTextWriter-Args"></a> Using an Action\<ICodegenTextWriter> to invoke a method that requires arguments

The prefered method of invoking subtemplates and passing arguments is using [`Template Interfaces`](#TemplateInterfaces), but it's also possible to wrap any method into an `Action<ICodegenTextWriter>` like this:

```cs
// Regular C# void method being invoked explicitly (wrapped inside a new Action)
void GenerateClass(ICodegenTextWriter w, string className)
{
  w.Write($$"""
    public class {{ className }}()
    {
        void Method1() { /* ... */ }
        void Method2() { /* ... */ }
    }
    """);
}

Action<ICodegenTextWriter> GenerateFile = w => w.WriteLine($$"""
    using System;
    using System.Collections.Generic;
    namespace {{ ns }}
    {
        {{ new Action<ICodegenTextWriter>(w => GenerateClass(w, "ClassName1")) }}
        {{ new Action<ICodegenTextWriter>(w => GenerateClass(w, "ClassName2")) }}
    }
    """);

GenerateFile(w);
```

Or like this:

```cs
// Similar to previous, but the function itself will get the parameters and return a wrapper Action
Action<ICodegenTextWriter> GenerateClass(string className) = new Action<ICodegenTextWriter>(w => w.Write($$"""
    public class {{ className }}()
    {
        void Method1() { /* ... */ }
        void Method2() { /* ... */ }
    }
    """);

Action<ICodegenTextWriter> GenerateFile = w => w.WriteLine($$"""
    using System;
    using System.Collections.Generic;
    namespace {{ ns }}
    {
        {{ GenerateClass("ClassName1")) }}
        {{ GenerateClass("ClassName2")) }}
    }
    """);

GenerateFile(w);
```




# Advanced uses of Templating Interfaces

(this will probably be refactored in favor of simple delegates)

## Programatically Invoking Templates

Let's say we have a template implementing `ICodegenTemplate<TModel>` (a "single-file template"):

```cs
using CodegenCS;
using CodegenCS.Models.DbSchema;
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

The template above could be programatically loaded and invoked directly from `CodegenTextWriter`:

```cs
using CodegenCS;
using CodegenCS.Models.DbSchema;
using System.Linq;

class MyGenerator
{
    static void Main()
    {
        var schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
        var writer = new CodegenTextWriter(); // writing all POCOs in a single file.
        
        // We can load and render the template directly from the writer (no string interpolation)
        foreach (var table in schema.Tables)
            writer.LoadTemplate<MyPocoTemplate>().Render(table);

        File.WriteAllText("MyPocos.cs", writer.GetContents());
    }
}
```

If we want to save each POCO in a different file we could use ICodegenContext:

```cs
    static void Main()
    {
        var schema = JsonConvert.DeserializeObject<DatabaseSchema>(File.ReadAllText("AdventureWorks.json"));
        var context = new CodegenContext();
        
        foreach (var table in schema.Tables)
            context[table.TableName + ".cs"].LoadTemplate<MyPocoTemplate>().Render(table);
            
        context.SaveFiles(outputFolder: ".");
    }
```

## <a name="EmbeddingTemplate"></a> Embedding a Template (within an Interpolated String)

As explained earlier, CodegenTextWriter can render embedded `Actions<>` and `Func<>`, but in some scenarios that can get a little ugly when we need to [convert between delegates to pass parameters](https://github.com/Drizin/CodegenCS/tree/master/src/Core/CodegenCS/Deprecated.md#Action-ICodegenTextWriter-Args).  
If you implement the templating interfaces there are helpers to load and invoke templates (arguments are strongly-typed):


```cs
static void Main()
{
    // Using string interpolation to load and render directly inside string
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

## Embedding a Template to be invoked by a list of items (within an Interpolated String)

Template can be rendered for each element of the `IEnumerable`:

```cs
static void Main()
{
    // If we are "inside a literal and don't want to leave it" 
    // we can avoid the outer foreach and just embed and Render() an IEnumerable:
    writer.WriteLine($$"""
        // All my POCOs, rendered one by one.
        {{ tables.Select(table => Template.Load<MyPocoTemplate>().Render(table)).Render() }}
        """);
    // ...
    // Note that the second Render() above is optional - it allows to specify details like the separator 
    // between the items, but CodegenTextWriter is lenient and will render the items even if you forget the Render()
}
```

## Embedding Templates Summary:

- There is an extension `Load<T>()` to Load any template (by the Class Type `T` - we can load any type that implements templating interfaces). `Load<T>()` has Dependency Injection support (it can inject into the template constructor any required dependency)
- After a template is loaded there is an extension `Render(TModel model)` (or `Render()`)  to invoke (render) that template (providing the required input models, if any).
- Templates can be loaded and rendered directly from ICodegenTextWriter or from ICodegenContext using `textWriter.LoadTemplate<T>().Render(TModel model)`
- Templates can be loaded and rendered directly from interpolated strings (`{{ Template.Load<T>().Render(TModel model) }}`)
- Everything will be strongly typed, with intellisense/autocomplete and type-checking (e.g. `Render()` will expect a type depending on the template that was loaded)
- We can embed subtemplates inside other templates, and they can receive/pass models, meaning complex templates can be well organized (instead of a single huge/ugly template)
<!-- - Templates can rely on some **Ready to Use Input Models** like [CodegenCS.Models.DbSchema](https://github.com/Drizin/CodegenCS/tree/master/src/Models/CodegenCS.Models.DbSchema) - this model represents the database schema of a MSSQL database or a PostgreSQL database, and can be used by templates that generate POCOs or even complete data access layers. [dotnet-codegencs dbschema extract](https://github.com/Drizin/CodegenCS/tree/master/src/dotnet-codegencs) is a tool that extract the schema of those databases into a JSON file.
- Another input model (under development) is [CodegenCS.Models.OpenAPI](https://github.com/Drizin/CodegenCS/tree/master/src/Models/CodegenCS.Models.NSWagAdapter) - this model represents an OpenAPI (Swagger) specification, and can be used by templates that generate REST API clients or servers.
- You can use any other structured data source (that can be read using C#) as an input model (so you can read from JSON, YAML, XML, schema of other database vendors, etc)
 -->





# License
MIT License
