using CodegenCS;
using CodegenCS.Runtime;
using CodegenCS.Models.DbSchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static CodegenCS.Symbols;
using static InterpolatedColorConsole.Symbols;
using System.CommandLine.Binding;
using System.CommandLine;

/// <summary>
/// DapperExtensionPocos.cs: Given a Database Schema will Generate POCOs with extensions-methods for Insert and Update using Dapper
///
/// Usage: dotnet-codegencs template run DapperExtensionPocos.cs <DbSchema.json> <Namespace> [-p:SingleFile] [-p:AddTableAttribute <true/false>] [-p:AddKeyAttribute <true/false>] [-p:AddKeyAttribute <true/false>] [-p:GenerateEqualsHashCode <true/false>] [-p:TrackPropertiesChange <true/false>]
/// e.g.:  dotnet-codegencs template run DapperExtensionPocos.cs AdventureWorksSchema.json MyPOCOs -db false
///
/// Arguments:
///  <Namespace>  Namespace of generated POCOs
///
/// Options:
///   --SingleFile                           If set all POCOs will be generated under a single filename
///                                          (default output file)
///   -t, --AddTableAttribute                If true will add [Table] attributes to POCOs. [default: True]
///   -k, --AddKeyAttribute                  If true will add [Key] attributes to primary-key columns.
///                                          This is required by FastCRUD and Entity Framework [default: True]
///   -db, --AddDatabaseGeneratedAttribute   If true will add [DatabaseGenerated] attributes to identity
///                                          and computed columns.
///                                          This is required by FastCRUD and Entity Framework [default: True]
///   -eq, --GenerateEqualsHashCode          If true POCOs will have override Equals/GetHashCode and
///                                          equality/inequality operators (== and !=) [default: True]
/// </summary>
public class DapperPOCOGenerator : ICodegenMultifileTemplate<DatabaseSchema>
{
    private ICodegenContext _generatorContext;
    private ILogger _logger;
    private bool _allTablesInSameSchema;
    private bool _duplicatedTableNames;
    private static Dictionary<Table, Dictionary<string, string>> _tablePropertyNames;
    private DapperPOCOGeneratorOptions _options;

    public DapperPOCOGenerator(ILogger logger, DapperPOCOGeneratorOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public static void ConfigureCommand(Command command)
    {
        command.AddArgument(new Argument<string>("Namespace", "Namespace of generated POCOs") { Arity = ArgumentArity.ExactlyOne });
        command.AddOption(new Option<bool>("-p:SingleFile") { Description = "If defined, all POCOs will be generated under a single filename (default output file)" });
        command.AddOption(new Option<bool>("-p:AddTableAttribute", getDefaultValue: () => true) { Description = "If true will add [Table] attributes to POCOs." });
        command.AddOption(new Option<bool>("-p:AddKeyAttribute", getDefaultValue: () => true) { Description = "If true will add [Key] attributes to primary-key columns.\nThis is required by FastCRUD and Entity Framework" });
        command.AddOption(new Option<bool>("-p:AddDatabaseGeneratedAttribute", getDefaultValue: () => true) { Description = "If true will add [DatabaseGenerated] attributes to identity and computed columns.\nThis is required by FastCRUD and Entity Framework" });
        command.AddOption(new Option<bool>("-p:GenerateEqualsHashCode", getDefaultValue: () => true) { Description = "If true POCOs will have override Equals/GetHashCode and equality/inequality operators (== and !=)" });
        command.AddOption(new Option<bool>("-p:TrackPropertiesChange", getDefaultValue: () => false) { Description = "If true POCOs will implement INotifyPropertyChanged (PropertyChanged event), and will expose a HashSet of \"Dirty\" properties and bool IsDirty" });
        command.AddOption(new Option<string>("-p:CrudClass") { Arity = ArgumentArity.ExactlyOne, Description = "Class name for CRUD extensions" });
        command.AddOption(new Option<string>("-p:CrudFile") { Arity = ArgumentArity.ExactlyOne, Description = "File name for CRUD extensions" });
        command.AddOption(new Option<string>("-p:CrudNamespace") { Arity = ArgumentArity.ExactlyOne, Description = "Namespace for CRUD extensions" });
    }


    #region DapperPOCOGeneratorOptions
    public class DapperPOCOGeneratorOptions : IAutoBindCommandLineArgs
    {
        /// <summary>
        /// Namespace of generated POCOs
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Class name for CRUD Extensions
        /// </summary>
        public string CrudClass { get; set; } = "CRUDExtensions";

        /// <summary>
        /// File for CRUD Extensions
        /// </summary>
        public string CrudFile { get; set; } = "CRUDExtensions.cs";

        /// <summary>
        /// Namespace for CRUD Extensions. If not defined will be the same as POCOs
        /// </summary>
        public string CrudNamespace { get; set; } = null;
        

        /// <summary>
        /// If set all POCOs will be generated under a single filename (default output file)
        /// </summary>
        public bool SingleFile { get; set; } = false;

        /// <summary>
        /// If true (default is true) will add [Table] attributes to POCOs.
        /// </summary>
        public bool AddTableAttribute { get; set; } = true;

        /// <summary>
        /// If true (default is true) will add [Key] attributes to primary-key columns.
        /// This is required by FastCRUD and Entity Framework
        /// </summary>
        public bool AddKeyAttribute { get; set; } = true;

        /// <summary>
        /// If true (default is true) will add [DatabaseGenerated] attributes to identity and computed columns.
        /// This is required by FastCRUD and Entity Framework
        /// </summary>
        public bool AddDatabaseGeneratedAttribute { get; set; } = true;

        /// <summary>
        /// If true (default is true) POCOs will have override Equals/GetHashCode and equality/inequality operators (== and !=)
        /// </summary>
        public bool GenerateEqualsHashCode { get; set; } = true;
        
        /// <summary>
        /// If true (default is false), POCOs will implement INotifyPropertyChanged (PropertyChanged event), and will expose a HashSet of "Dirty" properties and bool IsDirty
        /// </summary>
        public bool TrackPropertiesChange { get; set; } = false;
    }
    #endregion /DapperPOCOGeneratorOptions


    public void Render(ICodegenContext context, DatabaseSchema schema)
    {
        _generatorContext = context;
        _tablePropertyNames = new Dictionary<Table, Dictionary<string, string>>();
        _allTablesInSameSchema = schema.Tables.Select(t => t.TableSchema).Distinct().Count() == 1;
        _duplicatedTableNames = schema.Tables.Select(t => t.TableName).GroupBy(name => name).Where(g => g.Count() > 1).Any();
        if (_duplicatedTableNames)
            _logger.WriteLineAsync(ConsoleColor.Yellow, $"Warning: There are tables with same name (in different schemas?), class names will contain schema...");
        GeneratePOCOs(schema);
        _logger.WriteLineAsync($"Generating CRUD Extensions {ConsoleColor.Yellow}'{_options.CrudFile}'{PREVIOUS_COLOR}...");
        GenerateCRUD(_generatorContext[_options.CrudFile], schema);
    }


    /// <summary>
    /// Generates POCOS
    /// </summary>
    public void GeneratePOCOs(DatabaseSchema schema)
    {
        var tablesAndViews = schema.Tables
            .Where(t => ShouldProcessTable(t))
            .OrderBy(t => GetClassNameForTable(t));

        if (_options.SingleFile)
        {
            var singleFile = _generatorContext.DefaultOutputFile;
            singleFile.WriteLine($$"""
            //------------------------------------------------------------------------------
            // <auto-generated>
            //     This code was generated by dotnet-codegencs tool.
            //     Changes to this file may cause incorrect behavior and will be lost if
            //     the code is regenerated.
            // </auto-generated>
            //------------------------------------------------------------------------------
            using System;
            using System.Collections.Generic;
            using System.ComponentModel.DataAnnotations;
            using System.ComponentModel.DataAnnotations.Schema;
            using System.Linq;{{IF(_options.TrackPropertiesChange)}}
            using System.ComponentModel;{{ENDIF}}

            namespace {{_options.Namespace}}
            {
                {{tablesAndViews.Render(table => GeneratePOCO(singleFile, table))}}
            }
            """);
        }
        else
        {
            foreach (var table in tablesAndViews)
            {
                var pocoFile = _generatorContext[GetFileNameForTable(table)];
                GeneratePOCO(pocoFile, table);
            }
        }
    }

    private void GeneratePOCO(ICodegenOutputFile file, Table table)
    {
        if (!_options.SingleFile)
        {
            _logger.WriteLineAsync($"Generating POCO for {ConsoleColor.Yellow}'{table.TableName}'{PREVIOUS_COLOR}...");

            file.WriteLine($$"""
                //------------------------------------------------------------------------------
                // <auto-generated>
                //     This code was generated by dotnet-codegencs tool.
                //     Changes to this file may cause incorrect behavior and will be lost if
                //     the code is regenerated.
                // </auto-generated>
                //------------------------------------------------------------------------------
                using System;
                using System.Collections.Generic;
                using System.ComponentModel.DataAnnotations;
                using System.ComponentModel.DataAnnotations.Schema;
                using System.Linq;{{IF(_options.TrackPropertiesChange)}}
                using System.ComponentModel;{{ENDIF}}

                namespace {{_options.Namespace}}
                {
                    {{() => GeneratePOCOClass(file, table)}}
                }
                """);
        }
        else
        {
            _logger.WriteLineAsync($"Generating POCO for {ConsoleColor.Yellow}{table.TableName} ('{file.RelativePath}'){PREVIOUS_COLOR}...");
            file.WriteLine($$"""{{() => GeneratePOCOClass(file, table)}}""");
        }
    }

    private void GeneratePOCOClass(ICodegenOutputFile file, Table table)
    {
        string entityClassName = GetClassNameForTable(table);

        if (_options.AddTableAttribute)
        {
            // We'll decorate [Table("Name")] only if schema not default or if table name doesn't match entity name
            if (table.TableSchema != "dbo") //TODO or table different than clas name?
                file.WriteLine($"[Table(\"{table.TableName}\", Schema = \"{table.TableSchema}\")]");
            else if (entityClassName.ToLower() != table.TableName.ToLower())
                file.WriteLine($"[Table(\"{table.TableName}\")]");
        }

        List<string> baseClasses = new List<string>();
        if (_options.TrackPropertiesChange)
            baseClasses.Add("INotifyPropertyChanged");

        var columns = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .OrderBy(c => c.IsPrimaryKeyMember ? 0 : 1)
            .ThenBy(c => c.IsPrimaryKeyMember ? c.OrdinalPosition : 0) // respect PK order... 
            .ThenBy(c => table.TableType == "VIEW" ? c.OrdinalPosition : 0) // for views respect the columns order
            .ThenBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName)); // but for other columns do alphabetically;


        file.WithCBlock($"public partial class {entityClassName}{(baseClasses.Any() ? " : " + string.Join(", ", baseClasses) : "")}", () =>
        {
            file.WriteLine($$"""
                #region Members
                {{columns.Render(column => GenerateProperty(file, table, column))}}
                #endregion Members
                """);

            if (_options.GenerateEqualsHashCode)
            {
                file.WriteLine($$"""

                #region Equals/GetHashCode
                {{GenerateEquals(table)}}
                {{GenerateGetHashCode(table)}}
                {{GenerateInequalityOperatorOverloads(table)}}
                #endregion Equals/GetHashCode
                """);
            }

            if (_options.TrackPropertiesChange)
            {
                file.WriteLine($$"""

                #region INotifyPropertyChanged/IsDirty
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
                }
                #endregion INotifyPropertyChanged/IsDirty
                """);
            }
        });
    }

    private void GenerateProperty(ICodegenOutputFile writer, Table table, Column column)
    {
        string propertyName = GetPropertyNameForDatabaseColumn(table, column.ColumnName);
        string privateVariable = $"_{propertyName.Substring(0, 1).ToLower()}{propertyName.Substring(1)}";

        if (_options.TrackPropertiesChange)
            writer.WriteLine($"private {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {privateVariable};");
        if (column.IsPrimaryKeyMember && _options.AddKeyAttribute)
            writer.WriteLine("[Key]");
        if (column.IsIdentity && _options.AddDatabaseGeneratedAttribute)
            writer.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
        else if (column.IsComputed && _options.AddDatabaseGeneratedAttribute)
            writer.WriteLine("[DatabaseGenerated(DatabaseGeneratedOption.Computed)]");

        // We'll decorate [Column("Name")] only if column name doesn't match property name
        if (propertyName.ToLower() != column.ColumnName.ToLower())
            writer.WriteLine($"[Column(\"{column.ColumnName}\")]");
        if (_options.TrackPropertiesChange)
            writer.Write($$"""
                public {{GetTypeDefinitionForDatabaseColumn(table, column) ?? ""}} {{propertyName}}
                { 
                    get { return {{privateVariable}}; }
                    set { SetField(ref {{ privateVariable}}, value, nameof({{propertyName}})); }
                }
                """);
        else
            writer.Write($"public {GetTypeDefinitionForDatabaseColumn(table, column) ?? ""} {propertyName} {{ get; set; }}");
    }

    private FormattableString GenerateEquals(Table table)
    {
        //TODO: GenerateIEquatable, which is a little faster for Generic collections - and our Equals(object other) can reuse this IEquatable<T>.Equals(T other) 

        string entityClassName = GetClassNameForTable(table);
        var cols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => !c.IsIdentity)
            .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName))
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName) });

        return $$"""
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                {{entityClassName}} other = obj as {{entityClassName}};
                if (other == null) return false;

                {{cols.Select(col => $$"""
                    if ({{col.PropertyName}} != other.{{col.PropertyName}})
                        return false;
                    """).Render(RenderEnumerableOptions.LineBreaksWithoutSpacer)}}
                return true;
            }
            """;
    }
    
    private FormattableString GenerateGetHashCode(Table table)
    {
        var cols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => !c.IsIdentity)
            .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName))
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName), DefaultTypeValue = GetDefaultValue(GetTypeForDatabaseColumn(table, c)) });

        //TODO: for dotnetcore we can use HashCode.Combine(field1, field2, field3)
        return $$"""
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    {{cols.Select(col => $$"""hash = hash * 23 + ({{col.PropertyName}} == {{col.DefaultTypeValue}} ? 0 : {{col.PropertyName}}.GetHashCode());""")}}
                    return hash;
                }
            }
            """;
    }
    
    private FormattableString GenerateInequalityOperatorOverloads(Table table)
    {
        string entityClassName = GetClassNameForTable(table);
        return $$"""
            public static bool operator ==({{entityClassName}} left, {{entityClassName}} right)
            {
                return Equals(left, right);
            }

            public static bool operator !=({{entityClassName}} left, {{entityClassName}} right)
            {
                return !Equals(left, right);
            }
            """;
    }

    private string GetFileNameForTable(Table table)
    {
        //return $"{table.TableName}.generated.cs";
        // if default schema or all tables under a single schema then just omit the schema:
        if (table.TableSchema == "dbo" || _allTablesInSameSchema)
            return $"{table.TableName}.generated.cs";
        else
            return $"{table.TableSchema}.{table.TableName}.generated.cs";
    }
    private static string GetClassNameForTable(Table table)
    {
        return $"{table.TableName}";
        if (table.TableSchema == "dbo")
            return $"{table.TableName}";
        else
            return $"{table.TableSchema}_{table.TableName}";
    }
    private bool ShouldProcessTable(Table table)
    {
    	// Convention: "CHECK views" are just for integrity constraints no need to map
        if (table.TableType == "VIEW" && table.TableName.StartsWith("CK_"))
            return false;
        // You may or may not generate POCOs for your views
        if (table.TableType == "VIEW")
            return false;
        if (table.TableName == "sysdiagrams")
            return false;
        return true;
    }
    private static bool ShouldProcessColumn(Table table, Column column)
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

    private static Type GetTypeForDatabaseColumn(Table table, Column column)
    {
        System.Type type;
        try
        {
            type = Type.GetType(column.ClrType);
        }
        catch (Exception ex)
        {
            return null; // ignore vendor specific types that DbSchema doesn't recognize
        }

        bool isNullable = column.IsNullable;

        // Some developers use POCO instances with null Primary Key to represent a new (in-memory) object, so they prefer to set POCO PKs as Nullable 
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
        //if (column == null)
        //    return null; // IF/IIF symbols will evaluate both TRUE and FALSE statements, but this won't get rendered
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
    private static string GetPropertyNameForDatabaseColumn(Table table, string columnName)
    {
        if (columnName == null)
            return null;

        if (_tablePropertyNames.ContainsKey(table) && _tablePropertyNames[table].ContainsKey(columnName))
            return _tablePropertyNames[table][columnName];

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
        if (!_tablePropertyNames.ContainsKey(table))
            _tablePropertyNames[table] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        int n = 0;
        string attemptName = name;
        while ((GetClassNameForTable(table) == attemptName || _tablePropertyNames[table].ContainsValue((attemptName))) && n < 100)
        {
            n++;
            attemptName = name + n.ToString();
        }
        _tablePropertyNames[table].Add(columnName, attemptName);

        return attemptName;
    }

    // Splits both camelCaseWords and also TitleCaseWords. Underscores and dashes are also splitted. Uppercase acronyms are also splitted.
    // E.g. "BusinessEntityID" becomes ["Business","Entity","ID"]
    // E.g. "Employee_SSN" becomes ["employee","_","SSN"]
    private static Regex splitUpperCase = new Regex(@"
        (?<=[A-Z])(?=[A-Z][a-z0-9]) |
            (?<=[^A-Z])(?=[A-Z]) |
            (?<=[A-Za-z0-9])(?=[^A-Za-z0-9])", RegexOptions.IgnorePatternWhitespace);


    private void GenerateCRUD(ICodegenOutputFile file, DatabaseSchema schema)
    {
        var tables = schema.Tables
            .Where(t => ShouldProcessTable(t))
            .Where(t => t.TableType == "TABLE" && t.Columns.Any(c => c.IsPrimaryKeyMember))
            .OrderBy(t => GetClassNameForTable(t));

        var renderTable = (Table table) => (FormattableString)$$"""
                #region {{GetClassNameForTable(table)}}
                {{GenerateCrudSave.WithArguments(table, null)}}
                {{GenerateCrudInsert(table)}}
                {{GenerateCrudUpdate.WithArguments(table, _options, null)}}
                {{GenerateCrudDelete(table)}}
                #endregion {{GetClassNameForTable(table)}}
                """;

        file.WriteLine($$"""
            //------------------------------------------------------------------------------
            // <auto-generated>
            //     This code was generated by dotnet-codegencs tool.
            //     Changes to this file may cause incorrect behavior and will be lost if
            //     the code is regenerated.
            // </auto-generated>
            //------------------------------------------------------------------------------
            using Dapper;
            using System;
            using System.Collections.Generic;
            using System.Data;
            using System.Linq;
            using System.Runtime.CompilerServices;{{IF(_options.CrudNamespace != null && _options.CrudNamespace != _options.Namespace)}}
            using {{_options.Namespace}};{{ENDIF}}

            namespace {{_options.CrudNamespace ?? _options.Namespace}}
            {
                /// <summary>
                /// CRUD static extensions using Dapper (using static SQL statements)
                /// </summary>
                public static class {{_options.CrudClass}}
                {
                    {{tables.Select(table => renderTable(table)).Render()}}
                }
            }
            """);
    }

    private Action<Table, ICodegenTextWriter> GenerateCrudSave = (table, w) =>
    {
        var pkCols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => c.IsPrimaryKeyMember).OrderBy(c => c.OrdinalPosition)
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName), DefaultTypeValue = GetDefaultValue(GetTypeForDatabaseColumn(table, c)) });
        
        if (!pkCols.Any())
            return;
            
        var pkHasNoValue = string.Join(" && ", pkCols.Select(col => "e." + col.PropertyName + " == " + col.DefaultTypeValue));

        w.Write($$"""
            /// <summary>
            /// Saves (if new) or Updates (if existing)
            /// </summary>
            public static void Save{{GetClassNameForTable(table)}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
            {
                if ({{pkHasNoValue}})
                    conn.Insert{{GetClassNameForTable(table)}}(e, transaction, commandTimeout);
                else
                    conn.Update{{GetClassNameForTable(table)}}(e, transaction, commandTimeout);
            }
            """);

        foreach (var uniqueIndex in table.Indexes.Where(i => i.IsUnique==true && i.IsPrimaryKey == false))
        {
            pkCols = uniqueIndex
                .Columns
                .OrderBy(col => col.IndexOrdinalPosition)
                .Select(col => col.ColumnName)
                .Select(colName => table.Columns.Single(c => c.ColumnName == colName))
                .Where(c => ShouldProcessColumn(table, c))
                .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName), DefaultTypeValue = GetDefaultValue(GetTypeForDatabaseColumn(table, c)) });
            var colNames = string.Join("_", pkCols.Select(col => col.ColumnName));
            pkHasNoValue = string.Join(" && ", pkCols.Select(col => "e." + col.PropertyName + " == " + col.DefaultTypeValue));

            w.Write($$"""


                /// <summary>
                /// Saves (if new) or Updates (if existing)
                /// </summary>
                public static void Save{{GetClassNameForTable(table)}}_by_{{colNames}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
                {
                    if ({{pkHasNoValue}})
                        conn.Insert{{GetClassNameForTable(table)}}(e, transaction, commandTimeout);
                    else
                        conn.Update{{GetClassNameForTable(table)}}_by_{{colNames}}(e, transaction, commandTimeout);
                }
                """);
        }
    };

    private FormattableString GenerateCrudInsert(Table table)
    {
        var cols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
            .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
            .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName));
        
        if (!cols.Any())
            return $"";

        var identityCol = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => c.IsIdentity).FirstOrDefault();

        return $$"""
            /// <summary>
            /// Saves new record
            /// </summary>
            public static void Insert{{GetClassNameForTable(table)}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
            {
                string cmd = @"
                    INSERT INTO {{(table.TableSchema == "dbo" ? $"[{table.TableName}]" : $"[{table.TableSchema}].[{table.TableName}]")}}
                    (
                        {{cols.Select(col => "[" + col.ColumnName + "]").Render(RenderEnumerableOptions.MultiLineCSV)}}
                    )
                    VALUES
                    (
                        {{cols.Select(col => "@" + GetPropertyNameForDatabaseColumn(table, col.ColumnName)).Render(RenderEnumerableOptions.MultiLineCSV)}}
                    )";
                {{IIF(identityCol != null,
                        () => $$"""e.{{() => GetPropertyNameForDatabaseColumn(table, identityCol?.ColumnName)}} = conn.Query<{{GetTypeDefinitionForDatabaseColumn(table, identityCol)}}>(cmd + "SELECT SCOPE_IDENTITY();", e, transaction, commandTimeout: commandTimeout).Single();""",
                        () => $$"""conn.Execute(cmd, e, transaction, commandTimeout);"""
                )}}{{IF(_options.TrackPropertiesChange)}}

                e.MarkAsClean();{{ENDIF}}
            }
            """;
    }

    private Action<Table, DapperPOCOGeneratorOptions, ICodegenTextWriter> GenerateCrudUpdate = (table, _options, w) =>
    {
        var cols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => !c.IsIdentity)
            .Where(c => !c.IsRowGuid) //TODO: should be used only if they have value set (not default value)
            .Where(c => !c.IsComputed) //TODO: should be used only if they have value set (not default value)
            .OrderBy(c => GetPropertyNameForDatabaseColumn(table, c.ColumnName))
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName) });

        var pkCols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => c.IsPrimaryKeyMember)
            .OrderBy(c => c.OrdinalPosition)
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName) });

        if (!cols.Any() || !pkCols.Any())
            return;

        w.Write($$"""
            /// <summary>
            /// Updates existing record
            /// </summary>
            public static void Update{{GetClassNameForTable(table)}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
            {
                string cmd = @"
                    UPDATE {{(table.TableSchema == "dbo" ? $"[{table.TableName}]" : $"[{table.TableSchema}].[{table.TableName}]")}} SET
                        {{cols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.MultiLineCSV)}}
                    WHERE
                        {{pkCols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.CreateWithCustomSeparator(" AND\n", false))}}";
                conn.Execute(cmd, e, transaction, commandTimeout);{{IF(_options.TrackPropertiesChange)}}
            
                e.MarkAsClean();{{ENDIF}}
            }
            """);

        foreach (var uniqueIndex in table.Indexes.Where(i => i.IsUnique == true && i.IsPrimaryKey == false))
        {
            pkCols = uniqueIndex
                .Columns.OrderBy(col => col.IndexOrdinalPosition)
                .Select(col => col.ColumnName)
                .Select(colName => table.Columns.Single(c => c.ColumnName == colName))
                .Where(c => ShouldProcessColumn(table, c))
                .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName) });
            var colNames = string.Join("_", pkCols.Select(col => col.ColumnName));

            w.Write($$"""


                /// <summary>
                /// Updates existing record
                /// </summary>
                public static void Update{{GetClassNameForTable(table)}}_by_{{colNames}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
                {
                    string cmd = @"
                        UPDATE {{(table.TableSchema == "dbo" ? $"[{table.TableName}]" : $"[{table.TableSchema}].[{table.TableName}]")}} SET
                            {{cols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.MultiLineCSV)}}
                        WHERE
                            {{pkCols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.CreateWithCustomSeparator(" AND\n", false))}}";
                    conn.Execute(cmd, e, transaction, commandTimeout);{{IF(_options.TrackPropertiesChange)}}
                
                    e.MarkAsClean();{{ENDIF}}
                }
                """);
        }

    };

    private FormattableString GenerateCrudDelete(Table table)
    {
        var pkCols = table.Columns
            .Where(c => ShouldProcessColumn(table, c))
            .Where(c => c.IsPrimaryKeyMember)
            .OrderBy(c => c.OrdinalPosition)
            .Select(c => new { ColumnName = c.ColumnName, PropertyName = GetPropertyNameForDatabaseColumn(table, c.ColumnName), PropertyType = GetTypeDefinitionForDatabaseColumn(table, c) });

        if (!pkCols.Any())
            return $"";

        return $$"""
            /// <summary>
            /// Deletes record
            /// </summary>
            public static bool Delete{{GetClassNameForTable(table)}}(this IDbConnection conn, {{GetClassNameForTable(table)}} e, IDbTransaction transaction = null, int? commandTimeout = null)
            {
                string cmd = @"
                    DELETE {{(table.TableSchema == "dbo" ? $"[{table.TableName}]" : $"[{table.TableSchema}].[{table.TableName}]")}}
                    WHERE
                        {{pkCols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.CreateWithCustomSeparator(" AND\n", false))}}";
                int deleted = conn.Execute(cmd, e, transaction, commandTimeout);
                return deleted > 0;
            }
            /// <summary>
            /// Deletes record by the primary key
            /// </summary>
            public static bool Delete{{GetClassNameForTable(table)}}(this IDbConnection conn, {{pkCols.Select(col => $"{col.PropertyType} {col.PropertyName}").Render(RenderEnumerableOptions.CreateWithCustomSeparator(", ", false))}}, IDbTransaction transaction = null, int? commandTimeout = null)
            {
                string cmd = @"
                    DELETE {{(table.TableSchema == "dbo" ? $"[{table.TableName}]" : $"[{table.TableSchema}].[{table.TableName}]")}}
                    WHERE
                        {{pkCols.Select(col => $"[{col.ColumnName}] = @{col.PropertyName}").Render(RenderEnumerableOptions.CreateWithCustomSeparator(" AND\n", false))}}";
                int deleted = conn.Execute(cmd, new { {{pkCols.Select(col => col.PropertyName).Render(RenderEnumerableOptions.CreateWithCustomSeparator(", ", false))}} }, transaction, commandTimeout);
                return deleted > 0;
            }
            """;
    }


}
