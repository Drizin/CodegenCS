using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Text;

namespace CodegenCS.DbSchema.Templates.SimplePOCOGenerator
{
    public static class CliCommand
    {
        public static Command GetCommand()
        {
            var command = new Command("simplepocogenerator");
            //command.AddAlias("poco");

            command.AddArgument(new Argument<string>("input", description: "Input JSON schema. E.g. \"Schema.json\"") { Arity = ArgumentArity.ExactlyOne }); //TODO: validate HERE if file exists
            command.AddOption(new Option<string>(new[] { "--TargetFolder", "-t" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Folder" });
            command.AddOption(new Option<string>(new[] { "--Namespace", "-n" }, description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });
            command.AddOption(new Option<string>(new[] { "--SingleFile", "-s" }, description: "Generates all POCOs in a single file", getDefaultValue: () => "POCOs.generated.cs") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName="File.cs", });
            command.AddOption(new Option<bool>(new[] { "--EqualsHashCode", "-h" }, description: "Override Equals/GetHashCode and equality/inequality operators", getDefaultValue: () => true) { Arity = ArgumentArity.ZeroOrOne });
            command.AddOption(new Option<bool>(new[] { "--INotifyPropertyChanged", "-p" }, getDefaultValue: () => false, description: "Implement INotifyPropertyChanged, and expose \"Dirty\" properties and IsDirty flag"));
            command.AddOption(new Option<bool>(new[] { "--DatabaseGenerated", "-d" }, getDefaultValue: () => false, description: "Adds [DatabaseGenerated] attributes to identity (auto-number) and computed columns\n"));

            command.AddOption(new Option<bool>(new[] { "--CrudActiveRecord", "-a" }, getDefaultValue: () => false, description: "Implement Active Record pattern (CRUD inside the class)"));
            command.AddOption(new Option<string>(new[] { "--ActiveRecordIDbConnectionFactoryFile" }, getDefaultValue: () => "IDbConnectionFactory.cs", description: "Filepath where the template generates a sample factory\n") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs" });

            command.AddOption(new Option<bool>(new[] { "--CrudExtensions", "-e" }, getDefaultValue: () => false, description: "Generate a static class with CRUD extension-methods for all POCOs"));
            command.AddOption(new Option<string>(new[] { "--CrudExtensionsNamespace" }, description: "Namespace of Crud Extensions file (if not defined will be the same as POCOs namespace)") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });
            command.AddOption(new Option<string>(new[] { "--CrudExtensionsFile" }, description: "File for generating CRUD extensions") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs" });
            command.AddOption(new Option<string>(new[] { "--CrudExtensionsClassName" }, description: "Class Name\n") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "ClassName" });

            command.AddOption(new Option<bool>(new[] { "--CrudClassMethods", "-c" }, getDefaultValue: () => false, description: "Generate a single class with CRUD methods for all POCOs"));
            command.AddOption(new Option<string>(new[] { "--CrudClassMethodsNamespace" }, description: "Namespace of Crud methods file (if not defined will be the same as POCOs namespace)") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });
            command.AddOption(new Option<string>(new[] { "--CrudClassMethodsFile" }, description: "File for generating CRUD methods") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs" });
            command.AddOption(new Option<string>(new[] { "--CrudClassMethodsClassName" }, description: "Class Name\n") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "ClassName" });

            command.Handler = CommandHandler.Create<ParseResult, SimplePOCOGeneratorArgs>(HandleCommand);

            return command;
        }

        static int HandleCommand(ParseResult parseResult, SimplePOCOGeneratorArgs cliArgs)
        {
            if (parseResult.HasOption("--debug"))
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(cliArgs, Newtonsoft.Json.Formatting.Indented));

            var options = cliArgs.ToOptions();
            SimplePOCOGeneratorConsoleHelper.GetOptions(options); // if mandatory args were not provided, ask in Console

            var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Executing '{typeof(SimplePOCOGenerator).Name}' template...");
            Console.ForegroundColor = previousColor;

            var generator = new SimplePOCOGenerator(options);
            generator.Generate();
            generator.AddCSX();
            generator.Save();

            previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Finished executing '{typeof(SimplePOCOGenerator).Name}' template.");
            Console.WriteLine($"A copy of the original template (with the options used) was saved as '{typeof(SimplePOCOGenerator).Name}.csx'.");
            Console.WriteLine($"To customize the template outputs you can modify the csx file and regenerate using 'codegencs run {typeof(SimplePOCOGenerator).Name}.csx'.");
            Console.ForegroundColor = previousColor;

            return 0;
        }

        public class SimplePOCOGeneratorArgs
        {
            public string Input { get; set; }
            public string TargetFolder { get; set; }
            public string Namespace { get; set; }
            public bool EqualsHashCode { get; set; }
            public bool INotifyPropertyChanged { get; set; }
            public bool DatabaseGenerated { get; set; }
            public string SingleFile { get; set; }
            
            public bool CrudActiveRecord { get; set; }
            public string ActiveRecordIDbConnectionFactoryFile { get; set; }
            
            public bool CrudExtensions { get; set; }
            public string CrudExtensionsNamespace { get; set; }
            public string CrudExtensionsFile { get; set; }
            public string CrudExtensionsClassName { get; set; }

            public bool CrudClassMethods { get; set; }
            public string CrudClassMethodsNamespace { get; set; }
            public string CrudClassMethodsFile { get; set; }
            public string CrudClassMethodsClassName { get; set; }

            public SimplePOCOGeneratorOptions ToOptions()
            {
                var options = new SimplePOCOGeneratorOptions(this.Input); // if not provided SimplePOCOGeneratorConsoleHelper.GetOptions will ask for it
                
                if (!string.IsNullOrEmpty(this.TargetFolder))
                    options.TargetFolder = this.TargetFolder;
                else
                    options.TargetFolder = Directory.GetCurrentDirectory();

                options.POCOsNamespace = this.Namespace;
                options.GenerateEqualsHashCode = this.EqualsHashCode;
                options.TrackPropertiesChange = this.INotifyPropertyChanged;
                options.AddDatabaseGeneratedAttributes = this.DatabaseGenerated;
                options.SingleFileName = this.SingleFile; //TODO: if not provided use "POCOs.generated.cs"
                if (this.CrudActiveRecord)
                {
                    options.ActiveRecordSettings = new SimplePOCOGeneratorOptions.ActiveRecordOptions();
                    if (!string.IsNullOrEmpty(this.ActiveRecordIDbConnectionFactoryFile))
                        options.ActiveRecordSettings.ActiveRecordIDbConnectionFactoryFile = this.ActiveRecordIDbConnectionFactoryFile;
                }
                if (this.CrudExtensions)
                {
                    options.CRUDExtensionSettings = new SimplePOCOGeneratorOptions.CRUDExtensionOptions();
                    if (!string.IsNullOrEmpty(this.CrudExtensionsNamespace))
                        options.CRUDExtensionSettings.CrudExtensionsNamespace = this.CrudExtensionsNamespace;
                    if (!string.IsNullOrEmpty(this.CrudExtensionsFile))
                        options.CRUDExtensionSettings.CrudExtensionsFile = this.CrudExtensionsFile;
                    if (!string.IsNullOrEmpty(this.CrudExtensionsClassName))
                        options.CRUDExtensionSettings.CrudExtensionsClassName = this.CrudExtensionsClassName;
                }
                if (this.CrudClassMethods)
                {
                    options.CRUDClassMethodsSettings = new SimplePOCOGeneratorOptions.CRUDClassMethodsOptions();
                    if (!string.IsNullOrEmpty(this.CrudClassMethodsNamespace))
                        options.CRUDClassMethodsSettings.CrudClassNamespace = this.CrudClassMethodsNamespace;
                    if (!string.IsNullOrEmpty(this.CrudClassMethodsFile))
                        options.CRUDClassMethodsSettings.CrudClassFile = this.CrudClassMethodsFile;
                    if (!string.IsNullOrEmpty(this.CrudClassMethodsClassName))
                        options.CRUDClassMethodsSettings.CrudClassName = this.CrudClassMethodsClassName;
                }
                return options;
            }

        }

    }
}
