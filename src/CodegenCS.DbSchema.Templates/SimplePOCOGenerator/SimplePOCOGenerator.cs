using CodegenCS;
using CodegenCS.DbSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace CodegenCS.DbSchema.Templates.SimplePOCOGenerator
{
    #region SimplePOCOGeneratorOptions
    public class SimplePOCOGeneratorOptions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputJsonSchema">Absolute path of Database JSON schema</param>
        public SimplePOCOGeneratorOptions(string inputJsonSchema)
        {
            InputJsonSchema = inputJsonSchema;
        }

        #region Basic/Mandatory settings
        /// <summary>
        /// Absolute path of Database JSON schema
        /// </summary>
        public string InputJsonSchema { get; set; }

        /// <summary>
        /// Absolute path of the target folder where files will be written.
        /// Trailing slash not required.
        /// </summary>
        public string TargetFolder { get; set; } = "."; // by default generate in current dir.

        /// <summary>
        /// Namespace of generated POCOs.
        /// </summary>
        public string POCOsNamespace { get; set; } = "MyPOCOs";
        #endregion

        #region POCO optional settings
        /// <summary>
        /// If defined (default is true) POCOs will have override Equals/GetHashCode and equality/inequality operators (== and !=)
        /// </summary>
        public bool GenerateEqualsHashCode { get; set; } = true;


        /// <summary>
        /// If defined (default is false), POCOs will implement INotifyPropertyChanged (PropertyChanged event), and will expose a HashSet of "Dirty" properties and bool IsDirty
        /// </summary>
        public bool TrackPropertiesChange { get; set; } = false;

        /// <summary>
        /// If defined (default is true) will add [DatabaseGenerated] attributes to identity and computed columns.
        /// This is required by FastCRUD and Entity Framework
        /// </summary>
        public bool AddDatabaseGeneratedAttributes { get; set; } = true;


        /// <summary>
        /// If defined (default is null), all POCOs will be generated under this single filename.
        /// Example: "POCOs.Generated.cs"
        /// </summary>
        public string SingleFileName { get; set; } = null;


        #region Active Record
        /// <summary>
        /// If defined (not null), POCOs will have Active Record pattern (CRUD defined inside the class)
        /// </summary>
        public ActiveRecordOptions ActiveRecordSettings { get; set; } = null;
        public class ActiveRecordOptions
        {
            /// <summary>
            /// Active Record CRUD need a IDbConnection factory (since POCOs don't hold references to connections).
            /// This is the filepath where the template generates a sample factory. 
            /// By default it's named IDbConnectionFactory.cs
            /// </summary>
            public string ActiveRecordIDbConnectionFactoryFile { get; set; } = "IDbConnectionFactory.cs";

        }
        #endregion

        #region CRUD Extension Methods
        /// <summary>
        /// If defined (not null), will generate a static class with CRUD extension-methods for all POCOs
        /// </summary>
        public CRUDExtensionOptions CRUDExtensionSettings { get; set; } = null;
        public class CRUDExtensionOptions
        {
            /// <summary>
            /// If not defined will be the same as <see cref="POCOsNamespace"/>
            /// </summary>
            public string CrudExtensionsNamespace { get; set; }

            /// <summary>
            /// This is the filepath where the template generates CRUD extensions.
            /// By default it's named CRUDExtensions.cs
            /// </summary>
            public string CrudExtensionsFile { get; set; } = "CRUDExtensions.cs";

            /// <summary>
            /// Class Name
            /// </summary>
            public string CrudExtensionsClassName { get; set; } = "CRUDExtensions";
        }
        #endregion

        #region CRUD Class Methods
        /// <summary>
        /// If defined (not null), will generate a single class with CRUD methods for all POCOs
        /// </summary>
        public CRUDClassMethodsOptions CRUDClassMethodsSettings { get; set; } = null;
        public class CRUDClassMethodsOptions
        {
            /// <summary>
            /// If not defined will be the same as <see cref="POCOsNamespace"/>
            /// </summary>
            public string CrudClassNamespace { get; set; }

            /// <summary>
            /// This is the filepath where the template generates class with CRUD methods.
            /// By default it's named CRUDMethods.cs
            /// </summary>
            public string CrudClassFile { get; set; } = "CRUDMethods.cs";

            /// <summary>
            /// Class Name
            /// </summary>
            public string CrudClassName { get; set; } = "CRUDMethods";
        }
        #endregion

        #endregion

    }
    #endregion /SimplePOCOGeneratorOptions

    #region SimplePOCOGenerator
    public class SimplePOCOGenerator
    {
        public SimplePOCOGenerator(SimplePOCOGeneratorOptions options)
        {
            _options = options;
            schema = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicalSchema>(File.ReadAllText(_options.InputJsonSchema));
            //schema.Tables = schema.Tables.Select(t => Map<LogicalTable, Table>(t)).ToList<Table>(); 
        }
        private SimplePOCOGeneratorOptions _options { get; set; }
        private LogicalSchema schema { get; set; }

        public Action<string> WriteLog = (x) => Console.WriteLine(x);

        /// <summary>
        /// In-memory context which tracks all generated files, and later saves all files at once
        /// </summary>
        private CodegenContext _generatorContext { get; set; } = new CodegenContext();

        public CodegenContext GeneratorContext { get { return _generatorContext; } }

        private CodegenOutputFile _dbConnectionCrudExtensions = null;
        private CodegenOutputFile _dbConnectionCrudClassMethods = null;

        /// <summary>
        /// Generates POCOS
        /// </summary>
        public void Generate()
        {
            schema = schema ?? JsonConvert.DeserializeObject<LogicalSchema>(File.ReadAllText(_options.InputJsonSchema));

            CodegenOutputFile writer = null;
            if (_options.SingleFileName != null)
            {
                writer = _generatorContext[_options.SingleFileName];
                writer.WriteLine(@"
                    //------------------------------------------------------------------------------
                    // <auto-generated>
                    //     This code was generated by a tool.
                    //     Changes to this file may cause incorrect behavior and will be lost if
                    //     the code is regenerated.
                    // </auto-generated>
                    //------------------------------------------------------------------------------
                    using System;
                    using System.Collections.Generic;
                    using System.ComponentModel.DataAnnotations;
                    using System.ComponentModel.DataAnnotations.Schema;
                    using System.Linq;");
                if (_options.ActiveRecordSettings != null)
                    writer.WriteLine(@"using Dapper;");
                if (_options.TrackPropertiesChange)
                    writer.WriteLine(@"using System.ComponentModel;");
                writer
                    .WriteLine()
                    .WriteLine($"namespace {_options.POCOsNamespace}").WriteLine("{").IncreaseIndent();
            }

            if (_options.CRUDExtensionSettings != null)
            {
                _dbConnectionCrudExtensions = _generatorContext[_options.CRUDExtensionSettings.CrudExtensionsFile];
                _dbConnectionCrudExtensions.WriteLine(@"
                //------------------------------------------------------------------------------
                // <auto-generated>
                //     This code was generated by a tool.
                //     Changes to this file may cause incorrect behavior and will be lost if
                //     the code is regenerated.
                // </auto-generated>
                //------------------------------------------------------------------------------
                using Dapper;
                using System;
                using System.Collections.Generic;
                using System.Data;
                using System.Linq;
                using System.Runtime.CompilerServices;");
                if (_options.CRUDExtensionSettings.CrudExtensionsNamespace != null && _options.CRUDExtensionSettings.CrudExtensionsNamespace != _options.POCOsNamespace)
                    _dbConnectionCrudExtensions.WriteLine($@"using {_options.POCOsNamespace};");
                _dbConnectionCrudExtensions //TODO: IDisposable Scope 
                    .WriteLine()
                    .WriteLine($"namespace {_options.CRUDExtensionSettings.CrudExtensionsNamespace ?? _options.POCOsNamespace}").WriteLine("{").IncreaseIndent()
                    .WriteLine($"/// <summary>")
                    .WriteLine($"/// CRUD static extensions using Dapper (using static SQL statements)")
                    .WriteLine($"/// </summary>")
                    .WriteLine($"public static class {_options.CRUDExtensionSettings.CrudExtensionsClassName}").WriteLine("{").IncreaseIndent();
            }
            if (_options.CRUDClassMethodsSettings != null)
            {
                _dbConnectionCrudClassMethods = _generatorContext[_options.CRUDClassMethodsSettings.CrudClassFile];
                _dbConnectionCrudClassMethods.WriteLine(@"
                //------------------------------------------------------------------------------
                // <auto-generated>
                //     This code was generated by a tool.
                //     Changes to this file may cause incorrect behavior and will be lost if
                //     the code is regenerated.
                // </auto-generated>
                //------------------------------------------------------------------------------
                using Dapper;
                using System;
                using System.Collections.Generic;
                using System.Data;
                using System.Data.SqlClient;
                using System.Linq;
                using System.Runtime.CompilerServices;");
                if (_options.CRUDClassMethodsSettings.CrudClassNamespace != null && _options.CRUDClassMethodsSettings.CrudClassNamespace != _options.POCOsNamespace)
                    _dbConnectionCrudExtensions.WriteLine($@"using {_options.POCOsNamespace};");
                _dbConnectionCrudClassMethods
                    .WriteLine()
                    .WriteLine($"namespace {_options.CRUDClassMethodsSettings.CrudClassNamespace ?? _options.POCOsNamespace}").WriteLine("{").IncreaseIndent()
                    .WriteLine($"/// <summary>")
                    .WriteLine($"/// CRUD Methods using Dapper (using static SQL statements)")
                    .WriteLine($"/// </summary>")
                    .WriteLine($"partial class {_options.CRUDClassMethodsSettings.CrudClassName}").WriteLine("{").IncreaseIndent();
                _dbConnectionCrudClassMethods.WriteLine(@"
                    #region SQL/Dapper
                    IDbConnection CreateConnection()
                    {
                        string connectionString = @""Data Source=MYWORKSTATION\\SQLEXPRESS;
                                        Initial Catalog=AdventureWorks;
                                        Integrated Security=True;"";

                        return new SqlConnection(connectionString);
                    }
                    IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
                    {
                        using (var cn = CreateConnection())
                        {
                            return cn.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType);
                        }
                    }
                    int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null)
                    {
                        using (var cn = CreateConnection())
                        {
                            return cn.Execute(sql, param, transaction, commandTimeout, commandType);
                        }
                    }
                    #endregion End of SQL/Dapper").WriteLine();
            }


            if (_options.ActiveRecordSettings != null)
            {
                using (var writerConnectionFactory = _generatorContext[_options.ActiveRecordSettings.ActiveRecordIDbConnectionFactoryFile])
                {
                    writerConnectionFactory.WriteLine($@"
                    using System;
                    using System.Data;
                    using System.Data.SqlClient;

                    namespace {_options.POCOsNamespace}
                    {{
                        public class IDbConnectionFactory
                        {{
                            public static IDbConnection CreateConnection()
                            {{
                                string connectionString = @""Data Source=MYWORKSTATION\\SQLEXPRESS;
                                                Initial Catalog=AdventureWorks;
                                                Integrated Security=True;"";

                                return new SqlConnection(connectionString);
                            }}
                        }}
                    }}
                ");
                }
            }

            foreach (var table in schema.Tables.OrderBy(t => GetClassNameForTable(t)))
            {
                if (!ShouldProcessTable(table))
                    continue;

                GeneratePOCO(table);
            }

            if (_options.CRUDExtensionSettings != null)
                _dbConnectionCrudExtensions
                    .DecreaseIndent().WriteLine("}") // end of class
                    .DecreaseIndent().WriteLine("}"); // end of namespace

            if (_options.CRUDClassMethodsSettings != null)
                _dbConnectionCrudClassMethods
                    .DecreaseIndent().WriteLine("}") // end of class
                    .DecreaseIndent().WriteLine("}"); // end of namespace

            if (_options.SingleFileName != null)
                writer.DecreaseIndent().WriteLine("}"); // end of namespace
        }

        public void AddCSX()
        {
            #region Adding SimplePOCOGenerator.csx
            var mainProgram = new CodegenTextWriter();
            mainProgram.WriteLine($@"
                class Program
                {{
                    static void Main()
                    {{
                        //var options = new CodegenCS.DbSchema.Templates.SimplePOCOGenerator.SimplePOCOGeneratorOptions(inputJsonSchema: @""{_options.InputJsonSchema}"");
                        var options = Newtonsoft.Json.JsonConvert.DeserializeObject<CodegenCS.DbSchema.Templates.SimplePOCOGenerator.SimplePOCOGeneratorOptions>(@""
                            {Newtonsoft.Json.JsonConvert.SerializeObject(_options, Newtonsoft.Json.Formatting.Indented).Replace("\"", "\"\"")}
                        "");
                        var generator = new CodegenCS.DbSchema.Templates.SimplePOCOGenerator.SimplePOCOGenerator(options);
                        generator.Generate();
                        generator.Save();
                    }}
                }}
            ");
            // Export CS template (for customization)
            // Save with CSX extension so that it doesn't interfere with other existing CSPROJs (which by default include *.cs)
            GeneratorContext[typeof(SimplePOCOGenerator).Name + ".csx"].WriteLine(
                $"//This file is supposed to be launched using: codegencs run {typeof(SimplePOCOGenerator).Name}.csx" + Environment.NewLine
                + new StreamReader(typeof(SimplePOCOGenerator).Assembly.GetManifestResourceStream(typeof(SimplePOCOGenerator).FullName + ".cs")).ReadToEnd() + Environment.NewLine
                + mainProgram.ToString()
            );
            #endregion
        }

        /// <summary>
        /// Saves output
        /// </summary>
        public void Save()
        {
            // since no errors happened, let's save all files
            if (_options.TargetFolder != null)
                _generatorContext.SaveFiles(outputFolder: _options.TargetFolder);

            var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.White;
            WriteLog("Success!");
            Console.ForegroundColor = previousColor;
        }

        //public void SaveToZip(string zipFileName, string zipFolder)
        //{
        //    _generatorContext.SaveToZip(zipFileName, zipFolder);
        //}

        private void GeneratePOCO(Table table)
        {
            WriteLog($"Generating {table.TableName}...");

            CodegenOutputFile writer = null;
            if (_options.SingleFileName != null)
            {
                writer = _generatorContext[_options.SingleFileName];
            }
            else
            {
                writer = _generatorContext[GetFileNameForTable(table)];
                writer.WriteLine(@"
                    //------------------------------------------------------------------------------
                    // <auto-generated>
                    //     This code was generated by a tool.
                    //     Changes to this file may cause incorrect behavior and will be lost if
                    //     the code is regenerated.
                    // </auto-generated>
                    //------------------------------------------------------------------------------
                    using System;
                    using System.Collections.Generic;
                    using System.ComponentModel.DataAnnotations;
                    using System.ComponentModel.DataAnnotations.Schema;
                    using System.Linq;");
                if (_options.ActiveRecordSettings != null)
                    writer.WriteLine(@"using Dapper;");
                if (_options.TrackPropertiesChange)
                    writer.WriteLine(@"using System.ComponentModel;");
                writer
                    .WriteLine()
                    .WriteLine($"namespace {_options.POCOsNamespace}").WriteLine("{").IncreaseIndent();
            }

            string entityClassName = GetClassNameForTable(table);

            // We'll decorate [Table("Name")] only if schema not default or if table name doesn't match entity name
            if (table.TableSchema != "dbo") //TODO or table different than clas name?
                writer.WriteLine($"[Table(\"{table.TableName}\", Schema = \"{table.TableSchema}\")]");
            else if (entityClassName.ToLower() != table.TableName.ToLower())
                writer.WriteLine($"[Table(\"{table.TableName}\")]");

            List<string> baseClasses = new List<string>();
            if (_options.TrackPropertiesChange)
                baseClasses.Add("INotifyPropertyChanged");

            writer.WithCBlock($"public partial class {entityClassName}{(baseClasses.Any() ? " : " + string.Join(", ", baseClasses) : "")}", () =>
            {
                writer.WriteLine("#region Members");
                var columns = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .OrderBy(c => c.IsPrimaryKeyMember ? 0 : 1)
                    .ThenBy(c => c.IsPrimaryKeyMember ? c.OrdinalPosition : 0) // respect PK order... 
                    .ThenBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName)); // but for other columns do alphabetically;

                foreach (var column in columns)
                    GenerateProperty(writer, table, column);

                writer.WriteLine("#endregion Members");
                if (table.TableType == "TABLE" && columns.Any(c => c.IsPrimaryKeyMember))
                {
                    if (_options.ActiveRecordSettings != null)
                    {
                        writer.WriteLine();
                        writer.WriteLine("#region ActiveRecord");
                        GenerateActiveRecordSave(writer, table);
                        GenerateActiveRecordInsert(writer, table);
                        GenerateActiveRecordUpdate(writer, table);
                        writer.WriteLine("#endregion ActiveRecord");
                    }
                    if (_options.CRUDExtensionSettings != null)
                    {
                        _dbConnectionCrudExtensions.WriteLine();
                        _dbConnectionCrudExtensions.WriteLine($"#region {GetClassNameForTable(table)}");
                        GenerateCrudExtensionsSave(_dbConnectionCrudExtensions, table);
                        GenerateCrudExtensionsInsert(_dbConnectionCrudExtensions, table);
                        GenerateCrudExtensionsUpdate(_dbConnectionCrudExtensions, table);
                        _dbConnectionCrudExtensions.WriteLine($"#endregion {GetClassNameForTable(table)}");
                    }
                    if (_options.CRUDClassMethodsSettings != null)
                    {
                        _dbConnectionCrudClassMethods.WriteLine();
                        _dbConnectionCrudClassMethods.WriteLine($"#region {GetClassNameForTable(table)}");
                        GenerateCrudClassMethodsSave(_dbConnectionCrudClassMethods, table);
                        GenerateCrudClassMethodsInsert(_dbConnectionCrudClassMethods, table);
                        GenerateCrudClassMethodsUpdate(_dbConnectionCrudClassMethods, table);
                        _dbConnectionCrudClassMethods.WriteLine($"#endregion {GetClassNameForTable(table)}");
                    }
                }
                if (_options.GenerateEqualsHashCode)
                {
                    writer.WriteLine();
                    writer.WriteLine("#region Equals/GetHashCode");
                    GenerateEquals(writer, table);
                    GenerateGetHashCode(writer, table);
                    GenerateInequalityOperatorOverloads(writer, table);
                    writer.WriteLine("#endregion Equals/GetHashCode");
                }

                if (_options.TrackPropertiesChange)
                {
                    writer.WriteLine();
                    writer.WriteLine("#region INotifyPropertyChanged/IsDirty");
                    writer.WriteLine(@"
                        public HashSet<string> ChangedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        public void MarkAsClean()
                        {
                            ChangedProperties.Clear();
                        }
                        public virtual bool IsDirty => ChangedProperties.Any();

                        public event PropertyChangedEventHandler PropertyChanged;
                        protected void SetField<T>(ref T field, T value, string propertyName) {
                            if (!EqualityComparer<T>.Default.Equals(field, value)) {
                                field = value;
                                ChangedProperties.Add(propertyName);
                                OnPropertyChanged(propertyName);
                            }
                        }
                        protected virtual void OnPropertyChanged(string propertyName) {
                            if (PropertyChanged != null) {
                                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                            }
                        }");
                    writer.WriteLine("#endregion INotifyPropertyChanged/IsDirty");
                }

            });

            if (_options.SingleFileName == null)
            {
                writer.DecreaseIndent().WriteLine("}"); // end of namespace
            }
        }

        private void GenerateProperty(CodegenOutputFile writer, Table table, Column column)
        {
            string propertyName = GetPropertyNameForDatabaseColumn(table, column.ColumnName);
            string privateVariable = $"_{propertyName.Substring(0, 1).ToLower()}{propertyName.Substring(1)}";
            if (_options.TrackPropertiesChange)
                writer.WriteLine($"private {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {privateVariable};");
            if (column.IsPrimaryKeyMember)
                writer.WriteLine("[Key]");
            if (column.IsIdentity && _options.AddDatabaseGeneratedAttributes)
                writer.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            else if (column.IsComputed && _options.AddDatabaseGeneratedAttributes)
                writer.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Computed)]");

            // We'll decorate [Column("Name")] only if column name doesn't match property name
            if (propertyName.ToLower() != column.ColumnName.ToLower())
                writer.WriteLine($"[Column(\"{column.ColumnName}\")]");
            if (_options.TrackPropertiesChange)
                writer.WriteLine($@"
                public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} 
                {{ 
                    get {{ return {privateVariable}; }} 
                    set {{ SetField(ref {privateVariable}, value, nameof({propertyName})); }} 
                }}");
            else
                writer.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} {{ get; set; }}");
        }

        private void GenerateActiveRecordSave(CodegenOutputFile writer, Table table)
        {
            writer.WithCBlock("public void Save()", () =>
            {
                var pkCols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => c.IsPrimaryKeyMember).OrderBy(c => c.OrdinalPosition);
                writer.WriteLine($@"
                if ({string.Join(" && ", pkCols.Select(col => GetPropertyNameForDatabaseColumn(table, col.ColumnName) + $" == {GetDefaultValue(GetTypeForDatabaseColumn(table, col))}"))})
                    Insert();
                else
                    Update();");
            });
        }
        private void GenerateCrudExtensionsSave(CodegenOutputFile writer, Table table)
        {
            GenerateCrudSave(writer, table, "public static ", "IDbConnection", "conn");
        }
        private void GenerateCrudClassMethodsSave(CodegenOutputFile writer, Table table)
        {
            GenerateCrudSave(writer, table, "public virtual ");
        }
        private void GenerateCrudSave(CodegenOutputFile writer, Table table, string modifier, string extendedType = null, string extendedTypeIdentifier = null)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Saves (if new) or Updates (if existing)
            /// </summary>");
            writer.WithCBlock($"{modifier}void Save({(extendedType == null ? "" : "this " + extendedType + " " + extendedTypeIdentifier + ", ")}{GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
            {
                var pkCols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => c.IsPrimaryKeyMember).OrderBy(c => c.OrdinalPosition);
                writer.WriteLine($@"
                if ({string.Join(" && ", pkCols.Select(col => "e." + GetPropertyNameForDatabaseColumn(table, col.ColumnName) + $" == {GetDefaultValue(GetTypeForDatabaseColumn(table, col))}"))})
                    {extendedTypeIdentifier ?? "this"}.Insert(e, transaction, commandTimeout);
                else
                    {extendedTypeIdentifier ?? "this"}.Update(e, transaction, commandTimeout);");
            });
        }

        private void GenerateActiveRecordInsert(CodegenOutputFile writer, Table table)
        {
            writer.WithCBlock("public void Insert()", () =>
            {
                writer.WithCBlock("using (var conn = IDbConnectionFactory.CreateConnection())", () =>
                {
                    var cols = table.Columns
                        .Where(c => ShouldProcessColumn(table, c))
                        .Where(c => !c.IsIdentity)
                        .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
                        .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
                        .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                    writer.WithIndent($"string cmd = @\"{Environment.NewLine}INSERT INTO {(table.TableSchema == "dbo" ? "" : $"[{table.TableSchema}].")}[{table.TableName}]{Environment.NewLine}(", ")", () =>
                    {
                        writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"[{col.ColumnName}]")));
                    });
                    writer.WithIndent($"VALUES{Environment.NewLine}(", ")\";", () =>
                    {
                        writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"@{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}")));
                    });

                    writer.WriteLine();
                    var identityCol = table.Columns
                        .Where(c => ShouldProcessColumn(table, c))
                        .Where(c => c.IsIdentity).FirstOrDefault();
                    if (identityCol != null)
                        writer.WriteLine($"this.{GetPropertyNameForDatabaseColumn(table, identityCol.ColumnName)} = conn.Query<{GetTypeDefinitionForDatabaseColumn(table, identityCol)}>(cmd + \"SELECT SCOPE_IDENTITY();\", this).Single();");
                    else
                        writer.WriteLine($"conn.Execute(cmd, this);");
                });
            });
        }
        private void GenerateCrudExtensionsInsert(CodegenOutputFile writer, Table table)
        {
            GenerateCrudInsert(writer, table, "public static ", "IDbConnection", "conn");
        }
        private void GenerateCrudClassMethodsInsert(CodegenOutputFile writer, Table table)
        {
            GenerateCrudInsert(writer, table, "public virtual ");
        }
        private void GenerateCrudInsert(CodegenOutputFile writer, Table table, string modifier, string extendedType = null, string extendedTypeIdentifier = null)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Saves new record
            /// </summary>");
            writer.WithCBlock($"{modifier}void Insert({(extendedType == null ? "" : "this " + extendedType + " " + extendedTypeIdentifier + ", ")}{GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
            {
                var cols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => !c.IsIdentity)
                    .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
                    .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
                    .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                writer.WithIndent($"string cmd = @\"{Environment.NewLine}INSERT INTO {(table.TableSchema == "dbo" ? "" : $"[{table.TableSchema}].")}[{table.TableName}]{Environment.NewLine}(", ")", () =>
                {
                    writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"[{col.ColumnName}]")));
                });
                writer.WithIndent($"VALUES{Environment.NewLine}(", ")\";", () =>
                {
                    writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"@{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}")));
                });

                writer.WriteLine();
                var identityCol = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => c.IsIdentity).FirstOrDefault();
                if (identityCol != null)
                    writer.WriteLine($"e.{GetPropertyNameForDatabaseColumn(table, identityCol.ColumnName)} = {extendedTypeIdentifier ?? "this"}.Query<{GetTypeDefinitionForDatabaseColumn(table, identityCol)}>(cmd + \"SELECT SCOPE_IDENTITY();\", e, transaction, commandTimeout: commandTimeout).Single();");
                else
                    writer.WriteLine($"{extendedTypeIdentifier ?? "this"}.Execute(cmd, e, transaction, commandTimeout);");

                if (_options.TrackPropertiesChange)
                    writer.WriteLine().WriteLine("e.MarkAsClean();");
            });
        }

        private void GenerateActiveRecordUpdate(CodegenOutputFile writer, Table table)
        {
            writer.WithCBlock("public void Update()", () =>
            {
                writer.WithCBlock("using (var conn = IDbConnectionFactory.CreateConnection())", () =>
                {
                    var cols = table.Columns
                        .Where(c => ShouldProcessColumn(table, c))
                        .Where(c => !c.IsIdentity)
                        .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
                        .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
                        .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                    writer.WithIndent($"string cmd = @\"{Environment.NewLine}UPDATE {(table.TableSchema == "dbo" ? "" : $"[{table.TableSchema}].")}[{table.TableName}] SET", "", () =>
                    {
                        writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"[{col.ColumnName}] = @{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}")));
                    });

                    var pkCols = table.Columns
                        .Where(c => ShouldProcessColumn(table, c))
                        .Where(c => c.IsPrimaryKeyMember)
                        .OrderBy(c => c.OrdinalPosition);
                    writer.WriteLine($@"
                    WHERE
                        {string.Join($" AND {Environment.NewLine}", pkCols.Select(col => $"[{col.ColumnName}] = @{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}"))}"";");
                    writer.WriteLine($"conn.Execute(cmd, this);");
                });
            });
        }
        private void GenerateCrudExtensionsUpdate(CodegenOutputFile writer, Table table)
        {
            GenerateCrudUpdate(writer, table, "public static ", "IDbConnection", "conn");
        }
        private void GenerateCrudClassMethodsUpdate(CodegenOutputFile writer, Table table)
        {
            GenerateCrudUpdate(writer, table, "public virtual ");
        }
        private void GenerateCrudUpdate(CodegenOutputFile writer, Table table, string modifier, string extendedType = null, string extendedTypeIdentifier = null)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Updates existing record
            /// </summary>");
            writer.WithCBlock($"{modifier}void Update({(extendedType == null ? "" : "this " + extendedType + " " + extendedTypeIdentifier + ", ")}{GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
            {
                var cols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => !c.IsIdentity)
                    .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
                    .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
                    .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                writer.WithIndent($"string cmd = @\"{Environment.NewLine}UPDATE {(table.TableSchema == "dbo" ? "" : $"[{table.TableSchema}].")}[{table.TableName}] SET", "", () =>
                {
                    writer.WriteLine(string.Join($",{Environment.NewLine}", cols.Select(col => $"[{col.ColumnName}] = @{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}")));
                });

                var pkCols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => c.IsPrimaryKeyMember)
                    .OrderBy(c => c.OrdinalPosition);
                writer.WriteLine($@"
                WHERE
                    {string.Join($" AND {Environment.NewLine}", pkCols.Select(col => $"[{col.ColumnName}] = @{GetPropertyNameForDatabaseColumn(table, col.ColumnName)}"))}"";");
                writer.WriteLine($"{extendedTypeIdentifier ?? "this"}.Execute(cmd, e, transaction, commandTimeout);");
                if (_options.TrackPropertiesChange)
                    writer.WriteLine().WriteLine("e.MarkAsClean();");
            });

        }

        private void GenerateEquals(CodegenOutputFile writer, Table table)
        {
            //TODO: GenerateIEquatable, which is a little faster for Generic collections - and our Equals(object other) can reuse this IEquatable<T>.Equals(T other) 
            writer.WithCBlock("public override bool Equals(object obj)", () =>
            {
                string entityClassName = GetClassNameForTable(table);
                writer.WriteLine($@"
                if (ReferenceEquals(null, obj))
                {{
                    return false;
                }}
                if (ReferenceEquals(this, obj))
                {{
                    return true;
                }}
                {entityClassName} other = obj as {entityClassName};
                if (other == null) return false;
                ");

                var cols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => !c.IsIdentity)
                    .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                foreach (var col in cols)
                {
                    string prop = GetPropertyNameForDatabaseColumn(table, col.ColumnName);
                    writer.WriteLine($@"
                    if ({prop} != other.{prop})
                        return false;");
                }
                writer.WriteLine("return true;");
            });
        }
        private void GenerateGetHashCode(CodegenOutputFile writer, Table table)
        {
            writer.WithCBlock("public override int GetHashCode()", () =>
            {
                writer.WithCBlock("unchecked", () =>
                {
                    writer.WriteLine("int hash = 17;");
                    var cols = table.Columns
                        .Where(c => ShouldProcessColumn(table, c))
                        .Where(c => !c.IsIdentity)
                        .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
                //TODO: for dotnetcore we can use HashCode.Combine(field1, field2, field3)
                foreach (var col in cols)
                    {
                        string prop = GetPropertyNameForDatabaseColumn(table, col.ColumnName);
                        writer.WriteLine($@"hash = hash * 23 + ({prop} == {GetDefaultValue(GetTypeForDatabaseColumn(table, col))} ? 0 : {prop}.GetHashCode());");
                    }
                    writer.WriteLine("return hash;");
                });
            });
        }
        private void GenerateInequalityOperatorOverloads(CodegenOutputFile writer, Table table)
        {
            string entityClassName = GetClassNameForTable(table);
            writer.WriteLine($@"
            public static bool operator ==({entityClassName} left, {entityClassName} right)
            {{
                return Equals(left, right);
            }}

            public static bool operator !=({entityClassName} left, {entityClassName} right)
            {{
                return !Equals(left, right);
            }}
            ");
        }

        private string GetFileNameForTable(Table table)
        {
            return $"{table.TableName}.generated.cs";
            if (table.TableSchema == "dbo")
                return $"{table.TableName}.generated.cs";
            else
                return $"{table.TableSchema}.{table.TableName}.generated.cs";
        }
        private string GetClassNameForTable(Table table)
        {
            return $"{table.TableName}";
            if (table.TableSchema == "dbo")
                return $"{table.TableName}";
            else
                return $"{table.TableSchema}_{table.TableName}";
        }
        private bool ShouldProcessTable(Table table)
        {
            if (table.TableType == "VIEW")
                return false;
            return true;
        }
        private bool ShouldProcessColumn(Table table, Column column)
        {
            string sqlDataType = column.SqlDataType;
            switch (sqlDataType)
            {
                case "hierarchyid":
                case "geography":
                    return false;
                default:
                    break;
            }

            return true;
        }

        private static Dictionary<Type, string> _typeAlias = new Dictionary<Type, string>
        {
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(object), "object" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(string), "string" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            // Yes, this is an odd one.  Technically it's a type though.
            { typeof(void), "void" }
        };

        private Type GetTypeForDatabaseColumn(Table table, Column column)
        {
            System.Type type;
            try
            {
                type = Type.GetType(column.ClrType);
            }
            catch (Exception ex)
            {
                return null; // third-party (vendor specific) types which our script doesn't recognize
            }

            bool isNullable = column.IsNullable;

            // Many developers use POCO instances with null Primary Key to represent a new (in-memory) object, so they prefer to set POCO PKs as Nullable 
            //if (column.IsPrimaryKeyMember)
            //    isNullable = true;

            // reference types (basically only strings?) are nullable by default are nullable, no need to make it explicit
            if (!type.IsValueType)
                isNullable = false;

            if (isNullable)
                return typeof(Nullable<>).MakeGenericType(type);

            return type;
        }
        private string GetTypeDefinitionForDatabaseColumn(Table table, Column column)
        {
            Type type = GetTypeForDatabaseColumn(table, column);
            if (type == null)
                return "?!";

            // unwrap nullable types
            bool isNullable = false;
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            if (underlyingType != type)
                isNullable = true;

            string typeName = underlyingType.Name;

            // Let's use short type names (int instead of Int32, long instead of Int64, string instead of String, etc)
            if (_typeAlias.TryGetValue(underlyingType, out string alias))
                typeName = alias;

            if (!isNullable)
                return typeName;
            return $"{typeName}?"; // some might prefer $"System.Nullable<{typeName}>"
        }
        private static string GetDefaultValue(Type type)
        {
            // all reference-types default to null
            if (type == null || !type.IsValueType)
                return "null";

            // all nullables default to null
            if (Nullable.GetUnderlyingType(type) != null)
                return "null";

            // Maybe we should replace by 0, DateTime.MinValue, Guid.Empty, etc? 
            string typeName = type.Name;
            // Let's use short type names (int instead of Int32, long instead of Int64, string instead of String, etc)
            if (_typeAlias.TryGetValue(type, out string alias))
                typeName = alias;
            return $"default({typeName})";
        }

        // From PetaPoco - https://github.com/CollaboratingPlatypus/PetaPoco/blob/development/T4Templates/PetaPoco.Core.ttinclude
        private static Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);
        private static string[] cs_keywords = { "abstract", "event", "new", "struct", "as", "explicit", "null",
             "switch", "base", "extern", "object", "this", "bool", "false", "operator", "throw",
             "break", "finally", "out", "true", "byte", "fixed", "override", "try", "case", "float",
             "params", "typeof", "catch", "for", "private", "uint", "char", "foreach", "protected",
             "ulong", "checked", "goto", "public", "unchecked", "class", "if", "readonly", "unsafe",
             "const", "implicit", "ref", "ushort", "continue", "in", "return", "using", "decimal",
             "int", "sbyte", "virtual", "default", "interface", "sealed", "volatile", "delegate",
             "internal", "short", "void", "do", "is", "sizeof", "while", "double", "lock",
             "stackalloc", "else", "long", "static", "enum", "namespace", "string" };

        /// <summary>
        /// Gets a unique identifier name for the column, which doesn't conflict with the POCO class itself or with previous identifiers for this POCO.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="column"></param>
        /// <param name="previouslyUsedIdentifiers"></param>
        /// <returns></returns>
        string GetPropertyNameForDatabaseColumn(Table table, string columnName)
        {
            if (table.ColumnPropertyNames.ContainsKey(columnName))
                return table.ColumnPropertyNames[columnName];

            string name = columnName;

            // Replace forbidden characters
            name = rxCleanUp.Replace(name, "_");

            // Split multiple words
            var parts = splitUpperCase.Split(name).Where(part => part != "_" && part != "-").ToList();
            // we'll put first word into TitleCase except if it's a single-char in lowercase (like vNameOfTable) which we assume is a prefix (like v for views) and should be preserved as is
            // if first world is a single-char in lowercase (like vNameOfTable) which we assume is a prefix (like v for views) and should be preserved as is

            // Recapitalize (to TitleCase) all words
            for (int i = 0; i < parts.Count; i++)
            {
                // if first world is a single-char in lowercase (like vNameOfTable), we assume it's a prefix (like v for views) and should be preserved as is
                if (i == 0 && parts[i].Length == 1 && parts[i].ToLower() != parts[i])
                    continue;

                switch (parts[i])
                {
                    //case "ID": // don't convert "ID" for "Id"
                    //    break;
                    default:
                        parts[i] = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(parts[i].ToLower());
                        break;
                }
            }

            name = string.Join("", parts);

            // can't start with digit
            if (char.IsDigit(name[0]))
                name = "_" + name;

            // can't be a reserved keyword
            if (cs_keywords.Contains(name))
                name = "@" + name;

            // check for name clashes
            int n = 0;
            string attemptName = name;
            while ((GetClassNameForTable(table) == attemptName || table.ColumnPropertyNames.ContainsValue((attemptName))) && n < 100)
            {
                n++;
                attemptName = name + n.ToString();
            }
            table.ColumnPropertyNames.Add(columnName, attemptName);

            return attemptName;
        }

        // Splits both camelCaseWords and also TitleCaseWords. Underscores and dashes are also splitted. Uppercase acronyms are also splitted.
        // E.g. "BusinessEntityID" becomes ["Business","Entity","ID"]
        // E.g. "Employee_SSN" becomes ["employee","_","SSN"]
        private static Regex splitUpperCase = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z0-9]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z0-9])(?=[^A-Za-z0-9])", RegexOptions.IgnorePatternWhitespace);

        public static T Map<T, S>(S source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

    }
    #endregion /SimplePOCOGenerator

    #region SimplePOCOGeneratorConsoleHelper
    public class SimplePOCOGeneratorConsoleHelper
    {
        public static SimplePOCOGeneratorOptions GetOptions(SimplePOCOGeneratorOptions options = null)
        {
            options = options ?? new SimplePOCOGeneratorOptions(null);
            while (string.IsNullOrEmpty(options.InputJsonSchema))
            {
                Console.WriteLine($"[Choose an Input JSON Schema File]");
                Console.Write($"Input file: ");
                options.InputJsonSchema = Console.ReadLine();
                if (!File.Exists(options.InputJsonSchema))
                {
                    Console.WriteLine($"File {options.InputJsonSchema} does not exist");
                    options.InputJsonSchema = null;
                    continue;
                }
                options.InputJsonSchema = new FileInfo(options.InputJsonSchema).FullName;
            }

            while (string.IsNullOrEmpty(options.TargetFolder))
            {
                Console.WriteLine($"[Choose a Target Folder]");
                Console.Write($"Target Folder: ");
                options.TargetFolder = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(options.POCOsNamespace))
            {
                Console.WriteLine($"[Choose a Namespace]");
                Console.Write($"Namespace: ");
                options.POCOsNamespace = Console.ReadLine();
            }
            return options;
        }
    }
    #endregion /SimplePOCOGeneratorConsoleHelper

    #region LogicalSchema
    /*************************************************************************************************************************
     The serialized JSON schema (http://codegencs.com/schemas/dbschema/2021-07/dbschema.json) has only Physical Properties.
     Here we extend the Physical definitions with some new Logical definitions.
     For example: ForeignKeys in a logical model have the "Navigation Property Name".
     And POCOs (mapped 1-to-1 by Entities) track the list of Property Names used by Columns, used by Navigation Properties, etc., 
     to avoid naming conflicts.
    **************************************************************************************************************************/

    public class LogicalSchema : CodegenCS.DbSchema.DatabaseSchema
    {
        public new List<Table> Tables { get; set; }
    }
    public class Table : CodegenCS.DbSchema.Table
    {
        public Dictionary<string, string> ColumnPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> FKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, string> ReverseFKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public new List<Column> Columns { get; set; } = new List<Column>();
        public new List<ForeignKey> ForeignKeys { get; set; } = new List<ForeignKey>();
        public new List<ForeignKey> ChildForeignKeys { get; set; } = new List<ForeignKey>();
    }

    public class ForeignKey : CodegenCS.DbSchema.ForeignKey
    {
        public string NavigationPropertyName { get; set; }
    }
    public class Column : CodegenCS.DbSchema.Column
    {

    }


    #endregion /LogicalSchema

}
