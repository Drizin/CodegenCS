using CodegenCS;
using CodegenCS.DbSchema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Microsoft.CodeAnalysis;

namespace CodegenCS.POCO
{
    public class SimplePOCOGenerator
    {
        /// <summary>
        /// Absolute path of JSON schema
        /// </summary>
        public string InputJsonSchema { get; set; }

        /// <summary>
        /// Absolute path of the target folder where files will be written
        /// </summary>
        public string TargetFolder { get; set; }

        public string Namespace { get; set; }
        public bool SingleFile { get; set; } = false;
        public bool GenerateActiveRecord { get; set; } = true;
        public string ActiveRecordIDbConnectionFactoryFile { get; set; } = "..\\IDbConnectionFactory.cs";
        public bool GenerateCrudExtensions { get; set; } = true;
        public string CrudExtensionsFile { get; set; } = "..\\CRUDExtensions.cs";
        public string CrudExtensionsClass { get; set; } = "CRUDExtensions";
        public bool GenerateEqualsHashCode { get; set; } = true;
        public bool TrackPropertiesChange { get; set; } = true;

        /// <summary>
        /// In-memory context which tracks all generated files (with indentation support), and later saves all files at once
        /// </summary>
        public CodegenContext _generatorContext { get; set; }

        private CodegenOutputFile _dbConnectionCrudExtensions = null;

        public SimplePOCOGenerator()
        {
        }

        string singleFileName = "POCOs.Generated.cs";

        /// <summary>
        /// Generates POCOS
        /// </summary>
        public void Generate() // TODO: GeneratorExecutionContext context
        {
            _generatorContext = new CodegenContext();

            Console.WriteLine("Reading Schema...");

            LogicalSchema schema = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicalSchema>(File.ReadAllText(InputJsonSchema));
            //schema.Tables = schema.Tables.Select(t => Map<LogicalTable, Table>(t)).ToList<Table>(); 

            CodegenOutputFile writer = null;
            if (SingleFile)
            {
                writer = _generatorContext[singleFileName];
                writer
                    .WriteLine(@"using System;")
                    .WriteLine(@"using System.Collections.Generic;")
                    .WriteLine(@"using System.ComponentModel.DataAnnotations;")
                    .WriteLine(@"using System.ComponentModel.DataAnnotations.Schema;")
                    .WriteLine(@"using System.Linq;");
                if (GenerateActiveRecord)
                    writer.WriteLine(@"using Dapper;");
                if (TrackPropertiesChange)
                    writer.WriteLine(@"using System.ComponentModel;");
                writer
                    .WriteLine()
                    .WriteLine($"namespace {Namespace}").WriteLine("{").IncreaseIndent();
            }

            if (GenerateCrudExtensions)
            {
                _dbConnectionCrudExtensions = _generatorContext[CrudExtensionsFile];
                _dbConnectionCrudExtensions.Write(@"
                using Dapper;
                using System;
                using System.Collections.Generic;
                using System.Data;
                using System.Linq;
                using System.Runtime.CompilerServices;
                ");
                _dbConnectionCrudExtensions
                    .WriteLine()
                    .WriteLine($"namespace {Namespace}").WriteLine("{").IncreaseIndent()
                    .WriteLine($"public static class {CrudExtensionsClass}").WriteLine("{").IncreaseIndent();
            }

            /* // IDbConnectionFactory will not be defined by the code generator... should be created manually
            if (GenerateActiveRecord)
            {
                using (var writerConnectionFactory = _generatorContext[ActiveRecordIDbConnectionFactoryFile])
                {
                    writerConnectionFactory.WriteLine($@"
                    using System;
                    using System.Data;
                    using System.Data.SqlClient;

                    namespace {Namespace}
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
            */

            foreach (var table in schema.Tables.OrderBy(t => GetClassNameForTable(t)))
            {
                if (!ShouldProcessTable(table))
                    continue;

                GeneratePOCO(table);
            }

            if (GenerateCrudExtensions)
                _dbConnectionCrudExtensions
                    .DecreaseIndent().WriteLine("}") // end of class
                    .DecreaseIndent().WriteLine("}"); // end of namespace

            if (SingleFile)
                writer.DecreaseIndent().WriteLine("}"); // end of namespace

            // since no errors happened, let's save all files
            //_generatorContext.SaveFiles(outputFolder: TargetFolder);

            Console.WriteLine("Success!");
        }
        void GeneratePOCO(Table table)
        {
            Console.WriteLine($"Generating {table.TableName}...");

            CodegenOutputFile writer = null;
            if (SingleFile)
            {
                writer = _generatorContext[singleFileName];
            }
            else
            {
                writer = _generatorContext[GetFileNameForTable(table)];
                writer
                    .WriteLine(@"using System;")
                    .WriteLine(@"using System.Collections.Generic;")
                    .WriteLine(@"using System.ComponentModel.DataAnnotations;")
                    .WriteLine(@"using System.ComponentModel.DataAnnotations.Schema;")
                    .WriteLine(@"using System.Linq;");
                if (GenerateActiveRecord)
                    writer.WriteLine(@"using Dapper;");
                if (TrackPropertiesChange)
                    writer.WriteLine(@"using System.ComponentModel;");
                writer
                    .WriteLine()
                    .WriteLine($"namespace {Namespace}").WriteLine("{").IncreaseIndent();
            }

            string entityClassName = GetClassNameForTable(table);

            // We'll decorate [Table("Name")] only if schema not default or if table name doesn't match entity name
            if (table.TableSchema != "dbo") //TODO or table different than clas name?
                writer.WriteLine($"[Table(\"{table.TableName}\", Schema = \"{table.TableSchema}\")]");
            else if (entityClassName.ToLower() != table.TableName.ToLower())
                writer.WriteLine($"[Table(\"{table.TableName}\")]");

            List<string> baseClasses = new List<string>();
            if (TrackPropertiesChange)
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
                    if (GenerateActiveRecord)
                    {
                        writer.WriteLine();
                        writer.WriteLine("#region ActiveRecord");
                        GenerateActiveRecordSave(writer, table);
                        GenerateActiveRecordInsert(writer, table);
                        GenerateActiveRecordUpdate(writer, table);
                        writer.WriteLine("#endregion ActiveRecord");
                    }
                    if (GenerateCrudExtensions)
                    {
                        _dbConnectionCrudExtensions.WriteLine();
                        _dbConnectionCrudExtensions.WriteLine($"#region {GetClassNameForTable(table)}");
                        GenerateCrudExtensionsSave(_dbConnectionCrudExtensions, table);
                        GenerateCrudExtensionsInsert(_dbConnectionCrudExtensions, table);
                        GenerateCrudExtensionsUpdate(_dbConnectionCrudExtensions, table);
                        _dbConnectionCrudExtensions.WriteLine($"#endregion {GetClassNameForTable(table)}");
                    }
                }
                if (GenerateEqualsHashCode)
                {
                    writer.WriteLine();
                    writer.WriteLine("#region Equals/GetHashCode");
                    GenerateEquals(writer, table);
                    GenerateGetHashCode(writer, table);
                    GenerateInequalityOperatorOverloads(writer, table);
                    writer.WriteLine("#endregion Equals/GetHashCode");
                }

                if (TrackPropertiesChange)
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

            if (!SingleFile)
            {
                writer.DecreaseIndent().WriteLine("}"); // end of namespace
            }
        }

        void GenerateProperty(CodegenOutputFile writer, Table table, Column column)
        {
            string propertyName = GetPropertyNameForDatabaseColumn(table, column.ColumnName);
            string privateVariable = $"_{propertyName.Substring(0, 1).ToLower()}{propertyName.Substring(1)}";
            if (TrackPropertiesChange)
                writer.WriteLine($"private {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {privateVariable};");
            if (column.IsPrimaryKeyMember)
                writer.WriteLine("[Key]");
            // We'll decorate [Column("Name")] only if column name doesn't match property name
            if (propertyName.ToLower() != column.ColumnName.ToLower())
                writer.WriteLine($"[Column(\"{column.ColumnName}\")]");
            if (TrackPropertiesChange)
                writer.WriteLine($@"
                public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} 
                {{ 
                    get {{ return {privateVariable}; }} 
                    set {{ SetField(ref {privateVariable}, value, nameof({propertyName})); }} 
                }}");
            else
                writer.WriteLine($"public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} {{ get; set; }}");
        }

        void GenerateActiveRecordSave(CodegenOutputFile writer, Table table)
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
        void GenerateCrudExtensionsSave(CodegenOutputFile writer, Table table)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Saves (if new) or Updates (if existing)
            /// </summary>");
            writer.WithCBlock($"public static void Save(this IDbConnection conn, {GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
            {
                var pkCols = table.Columns
                    .Where(c => ShouldProcessColumn(table, c))
                    .Where(c => c.IsPrimaryKeyMember).OrderBy(c => c.OrdinalPosition);
                writer.WriteLine($@"
                if ({string.Join(" && ", pkCols.Select(col => "e." + GetPropertyNameForDatabaseColumn(table, col.ColumnName) + $" == {GetDefaultValue(GetTypeForDatabaseColumn(table, col))}"))})
                    conn.Insert(e, transaction, commandTimeout);
                else
                    conn.Update(e, transaction, commandTimeout);");
            });
        }

        void GenerateActiveRecordInsert(CodegenOutputFile writer, Table table)
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
                        .Where(c => c.IsPrimaryKeyMember && c.IsIdentity).FirstOrDefault();
                    if (identityCol != null && table.Columns.Where(c => c.IsPrimaryKeyMember).Count() == 1)
                        writer.WriteLine($"this.{GetPropertyNameForDatabaseColumn(table, identityCol.ColumnName)} = conn.Query<{GetTypeDefinitionForDatabaseColumn(table, identityCol)}>(cmd + \"SELECT SCOPE_IDENTITY();\", this).Single();");
                    else
                        writer.WriteLine($"conn.Execute(cmd, this);");
                });
            });
        }
        void GenerateCrudExtensionsInsert(CodegenOutputFile writer, Table table)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Saves new record
            /// </summary>");
            writer.WithCBlock($"public static void Insert(this IDbConnection conn, {GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
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
                    .Where(c => c.IsPrimaryKeyMember && c.IsIdentity).FirstOrDefault();
                if (identityCol != null && table.Columns.Where(c => c.IsPrimaryKeyMember).Count() == 1)
                    writer.WriteLine($"e.{GetPropertyNameForDatabaseColumn(table, identityCol.ColumnName)} = conn.Query<{GetTypeDefinitionForDatabaseColumn(table, identityCol)}>(cmd + \"SELECT SCOPE_IDENTITY();\", e, transaction, commandTimeout: commandTimeout).Single();");
                else
                    writer.WriteLine($"conn.Execute(cmd, e, transaction, commandTimeout);");

                if (TrackPropertiesChange)
                    writer.WriteLine().WriteLine("e.MarkAsClean();");
            });
        }

        void GenerateActiveRecordUpdate(CodegenOutputFile writer, Table table)
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
        void GenerateCrudExtensionsUpdate(CodegenOutputFile writer, Table table)
        {
            writer.WriteLine(@"
            /// <summary>
            /// Updates existing record
            /// </summary>");
            writer.WithCBlock($"public static void Update(this IDbConnection conn, {GetClassNameForTable(table)} e, IDbTransaction transaction = null, int? commandTimeout = null)", () =>
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
                writer.WriteLine($"conn.Execute(cmd, e, transaction, commandTimeout);");
                if (TrackPropertiesChange)
                    writer.WriteLine().WriteLine("e.MarkAsClean();");
            });

        }

        void GenerateEquals(CodegenOutputFile writer, Table table)
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
        void GenerateGetHashCode(CodegenOutputFile writer, Table table)
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
        void GenerateInequalityOperatorOverloads(CodegenOutputFile writer, Table table)
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

        string GetFileNameForTable(Table table)
        {
            return $"{table.TableName}.generated.cs";
            if (table.TableSchema == "dbo")
                return $"{table.TableName}.cs";
            else
                return $"{table.TableSchema}.{table.TableName}.cs";
        }
        string GetClassNameForTable(Table table)
        {
            return $"{table.TableName}";
            if (table.TableSchema == "dbo")
                return $"{table.TableName}";
            else
                return $"{table.TableSchema}_{table.TableName}";
        }
        bool ShouldProcessTable(Table table)
        {
            if (table.TableType == "VIEW")
                return false;
            return true;
        }
        bool ShouldProcessColumn(Table table, Column column)
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

        static Dictionary<Type, string> _typeAlias = new Dictionary<Type, string>
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

        Type GetTypeForDatabaseColumn(Table table, Column column)
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
        string GetTypeDefinitionForDatabaseColumn(Table table, Column column)
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
        public static string GetDefaultValue(Type type)
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
        static Regex rxCleanUp = new Regex(@"[^\w\d_]", RegexOptions.Compiled);
        static string[] cs_keywords = { "abstract", "event", "new", "struct", "as", "explicit", "null",
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
        static Regex splitUpperCase = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z0-9]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z0-9])(?=[^A-Za-z0-9])", RegexOptions.IgnorePatternWhitespace);

        public static T Map<T, S>(S source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }


    }
}
