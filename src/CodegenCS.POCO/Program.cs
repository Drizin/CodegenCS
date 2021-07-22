using CodegenCS.Utils;
using System;
using System.IO;

namespace CodegenCS.POCO
{
    public class Program
    {
        // Helpers to get the location of the current CS file
        public static string GetScriptPath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path;
        public static string GetScriptFolder([System.Runtime.CompilerServices.CallerFilePath] string path = null) => System.Diagnostics.Debugger.IsAttached ? Path.GetDirectoryName(path) : System.IO.Directory.GetCurrentDirectory();

        private string _commandLine { get; set; }
        public Program(string commandLine)
        {
            _commandLine = commandLine;
        }

        static void Main(string[] args)
        {
            new Program(System.AppDomain.CurrentDomain.FriendlyName).Run(args);
        }
        public void Run(string[] args)
        { 
            #region Command-Line Arguments
            var argsParser = new CommandLineArgsParser(args);
            if (argsParser["?"] != null || argsParser["help"] != null)
            {
                ShowUsage();
                Environment.Exit(0);
            }
            #endregion

            //string outputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), @".\AdventureWorksSchema.json"));
            var options = new SimplePOCOGeneratorOptions(inputJsonSchema: argsParser["input"]); // this is required, but if not provided SimplePOCOGeneratorConsoleHelper.GetOptions will ask for it

            if (argsParser.ContainsKey("targetFolder"))
                options.TargetFolder = argsParser["targetFolder"];
            if (argsParser.ContainsKey("namespace"))
                options.POCOsNamespace = argsParser["namespace"];

            if (argsParser["EqualsHashCode"] == "false")
                options.GenerateEqualsHashCode = false;
            if (argsParser["INotifyPropertyChanged"] == "true")
                options.TrackPropertiesChange = true;
            if (argsParser["DatabaseGenerated"] == "true")
                options.AddDatabaseGeneratedAttributes = true;

            if (argsParser["SingleFile"] == "true")
                options.SingleFileName = "POCOs.generated.cs";
            else if (argsParser.ContainsKey("SingleFile"))
                options.SingleFileName = argsParser["SingleFile"];

            if (argsParser["CrudActiveRecord"] == "true")
            {
                options.ActiveRecordSettings = new SimplePOCOGeneratorOptions.ActiveRecordOptions();
                if (argsParser.ContainsKey("ActiveRecordIDbConnectionFactoryFile") && argsParser["ActiveRecordIDbConnectionFactoryFile"] != "true")
                    options.ActiveRecordSettings.ActiveRecordIDbConnectionFactoryFile = argsParser["ActiveRecordIDbConnectionFactoryFile"];
            }

            if (argsParser["CrudExtensions"] == "true")
            {
                options.CRUDExtensionSettings = new SimplePOCOGeneratorOptions.CRUDExtensionOptions();
                if (argsParser.ContainsKey("CrudExtensionsNamespace") && argsParser["CrudExtensionsNamespace"] != "true")
                    options.CRUDExtensionSettings.CrudExtensionsNamespace = argsParser["CrudExtensionsNamespace"];
                if (argsParser.ContainsKey("CrudExtensionsFile") && argsParser["CrudExtensionsFile"] != "true")
                    options.CRUDExtensionSettings.CrudExtensionsFile = argsParser["CrudExtensionsFile"];
                if (argsParser.ContainsKey("CrudExtensionsClassName") && argsParser["CrudExtensionsClassName"] != "true")
                    options.CRUDExtensionSettings.CrudExtensionsClassName = argsParser["CrudExtensionsClassName"];
            }

            if (argsParser["CrudClassMethods"] == "true")
            {
                options.CRUDClassMethodsSettings = new SimplePOCOGeneratorOptions.CRUDClassMethodsOptions();
                if (argsParser.ContainsKey("CrudClassMethodsNamespace") && argsParser["CrudClassMethodsNamespace"] != "true")
                    options.CRUDClassMethodsSettings.CrudClassNamespace = argsParser["CrudClassMethodsNamespace"];
                if (argsParser.ContainsKey("CrudClassMethodsFile") && argsParser["CrudClassMethodsFile"] != "true")
                    options.CRUDClassMethodsSettings.CrudClassFile = argsParser["CrudClassMethodsFile"];
                if (argsParser.ContainsKey("CrudClassMethodsClassName") && argsParser["CrudClassMethodsClassName"] != "true")
                    options.CRUDClassMethodsSettings.CrudClassName = argsParser["CrudClassMethodsClassName"];
            }

            SimplePOCOGeneratorConsoleHelper.GetOptions(options); // if mandatory args were not provided, ask in Console
            var generator = new SimplePOCOGenerator(options);

            var mainProgram = new CodegenTextWriter();
            mainProgram.WriteLine($@"
                class Program
                {{
                    static void Main()
                    {{
                        //var options = new CodegenCS.POCO.SimplePOCOGeneratorOptions(inputJsonSchema: @""{options.InputJsonSchema}"");
                        var options = Newtonsoft.Json.JsonConvert.DeserializeObject<CodegenCS.POCO.SimplePOCOGeneratorOptions>(@""
                            {Newtonsoft.Json.JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented).Replace("\"","\"\"")}
                        "");
                        var generator = new CodegenCS.POCO.SimplePOCOGenerator(options);
                        generator.Generate();
                        generator.Save();
                    }}
                }}
            ");
            // Export CS template (for customization)
            // Save with CSX extension so that it doesn't interfere with other existing CSPROJs (which by default include *.cs)
            generator.GeneratorContext["Template.csx"].WriteLine(
                "//This file is supposed to be launched using: dotnet run Template.csproj" + Environment.NewLine
                + new StreamReader(typeof(SimplePOCOGenerator).Assembly.GetManifestResourceStream("CodegenCS.POCO.SimplePOCOGenerator.cs")).ReadToEnd() + Environment.NewLine
                + mainProgram.ToString()
            );
            generator.GeneratorContext["Template.csproj"].WriteLine(
                $@"
<Project Sdk=""Microsoft.NET.Sdk"">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net5.0</TargetFramework>
	<EnableDefaultItems>false</EnableDefaultItems>
    <NoWarn>CS0162,CS0168</NoWarn>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include=""Template.csx"" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include=""CodegenCS"" Version=""1.*"" />
    <PackageReference Include=""CodegenCS.DbSchema"" Version=""1.*"" />
    <PackageReference Include=""Newtonsoft.Json"" Version=""12.*"" />
  </ItemGroup>

</Project>".TrimStart()
            );


            generator.Generate();
            generator.Save();
            var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("To regenerate the outputs use \"dotnet run Template.csproj\". Use Template.csx to customize the output.");
            Console.ForegroundColor = previousColor;
        }

        void ShowUsage()
        {
            Console.WriteLine(string.Format("Basic Usage: {0} [/input=jsonschema] [/targetfolder=folder] [/namespace=namespace]", _commandLine));
            Console.WriteLine(string.Format(""));
            Console.WriteLine(string.Format("Options:"));
            Console.WriteLine(string.Format("     [/EqualsHashCode | /EqualsHashCode=true | /EqualsHashCode=false] - default is true"));
            Console.WriteLine(string.Format("          If true POCOs will override Equals, GetHashCode and equality/inequality operators"));
            Console.WriteLine(string.Format("     [/INotifyPropertyChanged | /INotifyPropertyChanged=true | /INotifyPropertyChanged=false] - default is false"));
            Console.WriteLine(string.Format("          If true POCOs will implement INotifyPropertyChanged and track \"Dirty\" properties"));
            Console.WriteLine(string.Format("     [/DatabaseGenerated | /DatabaseGenerated=true | /DatabaseGenerated=false] - default is true"));
            Console.WriteLine(string.Format("          If true POCOs will add [DatabaseGenerated] attributes to identity (auto-number) and computed columns."));
            Console.WriteLine(string.Format("          This is required by FastCRUD and Entity Framework"));
            Console.WriteLine(string.Format("     [/SingleFile | /SingleFile=<filename>]"));
            Console.WriteLine(string.Format("          If defined all POCOs will be generated under this single filename."));
            Console.WriteLine(string.Format("          If filename is not specified default is \"POCOs.generated.cs\""));

            Console.WriteLine(string.Format("     [/CrudActiveRecord]"));
            Console.WriteLine(string.Format("     [/CrudActiveRecord [ActiveRecordIDbConnectionFactoryFile=<filename>]]"));
            Console.WriteLine(string.Format("          If defined POCOs will have CRUD generated inside the classes (ActiveRecord pattern)"));

            Console.WriteLine(string.Format("     [/CrudExtensions]"));
            Console.WriteLine(string.Format("     [/CrudExtensions [CrudExtensionsNamespace=<namespace>] [CrudExtensionsFile=<file>] [CrudExtensionsClassName=<class>]]"));
            Console.WriteLine(string.Format("          If defined will generate CRUD as static method extensions in an external class"));
            Console.WriteLine(string.Format("     [/CrudClassMethods]"));
            Console.WriteLine(string.Format("     [/CrudClassMethods [CrudClassMethodsNamespace=<namespace>] [CrudClassMethodsFile=<file>] [CrudClassMethodsClassName=<class>]]"));
            Console.WriteLine(string.Format("          If defined will generate CRUD as regular class methods in an external class"));
            Console.WriteLine(string.Format(""));
            Console.WriteLine(string.Format("Examples: {0} /input=AdventureWorks.json /targetfolder=.\\POCOS /namespace=My.Namespace", _commandLine));
            Console.WriteLine(string.Format("Examples: {0} /input=AdventureWorks.json /targetfolder=. /namespace=My.Namespace", _commandLine));
            Console.WriteLine(string.Format("              /CrudActiveRecord"));
            Console.WriteLine(string.Format("              /CrudExtensions CrudExtensionsFile=CRUD.generated.cs"));
        }

    }
}
