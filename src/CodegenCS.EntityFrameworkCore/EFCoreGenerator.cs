using CodegenCS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.EntityFrameworkCore
{
#endif
    public class EFCoreGenerator
    {
        /// <summary>
        /// Absolute path of JSON schema
        /// </summary>
        public string InputJsonSchema
        {
            get { return _inputJsonSchema; }
            set { if (value != null && !System.IO.Path.IsPathRooted(value)) value = System.IO.Path.Combine(Program.GetScriptFolder(), value); _inputJsonSchema = value; }
        }
        private string _inputJsonSchema = null;

        /// <summary>
        /// Absolute path of the target folder where files will be written
        /// </summary>
        public string TargetFolder 
        {
            get { return _targetFolder; }
            set { if (value != null && !System.IO.Path.IsPathRooted(value)) value = System.IO.Path.Combine(Program.GetScriptFolder(), value); _targetFolder = value; }
        }
        private string _targetFolder = null;

        public string Namespace { get; set; }
        public string ContextName { get; set; }

        /// <summary>
        /// In-memory context which tracks all generated files (with indentation support), and later saves all files at once
        /// </summary>
        CodegenContext _generatorContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputJsonSchema">Absolute path of JSON schema</param>
        public EFCoreGenerator()
        {
        }

        bool withAttributes = false;

        /// <summary>
        /// Generates Entities and DbContext
        /// </summary>
        public void Generate()
        {

            while (string.IsNullOrEmpty(InputJsonSchema))
            {

                Console.WriteLine($"[Choose an Input JSON Schema File]");
                Console.Write($"Input file: ");
                InputJsonSchema = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(TargetFolder))
            {

                Console.WriteLine($"[Choose a Target Folder]");
                Console.Write($"Target Folder: ");
                TargetFolder = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(Namespace))
            {

                Console.WriteLine($"[Choose a Namespace]");
                Console.Write($"Namespace: ");
                Namespace = Console.ReadLine();
            }

            while (string.IsNullOrEmpty(ContextName))
            {

                Console.WriteLine($"[Choose a DbContext Name]");
                Console.Write($"DbContext Name: ");
                ContextName = Console.ReadLine();
            }


            _generatorContext = new CodegenContext();

            Console.WriteLine("Reading Schema...");

            LogicalSchema schema = Newtonsoft.Json.JsonConvert.DeserializeObject<LogicalSchema>(File.ReadAllText(InputJsonSchema));
            //schema.Tables = schema.Tables.Select(t => Map<LogicalTable, Table>(t)).ToList<Table>();

            // Define a unique property name for each column
            foreach (var table in schema.Tables)
            {
                if (!ShouldProcessTable(table))
                    continue;
                var columns = table.Columns.Where(c => ShouldProcessColumn(table, c));
                foreach (var column in columns)
                {
                    string propertyName = GetPropertyNameForDatabaseColumn(table, column);
                }
            }

            // Define a unique property name for each navigation property and reverse navigation property
            foreach (var table in schema.Tables)
            {
                foreach (var fk in table.ForeignKeys.ToList())
                {
                    var fkTable = table;
                    var pkTable = schema.Tables.SingleOrDefault(t => t.TableSchema == fk.PKTableSchema && t.TableName == fk.PKTableName);
                    if (pkTable == null)
                    {
                        Console.WriteLine($"Can't find table {fk.PKTableName}");
                        continue;
                    }
                    var reverseFk = pkTable.ChildForeignKeys.Single(rfk => rfk.ForeignKeyConstraintName == fk.ForeignKeyConstraintName);

                    SetForeignKeyPropertyNames(pkTable, fkTable, fk, reverseFk);
                }

            }

            #region foreach (var table in schema.Tables)
            foreach (var table in schema.Tables)
            {
                if (!ShouldProcessTable(table))
                    continue;
                string entityClassName = GetClassNameForTable(table);

                string tableFilePath = GetFileNameForTable(table);
                Console.WriteLine($"Generating {tableFilePath}...");
                using (var writer = _generatorContext[tableFilePath])
                {
                    writer.WriteLine(@"using System;");
                    writer.WriteLine(@"using System.Collections.Generic;");
                    if (withAttributes)
                    {
                        writer.WriteLine(@"using System.ComponentModel.DataAnnotations;");
                        writer.WriteLine(@"using System.ComponentModel.DataAnnotations.Schema;");
                    }
                    writer.WriteLine();
                    using (writer.WithCBlock($"namespace {Namespace}"))
                    {
                        if (withAttributes && table.TableSchema != "dbo") //TODO or table different than class name?
                            writer.WriteLine($"[Table(\"{table.TableName}\", Schema = \"{table.TableSchema}\")]");
                        else if (entityClassName.ToLower() != table.TableName.ToLower())
                            writer.WriteLine($"[Table(\"{table.TableName}\")]");
                        using (writer.WithCBlock($"public partial class {entityClassName}"))
                        {
                            if (table.ChildForeignKeys.Any())
                            {
                                using (writer.WithCBlock($"public {entityClassName}()"))
                                {
                                    foreach (var fk in table.ChildForeignKeys.OrderBy(fk => fk.NavigationPropertyName))
                                    {
                                        var fkTable = schema.Tables.Single(t => t.TableSchema == fk.FKTableSchema && t.TableName == fk.FKTableName);
                                        writer.WriteLine($"{fk.NavigationPropertyName} = new HashSet<{GetClassNameForTable(fkTable)}>();");
                                    }
                                }
                                writer.WriteLine();
                            }

                            var columns = table.Columns.Where(c => ShouldProcessColumn(table, c));
                            foreach (var column in columns)
                            {
                                string propertyName = GetPropertyNameForDatabaseColumn(table, column);
                                string clrType = GetTypeDefinitionForDatabaseColumn(table, column) ?? "";
                                if (withAttributes)
                                {
                                    if (column.IsPrimaryKeyMember)
                                        writer.WriteLine("[Key]");
                                    if (!column.IsNullable && clrType == "System.String" && !column.IsPrimaryKeyMember) // reference types are always nullable (no need "?"), so must specify this.
                                        writer.WriteLine($"[Required]");

                                    string typeName = null; // TODO: combine with identical block
                                    if (column.SqlDataType == "datetime" || column.SqlDataType == "smallmoney" || column.SqlDataType == "money" || column.SqlDataType == "xml")
                                        typeName = column.SqlDataType;
                                    else if (column.SqlDataType == "decimal")
                                        typeName = $"decimal({column.NumericPrecision}, {column.NumericScale})";

                                    if (column.ColumnName != propertyName && typeName != null)
                                        writer.WriteLine($"[Column(\"{column.ColumnName}\", TypeName = \"{typeName}\")]");
                                    else if (column.ColumnName != propertyName && typeName == null)
                                        writer.WriteLine($"[Column(\"{column.ColumnName}\")]");
                                    else if (column.ColumnName == propertyName && typeName != null)
                                        writer.WriteLine($"[Column(TypeName = \"{typeName}\")]");

                                    if (clrType == "System.String" && column.MaxLength != -1)
                                        writer.WriteLine($"[StringLength({column.MaxLength})]");
                                }

                                writer.WriteLine($"public {clrType} {propertyName} {{ get; set; }}");
                            }

                            if (table.ForeignKeys.Any() || table.ChildForeignKeys.Any())
                                writer.WriteLine();
                            foreach (var childToParentFK in table.ForeignKeys.OrderBy(fk => fk.NavigationPropertyName))
                            {
                                var fkTable = table;
                                var pkTable = schema.Tables.Single(t => t.TableSchema == childToParentFK.PKTableSchema && t.TableName == childToParentFK.PKTableName);
                                var parentToChildFK = pkTable.ChildForeignKeys.Single(fk => fk.ForeignKeyConstraintName == childToParentFK.ForeignKeyConstraintName);

                                var fkCol = childToParentFK.Columns.First().FKColumnName; //TODO: composite keys
                                Console.WriteLine($"{table.TableName}{fkCol}");
                                if (withAttributes)
                                {
                                    writer.WriteLine($"[ForeignKey(nameof({table.ColumnPropertyNames[fkCol]}))]");
                                    writer.WriteLine($"[InverseProperty(nameof({GetClassNameForTable(pkTable)}.{parentToChildFK.NavigationPropertyName}))]");
                                }
                                writer.WriteLine($"public virtual {GetClassNameForTable(pkTable)} {childToParentFK.NavigationPropertyName} {{ get; set; }}");
                            }
                            foreach (var parentToChildFK in table.ChildForeignKeys.OrderBy(fk => fk.NavigationPropertyName))
                            {
                                var pkTable = table;
                                var fkTable = schema.Tables.Single(t => t.TableSchema == parentToChildFK.FKTableSchema && t.TableName == parentToChildFK.FKTableName);
                                var childToParentFK = fkTable.ForeignKeys.Single(fk => fk.ForeignKeyConstraintName == parentToChildFK.ForeignKeyConstraintName);
                                var fkCol = parentToChildFK.Columns.First().FKColumnName; //TODO: composite keys
                                if (withAttributes)
                                {
                                    //writer.WriteLine($"[InverseProperty(nameof({GetClassNameForTable(fkTable)}.{fk.ReverseNavigationPropertyName}))]"); // some cases attribute is set by nameof?
                                    writer.WriteLine($"[InverseProperty(\"{childToParentFK.NavigationPropertyName}\")]"); // some cases attribute is set by nameof?
                                }
                                writer.WriteLine($"public virtual ICollection<{GetClassNameForTable(fkTable)}> {parentToChildFK.NavigationPropertyName} {{ get; set; }} ");
                            }
                        }
                    }
                }
            }
            #endregion

            #region DbContext
            using (var dbContextWriter = _generatorContext[ContextName + ".cs"])
            {
                dbContextWriter.WriteLine("using System;");
                dbContextWriter.WriteLine("using Microsoft.EntityFrameworkCore;");
                dbContextWriter.WriteLine("using Microsoft.EntityFrameworkCore.Metadata;");
                dbContextWriter.WriteLine("");
                using (dbContextWriter.WithCBlock($"namespace {Namespace}"))
                {
                    using (dbContextWriter.WithCBlock($"public partial class {ContextName} : DbContext"))
                    {
                        using (dbContextWriter.WithCBlock($"public {ContextName}()"))
                        {
                        }
                        dbContextWriter.WriteLine();

                        using (dbContextWriter.WithCBlock($"public {ContextName}(DbContextOptions<{ContextName}> options){Environment.NewLine}    : base(options)"))
                        {
                        }
                        dbContextWriter.WriteLine();

                        foreach (var table in schema.Tables.OrderBy(t => GetClassNameForTable(t)))
                        {
                            if (!ShouldProcessTable(table))
                                continue;
                            string entityClassName = GetClassNameForTable(table);

                            dbContextWriter.WriteLine($"public virtual DbSet<{entityClassName}> {entityClassName} {{ get; set; }}");
                        }

                        dbContextWriter.WriteLine();
                        dbContextWriter.WriteLine();
                        dbContextWriter.WriteLine();
                        using (dbContextWriter.WithCBlock($"protected override void OnModelCreating(ModelBuilder modelBuilder)"))
                        {
                            foreach (var table in schema.Tables.OrderBy(t => GetClassNameForTable(t)))
                            {

                                if (!ShouldProcessTable(table))
                                    continue;
                                string entityClassName = GetClassNameForTable(table);
                                using (dbContextWriter.WithIndent($"modelBuilder.Entity<{GetClassNameForTable(table)}>(entity =>{Environment.NewLine}{{{Environment.NewLine}", $"{Environment.NewLine}}});{Environment.NewLine}{Environment.NewLine}"))
                                {
                                    var pkCols = table.Columns.Where(c => c.IsPrimaryKeyMember);
                                    if (pkCols.Any() && !string.IsNullOrEmpty(table.PrimaryKeyName))
                                    {
                                        if (pkCols.Count() == 1)
                                            dbContextWriter.Write($"entity.HasKey(e => e.{GetPropertyNameForDatabaseColumn(table, pkCols.Single())})");
                                        else
                                            dbContextWriter.Write($"entity.HasKey(e => new {{ " +
                                                string.Join(", ", pkCols.Select(pk => $"e.{GetPropertyNameForDatabaseColumn(table, pk)}")) +
                                                $"}})");

                                        List<string> commands = new List<string>();
                                        dbContextWriter.Write($"{Environment.NewLine}    .HasName(\"{table.PrimaryKeyName}\")");
                                        //commands.Add($"    .HasName(\"{table.PrimaryKeyName}\");");
                                        if (!table.PrimaryKeyIsClustered)
                                            dbContextWriter.Write($"{Environment.NewLine}    .IsClustered(false)");
                                        //commands.Add($"    .IsClustered(false);");
                                        //Extensions.WriteChainedMethods(dbContextWriter, commands); // dbContextWriter.WriteChainedMethods(commands); - CSX doesn't allow extensions
                                        dbContextWriter.WriteLine($";{Environment.NewLine}");
                                    }
                                    else
                                        dbContextWriter.WriteLine($"entity.HasNoKey();{Environment.NewLine}");

                                    if (!withAttributes)
                                        dbContextWriter.WriteLine($"entity.ToTable(\"{table.TableName}\", \"{table.TableSchema}\");{Environment.NewLine}");

                                    if (!string.IsNullOrEmpty(table.TableDescription))
                                    {
                                        dbContextWriter.WriteLine($"entity.HasComment(\"{table.TableDescription.Replace("\"", "\\\"")}\");");
                                        dbContextWriter.WriteLine();
                                    }

                                    dbContextWriter.Write($"{null}");

                                    foreach (var index in table.Indexes
                                        .Where(i => i.PhysicalType == "CLUSTERED" || i.PhysicalType == "NONCLUSTERED")
                                        .Where(i => i.LogicalType != "PRIMARY_KEY")
                                        .Where(i => i.Columns.Any())
                                        .OrderBy(i => GetPropertyNameForDatabaseColumn(table, i.Columns.First().ColumnName))
                                        )
                                    {
                                        dbContextWriter.Write($"entity.HasIndex(e => e.{GetPropertyNameForDatabaseColumn(table, index.Columns.First().ColumnName)})");
                                        dbContextWriter.Write($"{Environment.NewLine}    .HasName(\"{index.IndexName}\")");
                                        if (index.LogicalType == "UNIQUE_INDEX" || index.LogicalType == "UNIQUE_CONSTRAINT")
                                            dbContextWriter.Write($"{Environment.NewLine}    .IsUnique()");
                                        dbContextWriter.WriteLine($";{Environment.NewLine}");
                                    }

                                    foreach (var column in table.Columns
                                        .OrderBy(c => c.IsPrimaryKeyMember ? 0 : 1)
                                        .ThenBy(c => c.IsPrimaryKeyMember ? c.OrdinalPosition : 0) // respect PK order... 
                                        .ThenBy(c => GetPropertyNameForDatabaseColumn(table, c)) // but for other columns do alphabetically
                                        )
                                    {
                                        dbContextWriter.Write($"entity.Property(e => e.{GetPropertyNameForDatabaseColumn(table, column)})");
                                        if (!withAttributes && column.ColumnName != GetPropertyNameForDatabaseColumn(table, column))
                                            dbContextWriter.Write($"{Environment.NewLine}    .HasColumnName(\"{column.ColumnName}\")");

                                        string typeName = null; // TODO: combine with identical block
                                        if (column.SqlDataType == "datetime" || column.SqlDataType == "smallmoney" || column.SqlDataType == "money" || column.SqlDataType == "xml")
                                            typeName = column.SqlDataType;
                                        else if (column.SqlDataType == "decimal")
                                            typeName = $"decimal({column.NumericPrecision}, {column.NumericScale})";
                                        if (typeName != null)
                                            dbContextWriter.Write($"{Environment.NewLine}    .HasColumnType(\"{typeName}\")");

                                        string defaultSetting = column.DefaultSetting;
                                        if (!string.IsNullOrEmpty(defaultSetting))
                                        {
                                            try
                                            {
                                                Type clrType = Type.GetType(column.ClrType);
                                                if ((clrType == typeof(int) ||
                                                    clrType == typeof(decimal) ||
                                                    clrType == typeof(byte) ||
                                                    clrType == typeof(float) ||
                                                    clrType == typeof(long) ||
                                                    clrType == typeof(double) ||
                                                    clrType == typeof(short) ||
                                                    clrType == typeof(uint) ||
                                                    clrType == typeof(ulong)
                                                    ) && !column.IsNullable && defaultSetting == "((0))")
                                                    defaultSetting = null;
                                                //TODO: object def = GetDefault(clrType);

                                            }
                                            catch (Exception ex)
                                            {
                                            }
                                        }
                                        if (defaultSetting != null) //TODO: non-nullable numerics will have default 0, so ((0)) can be ignored. etc.
                                            dbContextWriter.Write($"{Environment.NewLine}    .HasDefaultValueSql(\"{defaultSetting}\")");

                                        if (!column.IsNullable && column.ClrType == "System.String" && !column.IsPrimaryKeyMember) // reference types are always nullable (no need "?"), so must specify this.
                                            dbContextWriter.Write($"{Environment.NewLine}    .IsRequired()");
                                        if (column.ClrType == "System.String" && column.MaxLength != -1)
                                            dbContextWriter.Write($"{Environment.NewLine}    .HasMaxLength({column.MaxLength})");
                                        if (column.SqlDataType == "char" || column.SqlDataType == "nchar")
                                            dbContextWriter.Write($"{Environment.NewLine}    .IsFixedLength()");

                                        if (!string.IsNullOrEmpty(column.ColumnDescription))
                                            dbContextWriter.Write($"{Environment.NewLine}    .HasComment(\"{column.ColumnDescription.Replace("\"", "\\\"")}\")");
                                        dbContextWriter.WriteLine($";{Environment.NewLine}");
                                        /*
                                        bool hasLineBreaks = false;

                                        if (!string.IsNullOrEmpty(c.DefaultSetting))
                                        {
                                            if (!hasLineBreaks) { dbContextWriter.IncreaseIndent(); hasLineBreaks = true; }
                                            dbContextWriter.WriteLine($"{Environment.NewLine}.HasDefaultValueSql(\"{c.DefaultSetting}\")");
                                        }
                                        if (c.SqlDataType == "char" || c.SqlDataType == "nchar")
                                        {
                                            if (!hasLineBreaks) { dbContextWriter.IncreaseIndent(); hasLineBreaks = true; }
                                            dbContextWriter.WriteLine($"{Environment.NewLine}.IsFixedLength()");
                                        }

                                        if (!string.IsNullOrEmpty(c.ColumnDescription))
                                            dbContextWriter.WriteLine($".HasComment(\"{c.ColumnDescription.Replace("\"", "\\\"")}\");");

                                        if (hasLineBreaks)
                                            dbContextWriter.DecreaseIndent();
                                        dbContextWriter.WriteLine();
                                        */
                                    }
                                    foreach (var childToParentFK in table.ForeignKeys
                                            .OrderBy(fk => fk.NavigationPropertyName)
                                            )
                                    {
                                        var fkTable = table;
                                        var pkTable = schema.Tables.Single(t => t.TableSchema == childToParentFK.PKTableSchema && t.TableName == childToParentFK.PKTableName);
                                        var parentToChildFK = pkTable.ChildForeignKeys.Single(fk => fk.ForeignKeyConstraintName == childToParentFK.ForeignKeyConstraintName);

                                        var fkCol = fkTable.Columns.Single(c => c.ColumnName == childToParentFK.Columns.First().FKColumnName); //TODO: composite keys

                                        dbContextWriter.Write($"{Environment.NewLine}entity.HasOne(d => d.{childToParentFK.NavigationPropertyName})");
                                        using (dbContextWriter.WithIndent())
                                        {
                                            dbContextWriter.Write($"{Environment.NewLine}.WithMany(p => p.{parentToChildFK.NavigationPropertyName})");
                                            dbContextWriter.Write($"{Environment.NewLine}.HasForeignKey(d => d.{GetPropertyNameForDatabaseColumn(fkTable, fkCol)})");

                                            // NO_ACTION seems like a bug in ef dbcontext scaffold when we use -d (annotations) ?
                                            if (parentToChildFK.OnDeleteCascade == "SET_NULL" || (withAttributes && parentToChildFK.OnDeleteCascade == "NO_ACTION"))
                                                dbContextWriter.Write($"{Environment.NewLine}.OnDelete(DeleteBehavior.ClientSetNull)");

                                            dbContextWriter.WriteLine($";");
                                        }
                                    }
                                }
                            }

                        }

                    }
                }
            }
            #endregion

            // since no errors happened, let's save all files
            _generatorContext.SaveFiles(outputFolder: TargetFolder);

            Console.WriteLine("Success!");
        }

        string GetFileNameForTable(Table table)
        {
            return $"{table.TableName}.cs";
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
            //if (table.TableName.StartsWith("CK_")) // check constraints
            //    return false;
            //if (table.TableSchema == "audit")
            //    return false;
            //if (table.TableName.StartsWith("Audit_"))
            //    return false;
            return true;
        }

        bool ShouldProcessColumn(Table table, Column column)
        {
            string sqlDataType = column.SqlDataType;
            switch (sqlDataType)
            {
                case "hierarchyid":
                case "geography":
                    return true; // some databases may not allow these types
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

        string GetTypeDefinitionForDatabaseColumn(Table table, Column column)
        {
            string typeName;
            bool isReferenceType;
            try
            {
                System.Type type = Type.GetType(column.ClrType);

                isReferenceType = !type.IsValueType;

                // Prefer shorter type aliases (int instead of Int32, long instead of Int64, string instead of String)
                if (_typeAlias.TryGetValue(type, out string alias))
                    typeName = alias;
                else if (type.IsArray && _typeAlias.TryGetValue(type.GetElementType(), out string alias2))
                    typeName = alias2 + "[]";
                else
                    typeName = type.Name;
            }
            catch (Exception ex)
            {
                // some types are vendor-specific and may require specific DLLs (e.g. Microsoft.SqlServer.Types.SqlGeography)
                Console.WriteLine($"Warning - unknown Type {column.ClrType} - you may need to add some reference to your project");
                typeName = column.ClrType;
                isReferenceType = true; // non-standard types are probably reference types
            }

            bool isNullable = column.IsNullable;

            // Many developers use POCO instances with null Primary Key to represent a new (in-memory) object, so we can force PKs as nullable
            //if (column.IsPrimaryKeyMember)
            //    isNullable = true;

            // Reference-types (including strings) are always nullable, no need to specify nullable modifier
            if (isNullable && !isReferenceType)
                return $"{typeName}?"; // some might prefer $"System.Nullable<{typeName}>"

            return typeName;
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

        string GetPropertyNameForDatabaseColumn(Table table, Column column)
        {
            return GetPropertyNameForDatabaseColumn(table, column.ColumnName);
        }

        /// <summary>
        /// Gets a unique identifier name for the column, which doesn't conflict with the Entity class itself or with previous identifiers for this Entity.
        /// </summary>
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

        /// <summary>
        /// Sets unique names for NavigationProperty (from child fkTable to parent pkTable) and ReverseNavigationProperty (from parent pkTable to child fkTable)
        /// Name should not conflict with the Entity class itself or with previous identifiers for this Entity.
        /// </summary>
        /// <returns></returns>
        void SetForeignKeyPropertyNames(Table pkTable, Table fkTable, ForeignKey fk, ForeignKey reverseFk)
        {
            string navigationPropertyName = fk.Columns.First().FKColumnName; //TODO: composite keys?
            if (navigationPropertyName.ToUpper().EndsWith("ID"))
                navigationPropertyName = navigationPropertyName.Substring(0, navigationPropertyName.Length - 2);

            // check for name clashes
            int n = 0;
            string attemptName = navigationPropertyName;
            while ((GetClassNameForTable(fkTable) == attemptName || fkTable.ColumnPropertyNames.ContainsValue(attemptName) || fkTable.FKPropertyNames.ContainsValue(attemptName) || fkTable.ReverseFKPropertyNames.ContainsValue(attemptName)) && n < 100)
            {
                n++;
                attemptName = navigationPropertyName + n.ToString();
            }
            fk.NavigationPropertyName = attemptName;
            fkTable.FKPropertyNames[fk.ForeignKeyConstraintName] = attemptName;

            string reverseNavigationPropertyName = GetClassNameForTable(fkTable);// + navigationPropertyName;
            attemptName = reverseNavigationPropertyName;
            while ((GetClassNameForTable(pkTable) == attemptName || pkTable.ColumnPropertyNames.ContainsValue(attemptName) || pkTable.FKPropertyNames.ContainsValue(attemptName) || pkTable.ReverseFKPropertyNames.ContainsValue(attemptName)) && n < 100)
            {
                n++;
                attemptName = reverseNavigationPropertyName + n.ToString();
            }
            reverseFk.NavigationPropertyName = attemptName;
            pkTable.FKPropertyNames[fk.ForeignKeyConstraintName] = attemptName;
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        public static T Map<T, S>(S source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

    }
#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
}
#endif