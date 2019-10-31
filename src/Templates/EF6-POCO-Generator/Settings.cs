using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EF6POCOGenerator
{
    public static class Settings
    {
        // Main settings
        public static string ConnectionStringName;
        //public static string ConnectionString;
        //public static string ProviderName;
        public static string Namespace;
        public static int CommandTimeout = 0;

        public static bool IncludeViews;
        public static bool IncludeSynonyms;
        public static bool IncludeStoredProcedures;
        public static bool IncludeTableValuedFunctions;
        public static bool AddIDbContextFactory;
        public static bool AddUnitTestingDbContext;
        public static string DbContextName;

        private static string _dbContextInterfaceName;
        public static string DbContextInterfaceName
        {
            get { return _dbContextInterfaceName ?? ("I" + DbContextName); }
            set { _dbContextInterfaceName = value; }
        }

        public static string DbContextInterfaceBaseClasses;
        public static string DbContextBaseClass;

        public static bool AddParameterlessConstructorToDbContext = true;
        private static bool _explicitDefaultConstructorArgument;
        private static string _defaultConstructorArgument;
        public static string DefaultConstructorArgument
        {
            get { return _explicitDefaultConstructorArgument ? _defaultConstructorArgument : string.Format('"' + "Name={0}" + '"', ConnectionStringName); }
            set { _explicitDefaultConstructorArgument = true; _defaultConstructorArgument = value; }
        }

        public static string ConfigurationClassName = "Configuration";
        public static string CollectionInterfaceType = "System.Collections.Generic.ICollection";
        public static string CollectionType = "System.Collections.Generic.List";
        public static bool NullableShortHand;
        public static bool UseDataAnnotations;
        public static bool UseDataAnnotationsWithFluent;
        public static string EntityClassesModifiers = "public partial";
        public static string ConfigurationClassesModifiers = "internal";
        public static string DbContextClassModifiers = "public partial";
        public static string DbContextInterfaceModifiers = "public partial";
        public static string MigrationClassModifiers = "internal sealed";
        public static string ResultClassModifiers = "public partial";
        public static bool DbContextClassIsPartial()
        {
            return DbContextClassModifiers != null && DbContextClassModifiers.Contains("partial");
        }

        public static bool EntityClassesArePartial()
        {
            return EntityClassesModifiers != null && EntityClassesModifiers.Contains("partial");
        }

        public static bool ConfigurationClassesArePartial()
        {
            return ConfigurationClassesModifiers != null && ConfigurationClassesModifiers.Contains("partial");
        }
        public static bool GenerateSeparateFiles;
        public static bool UseMappingTables;
        public static bool UsePropertyInitializers;
        public static bool IsSqlCe;
        public static string FileExtension = ".cs";
        public static bool UsePascalCase;
        public static bool UsePrivateSetterForComputedColumns;
        public static CommentsStyle IncludeComments = CommentsStyle.AtEndOfField;
        public static bool IncludeQueryTraceOn9481Flag;
        public static CommentsStyle IncludeExtendedPropertyComments = CommentsStyle.InSummaryBlock;
        public static bool IncludeConnectionSettingComments;
        public static bool DisableGeographyTypes;
        public static bool PrependSchemaName;
        public static string TableSuffix;
        public static Regex SchemaFilterExclude;
        public static Regex SchemaFilterInclude;
        public static Regex TableFilterExclude;
        public static Regex TableFilterInclude;
        public static Regex StoredProcedureFilterExclude;
        public static Regex StoredProcedureFilterInclude;
        public static Func<Table, bool> TableFilter;
        public static Func<StoredProcedure, bool> StoredProcedureFilter;
        public static Func<Table, bool> ConfigurationFilter;
        public static Dictionary<string, string> StoredProcedureReturnTypes = new Dictionary<string, string>();
        public static Regex ColumnFilterExclude;
        public static bool UseLazyLoading;
        public static bool UseInheritedBaseInterfaceFunctions = false;
        public static string[] FilenameSearchOrder;
        public static string[] AdditionalNamespaces;
        public static string[] AdditionalContextInterfaceItems;
        public static string[] AdditionalReverseNavigationsDataAnnotations;
        public static string[] AdditionalForeignKeysDataAnnotations;
        //public static string ConfigFilePath;
        public static Func<string, string, bool, string> TableRename;
        public static Func<string, string, string, string> MappingTableRename;
        public static Func<StoredProcedure, string> StoredProcedureRename;
        public static Func<string, StoredProcedure, string> StoredProcedureReturnModelRename;
        public static Func<Column, Table, Column> UpdateColumn;
        public static Func<IList<ForeignKey>, Table, Table, bool, ForeignKey> ForeignKeyProcessing;
        public static Func<Table, Table, string, string, string[]> ForeignKeyAnnotationsProcessing;
        public static Func<ForeignKey, ForeignKey> ForeignKeyFilter;
        public static Func<string, ForeignKey, string, Relationship, short, string> ForeignKeyName;
        public static Action<Table> ViewProcessing;
        public static Action<List<ForeignKey>, Tables> AddForeignKeys;
        public static string MigrationConfigurationFileName;
        public static string MigrationStrategy = "MigrateDatabaseToLatestVersion";
        public static string ContextKey;
        public static bool AutomaticMigrationsEnabled;
        public static bool AutomaticMigrationDataLossAllowed;
        public static List<EnumDefinition> EnumDefinitions = new List<EnumDefinition>();
        public static Dictionary<string, string> ColumnNameToDataAnnotation;
        public static Dictionary<string, string> ColumnTypeToDataAnnotation;
        public static bool IncludeCodeGeneratedAttribute;
        public static Tables Tables;
        public static List<StoredProcedure> StoredProcs;

        public static Elements ElementsToGenerate;
        public static string PocoNamespace, ContextNamespace, UnitOfWorkNamespace, PocoConfigurationNamespace;

        public static decimal TargetFrameworkVersion;
        public static Func<string, bool> IsSupportedFrameworkVersion = (string frameworkVersion) =>
        {
            var nfi = CultureInfo.InvariantCulture.NumberFormat;
            var isSupported = decimal.Parse(frameworkVersion, nfi);
            return isSupported <= TargetFrameworkVersion;
        };
    };
}
