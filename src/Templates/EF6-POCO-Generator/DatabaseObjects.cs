using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF6POCOGenerator
{

    #region Nested type: Column

    public class PropertyAndComments
    {
        public string Definition;
        public string Comments;
        public string[] AdditionalDataAnnotations;
    }

    public class Column
    {
        public string Name; // Raw name of the column as obtained from the database
        public string NameHumanCase; // Name adjusted for C# output
        public string DisplayName;  // Name used in the data annotation [Display(Name = "<DisplayName> goes here")]
        public bool OverrideModifier = false; // Adds 'override' to the property declaration

        public int DateTimePrecision;
        public string Default;
        public int MaxLength;
        public int Precision;
        public string SqlPropertyType;
        public string PropertyType;
        public int Scale;
        public int Ordinal;
        public int PrimaryKeyOrdinal;
        public string ExtendedProperty;
        public string SummaryComments;
        public string UniqueIndexName;
        public bool AllowEmptyStrings = true;

        public bool IsIdentity;
        public bool IsRowGuid;
        public bool IsComputed;
        public ColumnGeneratedAlwaysType GeneratedAlwaysType;
        public bool IsNullable;
        public bool IsPrimaryKey;
        public bool IsUniqueConstraint;
        public bool IsUnique;
        public bool IsStoreGenerated;
        public bool IsRowVersion;
        public bool IsConcurrencyToken; //  Manually set via callback
        public bool IsFixedLength;
        public bool IsUnicode;
        public bool IsMaxLength;
        public bool Hidden;
        public bool IsForeignKey;

        public string Config;
        public List<string> ConfigFk = new List<string>();
        public string Entity;
        public List<PropertyAndComments> EntityFk = new List<PropertyAndComments>();

        public List<string> DataAnnotations;
        public List<Index> Indexes = new List<Index>();

        public Table ParentTable;

        public void ResetNavigationProperties()
        {
            ConfigFk = new List<string>();
            EntityFk = new List<PropertyAndComments>();
        }

        private void SetupEntity()
        {
            var comments = string.Empty;
            if (Settings.IncludeComments != CommentsStyle.None)
            {
                comments = Name;
                if (IsPrimaryKey)
                {
                    if (IsUniqueConstraint)
                        comments += " (Primary key via unique index " + UniqueIndexName + ")";
                    else
                        comments += " (Primary key)";
                }

                if (MaxLength > 0)
                    comments += string.Format(" (length: {0})", MaxLength);
            }

            var inlineComments = Settings.IncludeComments == CommentsStyle.AtEndOfField ? " // " + comments : string.Empty;

            SummaryComments = string.Empty;
            if (Settings.IncludeComments == CommentsStyle.InSummaryBlock && !string.IsNullOrEmpty(comments))
            {
                SummaryComments = comments;
            }
            if (Settings.IncludeExtendedPropertyComments == CommentsStyle.InSummaryBlock && !string.IsNullOrEmpty(ExtendedProperty))
            {
                if (string.IsNullOrEmpty(SummaryComments))
                    SummaryComments = ExtendedProperty;
                else
                    SummaryComments += ". " + ExtendedProperty;
            }

            if (Settings.IncludeExtendedPropertyComments == CommentsStyle.AtEndOfField && !string.IsNullOrEmpty(ExtendedProperty))
            {
                if (string.IsNullOrEmpty(inlineComments))
                    inlineComments = " // " + ExtendedProperty;
                else
                    inlineComments += ". " + ExtendedProperty;
            }
            var initialization = Settings.UsePropertyInitializers ? (Default == string.Empty ? "" : string.Format(" = {0};", Default)) : "";
            Entity = string.Format(
                "public {0}{1} {2} {{ get; {3}set; }}{4}{5}",
                (OverrideModifier ? "override " : ""), WrapIfNullable(PropertyType, this), NameHumanCase, Settings.UsePrivateSetterForComputedColumns && IsComputed ? "private " : string.Empty, initialization, inlineComments
            );
        }

        private string WrapIfNullable(string propType, Column col)
        {
            if (!col.IsNullable2())
                return propType;
            return string.Format(Settings.NullableShortHand ? "{0}?" : "System.Nullable<{0}>", propType);
        }

        private void SetupConfig()
        {
            DataAnnotations = new List<string>();
            string databaseGeneratedOption = null;
            var schemaReference = Settings.UseDataAnnotations
                ? string.Empty
                : "System.ComponentModel.DataAnnotations.Schema.";

            bool isNewSequentialId = !string.IsNullOrEmpty(Default) && Default.ToLower().Contains("newsequentialid");
            bool isTemporalColumn = this.GeneratedAlwaysType != ColumnGeneratedAlwaysType.NotApplicable;

            if (IsIdentity || isNewSequentialId || isTemporalColumn) // Identity, instead of Computed, seems the best for Temporal `GENERATED ALWAYS` columns: https://stackoverflow.com/questions/40742142/entity-framework-not-working-with-temporal-table
            {
                if (Settings.UseDataAnnotations || isNewSequentialId)
                    DataAnnotations.Add("DatabaseGenerated(DatabaseGeneratedOption.Identity)");
                else
                    databaseGeneratedOption = string.Format(".HasDatabaseGeneratedOption({0}DatabaseGeneratedOption.Identity)", schemaReference);
            }
            else if (IsComputed)
            {
                if (Settings.UseDataAnnotations)
                    DataAnnotations.Add("DatabaseGenerated(DatabaseGeneratedOption.Computed)");
                else
                    databaseGeneratedOption = string.Format(".HasDatabaseGeneratedOption({0}DatabaseGeneratedOption.Computed)", schemaReference);
            }
            else if (IsPrimaryKey)
            {
                if (Settings.UseDataAnnotations)
                    DataAnnotations.Add("DatabaseGenerated(DatabaseGeneratedOption.None)");
                else
                    databaseGeneratedOption = string.Format(".HasDatabaseGeneratedOption({0}DatabaseGeneratedOption.None)", schemaReference);
            }

            var sb = new StringBuilder();

            if (Settings.UseDataAnnotations)
                DataAnnotations.Add(string.Format("Column(@\"{0}\", Order = {1}, TypeName = \"{2}\")", Name, Ordinal, SqlPropertyType));
            else
                sb.AppendFormat(".HasColumnName(@\"{0}\").HasColumnType(\"{1}\")", Name, SqlPropertyType);

            if (Settings.UseDataAnnotations && Indexes.Any())
            {
                foreach (var index in Indexes)
                {
                    DataAnnotations.Add(string.Format("Index(@\"{0}\", {1}, IsUnique = {2}, IsClustered = {3})",
                        index.IndexName,
                        index.KeyOrdinal,
                        index.IsUnique ? "true" : "false",
                        index.IsClustered ? "true" : "false"));
                }
            }

            if (IsNullable)
            {
                sb.Append(".IsOptional()");
            }
            else
            {
                if (!IsComputed && (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent))
                {
                    if (PropertyType.Equals("string", StringComparison.InvariantCultureIgnoreCase) && this.AllowEmptyStrings)
                    {
                        DataAnnotations.Add("Required(AllowEmptyStrings = true)");
                    }
                    else
                    {
                        DataAnnotations.Add("Required");
                    }
                }

                if (!Settings.UseDataAnnotations)
                {
                    sb.Append(".IsRequired()");
                }
            }

            if (IsFixedLength || IsRowVersion)
            {
                sb.Append(".IsFixedLength()");
                // DataAnnotations.Add("????");
            }

            if (!IsUnicode)
            {
                sb.Append(".IsUnicode(false)");
                // DataAnnotations.Add("????");
            }

            if (!IsMaxLength && MaxLength > 0)
            {
                var doNotSpecifySize = (Settings.IsSqlCe && MaxLength > 4000); // Issue #179

                if (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent)
                {
                    DataAnnotations.Add(doNotSpecifySize ? "MaxLength" : string.Format("MaxLength({0})", MaxLength));

                    if (PropertyType.Equals("string", StringComparison.InvariantCultureIgnoreCase))
                        DataAnnotations.Add(string.Format("StringLength({0})", MaxLength));
                }

                if (!Settings.UseDataAnnotations)
                {
                    if (doNotSpecifySize)
                    {
                        sb.Append(".HasMaxLength(null)");
                    }
                    else
                    {
                        sb.AppendFormat(".HasMaxLength({0})", MaxLength);
                    }
                }
            }

            if (IsMaxLength)
            {
                if (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent)
                {
                    DataAnnotations.Add("MaxLength");
                }

                if (!Settings.UseDataAnnotations)
                {
                    sb.Append(".IsMaxLength()");
                }
            }

            if ((Precision > 0 || Scale > 0) && PropertyType == "decimal")
            {
                sb.AppendFormat(".HasPrecision({0},{1})", Precision, Scale);
                // DataAnnotations.Add("????");
            }

            if (IsRowVersion)
            {
                if (Settings.UseDataAnnotations)
                    DataAnnotations.Add("Timestamp");
                else
                    sb.Append(".IsRowVersion()");
            }

            if (IsConcurrencyToken)
            {
                sb.Append(".IsConcurrencyToken()");
                // DataAnnotations.Add("????");
            }

            if (databaseGeneratedOption != null)
                sb.Append(databaseGeneratedOption);

            var config = sb.ToString();
            if (!string.IsNullOrEmpty(config))
                Config = string.Format("Property(x => x.{0}){1};", NameHumanCase, config);

            if (IsPrimaryKey && Settings.UseDataAnnotations)
                DataAnnotations.Add("Key");

            string valueFromName, valueFromType;
            if (Settings.ColumnNameToDataAnnotation.TryGetValue(NameHumanCase.ToLowerInvariant(), out valueFromName))
            {
                DataAnnotations.Add(valueFromName);
                if (valueFromName.StartsWith("Display(Name", StringComparison.InvariantCultureIgnoreCase))
                    return; // Skip adding Display(Name = "") below
            }
            else if (Settings.ColumnTypeToDataAnnotation.TryGetValue(SqlPropertyType.ToLowerInvariant(), out valueFromType))
            {
                DataAnnotations.Add(valueFromType);
                if (valueFromType.StartsWith("Display(Name", StringComparison.InvariantCultureIgnoreCase))
                    return; // Skip adding Display(Name = "") below
            }

            DataAnnotations.Add(string.Format("Display(Name = \"{0}\")", DisplayName));
        }

        public void SetupEntityAndConfig()
        {
            SetupEntity();
            SetupConfig();
        }

        public void CleanUpDefault()
        {
            if (string.IsNullOrWhiteSpace(Default))
            {
                Default = string.Empty;
                return;
            }

            // Remove outer brackets
            while (Default.First() == '(' && Default.Last() == ')' && Default.Length > 2)
            {
                Default = Default.Substring(1, Default.Length - 2);
            }

            // Remove unicode prefix
            if (IsUnicode && Default.StartsWith("N") && !Default.Equals("NULL", StringComparison.InvariantCultureIgnoreCase))
                Default = Default.Substring(1, Default.Length - 1);

            if (Default.First() == '\'' && Default.Last() == '\'' && Default.Length >= 2)
                Default = string.Format("\"{0}\"", Default.Substring(1, Default.Length - 2));

            string lower = Default.ToLower();
            string lowerPropertyType = PropertyType.ToLower();

            // Cleanup default
            switch (lowerPropertyType)
            {
                case "bool":
                    Default = (Default == "0" || lower == "\"false\"" || lower == "false") ? "false" : "true";
                    break;

                case "string":
                case "datetime":
                case "datetime2":
                case "system.datetime":
                case "timespan":
                case "system.timespan":
                case "datetimeoffset":
                case "system.datetimeoffset":
                    if (Default.First() != '"')
                        Default = string.Format("\"{0}\"", Default);
                    if (Default.Contains('\\') || Default.Contains('\r') || Default.Contains('\n'))
                        Default = "@" + Default;
                    else
                        Default = string.Format("\"{0}\"", Default.Substring(1, Default.Length - 2).Replace("\"", "\\\"")); // #281 Default values must be escaped if contain double quotes
                    break;

                case "long":
                case "short":
                case "int":
                case "double":
                case "float":
                case "decimal":
                case "byte":
                case "guid":
                case "system.guid":
                    if (Default.First() == '\"' && Default.Last() == '\"' && Default.Length > 2)
                        Default = Default.Substring(1, Default.Length - 2);
                    break;

                case "byte[]":
                case "system.data.entity.spatial.dbgeography":
                case "system.data.entity.spatial.dbgeometry":
                    Default = string.Empty;
                    break;
            }

            // Ignore defaults we cannot interpret (we would need SQL to C# compiler)
            if (lower.StartsWith("create default"))
            {
                Default = string.Empty;
                return;
            }

            if (string.IsNullOrWhiteSpace(Default))
            {
                Default = string.Empty;
                return;
            }

            // Validate default
            switch (lowerPropertyType)
            {
                case "long":
                    long l;
                    if (!long.TryParse(Default, out l))
                        Default = string.Empty;
                    break;

                case "short":
                    short s;
                    if (!short.TryParse(Default, out s))
                        Default = string.Empty;
                    break;

                case "int":
                    int i;
                    if (!int.TryParse(Default, out i))
                        Default = string.Empty;
                    break;

                case "datetime":
                case "datetime2":
                case "system.datetime":
                    DateTime dt;
                    if (!DateTime.TryParse(Default, out dt))
                        Default = (lower.Contains("getdate()") || lower.Contains("sysdatetime")) ? "System.DateTime.Now" : (lower.Contains("getutcdate()") || lower.Contains("sysutcdatetime")) ? "System.DateTime.UtcNow" : string.Empty;
                    else
                        Default = string.Format("System.DateTime.Parse({0})", Default);
                    break;

                case "datetimeoffset":
                case "system.datetimeoffset":
                    DateTimeOffset dto;
                    if (!DateTimeOffset.TryParse(Default, out dto))
                        Default = (lower.Contains("getdate()") || lower.Contains("sysdatetimeoffset")) ? "System.DateTimeOffset.Now" : (lower.Contains("getutcdate()") || lower.Contains("sysutcdatetime")) ? "System.DateTimeOffset.UtcNow" : string.Empty;
                    else
                        Default = string.Format("System.DateTimeOffset.Parse({0})", Default);
                    break;

                case "timespan":
                case "system.timespan":
                    TimeSpan ts;
                    Default = TimeSpan.TryParse(Default, out ts) ? string.Format("System.TimeSpan.Parse({0})", Default) : string.Empty;
                    break;

                case "double":
                    double d;
                    if (!double.TryParse(Default, out d))
                        Default = string.Empty;
                    if (Default.ToLowerInvariant().EndsWith("."))
                        Default += "0";
                    break;

                case "float":
                    float f;
                    if (!float.TryParse(Default, out f))
                        Default = string.Empty;
                    if (!Default.ToLowerInvariant().EndsWith("f"))
                        Default += "f";
                    break;

                case "decimal":
                    decimal dec;
                    if (!decimal.TryParse(Default, out dec))
                        Default = string.Empty;
                    else
                        Default += "m";
                    break;

                case "byte":
                    byte b;
                    if (!byte.TryParse(Default, out b))
                        Default = string.Empty;
                    break;

                case "bool":
                    bool x;
                    if (!bool.TryParse(Default, out x))
                        Default = string.Empty;
                    break;

                case "string":
                    if (lower.Contains("newid()") || lower.Contains("newsequentialid()"))
                        Default = "System.Guid.NewGuid().ToString()";
                    if (lower.StartsWith("space("))
                        Default = "\"\"";
                    if (lower == "null")
                        Default = string.Empty;
                    break;

                case "guid":
                case "system.guid":
                    if (lower.Contains("newid()") || lower.Contains("newsequentialid()"))
                        Default = "System.Guid.NewGuid()";
                    else if (lower.Contains("null"))
                        Default = "null";
                    else
                        Default = string.Format("System.Guid.Parse(\"{0}\")", Default);
                    break;
            }
        }

        public bool IsNullable2() //TODO: why do we have both IsNullable and IsNullable2()?
        {
            return this.IsNullable && !SqlServerSchemaReader.NotNullable.Contains(this.PropertyType.ToLower());
        }


    }

    #endregion

    public enum Relationship
    {
        OneToOne,
        OneToMany,
        ManyToOne,
        ManyToMany,
        DoNotUse
    }


    #region Nested type: Stored Procedure

    public class StoredProcedure
    {
        public string Schema;
        public string Name;
        public string NameHumanCase;
        public List<StoredProcedureParameter> Parameters;
        public List<List<DataColumn>> ReturnModels;    // A list of return models, containing a list of return columns
        public bool IsTVF;

        public StoredProcedure()
        {
            Parameters = new List<StoredProcedureParameter>();
            ReturnModels = new List<List<DataColumn>>();
        }

        public static bool IsNullable(DataColumn col)
        {
            return col.AllowDBNull &&
                   !(SqlServerSchemaReader.NotNullable.Contains(col.DataType.Name.ToLower())
                   || SqlServerSchemaReader.NotNullable.Contains(col.DataType.Namespace.ToLower() + "." + col.DataType.Name.ToLower()));
        }

        public static string WrapTypeIfNullable(string propertyType, DataColumn col)
        {
            if (!IsNullable(col))
                return propertyType;
            return string.Format(Settings.NullableShortHand ? "{0}?" : "System.Nullable<{0}>", propertyType);
        }

    }

    public enum StoredProcedureParameterMode
    {
        In,
        InOut,
        Out
    };

    public class StoredProcedureParameter
    {
        public int Ordinal;
        public StoredProcedureParameterMode Mode;
        public string Name;
        public string NameHumanCase;
        public string SqlDbType;
        public string PropertyType;
        public string UserDefinedTypeName;
        public int DateTimePrecision;
        public int MaxLength;
        public int Precision;
        public int Scale;
    }

    #endregion

    public class ForeignKey
    {
        public string FkTableName { get; private set; }
        public string FkSchema { get; private set; }
        public string PkTableName { get; private set; }
        public string PkTableNameFiltered { get; private set; }
        public string PkSchema { get; private set; }
        public string FkColumn { get; private set; }
        public string PkColumn { get; private set; }
        public string ConstraintName { get; private set; }
        public int Ordinal { get; private set; }
        public bool CascadeOnDelete { get; private set; }

        // User settable via ForeignKeyFilter callback
        public string AccessModifier { get; set; }
        public bool IncludeReverseNavigation { get; set; }
        public bool IncludeRequiredAttribute { get; set; }
        public bool IsNotEnforced { get; set; }

        public ForeignKey(string fkTableName, string fkSchema, string pkTableName, string pkSchema, string fkColumn, string pkColumn, string constraintName, string pkTableNameFiltered, int ordinal, bool cascadeOnDelete, bool isNotEnforced)
        {
            ConstraintName = constraintName;
            PkColumn = pkColumn;
            FkColumn = fkColumn;
            PkSchema = pkSchema;
            PkTableName = pkTableName;
            FkSchema = fkSchema;
            FkTableName = fkTableName;
            PkTableNameFiltered = pkTableNameFiltered;
            Ordinal = ordinal;
            CascadeOnDelete = cascadeOnDelete;
            IsNotEnforced = isNotEnforced;

            IncludeReverseNavigation = true;
        }

        public string PkTableHumanCase(string suffix)
        {
            var singular = Inflector.MakeSingular(PkTableNameFiltered);
            var pkTableHumanCase = (Settings.UsePascalCase ? Inflector.ToTitleCase(singular) : singular).Replace(" ", "").Replace("$", "");
            if (string.Compare(PkSchema, "dbo", StringComparison.OrdinalIgnoreCase) != 0 && Settings.PrependSchemaName)
                pkTableHumanCase = PkSchema + "_" + pkTableHumanCase;
            pkTableHumanCase += suffix;
            return pkTableHumanCase;
        }
    }

    public class Index
    {
        public string Schema;
        public string TableName;
        public string IndexName;
        public byte KeyOrdinal;
        public string ColumnName;
        public int ColumnCount;
        public bool IsUnique;
        public bool IsPrimaryKey;
        public bool IsUniqueConstraint;
        public bool IsClustered;
    }

    public enum TableTemporalType
    {
        None,
        Verioned,
        History
    }

    public enum ColumnGeneratedAlwaysType
    {
        NotApplicable = 0,
        AsRowStart = 1,
        AsRowEnd = 2
    }

    public class Table
    {
        public string Name;
        public string NameHumanCase;
        public string Schema;
        public string Type;
        public string ClassName;
        public string Suffix;
        public string ExtendedProperty;
        public bool IsMapping;
        public bool IsView;
        public bool HasForeignKey;
        public bool HasNullableColumns;
        public bool HasPrimaryKey;
        public TableTemporalType TemporalType;

        public List<Column> Columns;
        public List<PropertyAndComments> ReverseNavigationProperty;
        public List<string> MappingConfiguration;
        public List<string> ReverseNavigationCtor;
        public List<string> ReverseNavigationUniquePropName;
        public List<string> ReverseNavigationUniquePropNameClashes;
        public List<string> DataAnnotations;

        public Table()
        {
            Columns = new List<Column>();
            ResetNavigationProperties();
            ReverseNavigationUniquePropNameClashes = new List<string>();
            DataAnnotations = new List<string>();
        }

        internal static string GetLazyLoadingMarker()
        {
            return Settings.UseLazyLoading ? "virtual " : string.Empty;
        }

        public string NameHumanCaseWithSuffix()
        {
            return NameHumanCase + Suffix;
        }

        public void ResetNavigationProperties()
        {
            MappingConfiguration = new List<string>();
            ReverseNavigationProperty = new List<PropertyAndComments>();
            ReverseNavigationCtor = new List<string>();
            ReverseNavigationUniquePropName = new List<string>();
            foreach (var col in Columns)
                col.ResetNavigationProperties();
        }

        public void SetPrimaryKeys()
        {
            HasPrimaryKey = Columns.Any(x => x.IsPrimaryKey);
            if (HasPrimaryKey)
                return; // Table has at least one primary key

            // This table is not allowed in EntityFramework as it does not have a primary key.
            // Therefore generate a composite key from all non-null fields.
            foreach (var col in Columns.Where(x => !x.IsNullable && !x.Hidden))
            {
                col.IsPrimaryKey = true;
                HasPrimaryKey = true;
            }
        }

        public IEnumerable<Column> PrimaryKeys
        {
            get
            {
                return Columns
                    .Where(x => x.IsPrimaryKey)
                    .OrderBy(x => x.PrimaryKeyOrdinal)
                    .ThenBy(x => x.Ordinal)
                    .ToList();
            }
        }

        public string PrimaryKeyNameHumanCase()
        {
            var data = PrimaryKeys.Select(x => "x." + x.NameHumanCase).ToList();
            var n = data.Count;
            if (n == 0)
                return string.Empty;
            if (n == 1)
                return "x => " + data.First();
            // More than one primary key
            return string.Format("x => new {{ {0} }}", string.Join(", ", data));
        }

        public Column this[string columnName]
        {
            get { return GetColumn(columnName); }
        }

        public Column GetColumn(string columnName)
        {
            return Columns.SingleOrDefault(x => string.Compare(x.Name, columnName, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public string GetUniqueColumnName(string tableNameHumanCase, ForeignKey foreignKey, bool checkForFkNameClashes, bool makeSingular, Relationship relationship)
        {
            var addReverseNavigationUniquePropName = (checkForFkNameClashes || Name == foreignKey.FkTableName || (Name == foreignKey.PkTableName && foreignKey.IncludeReverseNavigation));
            if (ReverseNavigationUniquePropName.Count == 0)
            {
                ReverseNavigationUniquePropName.Add(NameHumanCase);
                ReverseNavigationUniquePropName.AddRange(Columns.Select(c => c.NameHumanCase));
            }

            if (!makeSingular)
                tableNameHumanCase = Inflector.MakePlural(tableNameHumanCase);

            if (checkForFkNameClashes && ReverseNavigationUniquePropName.Contains(tableNameHumanCase) && !ReverseNavigationUniquePropNameClashes.Contains(tableNameHumanCase))
                ReverseNavigationUniquePropNameClashes.Add(tableNameHumanCase); // Name clash

            // Attempt 1
            string fkName = (Settings.UsePascalCase ? Inflector.ToTitleCase(foreignKey.FkColumn) : foreignKey.FkColumn).Replace(" ", "").Replace("$", "");
            string name = Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 1);
            string col;
            if (!ReverseNavigationUniquePropNameClashes.Contains(name) && !ReverseNavigationUniquePropName.Contains(name))
            {
                if (addReverseNavigationUniquePropName)
                {
                    ReverseNavigationUniquePropName.Add(name);
                }

                return name;
            }

            if (Name == foreignKey.FkTableName)
            {
                // Attempt 2
                if (fkName.ToLowerInvariant().EndsWith("id"))
                {
                    col = Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 2);
                    if (checkForFkNameClashes && ReverseNavigationUniquePropName.Contains(col) &&
                        !ReverseNavigationUniquePropNameClashes.Contains(col))
                        ReverseNavigationUniquePropNameClashes.Add(col); // Name clash

                    if (!ReverseNavigationUniquePropNameClashes.Contains(col) &&
                        !ReverseNavigationUniquePropName.Contains(col))
                    {
                        if (addReverseNavigationUniquePropName)
                        {
                            ReverseNavigationUniquePropName.Add(col);
                        }

                        return col;
                    }
                }

                // Attempt 3
                col = Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 3);
                if (checkForFkNameClashes && ReverseNavigationUniquePropName.Contains(col) &&
                    !ReverseNavigationUniquePropNameClashes.Contains(col))
                    ReverseNavigationUniquePropNameClashes.Add(col); // Name clash

                if (!ReverseNavigationUniquePropNameClashes.Contains(col) &&
                    !ReverseNavigationUniquePropName.Contains(col))
                {
                    if (addReverseNavigationUniquePropName)
                    {
                        ReverseNavigationUniquePropName.Add(col);
                    }

                    return col;
                }
            }

            // Attempt 4
            col = Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 4);
            if (checkForFkNameClashes && ReverseNavigationUniquePropName.Contains(col) && !ReverseNavigationUniquePropNameClashes.Contains(col))
                ReverseNavigationUniquePropNameClashes.Add(col); // Name clash

            if (!ReverseNavigationUniquePropNameClashes.Contains(col) && !ReverseNavigationUniquePropName.Contains(col))
            {
                if (addReverseNavigationUniquePropName)
                {
                    ReverseNavigationUniquePropName.Add(col);
                }

                return col;
            }

            // Attempt 5
            for (int n = 1; n < 99; ++n)
            {
                col = Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 5) + n;

                if (ReverseNavigationUniquePropName.Contains(col))
                    continue;

                if (addReverseNavigationUniquePropName)
                {
                    ReverseNavigationUniquePropName.Add(col);
                }

                return col;
            }

            // Give up
            return Settings.ForeignKeyName(tableNameHumanCase, foreignKey, fkName, relationship, 6);
        }

        public void AddReverseNavigation(Relationship relationship, string fkName, Table fkTable, string propName, string constraint, List<ForeignKey> fks, Table mappingTable = null)
        {
            var fkNames = "";
            switch (relationship)
            {
                case Relationship.OneToOne:
                case Relationship.OneToMany:
                case Relationship.ManyToOne:
                    fkNames = (fks.Count > 1 ? "(" : "") + string.Join(", ", fks.Select(x => "[" + x.FkColumn + "]").Distinct().ToArray()) + (fks.Count > 1 ? ")" : "");
                    break;
                case Relationship.ManyToMany:
                    break;
            }
            string accessModifier = fks != null && fks.FirstOrDefault() != null ? (fks.FirstOrDefault().AccessModifier ?? "public") : "public";
            switch (relationship)
            {
                case Relationship.OneToOne:
                    ReverseNavigationProperty.Add(
                        new PropertyAndComments()
                        {
                            AdditionalDataAnnotations = Settings.ForeignKeyAnnotationsProcessing(fkTable, this, propName, string.Empty),
                            Definition = string.Format("{0} {1}{2} {3} {{ get; set; }}{4}", accessModifier, GetLazyLoadingMarker(), fkTable.NameHumanCaseWithSuffix(), propName, Settings.IncludeComments != CommentsStyle.None ? " // " + constraint : string.Empty),
                            Comments = string.Format("Parent (One-to-One) {0} pointed by [{1}].{2} ({3})", NameHumanCaseWithSuffix(), fkTable.Name, fkNames, fks.First().ConstraintName)
                        }
                    );
                    break;

                case Relationship.OneToMany:
                    ReverseNavigationProperty.Add(
                        new PropertyAndComments()
                        {
                            AdditionalDataAnnotations = Settings.ForeignKeyAnnotationsProcessing(fkTable, this, propName, string.Empty),
                            Definition = string.Format("{0} {1}{2} {3} {{ get; set; }}{4}", accessModifier, GetLazyLoadingMarker(), fkTable.NameHumanCaseWithSuffix(), propName, Settings.IncludeComments != CommentsStyle.None ? " // " + constraint : string.Empty),
                            Comments = string.Format("Parent {0} pointed by [{1}].{2} ({3})", NameHumanCaseWithSuffix(), fkTable.Name, fkNames, fks.First().ConstraintName)
                        }
                    );
                    break;

                case Relationship.ManyToOne:
                    string initialization1 = string.Empty;
                    if (Settings.UsePropertyInitializers)
                        initialization1 = string.Format(" = new {0}<{1}>();", Settings.CollectionType, fkTable.NameHumanCaseWithSuffix());
                    ReverseNavigationProperty.Add(
                        new PropertyAndComments()
                        {
                            AdditionalDataAnnotations = Settings.ForeignKeyAnnotationsProcessing(fkTable, this, propName, string.Empty),
                            Definition = string.Format("{0} {1}{2}<{3}> {4} {{ get; set; }}{5}{6}", accessModifier, GetLazyLoadingMarker(), Settings.CollectionInterfaceType, fkTable.NameHumanCaseWithSuffix(), propName, initialization1, Settings.IncludeComments != CommentsStyle.None ? " // " + constraint : string.Empty),
                            Comments = string.Format("Child {0} where [{1}].{2} point to this entity ({3})", Inflector.MakePlural(fkTable.NameHumanCase), fkTable.Name, fkNames, fks.First().ConstraintName)
                        }
                    );
                    ReverseNavigationCtor.Add(string.Format("{0} = new {1}<{2}>();", propName, Settings.CollectionType, fkTable.NameHumanCaseWithSuffix()));
                    break;

                case Relationship.ManyToMany:
                    string initialization2 = string.Empty;
                    if (Settings.UsePropertyInitializers)
                        initialization2 = string.Format(" = new {0}<{1}>();", Settings.CollectionType, fkTable.NameHumanCaseWithSuffix());
                    ReverseNavigationProperty.Add(
                        new PropertyAndComments()
                        {
                            AdditionalDataAnnotations = Settings.ForeignKeyAnnotationsProcessing(fkTable, this, propName, string.Empty),
                            Definition = string.Format("{0} {1}{2}<{3}> {4} {{ get; set; }}{5}{6}", accessModifier, GetLazyLoadingMarker(), Settings.CollectionInterfaceType, fkTable.NameHumanCaseWithSuffix(), propName, initialization2, Settings.IncludeComments != CommentsStyle.None ? " // Many to many mapping" : string.Empty),
                            Comments = string.Format("Child {0} (Many-to-Many) mapped by table [{1}]", Inflector.MakePlural(fkTable.NameHumanCase), mappingTable == null ? string.Empty : mappingTable.Name)
                        }
                    );

                    ReverseNavigationCtor.Add(string.Format("{0} = new {1}<{2}>();", propName, Settings.CollectionType, fkTable.NameHumanCaseWithSuffix()));
                    break;

                default:
                    throw new ArgumentOutOfRangeException("relationship");
            }
        }

        public void AddMappingConfiguration(ForeignKey left, ForeignKey right, string leftPropName, string rightPropName)
        {
            MappingConfiguration.Add(string.Format(@"
                HasMany(t => t.{0}).WithMany(t => t.{1}).Map(m =>
                {{
                    m.ToTable(""{2}""{5});
                    m.MapLeftKey(""{3}"");
                    m.MapRightKey(""{4}"");
                }});", leftPropName, rightPropName, left.FkTableName, left.FkColumn, right.FkColumn, Settings.IsSqlCe ? string.Empty : ", \"" + left.FkSchema + "\""));
        }

        public void IdentifyMappingTable(List<ForeignKey> fkList, Tables tables, bool checkForFkNameClashes)
        {
            IsMapping = false;

            var nonReadOnlyColumns = Columns.Where(c => !c.IsIdentity && !c.IsRowVersion && !c.IsStoreGenerated && !c.Hidden).ToList();

            // Ignoring read-only columns, it must have only 2 columns to be a mapping table
            if (nonReadOnlyColumns.Count != 2)
                return;

            // Must have 2 primary keys
            if (nonReadOnlyColumns.Count(x => x.IsPrimaryKey) != 2)
                return;

            // No columns should be nullable
            if (nonReadOnlyColumns.Any(x => x.IsNullable))
                return;

            // Find the foreign keys for this table
            var foreignKeys = fkList.Where(x =>
                                            string.Compare(x.FkTableName, Name, StringComparison.OrdinalIgnoreCase) == 0 &&
                                            string.Compare(x.FkSchema, Schema, StringComparison.OrdinalIgnoreCase) == 0)
                                    .ToList();

            // Each column must have a foreign key, therefore check column and foreign key counts match
            if (foreignKeys.Select(x => x.FkColumn).Distinct().Count() != 2)
                return;

            ForeignKey left = foreignKeys[0];
            ForeignKey right = foreignKeys[1];
            if (!left.IncludeReverseNavigation || !right.IncludeReverseNavigation)
                return;

            Table leftTable = tables.GetTable(left.PkTableName, left.PkSchema);
            if (leftTable == null)
                return;

            Table rightTable = tables.GetTable(right.PkTableName, right.PkSchema);
            if (rightTable == null)
                return;

            var leftPropName = leftTable.GetUniqueColumnName(rightTable.NameHumanCase, right, checkForFkNameClashes, false, Relationship.ManyToOne); // relationship from the mapping table to each side is Many-to-One
            leftPropName = Settings.MappingTableRename(Name, leftTable.NameHumanCase, leftPropName);
            var rightPropName = rightTable.GetUniqueColumnName(leftTable.NameHumanCase, left, checkForFkNameClashes, false, Relationship.ManyToOne); // relationship from the mapping table to each side is Many-to-One
            rightPropName = Settings.MappingTableRename(Name, rightTable.NameHumanCase, rightPropName);
            leftTable.AddMappingConfiguration(left, right, leftPropName, rightPropName);

            IsMapping = true;
            rightTable.AddReverseNavigation(Relationship.ManyToMany, rightTable.NameHumanCase, leftTable, rightPropName, null, null, this);
            leftTable.AddReverseNavigation(Relationship.ManyToMany, leftTable.NameHumanCase, rightTable, leftPropName, null, null, this);
        }

        public void SetupDataAnnotations()
        {
            var schema = String.Empty;
            if (!Settings.IsSqlCe)
                schema = String.Format(", Schema = \"{0}\"", Schema);
            DataAnnotations = new List<string>
            {
                HasPrimaryKey
                    ? string.Format("Table(\"{0}\"{1})", Name, schema)
                    : "NotMapped"
            };

        }
    }

    public class Tables : List<Table>
    {
        public Table GetTable(string tableName, string schema)
        {
            return this.SingleOrDefault(x =>
                string.Compare(x.Name, tableName, StringComparison.OrdinalIgnoreCase) == 0 &&
                string.Compare(x.Schema, schema, StringComparison.OrdinalIgnoreCase) == 0);
        }

        public void SetPrimaryKeys()
        {
            foreach (var tbl in this)
            {
                tbl.SetPrimaryKeys();
            }
        }

        public void IdentifyMappingTables(List<ForeignKey> fkList, bool checkForFkNameClashes)
        {
            foreach (var tbl in this.Where(x => x.HasForeignKey))
            {
                tbl.IdentifyMappingTable(fkList, this, checkForFkNameClashes);
            }
        }

        public void ResetNavigationProperties()
        {
            foreach (var tbl in this)
            {
                tbl.ResetNavigationProperties();
            }
        }
    }

}
