using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using Console = InterpolatedColorConsole.ColoredConsole;

namespace CodegenCS.DbSchema.Templates.SimplePOCOGenerator
{
    public static class CliCommand
    {
        static Option optionSingleFile;
        static Option optionCrudExtensionMethods;
        static Option optionCrudClassMethods;
        public static Command GetCommand()
        {
            var command = new Command("simplepocogenerator");
            //command.AddAlias("poco");

            command.AddArgument(new Argument<string>("input", description: "Input JSON schema. E.g. \"Schema.json\"") { Arity = ArgumentArity.ExactlyOne }); //TODO: validate HERE if file exists
            command.AddOption(new Option<string>(new[] { "--Namespace", "-n" }, description: "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Namespace" });
            command.AddOption(new Option<string>(new[] { "--TargetFolder", "-t" }, description: "Folder to save output [default: current folder]") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "Folder" });
            command.AddOption(optionSingleFile = new Option<string>(new[] { "--SingleFile", "-s" },  description: "Generates all POCOs in a single file [default: POCOs.generated.cs]") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName="File.cs" });
            
            command.AddOption(new Option<bool>(new[] { "--EqualsHashCode", "-h" }, description: "Override Equals/GetHashCode and equality/inequality operators", getDefaultValue: () => Defaults.GenerateEqualsHashCode) { Arity = ArgumentArity.ZeroOrOne });
            command.AddOption(new Option<bool>(new[] { "--INotifyPropertyChanged", "-p" }, getDefaultValue: () => Defaults.TrackPropertiesChange, description: "Implement INotifyPropertyChanged, and expose \"Dirty\" properties and IsDirty flag"));
            command.AddOption(new Option<bool>(new[] { "--KeyAttribute", "-k" }, getDefaultValue: () => Defaults.AddColumnAttributeKey, description: "Adds [Key] attributes to primary-key columns"));
            command.AddOption(new Option<bool>(new[] { "--DatabaseGeneratedAttribute", "-d" }, getDefaultValue: () => Defaults.AddColumnAttributeDatabaseGenerated, description: "Adds [DatabaseGenerated(DatabaseGeneratedOption.Identity)] attributes to identity (auto-number) columns or [DatabaseGenerated(DatabaseGeneratedOption.Computed)] to other types of computed columns"));

            command.AddOption(new Option<bool>(new[] { "--CrudActiveRecord", "-a" }, description: "Generates CRUD inside each POCO (Active Record pattern)"));
            command.AddOption(new Option<string>(new[] { "--ActiveRecordIDbConnectionFactoryFile" }, getDefaultValue: () => Defaults.ActiveRecordSettings.ActiveRecordIDbConnectionFactoryFile, description: "Filepath where the template generates a sample factory") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs", IsHidden = true });

            command.AddOption(optionCrudExtensionMethods = new Option<string>(new[] { "--CrudExtensionMethods", "-e" }, description: $"Generates a static class with CRUD extension-methods for all POCOs.\nYou can specify a fully qualified namespace+class (e.g. \"MyNamespace.POCOs.Extensions.CRUDExtensions\") or just a class name (e.g. \"CRUDExtensions\") in which case will be in the same namespace as POCOs. [default: {Defaults.CRUDExtensionSettings.ClassName}]\n") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "ClassName" });
            command.AddOption(new Option<string>(new[] { "--CrudExtensionMethodsFile" }, description: $"If previous option is used, the file will be created in TargetFolder (with the POCOs) and named based on the ClassName (e.g. \"CRUDExtensions.cs\"). \nThis option allows to specify a different file name and/or folder.\n(e.g. \"CRUD.cs\", or \"Extensions\\ \", or \"Extensions\\CRUDExtensions.cs\")") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs" });

            command.AddOption(optionCrudClassMethods = new Option<string>(new[] { "--CrudClassMethods", "-c" }, description: $"Generates a single class with CRUD instance methods for all POCOs.\nYou can specify a fully qualified namespace+class (e.g. \"MyNamespace.DAL.Repository\") or just a class name (e.g. \"Repository\") in which case will be in the same namespace as POCOs. [default: {Defaults.CRUDClassMethodsSettings.ClassName}]\n") { Arity = ArgumentArity.ZeroOrOne, ArgumentHelpName = "ClassName" });
            command.AddOption(new Option<string>(new[] { "--CrudClassMethodsFile" }, description: $"If previous option is used, the file will be created in TargetFolder (with the POCOs) and named based on the ClassName (e.g. \"CRUDMethods.cs\"). \nThis option allows to specify a different file name and/or folder.\n(e.g. \"CRUD.cs\", or \"DAL\\ \", or \"DAL\\MyRepository.cs\")") { Arity = ArgumentArity.ExactlyOne, ArgumentHelpName = "File.cs" });

            command.Handler = CommandHandler.Create<ParseResult, SimplePOCOGeneratorArgs>(HandleCommand);

            return command;
        }

        static int HandleCommand(ParseResult parseResult, SimplePOCOGeneratorArgs cliArgs)
        {
            //TODO: HasOption(string) doesn't work in the lib - we have to check manually
            if (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--debug")) { Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(cliArgs, Newtonsoft.Json.Formatting.Indented)); }

            var options = cliArgs.ToOptions(parseResult);
            if (parseResult.Tokens.Any(t => t.Type == TokenType.Option && t.Value == "--debug")) { Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented)); }
            SimplePOCOGeneratorConsoleHelper.GetOptions(options); // if mandatory args were not provided, ask in Console

            Console.WriteLine(ConsoleColor.Green, $"Executing '{typeof(SimplePOCOGenerator).Name}' template...");

            var generator = new SimplePOCOGenerator(options);
            generator.Generate();
            generator.AddCSX();
            generator.Save();

            using (Console.WithColor(ConsoleColor.Green))
            {
                Console.WriteLine($"Finished executing '{typeof(SimplePOCOGenerator).Name}' template.");
                Console.WriteLine($"A copy of the original template (with the options used) was saved as '{typeof(SimplePOCOGenerator).Name}.csx'.");
                Console.WriteLine($"To customize the template outputs you can modify the csx file and regenerate using 'codegencs run {typeof(SimplePOCOGenerator).Name}.csx'.");
            }

            return 0;
        }

        static SimplePOCOGeneratorOptions Defaults = new SimplePOCOGeneratorOptions(null)
        {
            CRUDClassMethodsSettings = new SimplePOCOGeneratorOptions.CRUDClassMethodsOptions(),
            CRUDExtensionSettings = new SimplePOCOGeneratorOptions.CRUDExtensionOptions(),
            ActiveRecordSettings = new SimplePOCOGeneratorOptions.ActiveRecordOptions()
        };

        public class SimplePOCOGeneratorArgs
        {
            public string Input { get; set; }
            public string TargetFolder { get; set; }
            public string Namespace { get; set; }
            public string SingleFile { get; set; }
            public bool EqualsHashCode { get; set; }
            public bool INotifyPropertyChanged { get; set; }
            public bool KeyAttribute { get; set; }
            public bool DatabaseGeneratedAttribute { get; set; }
            
            public bool CrudActiveRecord { get; set; }
            public string ActiveRecordIDbConnectionFactoryFile { get; set; }
            
            public string CrudExtensionMethods { get; set; }
            public string CrudExtensionMethodsFile { get; set; }

            public string CrudClassMethods { get; set; }
            public string CrudClassMethodsFile { get; set; }

            public SimplePOCOGeneratorOptions ToOptions(ParseResult parseResult)
            {
                var options = new SimplePOCOGeneratorOptions(this.Input); // if not provided SimplePOCOGeneratorConsoleHelper.GetOptions will ask for it

                if (!string.IsNullOrEmpty(this.TargetFolder))
                    options.TargetFolder = this.TargetFolder;
                else
                    options.TargetFolder = "."; // Template will make it relative to Directory.GetCurrentDirectory();

                options.POCOsNamespace = this.Namespace;
                options.GenerateEqualsHashCode = this.EqualsHashCode;
                options.TrackPropertiesChange = this.INotifyPropertyChanged;
                options.AddColumnAttributeKey = this.KeyAttribute;
                options.AddColumnAttributeDatabaseGenerated = this.DatabaseGeneratedAttribute;
                
                options.SingleFileName = this.SingleFile ?? (parseResult.HasOption(optionSingleFile) ? "POCOs.generated.cs" : null);

                if (this.CrudActiveRecord)
                {
                    options.ActiveRecordSettings = new SimplePOCOGeneratorOptions.ActiveRecordOptions();
                    if (!string.IsNullOrEmpty(this.ActiveRecordIDbConnectionFactoryFile))
                        options.ActiveRecordSettings.ActiveRecordIDbConnectionFactoryFile = this.ActiveRecordIDbConnectionFactoryFile;
                }
                if (parseResult.HasOption(optionCrudExtensionMethods))
                {
                    (string className, string ns) = SplitFullyQualifiedClassName(this.CrudExtensionMethods);
                    className ??= Defaults.CRUDExtensionSettings.ClassName;
                    ns ??= this.Namespace;
                    options.CRUDExtensionSettings = new SimplePOCOGeneratorOptions.CRUDExtensionOptions()
                    {
                        ClassName = className,
                        Namespace = ns
                    };

                    (string folderPath, string fileName) = SplitPath(this.CrudExtensionMethodsFile?.Trim());
                    fileName ??= className + ".cs"; // if not specified, file is named after class name
                    folderPath ??= "";
                    options.CRUDExtensionSettings.FileName = folderPath + fileName;
                }
                if (parseResult.HasOption(optionCrudClassMethods))
                {
                    (string className, string ns) = SplitFullyQualifiedClassName(this.CrudClassMethods);
                    className ??= Defaults.CRUDClassMethodsSettings.ClassName;
                    ns ??= this.Namespace;
                    options.CRUDClassMethodsSettings = new SimplePOCOGeneratorOptions.CRUDClassMethodsOptions()
                    {
                        ClassName = className,
                        Namespace = ns
                    };

                    (string folderPath, string fileName) = SplitPath(this.CrudClassMethodsFile?.Trim());
                    fileName ??= className + ".cs"; // if not specified, file is named after class name
                    folderPath ??= "";
                    options.CRUDClassMethodsSettings.FileName = folderPath + fileName;
                }
                return options;
            }

        }
        
        /// <summary>
        /// Given a fully-qualified class name "MyNamespace.Subnamespace.ClassName" will split into namespace and ClassName.
        /// Namespace is optional (will return null), but class name is required.
        /// </summary>
        private static (string className, string ns) SplitFullyQualifiedClassName(string fq)
        {
            if (string.IsNullOrWhiteSpace(fq))
                return (null, null);

            string className = null; string ns = null;
           
            int pos = fq.LastIndexOf(".");
            if (pos >= 0)
            {
                ns = fq.Substring(0, pos);
                className = fq.Substring(pos + 1);
            }
            else
                className = fq;
            return (className, ns);
        }

        /// <summary>
        /// Given a path (relative or absolute) like "MyFolder\MyFile.cs" will split into path (with trailing slash) and file name.
        /// Both are optional: 
        /// - if no file is defined (after last trailing slash) it will return fileName=null; 
        /// - if no folder is defined (no trailing slash) it will will return folderPath=null;
        /// </summary>
        private static (string folderPath, string fileName) SplitPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return (null, null);

            string fileName = null; string folderPath = null;
            int pos = path.LastIndexOf("\\");
            if (pos >= 0)
            {
                folderPath = path.Substring(0, pos+1);
                if (pos + 1 < path.Length) // if empty just leave it as null
                    fileName = path.Substring(pos + 1);
            }
            else
                fileName = path;
            return (folderPath, fileName);
        }

    }
}
