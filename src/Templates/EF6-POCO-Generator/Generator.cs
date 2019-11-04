using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using CodegenCS;


namespace EF6POCOGenerator
{
    public class Generator
    {
        private Generator() { }
        public Generator(CodegenContext context, Func<DbConnection> createConnection, decimal targetFrameworkVersion)
        {
            //Settings.ConnectionString = connectionString;
            //Settings.ProviderName = providerName;
            Settings.TargetFrameworkVersion = targetFrameworkVersion;
            _createConnection = createConnection;
            _context = context;
            this.Configure();
        }

        private CodegenContext _context;
        private CodegenTextWriter w;
        Func<DbConnection> _createConnection;


        #region Configurable Callback methods
        Action<CodegenTextWriter, Table> WritePocoClassAttributes;
        Action<CodegenTextWriter, Table> WritePocoClassExtendedComments;
        Func<Table, string> WritePocoBaseClasses;
        Action<CodegenTextWriter, Table> WritePocoBaseClassBody;
        Action<CodegenTextWriter, Column> WritePocoColumn;
        #endregion

        public void Configure()
        {
            #region All this code mostly came from Simon Hughes T4 templates (with minor adjustments) - Database.tt - see https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator
            // v2.37.4
            // Please make changes to the settings below.
            // All you have to do is save this file, and the output file(s) is/are generated. Compiling does not regenerate the file(s).
            // A course for this generator is available on Pluralsight at https://www.pluralsight.com/courses/code-first-entity-framework-legacy-databases

            // Main settings **********************************************************************************************************************
            Settings.ConnectionStringName = "MyDbContext";   // Searches for this connection string in config files listed below in the ConfigFilenameSearchOrder setting
                                                             // ConnectionStringName is the only required setting.
            Settings.CommandTimeout = 600; // SQL Command timeout in seconds. 600 is 10 minutes, 0 will wait indefinately. Some databases can be slow retrieving schema information.
                                           // As an alternative to ConnectionStringName above, which must match your app/web.config connection string name, you can override them below
                                           // Settings.ConnectionString = "Data Source=(local);Initial Catalog=Northwind;Integrated Security=True;Application Name=EntityFramework Reverse POCO Generator";
                                           // Settings.ProviderName = "System.Data.SqlClient";

            Settings.Namespace = "EF6POCOGenerator.SampleOutput"; // Override the default namespace here
            Settings.DbContextName = "MyDbContext"; // Note: If generating separate files, please give the db context a different name from this tt filename.
                                                    //Settings.DbContextInterfaceName = "IMyDbContext"; // Defaults to "I" + DbContextName or set string empty to not implement any interface.
            Settings.DbContextInterfaceBaseClasses = "System.IDisposable";    // Specify what the base classes are for your database context interface
            Settings.DbContextBaseClass = "System.Data.Entity.DbContext";   // Specify what the base class is for your DbContext. For ASP.NET Identity use "Microsoft.AspNet.Identity.EntityFramework.IdentityDbContext<Microsoft.AspNet.Identity.EntityFramework.IdentityUser>";
            Settings.AddParameterlessConstructorToDbContext = true; // If true, then DbContext will have a default (parameterless) constructor which automatically passes in the connection string name, if false then no parameterless constructor will be created.
                                                                    //Settings.DefaultConstructorArgument = null; // Defaults to "Name=" + ConnectionStringName, use null in order not to call the base constructor
            Settings.ConfigurationClassName = "Configuration"; // Configuration, Mapping, Map, etc. This is appended to the Poco class name to configure the mappings.
            Settings.FilenameSearchOrder = new[] { "app.config", "web.config" }; // Add more here if required. The config files are searched for in the local project first, then the whole solution second.

            Settings.EntityClassesModifiers = "public"; // "public partial";
            Settings.ConfigurationClassesModifiers = "public"; // "public partial";
            Settings.DbContextClassModifiers = "public"; // "public partial";
            Settings.DbContextInterfaceModifiers = "public"; // "public partial";
            Settings.MigrationClassModifiers = "internal sealed";
            Settings.ResultClassModifiers = "public"; // "public partial";
            Settings.UseMappingTables = true; // If true, mapping will be used and no mapping tables will be generated. If false, all tables will be generated.
            Settings.UsePascalCase = true;    // This will rename the generated C# tables & properties to use PascalCase. If false table & property names will be left alone.
            Settings.UseDataAnnotations = false; // If true, will add data annotations to the poco classes.
            Settings.UseDataAnnotationsWithFluent = false; // If true, then non-Entity Framework-specific DataAnnotations (like [Required] and [StringLength]) will be applied to Entities even if UseDataAnnotations is false.
            Settings.UsePropertyInitializers = false; // Removes POCO constructor and instead uses C# 6 property initialisers to set defaults
            Settings.UseLazyLoading = true; // Marks all navigation properties as virtual or not, to support or disable EF Lazy Loading feature
            Settings.UseInheritedBaseInterfaceFunctions = false; // If true, the main DBContext interface functions will come from the DBContextInterfaceBaseClasses and not generated. If false, the functions will be generated.
            Settings.IncludeComments = CommentsStyle.AtEndOfField; // Adds comments to the generated code
            Settings.IncludeExtendedPropertyComments = CommentsStyle.InSummaryBlock; // Adds extended properties as comments to the generated code
            Settings.IncludeConnectionSettingComments = true; // Add comments describing connection settings used to generate file
            Settings.IncludeViews = true;
            Settings.IncludeSynonyms = false;
            Settings.IncludeStoredProcedures = true;
            Settings.IncludeTableValuedFunctions = false; // If true, you must set IncludeStoredProcedures = true, and install the "EntityFramework.CodeFirstStoreFunctions" Nuget Package.
            Settings.DisableGeographyTypes = false; // Turns off use of System.Data.Entity.Spatial.DbGeography and System.Data.Entity.Spatial.DbGeometry as OData doesn't support entities with geometry/geography types.
                                                    //Settings.CollectionInterfaceType = "System.Collections.Generic.List"; // Determines the declaration type of collections for the Navigation Properties. ICollection is used if not set.
            Settings.CollectionType = "System.Collections.Generic.List";  // Determines the type of collection for the Navigation Properties. "ObservableCollection" for example. Add "System.Collections.ObjectModel" to AdditionalNamespaces if setting the CollectionType = "ObservableCollection".
            Settings.NullableShortHand = true; //true => T?, false => Nullable<T>
            Settings.AddIDbContextFactory = true; // Will add a default IDbContextFactory<DbContextName> implementation for easy dependency injection
            Settings.AddUnitTestingDbContext = true; // Will add a FakeDbContext and FakeDbSet for easy unit testing
            Settings.IncludeQueryTraceOn9481Flag = false; // If SqlServer 2014 appears frozen / take a long time when this file is saved, try setting this to true (you will also need elevated privileges).
            Settings.IncludeCodeGeneratedAttribute = true; // If true, will include the GeneratedCode attribute, false to remove it.
            Settings.UsePrivateSetterForComputedColumns = true; // If the columns is computed, use a private setter.
            Settings.AdditionalNamespaces = new[] { "" };  // To include extra namespaces, include them here. i.e. "Microsoft.AspNet.Identity.EntityFramework"
            Settings.AdditionalContextInterfaceItems = new[] // To include extra db context interface items, include them here. Also set DbContextClassModifiers="public partial", and implement the partial DbContext class functions.
            {
        ""  //  example: "void SetAutoDetectChangesEnabled(bool flag);"
    };
            // If you need to serialize your entities with the JsonSerializer from Newtonsoft, this would serialize
            // all properties including the Reverse Navigation and Foreign Keys. The simplest way to exclude them is
            // to use the data annotation [JsonIgnore] on reverse navigation and foreign keys.
            // For more control, take a look at ForeignKeyAnnotationsProcessing() further down
            Settings.AdditionalReverseNavigationsDataAnnotations = new string[] // Data Annotations for all ReverseNavigationProperty.
            {
                // "JsonIgnore" // Also add "Newtonsoft.Json" to the AdditionalNamespaces array above
            };
            Settings.AdditionalForeignKeysDataAnnotations = new string[] // Data Annotations for all ForeignKeys.
            {
                // "JsonIgnore" // Also add "Newtonsoft.Json" to the AdditionalNamespaces array above
            };
            Settings.ColumnNameToDataAnnotation = new Dictionary<string, string>
    {
        // This is used when UseDataAnnotations == true or UseDataAnnotationsWithFluent == true;
        // It is used to set a data annotation on a column based on the columns name.
        // Make sure the column name is lowercase in the following array, regardless of how it is in the database
        // Column name       DataAnnotation to add
        { "email",           "EmailAddress" },
        { "emailaddress",    "EmailAddress" },
        { "creditcard",      "CreditCard" },
        { "url",             "Url" },
        { "fax",             "Phone" },
        { "phone",           "Phone" },
        { "phonenumber",     "Phone" },
        { "mobile",          "Phone" },
        { "mobilenumber",    "Phone" },
        { "telephone",       "Phone" },
        { "telephonenumber", "Phone" },
        { "password",        "DataType(DataType.Password)" },
        { "username",        "DataType(DataType.Text)" },
        { "postcode",        "DataType(DataType.PostalCode)" },
        { "postalcode",      "DataType(DataType.PostalCode)" },
        { "zip",             "DataType(DataType.PostalCode)" },
        { "zipcode",         "DataType(DataType.PostalCode)" }
    };
            Settings.ColumnTypeToDataAnnotation = new Dictionary<string, string>
    {
        // This is used when UseDataAnnotations == true or UseDataAnnotationsWithFluent == true;
        // It is used to set a data annotation on a column based on the columns's MS SQL type.
        // Make sure the column name is lowercase in the following array, regardless of how it is in the database
        // Column name       DataAnnotation to add
        { "date",            "DataType(System.ComponentModel.DataAnnotations.DataType.Date)" },
        { "datetime",        "DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)" },
        { "datetime2",       "DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)" },
        { "datetimeoffset",  "DataType(System.ComponentModel.DataAnnotations.DataType.DateTime)" },
        { "smallmoney",      "DataType(System.ComponentModel.DataAnnotations.DataType.Currency)" },
        { "money",           "DataType(System.ComponentModel.DataAnnotations.DataType.Currency)" }
    };

            // Migrations *************************************************************************************************************************
            Settings.MigrationConfigurationFileName = ""; // null or empty to not create migrations
            Settings.MigrationStrategy = "MigrateDatabaseToLatestVersion"; // MigrateDatabaseToLatestVersion, CreateDatabaseIfNotExists or DropCreateDatabaseIfModelChanges
            Settings.ContextKey = ""; // Sets the string used to distinguish migrations belonging to this configuration from migrations belonging to other configurations using the same database. This property enables migrations from multiple different models to be applied to applied to a single database.
            Settings.AutomaticMigrationsEnabled = true;
            Settings.AutomaticMigrationDataLossAllowed = true; // if true, can drop fields and lose data during automatic migration

            // Pluralization **********************************************************************************************************************
            // To turn off pluralization, use:
            //      Inflector.PluralizationService = null;
            // Default pluralization, use:
            //      Inflector.PluralizationService = new EnglishPluralizationService();
            // For Spanish pluralization:
            //      1. Intall the "EF6.Contrib" Nuget Package.
            //      2. Add the following to the top of this file and adjust path, and remove the space between the angle bracket and # at the beginning and end.
            //         < #@ assembly name="your full path to \EntityFramework.Contrib.dll" # >
            //      3. Change the line below to: Inflector.PluralizationService = new SpanishPluralizationService();
            Inflector.PluralizationService = new CodegenCS.Utils.HumanizerInflector();
            // If pluralisation does not do the right thing, override it here by adding in a custom entry.
            //Inflector.PluralizationService = new EnglishPluralizationService(new[]
            //{
            //    // Create custom ("Singular", "Plural") forms for one-off words as needed.
            //    new CustomPluralizationEntry("Course", "Courses"),
            //    new CustomPluralizationEntry("Status", "Status") // Use same value to prevent pluralisation
            //});


            // Elements to generate ***************************************************************************************************************
            // Add the elements that should be generated when the template is executed.
            // Multiple projects can now be used that separate the different concerns.
            Settings.ElementsToGenerate = Elements.Poco | Elements.Context | Elements.UnitOfWork | Elements.PocoConfiguration;

            // Use these namespaces to specify where the different elements now live. These may even be in different assemblies.
            // Please note this does not create the files in these locations, it only adds a using statement to say where they are.
            // The way to do this is to add the "EntityFramework Reverse POCO Code First Generator" into each of these folders.
            // Then set the .tt to only generate the relevant section you need by setting
            //      ElementsToGenerate = Elements.Poco; in your Entity folder,
            //      ElementsToGenerate = Elements.Context | Elements.UnitOfWork; in your Context folder,
            //      ElementsToGenerate = Elements.PocoConfiguration; in your Maps folder.
            //      PocoNamespace = "YourProject.Entities";
            //      ContextNamespace = "YourProject.Context";
            //      UnitOfWorkNamespace = "YourProject.Context";
            //      PocoConfigurationNamespace = "YourProject.Maps";
            // You also need to set the following to the namespace where they now live:
            Settings.PocoNamespace = "";
            Settings.ContextNamespace = "";
            Settings.UnitOfWorkNamespace = "";
            Settings.PocoConfigurationNamespace = "";


            // Schema *****************************************************************************************************************************
            // If there are multiple schemas, then the table name is prefixed with the schema, except for dbo.
            // Ie. dbo.hello will be Hello.
            //     abc.hello will be AbcHello.
            Settings.PrependSchemaName = true;   // Control if the schema name is prepended to the table name

            // Table Suffix ***********************************************************************************************************************
            // Prepends the suffix to the generated classes names
            // Ie. If TableSuffix is "Dto" then Order will be OrderDto
            //     If TableSuffix is "Entity" then Order will be OrderEntity
            Settings.TableSuffix = null;

            // Filtering **************************************************************************************************************************
            // Use the following table/view name regex filters to include or exclude tables/views
            // Exclude filters are checked first and tables matching filters are removed.
            //  * If left null, none are excluded.
            //  * If not null, any tables matching the regex are excluded.
            // Include filters are checked second.
            //  * If left null, all are included.
            //  * If not null, only the tables matching the regex are included.
            // For clarity: if you want to include all the customer tables, but not the customer billing tables.
            //      TableFilterInclude = new Regex("^[Cc]ustomer.*"); // This includes all the customer and customer billing tables
            //      TableFilterExclude = new Regex(".*[Bb]illing.*"); // This excludes all the billing tables
            //
            // Example:     TableFilterExclude = new Regex(".*auto.*");
            //              TableFilterInclude = new Regex("(.*_FR_.*)|(data_.*)");
            //              TableFilterInclude = new Regex("^table_name1$|^table_name2$|etc");
            //              ColumnFilterExclude = new Regex("^FK_.*$");
            Settings.SchemaFilterExclude = null;
            Settings.SchemaFilterInclude = null;
            Settings.TableFilterExclude = null;
            Settings.TableFilterInclude = null;
            Settings.ColumnFilterExclude = null;

            // Filtering of tables using a function. This can be used in conjunction with the Regex's above.
            // Regex are used first to filter the list down, then this function is run last.
            // Return true to include the table, return false to exclude it.
            Settings.TableFilter = (Table t) =>
            {
                // Example: Exclude any table in dbo schema with "order" in its name.
                //if(t.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase) && t.NameHumanCase.ToLowerInvariant().Contains("order"))
                //    return false;

                return true;
            };


            // Stored Procedures ******************************************************************************************************************
            // Use the following regex filters to include or exclude stored procedures
            Settings.StoredProcedureFilterExclude = null;
            Settings.StoredProcedureFilterInclude = null;

            // Filtering of stored procedures using a function. This can be used in conjunction with the Regex's above.
            // Regex are used first to filter the list down, then this function is run last.
            // Return true to include the stored procedure, return false to exclude it.
            Settings.StoredProcedureFilter = (StoredProcedure sp) =>
            {
                // Example: Exclude any stored procedure in dbo schema with "order" in its name.
                //if(sp.Schema.Equals("dbo", StringComparison.InvariantCultureIgnoreCase) && sp.NameHumanCase.ToLowerInvariant().Contains("order"))
                //    return false;

                return true;
            };


            // Table renaming *********************************************************************************************************************
            // Use the following function to rename tables such as tblOrders to Orders, Shipments_AB to Shipments, etc.
            // Example:
            Settings.TableRename = (string name, string schema, bool isView) =>
            {
                // Example
                //if (name.StartsWith("tbl"))
                //    name = name.Remove(0, 3);
                //name = name.Replace("_AB", "");

                //if(isView)
                //    name = name + "View";

                // If you turn pascal casing off (UsePascalCase = false), and use the pluralisation service, and some of your
                // tables names are all UPPERCASE, some words ending in IES such as CATEGORIES get singularised as CATEGORy.
                // Therefore you can make them lowercase by using the following
                // return Inflector.MakeLowerIfAllCaps(name);

                // If you are using the pluralisation service and you want to rename a table, make sure you rename the table to the plural form.
                // For example, if the table is called Treez (with a z), and your pluralisation entry is
                //     new CustomPluralizationEntry("Tree", "Trees")
                // Use this TableRename function to rename Treez to the plural (not singular) form, Trees:
                // if (name == "Treez") return "Trees";

                return name;
            };

            // Mapping Table renaming *********************************************************************************************************************
            // By default, name of the properties created relate to the table the foreign key points to and not the mapping table.
            // Use the following function to rename the properties created by ManytoMany relationship tables especially if you have 2 relationships between the same tables.
            // Example:
            Settings.MappingTableRename = (string mappingtable, string tablename, string entityname) =>
            {

                // Examples:
                // If you have two mapping tables such as one being UserRequiredSkills snd one being UserOptionalSkills, this would change the name of one property
                // if (mappingtable == "UserRequiredSkills" and tablename == "User")
                //    return "RequiredSkills";

                // or if you want to give the same property name on both classes
                // if (mappingtable == "UserRequiredSkills")
                //    return "UserRequiredSkills";

                return entityname;
            };

            // Column modification*****************************************************************************************************************
            // Use the following list to replace column byte types with Enums.
            // As long as the type can be mapped to your new type, all is well.
            //Settings.EnumDefinitions.Add(new EnumDefinition { Schema = "dbo", Table = "match_table_name", Column = "match_column_name", EnumType = "name_of_enum" });
            //Settings.EnumDefinitions.Add(new EnumDefinition { Schema = "dbo", Table = "OrderHeader", Column = "OrderStatus", EnumType = "OrderStatusType" }); // This will replace OrderHeader.OrderStatus type to be an OrderStatusType enum

            // Use the following function if you need to apply additional modifications to a column
            // eg. normalise names etc.
            Settings.UpdateColumn = (Column column, Table table) =>
            {
                // Rename column
                //if (column.IsPrimaryKey && column.NameHumanCase == "PkId")
                //    column.NameHumanCase = "Id";

                // .IsConcurrencyToken() must be manually configured. However .IsRowVersion() can be automatically detected.
                //if (table.NameHumanCase.Equals("SomeTable", StringComparison.InvariantCultureIgnoreCase) && column.NameHumanCase.Equals("SomeColumn", StringComparison.InvariantCultureIgnoreCase))
                //    column.IsConcurrencyToken = true;

                // Remove table name from primary key
                //if (column.IsPrimaryKey && column.NameHumanCase.Equals(table.NameHumanCase + "Id", StringComparison.InvariantCultureIgnoreCase))
                //    column.NameHumanCase = "Id";

                // Remove column from poco class as it will be inherited from a base class
                //if (column.IsPrimaryKey && table.NameHumanCase.Equals("SomeTable", StringComparison.InvariantCultureIgnoreCase))
                //    column.Hidden = true;

                // Use the extended properties to perform tasks to column
                //if (column.ExtendedProperty == "HIDE")
                //    column.Hidden = true;

                // Apply the "override" access modifier to a specific column.
                // if (column.NameHumanCase == "id")
                //    column.OverrideModifier = true;
                // This will create: public override long id { get; set; }

                // Perform Enum property type replacement
                var enumDefinition = Settings.EnumDefinitions.FirstOrDefault(e =>
                    (e.Schema.Equals(table.Schema, StringComparison.InvariantCultureIgnoreCase)) &&
                    (e.Table.Equals(table.Name, StringComparison.InvariantCultureIgnoreCase) || e.Table.Equals(table.NameHumanCase, StringComparison.InvariantCultureIgnoreCase)) &&
                    (e.Column.Equals(column.Name, StringComparison.InvariantCultureIgnoreCase) || e.Column.Equals(column.NameHumanCase, StringComparison.InvariantCultureIgnoreCase)));

                if (enumDefinition != null)
                {
                    column.PropertyType = enumDefinition.EnumType;
                    if (!string.IsNullOrEmpty(column.Default))
                        column.Default = "(" + enumDefinition.EnumType + ") " + column.Default;
                }

                return column;
            };


            // Using Views *****************************************************************************************************************
            // SQL Server does not support the declaration of primary-keys in VIEWs. Entity Framework's EDMX designer (and this T4 template)
            // assume that all non-null columns in a VIEW are primary-key columns, this will be incorrect for most non-trivial applications.
            // This callback will be invoked for each VIEW found in the database. Use it to declare which columns participate in that VIEW's
            // primary-key by setting 'IsPrimaryKey = true'.
            // If no columns are marked with 'IsPrimaryKey = true' then this T4 template defaults to marking all non-NULL columns as primary key columns.
            // To set-up Foreign-Key relationships between VIEWs and Tables (or even other VIEWs) use the 'AddForeignKeys' callback below.
            Settings.ViewProcessing = (Table view) =>
            {
                // Below is example code for the Northwind database that configures the 'VIEW [Orders Qry]' and 'VIEW [Invoices]'
                //switch(view.Name)
                //{
                //case "Orders Qry":
                //    // VIEW [Orders Qry] uniquely identifies rows with the 'OrderID' column:
                //    view.Columns.Single( col => col.Name == "OrderID" ).IsPrimaryKey = true;
                //    break;
                //case "Invoices":
                //    // VIEW [Invoices] has a composite primary key (OrderID+ProductID), so both columns must be marked as a Primary Key:
                //    foreach( Column col in view.Columns.Where( c => c.Name == "OrderID" || c.Name == "ProductID" ) ) col.IsPrimaryKey = true;
                //    break;
                //}
            };

            Settings.AddForeignKeys = (List<ForeignKey> foreignKeys, Tables tablesAndViews) =>
            {
                // In Northwind:
                // [Orders] (Table) to [Invoices] (View) is one-to-many using Orders.OrderID = Invoices.OrderID
                // [Order Details] (Table) to [Invoices] (View) is one-to-zeroOrOne - but uses a composite-key: ( [Order Details].OrderID,ProductID = [Invoices].OrderID,ProductID )
                // [Orders] (Table) to [Orders Qry] (View) is one-to-zeroOrOne ( [Orders].OrderID = [Orders Qry].OrderID )

                // AddRelationship is a helper function that creates ForeignKey objects and adds them to the foreignKeys list:
                //AddRelationship( foreignKeys, tablesAndViews, "orders_to_invoices"      , "dbo", "Orders"       , "OrderID"                       , "dbo", "Invoices", "OrderID" );
                //AddRelationship( foreignKeys, tablesAndViews, "orderDetails_to_invoices", "dbo", "Order Details", new[] { "OrderID", "ProductID" }, "dbo", "Invoices",  new[] { "OrderID", "ProductID" } );
                //AddRelationship( foreignKeys, tablesAndViews, "orders_to_ordersQry"     , "dbo", "Orders"       , "OrderID"                       , "dbo", "Orders Qry", "OrderID" );
            };

            // StoredProcedure renaming ************************************************************************************************************
            // Use the following function to rename stored procs such as sp_CreateOrderHistory to CreateOrderHistory, my_sp_shipments to Shipments, etc.
            // Example:
            /*Settings.StoredProcedureRename = (sp) =>
            {
                if (sp.NameHumanCase.StartsWith("sp_"))
                    return sp.NameHumanCase.Remove(0, 3);
                return sp.NameHumanCase.Replace("my_sp_", "");
            };*/
            Settings.StoredProcedureRename = (sp) => sp.NameHumanCase;   // Do nothing by default

            // Use the following function to rename the return model automatically generated for stored procedure.
            // By default it's <proc_name>ReturnModel.
            // Example:
            /*Settings.StoredProcedureReturnModelRename = (name, sp) =>
            {
                if (sp.NameHumanCase.Equals("ComputeValuesForDate", StringComparison.InvariantCultureIgnoreCase))
                    return "ValueSet";
                if (sp.NameHumanCase.Equals("SalesByYear", StringComparison.InvariantCultureIgnoreCase))
                    return "SalesSet";

                return name;
            };*/
            Settings.StoredProcedureReturnModelRename = (name, sp) => name; // Do nothing by default

            // StoredProcedure return types *******************************************************************************************************
            // Override generation of return models for stored procedures that return entities.
            // If a stored procedure returns an entity, add it to the list below.
            // This will suppress the generation of the return model, and instead return the entity.
            // Example:                       Proc name      Return this entity type instead
            //StoredProcedureReturnTypes.Add("SalesByYear", "SummaryOfSalesByYear");


            // Callbacks **********************************************************************************************************************
            // This method will be called right before we write the POCO header.
            this.WritePocoClassAttributes = (o, t) =>
            {
                if (Settings.UseDataAnnotations)
                {
                    foreach (var dataAnnotation in t.DataAnnotations)
                    {
                        o?.WriteLine("    [" + dataAnnotation + "]");
                    }
                }

                // Example:
                // if(t.ClassName.StartsWith("Order"))
                //     WriteLine("    [SomeAttribute]");
            };

            // This method will be called right before we write the POCO header.
            this.WritePocoClassExtendedComments = (o, t) =>
            {
                if (Settings.IncludeExtendedPropertyComments != CommentsStyle.None && !string.IsNullOrEmpty(t.ExtendedProperty))
                {
                    var lines = t.ExtendedProperty
                        .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                    o?.WriteLine("    ///<summary>");
                    foreach (var line in lines.Select(x => x.Replace("///", string.Empty).Trim()))
                    {
                        o?.WriteLine("    /// {0}", System.Security.SecurityElement.Escape(line));
                    }
                    o?.WriteLine("    ///</summary>");
                }
            };

            // Writes optional base classes
            this.WritePocoBaseClasses = (t) =>
            {
                //if (t.ClassName == "User")
                //    return ": IdentityUser<int, CustomUserLogin, CustomUserRole, CustomUserClaim>";

                // Or use the maker class to dynamically build more complex definitions
                /* Example:
                var r = new BaseClassMaker("POCO.Sample.Data.MetaModelObject");
                r.AddInterface("POCO.Sample.Data.IObjectWithTableName");
                r.AddInterface("POCO.Sample.Data.IObjectWithId",
                    t.Columns.Any(x => x.IsPrimaryKey && !x.IsNullable && x.NameHumanCase.Equals("Id", StringComparison.InvariantCultureIgnoreCase) && x.PropertyType == "long"));
                r.AddInterface("POCO.Sample.Data.IObjectWithUserId",
                    t.Columns.Any(x => !x.IsPrimaryKey && !x.IsNullable && x.NameHumanCase.Equals("UserId", StringComparison.InvariantCultureIgnoreCase) && x.PropertyType == "long"));
                return r.ToString();
                */
                return "";
            };

            // Writes any boilerplate stuff inside the POCO class
            this.WritePocoBaseClassBody = (o, t) =>
            {
                // Do nothing by default
                // Example:
                // WriteLine("        // " + t.ClassName);
            };

            this.WritePocoColumn = (o, c) =>
            {
                bool commentWritten = false;
                if ((Settings.IncludeExtendedPropertyComments == CommentsStyle.InSummaryBlock ||
                     Settings.IncludeComments == CommentsStyle.InSummaryBlock) &&
                    !string.IsNullOrEmpty(c.SummaryComments))
                {
                    o?.WriteLine(string.Empty);
                    o?.WriteLine("///<summary>");
                    o?.WriteLine("/// {0}", System.Security.SecurityElement.Escape(c.SummaryComments));
                    o?.WriteLine("///</summary>");
                    commentWritten = true;
                }
                if (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent)
                {
                    if (c.Ordinal > 1 && !commentWritten)
                        o?.WriteLine(string.Empty);    // Leave a blank line before the next property

                    foreach (var dataAnnotation in c.DataAnnotations)
                    {
                        o?.WriteLine("        [" + dataAnnotation + "]");
                    }
                }

                // Example of adding a [Required] data annotation attribute to all non-null fields
                //if (!c.IsNullable)
                //    return "        [System.ComponentModel.DataAnnotations.Required] " + c.Entity;

                o?.WriteLine(c.Entity);
            };

            Settings.ForeignKeyFilter = (ForeignKey fk) =>
            {
                // Return null to exclude this foreign key, or set IncludeReverseNavigation = false
                // to include the foreign key but not generate reverse navigation properties.
                // Example, to exclude all foreign keys for the Categories table, use:
                // if (fk.PkTableName == "Categories")
                //    return null;

                // Example, to exclude reverse navigation properties for tables ending with Type, use:
                // if (fk.PkTableName.EndsWith("Type"))
                //    fk.IncludeReverseNavigation = false;

                // You can also change the access modifier of the foreign-key's navigation property:
                // if(fk.PkTableName == "Categories") fk.AccessModifier = "internal";

                return fk;
            };

            Settings.ForeignKeyProcessing = (foreignKeys, fkTable, pkTable, anyNullableColumnInForeignKey) =>
            {
                var foreignKey = foreignKeys.First();

                // If using data annotations and to include the [Required] attribute in the foreign key, enable the following
                //if (!anyNullableColumnInForeignKey)
                //   foreignKey.IncludeRequiredAttribute = true;

                return foreignKey;
            };

            Settings.ForeignKeyName = (tableName, foreignKey, foreignKeyName, relationship, attempt) =>
            {
                string fkName;

                // 5 Attempts to correctly name the foreign key
                switch (attempt)
                {
                    case 1:
                        // Try without appending foreign key name
                        fkName = tableName;
                        break;

                    case 2:
                        // Only called if foreign key name ends with "id"
                        // Use foreign key name without "id" at end of string
                        fkName = foreignKeyName.Remove(foreignKeyName.Length - 2, 2);
                        break;

                    case 3:
                        // Use foreign key name only
                        fkName = foreignKeyName;
                        break;

                    case 4:
                        // Use table name and foreign key name
                        fkName = tableName + "_" + foreignKeyName;
                        break;

                    case 5:
                        // Used in for loop 1 to 99 to append a number to the end
                        fkName = tableName;
                        break;

                    default:
                        // Give up
                        fkName = tableName;
                        break;
                }

                // Apply custom foreign key renaming rules. Can be useful in applying pluralization.
                // For example:
                /*if (tableName == "Employee" && foreignKey.FkColumn == "ReportsTo")
                    return "Manager";

                if (tableName == "Territories" && foreignKey.FkTableName == "EmployeeTerritories")
                    return "Locations";

                if (tableName == "Employee" && foreignKey.FkTableName == "Orders" && foreignKey.FkColumn == "EmployeeID")
                    return "ContactPerson";
                */

                // FK_TableName_FromThisToParentRelationshipName_FromParentToThisChildsRelationshipName
                // (e.g. FK_CustomerAddress_Customer_Addresses will extract navigation properties "address.Customer" and "customer.Addresses")
                // Feel free to use and change the following
                /*if (foreignKey.ConstraintName.StartsWith("FK_") && foreignKey.ConstraintName.Count(x => x == '_') == 3)
                {
                    var parts = foreignKey.ConstraintName.Split('_');
                    if (!string.IsNullOrWhiteSpace(parts[2]) && !string.IsNullOrWhiteSpace(parts[3]) && parts[1] == foreignKey.FkTableName)
                    {
                        if (relationship == Relationship.OneToMany)
                            fkName = parts[3];
                        else if (relationship == Relationship.ManyToOne)
                            fkName = parts[2];
                    }
                }*/

                return fkName;
            };

            Settings.ForeignKeyAnnotationsProcessing = (Table fkTable, Table pkTable, string propName, string fkPropName) =>
            {
                /* Example:
                // Each navigation property that is a reference to User are left intact
                if (pkTable.NameHumanCase.Equals("User") && propName.Equals("User"))
                    return null;

                // all the others are marked with this attribute
                return new[] { "System.Runtime.Serialization.IgnoreDataMember" };
                */

                // Example, to include Inverse Property when using Data Annotations, use:
                // if (Settings.UseDataAnnotations && fkPropName != string.Empty)
                //     return new[] { "InverseProperty(\"" + fkPropName + "\")" };

                return null;
            };

            // Return true to include this table in the db context
            Settings.ConfigurationFilter = (Table t) =>
            {
                return true;
            };

            // That's it, nothing else to configure ***********************************************************************************************

            #endregion
        }

        List<string> usingsContext;
        List<string> usingsAll;

        public void GenerateMultipleFiles()
        {
            Settings.GenerateSeparateFiles = true;
            DefineUsings();

            // Read schema
            Settings.Tables = LoadTables();
            Settings.StoredProcs = LoadStoredProcs();

            Generate();
        }
        public void GenerateSingleFile(string file)
        {
            Settings.GenerateSeparateFiles = false;
            DefineUsings();
            if (file != null && !file.ToLower().EndsWith(Settings.FileExtension.ToLower()))
                file += Settings.FileExtension;
            StartNewFile(file, writeHeader: false);

            // Read schema
            Settings.Tables = LoadTables();
            Settings.StoredProcs = LoadStoredProcs();

            this.WriteFileHeader();

            Generate();
        }

        void DefineUsings()
        {
            usingsContext = new List<string>();
            usingsAll = new List<string>();
            usingsAll.AddRange(Settings.AdditionalNamespaces.Where(x => !string.IsNullOrEmpty(x)));
            if ((Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration) ||
                 Settings.ElementsToGenerate.HasFlag(Elements.Context) ||
                 Settings.ElementsToGenerate.HasFlag(Elements.UnitOfWork)) &&
                (!Settings.ElementsToGenerate.HasFlag(Elements.Poco) && !string.IsNullOrWhiteSpace(Settings.PocoNamespace)))
                usingsAll.Add(Settings.PocoNamespace);

            if (Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration) &&
                (!Settings.ElementsToGenerate.HasFlag(Elements.Context) && !string.IsNullOrWhiteSpace(Settings.ContextNamespace)))
                usingsAll.Add(Settings.ContextNamespace);

            if (Settings.ElementsToGenerate.HasFlag(Elements.Context) &&
                (!Settings.ElementsToGenerate.HasFlag(Elements.UnitOfWork) && !string.IsNullOrWhiteSpace(Settings.UnitOfWorkNamespace)))
                usingsAll.Add(Settings.UnitOfWorkNamespace);

            if (Settings.ElementsToGenerate.HasFlag(Elements.Context) &&
                (!Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration) && !string.IsNullOrWhiteSpace(Settings.PocoConfigurationNamespace)))
                usingsAll.Add(Settings.PocoConfigurationNamespace);

            if (Settings.ElementsToGenerate.HasFlag(Elements.Context))
            {
                if (Settings.AddUnitTestingDbContext || Settings.StoredProcs.Any())
                {
                    usingsContext.Add("System.Linq");
                }
            }
            if (!Settings.GenerateSeparateFiles)
            {
                usingsAll.AddRange(usingsContext);
            }
        }

        private void Generate()
        {
            #region All this code mostly came from Simon Hughes T4 templates (with minor adjustments) - Database.tt - see https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator

            if (Settings.Tables.Count == 0 && Settings.StoredProcs.Count > 0)
                return;

            #region All this code mostly came from Simon Hughes T4 templates (with minor adjustments) - EF.Reverse.POCO.ttinclude - see https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator
            #region Namespace / whole file (Line 31 to 1226)
            //_output?.WriteLine($"namespace { Settings.Namespace }"); // Line 31 // this is in StartNewFile()
            //_output?.WriteLine("{"); // this is in StartNewFile()
            //using (_output?.WithIndent()) // this is in StartNewFile()
            {
                Console.WriteLine("Unit of Work...");
                #region Unit of Work (Line 77 to 200)
                if (Settings.ElementsToGenerate.HasFlag(Elements.UnitOfWork) && !string.IsNullOrWhiteSpace(Settings.DbContextInterfaceName)) // Line 72
                {
                    if (Settings.GenerateSeparateFiles)
                        StartNewFile(Settings.DbContextInterfaceName + Settings.FileExtension);

                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("#region Unit of work\n"); // line 77

                    using (w?.WithCBlock($"{Settings.DbContextInterfaceModifiers ?? "public partial"} interface {Settings.DbContextInterfaceName} : {Settings.DbContextInterfaceBaseClasses}"))
                    {
                        foreach (Table tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).OrderBy(x => x.NameHumanCase))
                        {
                            w?.Write($"System.Data.Entity.DbSet<{ tbl.NameHumanCaseWithSuffix() }> { Inflector.MakePlural(tbl.NameHumanCase) } {{ get; set; }}");
                            w?.WriteLine(Settings.IncludeComments == CommentsStyle.None ? "" : $" // {tbl.Name}");
                        }
                        w?.WriteLine();

                        foreach (string s in Settings.AdditionalContextInterfaceItems.Where(x => !string.IsNullOrEmpty(x)))
                            w?.WriteLine(s);
                        if (!Settings.UseInheritedBaseInterfaceFunctions)
                        {
                            w?.WriteLine("int SaveChanges();");
                            if (Settings.IsSupportedFrameworkVersion("4.5"))
                            {
                                w?.WriteLine("System.Threading.Tasks.Task<int> SaveChangesAsync();");
                                w?.WriteLine("System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken);");
                            }
                            w?.WriteLine($@"
                                System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker {{ get; }}
                                System.Data.Entity.Infrastructure.DbContextConfiguration Configuration {{ get; }}
                                System.Data.Entity.Database Database {{ get; }}
                                System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
                                System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity);
                                System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors();
                                System.Data.Entity.DbSet Set(System.Type entityType);
                                System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class;
                                string ToString();
                                ");
                        }
                        if (Settings.StoredProcs.Any())
                        {
                            w?.WriteLine("// Stored Procedures"); // Line 117
                            foreach (StoredProcedure sp in Settings.StoredProcs.Where(s => !s.IsTVF).OrderBy(x => x.NameHumanCase))
                            {
                                int returnModelsCount = sp.ReturnModels.Count;
                                if (returnModelsCount == 1)
                                {
                                    w?.WriteLine($"{WriteStoredProcReturnType(sp)} {WriteStoredProcFunctionName(sp)}({WriteStoredProcFunctionParams(sp, false)});");
                                    w?.WriteLine($"{WriteStoredProcReturnType(sp)} {WriteStoredProcFunctionName(sp)}({WriteStoredProcFunctionParams(sp, true)});");
                                }
                                else
                                {
                                    w?.WriteLine($"{WriteStoredProcReturnType(sp)} {WriteStoredProcFunctionName(sp)}({WriteStoredProcFunctionParams(sp, false)});");
                                }
                                if (Settings.IsSupportedFrameworkVersion("4.5"))
                                {
                                    if (StoredProcHasOutParams(sp) || sp.ReturnModels.Count == 0)
                                    {
                                        w?.WriteLine($"// {WriteStoredProcFunctionName(sp)}Async cannot be created due to having out parameters, or is relying on the procedure result ({WriteStoredProcReturnType(sp)})");
                                    }
                                    else
                                    {
                                        w?.WriteLine($"System.Threading.Tasks.Task<{WriteStoredProcReturnType(sp)}> {WriteStoredProcFunctionName(sp)}Async({WriteStoredProcFunctionParams(sp, false)});");
                                    }
                                }
                                w?.WriteLine();
                            }
                            if (Settings.IncludeTableValuedFunctions)
                            {
                                w?.WriteLine("// Table Valued Functions");
                                foreach (StoredProcedure sp in Settings.StoredProcs.Where(s => s.IsTVF).OrderBy(x => x.NameHumanCase))
                                {
                                    string spExecName = WriteStoredProcFunctionName(sp);
                                    string spReturnClassName = WriteStoredProcReturnModelName(sp);
                                    w?.WriteLine($"[System.Data.Entity.DbFunction(\" { Settings.DbContextName} \", \"{ sp.Name} \")]");
                                    w?.Write($"[CodeFirstStoreFunctions.DbFunctionDetails(DatabaseSchema = \"{sp.Schema}\"");
                                    if (sp.ReturnModels.Count == 1 && sp.ReturnModels[0].Count == 1)
                                        w?.Write($", ResultColumnName = \"<{sp.ReturnModels[0][0].ColumnName});(");
                                    w?.WriteLine(")]");
                                    w?.WriteLine($"System.Linq.IQueryable<{ spReturnClassName }> { spExecName }({WriteStoredProcFunctionParams(sp, false)});");
                                }
                            }

                        }
                    }
                    w?.WriteLine("\n");
                }

                Console.WriteLine("Db Migration Configuration...");
                #region Db Migration Configuration (Line 161 to 196)
                if (!string.IsNullOrWhiteSpace(Settings.MigrationConfigurationFileName))
                {
                    if (Settings.GenerateSeparateFiles)
                        StartNewFile(Settings.MigrationConfigurationFileName + Settings.FileExtension);
                    if (!Settings.GenerateSeparateFiles)
                    {
                        w?.WriteLine($@"
                            // ************************************************************************
                            // Db Migration Configuration
                        ");
                    }
                    if (Settings.IncludeCodeGeneratedAttribute)
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($"{Settings.MigrationClassModifiers} class {Settings.MigrationConfigurationFileName}: System.Data.Entity.Migrations.DbMigrationsConfiguration<{Settings.DbContextName }> "))
                    {
                        using (w?.WithCBlock($"public {Settings.MigrationConfigurationFileName}()"))
                        {
                            w?.WriteLine($"AutomaticMigrationsEnabled = { Settings.AutomaticMigrationsEnabled.ToString() };");
                            w?.WriteLine($"AutomaticMigrationDataLossAllowed = { Settings.AutomaticMigrationDataLossAllowed.ToString() };");
                            if (!string.IsNullOrEmpty(Settings.ContextKey))
                                w?.WriteLine($@"ContextKey = ""{ Settings.ContextKey }"";");
                        }
                        w?.WriteLine(@"
                            //protected override void Seed({Settings.DbContextName} context)
                            //{

                                // This method will be called after migrating to the latest version.

                                // You can use the DbSet<T>.AddOrUpdate() helper extension method
                                // to avoid creating duplicate seed data. E.g.
                                //
                                //   context.People.AddOrUpdate(
                                //     p => p.FullName,
                                //     new Person { FullName = ""Andrew Peters"" },
                                //     new Person { FullName = ""Brice Lambson"" },
                                //     new Person { FullName = ""Rowan Miller"" }
                                //   );
                                //
                            //}
                            ");
                    }
                }
                #endregion Db Migration Configuration (Line 161 to 196)

                if (Settings.ElementsToGenerate.HasFlag(Elements.UnitOfWork) && !string.IsNullOrWhiteSpace(Settings.DbContextInterfaceName) && !Settings.GenerateSeparateFiles)
                    w?.WriteLine("#endregion\n"); // line 200
                #endregion Unit of Work (Line 77 to 200)


                Console.WriteLine("Database context...");
                #region Database context (Line 203 to 509)
                if (Settings.ElementsToGenerate.HasFlag(Elements.Context))
                {
                    if (Settings.GenerateSeparateFiles)
                        StartNewFile(Settings.DbContextName + Settings.FileExtension);
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("#region Database context\n"); // line 206
                    else foreach (var usingStatement in usingsContext.Distinct().OrderBy(x => x))
                            w?.WriteLine($"using { usingStatement };");
                    if (Settings.IncludeCodeGeneratedAttribute)
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($"{ Settings.DbContextClassModifiers } class {Settings.DbContextName} : {Settings.DbContextBaseClass}{ (string.IsNullOrWhiteSpace(Settings.DbContextInterfaceName) ? "" : ", " + Settings.DbContextInterfaceName)}"))
                    {
                        foreach (Table tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).OrderBy(x => x.NameHumanCase))
                        {
                            // 220 to 
                            w?.Write($"public System.Data.Entity.DbSet<{tbl.NameHumanCaseWithSuffix()}> {Inflector.MakePlural(tbl.NameHumanCase)} {{ get; set; }}");
                            if (Settings.IncludeComments != CommentsStyle.None)
                                w?.WriteLine($" // {tbl.Name}");
                            else
                                w?.WriteLine($"");
                        }

                        w?.WriteLine($"");
                        using (w?.WithCBlock($"static {Settings.DbContextName}()"))
                        {
                            if (string.IsNullOrWhiteSpace(Settings.MigrationConfigurationFileName))
                                w?.WriteLine($"System.Data.Entity.Database.SetInitializer<{Settings.DbContextName}>(null);");
                            else
                                w?.WriteLine($"System.Data.Entity.Database.SetInitializer(new System.Data.Entity.{Settings.MigrationStrategy}<{Settings.DbContextName}{ (Settings.MigrationStrategy == "MigrateDatabaseToLatestVersion" ? ", " + Settings.MigrationConfigurationFileName : "") }>());");
                        }
                        w?.WriteLine("");
                        if (Settings.AddParameterlessConstructorToDbContext)
                        {
                            using (w?.WithCBlock($"public {Settings.DbContextName}(){ (Settings.DefaultConstructorArgument == null ? "" : $" : base({ Settings.DefaultConstructorArgument})")}"))
                            {
                                if (Settings.DbContextClassIsPartial())
                                    w?.WriteLine("InitializePartial();");
                            }
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public {Settings.DbContextName}(string connectionString) : base(connectionString)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public {Settings.DbContextName}(string connectionString, System.Data.Entity.Infrastructure.DbCompiledModel model) : base(connectionString, model)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public {Settings.DbContextName}(System.Data.Common.DbConnection existingConnection, bool contextOwnsConnection) : base(existingConnection, contextOwnsConnection)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public {Settings.DbContextName}(System.Data.Common.DbConnection existingConnection, System.Data.Entity.Infrastructure.DbCompiledModel model, bool contextOwnsConnection) : base(existingConnection, model, contextOwnsConnection)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public {Settings.DbContextName}(System.Data.Entity.Core.Objects.ObjectContext objectContext, bool dbContextOwnsObjectContext) : base(objectContext, dbContextOwnsObjectContext)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"protected override void Dispose(bool disposing)"))
                        {
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("DisposePartial(disposing);");
                            w?.WriteLine("base.Dispose(disposing);");
                        }

                        if (!Settings.IsSqlCe)
                        {
                            w?.WriteLine($@"

                                    public bool IsSqlParameterNull(System.Data.SqlClient.SqlParameter param)
                                    {{
                                        var sqlValue = param.SqlValue;
                                        var nullableValue = sqlValue as System.Data.SqlTypes.INullable;
                                        if (nullableValue != null)
                                            return nullableValue.IsNull;
                                        return (sqlValue == null || sqlValue == System.DBNull.Value);
                                    }}");
                        }

                        w?.WriteLine();
                        using (w?.WithCBlock($"protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)"))
                        {
                            w?.WriteLine($"base.OnModelCreating(modelBuilder);\n");
                            if (Settings.StoredProcs.Any() && Settings.IncludeTableValuedFunctions)
                            {
                                w?.WriteLine($"modelBuilder.Conventions.Add(new CodeFirstStoreFunctions.FunctionsConvention<{Settings.DbContextName}>(\"dbo\"));");
                                foreach (var sp in Settings.StoredProcs.Where(s => s.IsTVF && !Settings.StoredProcedureReturnTypes.ContainsKey(s.NameHumanCase) && !Settings.StoredProcedureReturnTypes.ContainsKey(s.Name)).OrderBy(x => x.NameHumanCase))
                                {
                                    w?.WriteLine($"modelBuilder.ComplexType<{WriteStoredProcReturnModelName(sp)}>();");
                                }
                            }
                            foreach (var tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).Where(Settings.ConfigurationFilter).OrderBy(x => x.NameHumanCase))
                            {
                                w?.WriteLine($"modelBuilder.Configurations.Add(new {tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName}());");
                            }
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("OnModelCreatingPartial(modelBuilder);");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock("public static System.Data.Entity.DbModelBuilder CreateModel(System.Data.Entity.DbModelBuilder modelBuilder, string schema)"))
                        {
                            foreach (var tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).Where(Settings.ConfigurationFilter).OrderBy(x => x.NameHumanCase))
                            {
                                w?.WriteLine($"modelBuilder.Configurations.Add(new {tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName}(schema));");
                            }
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("OnCreateModelPartial(modelBuilder, schema);");
                            w?.WriteLine("return modelBuilder;");
                        }

                        if (Settings.DbContextClassIsPartial()) // Line 337
                        {
                            w?.WriteLine($@"
                                partial void InitializePartial();
                                partial void DisposePartial(bool disposing);
                                partial void OnModelCreatingPartial(System.Data.Entity.DbModelBuilder modelBuilder);
                                static partial void OnCreateModelPartial(System.Data.Entity.DbModelBuilder modelBuilder, string schema);
                                ");

                        }

                        #region Stored Procedures (Line 344 to 487)
                        if (Settings.StoredProcs.Any()) // Line 344
                        {
                            w?.WriteLine();
                            w?.WriteLine("// Stored Procedures");
                            foreach (var sp in Settings.StoredProcs.Where(s => !s.IsTVF).OrderBy(x => x.NameHumanCase)) // Line 349
                            {
                                string spReturnClassName = WriteStoredProcReturnModelName(sp);
                                string spExecName = WriteStoredProcFunctionName(sp);
                                int returnModelsCount = sp.ReturnModels.Count;
                                #region 354 to 486

                                if (returnModelsCount > 0) // Line 354?
                                {
                                    if (returnModelsCount == 1) // Line 356
                                    {
                                        // Line 358 to 362
                                        w?.WriteLine($@"
                                            public {WriteStoredProcReturnType(sp) } {WriteStoredProcFunctionName(sp) }({WriteStoredProcFunctionParams(sp, false) })
                                            {{
                                                int procResult;
                                                return { spExecName }({WriteStoredProcFunctionOverloadCall(sp) });
                                            }}
                                        ");
                                    }
                                    using (w?.WithCBlock($@"public {WriteStoredProcReturnType(sp) } {WriteStoredProcFunctionName(sp) }({WriteStoredProcFunctionParams(sp, (returnModelsCount == 1)) })"))
                                    {
                                        WriteStoredProcFunctionDeclareSqlParameter(w, sp, true);
                                        if (returnModelsCount == 1)
                                        {
                                            var exec = string.Format("EXEC @procResult = [{0}].[{1}] {2}", sp.Schema, sp.Name, WriteStoredProcFunctionSqlAtParams(sp));
                                            w?.WriteLine($"var procResultData = Database.SqlQuery<{ spReturnClassName }>(\"{ exec }\", { WriteStoredProcFunctionSqlParameterAnonymousArray(sp, true) }).ToList();");
                                            WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                            w?.WriteLine("\nprocResult = (int) procResultParam.Value;");
                                        }
                                        else
                                        {
                                            var exec = string.Format("[{0}].[{1}]", sp.Schema, sp.Name);
                                            WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                            w?.WriteLine($@"
                                            var procResultData = new { spReturnClassName }();
                                            var cmd = Database.Connection.CreateCommand();
                                            cmd.CommandType = System.Data.CommandType.StoredProcedure;
                                            cmd.CommandText = ""{ exec }"";");
                                            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
                                                w?.WriteLine($@"cmd.Parameters.Add({ WriteStoredProcSqlParameterName(p) });");

                                            using (w?.WithCBlock("try"))
                                            {
                                                w?.WriteLine($@"System.Data.Entity.Infrastructure.Interception.DbInterception.Dispatch.Connection.Open(Database.Connection, new System.Data.Entity.Infrastructure.Interception.DbInterceptionContext());");
                                                w?.WriteLine($@"var reader = cmd.ExecuteReader();");
                                                w?.WriteLine($@"var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter) this).ObjectContext;");
                                                w?.WriteLine($@"");
                                                int n = 0;
                                                var returnModelCount = sp.ReturnModels.Count;
                                                foreach (var returnModel in sp.ReturnModels)
                                                {
                                                    n++;
                                                    w?.WriteLine($@"procResultData.ResultSet{ n } = objectContext.Translate<{ spReturnClassName }.ResultSetModel{ n }>(reader).ToList();");
                                                    if (n < returnModelCount)
                                                        w?.WriteLine($@"reader.NextResult();");
                                                }
                                                w?.WriteLine($@"reader.Close();");
                                                WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                            }
                                            using (w?.WithCBlock("finally"))
                                            {
                                                w?.WriteLine("System.Data.Entity.Infrastructure.Interception.DbInterception.Dispatch.Connection.Close(Database.Connection, new System.Data.Entity.Infrastructure.Interception.DbInterceptionContext());");
                                            }
                                        }
                                        w?.WriteLine("return procResultData;");
                                    } // Line 417
                                    w?.WriteLine();
                                } // Line 419?
                                else
                                {
                                    using (w?.WithCBlock($@"public int { spExecName }({WriteStoredProcFunctionParams(sp, true) })"))
                                    {
                                        WriteStoredProcFunctionDeclareSqlParameter(w, sp, true);
                                        w?.WriteLine($@"Database.ExecuteSqlCommand(System.Data.Entity.TransactionalBehavior.DoNotEnsureTransaction, ""EXEC @procResult = [{sp.Schema }].[{ sp.Name } { WriteStoredProcFunctionSqlAtParams(sp) }"", { WriteStoredProcFunctionSqlParameterAnonymousArray(sp, true) });");
                                        WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                        w?.WriteLine("return (int) procResultParam.Value;");
                                    }
                                } // Line 430?
                                // Async
                                if (Settings.IsSupportedFrameworkVersion("4.5") && !StoredProcHasOutParams(sp) && returnModelsCount > 0) // Line 432
                                {
                                    using (w?.WithCBlock($@"public async System.Threading.Tasks.Task<{WriteStoredProcReturnType(sp) }> {WriteStoredProcFunctionName(sp) }Async({WriteStoredProcFunctionParams(sp, false) })"))
                                    {
                                        WriteStoredProcFunctionDeclareSqlParameter(w, sp, false);
                                        if (returnModelsCount == 1)
                                        {
                                            var parameters = WriteStoredProcFunctionSqlParameterAnonymousArray(sp, false);
                                            if (!string.IsNullOrWhiteSpace(parameters))
                                                parameters = ", " + parameters;
                                            var exec = string.Format("EXEC [{0}].[{1}] {2}", sp.Schema, sp.Name, WriteStoredProcFunctionSqlAtParams(sp));
                                            w?.WriteLine($@"var procResultData = await Database.SqlQuery<{ spReturnClassName }>(""{ exec }""{ parameters }).ToListAsync();");
                                            WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                        }
                                        else
                                        {
                                            var exec = string.Format("[{0}].[{1}]", sp.Schema, sp.Name);
                                            WriteStoredProcFunctionSetSqlParameters(w, sp, false);
                                            w?.WriteLine($@"var procResultData = new { spReturnClassName }();");
                                            w?.WriteLine("var cmd = Database.Connection.CreateCommand();");
                                            w?.WriteLine("cmd.CommandType = System.Data.CommandType.StoredProcedure;");
                                            w?.WriteLine($@"cmd.CommandText = ""{ exec }"";");
                                            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
                                                w?.WriteLine($@"cmd.Parameters.Add({ WriteStoredProcSqlParameterName(p) });");
                                            using (w?.WithCBlock("try"))
                                            {
                                                w?.WriteLine($@"await System.Data.Entity.Infrastructure.Interception.DbInterception.Dispatch.Connection.OpenAsync(Database.Connection, new System.Data.Entity.Infrastructure.Interception.DbInterceptionContext(), new System.Threading.CancellationToken()).ConfigureAwait(false);");
                                                w?.WriteLine($@"var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false);");
                                                w?.WriteLine($@"var objectContext = ((System.Data.Entity.Infrastructure.IObjectContextAdapter) this).ObjectContext;");
                                                int n = 0;
                                                var returnModelCount = sp.ReturnModels.Count;
                                                foreach (var returnModel in sp.ReturnModels)
                                                {
                                                    n++;
                                                    w?.WriteLine($@"procResultData.ResultSet{ n } = objectContext.Translate<{ spReturnClassName }.ResultSetModel{ n }>(reader).ToList();");
                                                    if (n < returnModelCount)
                                                        w?.WriteLine($@"await reader.NextResultAsync().ConfigureAwait(false);");
                                                }
                                            }
                                            using (w?.WithCBlock("finally"))
                                            {
                                                w?.WriteLine("    System.Data.Entity.Infrastructure.Interception.DbInterception.Dispatch.Connection.Close(Database.Connection, new System.Data.Entity.Infrastructure.Interception.DbInterceptionContext());");
                                            }
                                        }
                                        w?.WriteLine("\nreturn procResultData;");
                                    }
                                    w?.WriteLine();
                                } // 486?
                                #endregion
                            } // Line 486?
                        } // Line 487
                        #endregion Stored Procedures (Line 344 to 487)

                        Console.WriteLine("IncludeTableValuedFunctions...");
                        #region IncludeTableValuedFunctions (488 to 509)
                        if (Settings.IncludeTableValuedFunctions)
                        {
                            w?.WriteLine("// Table Valued Functions");
                            foreach (var sp in Settings.StoredProcs.Where(s => s.IsTVF).OrderBy(x => x.NameHumanCase))
                            {
                                string spExecName = WriteStoredProcFunctionName(sp);
                                string spReturnClassName = WriteStoredProcReturnModelName(sp);
                                w?.WriteLine($@"[System.Data.Entity.DbFunction(""{Settings.DbContextName}"", ""{sp.Name}"")]");
                                w?.WriteLine($@"[CodeFirstStoreFunctions.DbFunctionDetails(DatabaseSchema = ""{sp.Schema}""");
                                if (sp.ReturnModels.Count == 1 && sp.ReturnModels[0].Count == 1)
                                    w?.Write($", ResultColumnName = \"<{sp.ReturnModels[0][0].ColumnName});(");
                                w?.WriteLine(")]");
                                using (w?.WithCBlock($"public IQueryable<{ spReturnClassName }> { spExecName }({WriteStoredProcFunctionParams(sp, false)});"))
                                {
                                    string procParameters = WriteTableValuedFunctionDeclareSqlParameter(sp);
                                    if (!string.IsNullOrEmpty(procParameters))
                                        w?.WriteLine(procParameters);
                                    w?.WriteLine($@"return ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext.CreateQuery<{spReturnClassName}>(""[{ Settings.DbContextName}].[{sp.Name}]({ WriteStoredProcFunctionSqlAtParams(sp) })"", { WriteTableValuedFunctionSqlParameterAnonymousArray(sp) });");
                                }
                            }
                        }
                        #endregion



                    }
                    if (Settings.ElementsToGenerate.HasFlag(Elements.Context) && !Settings.GenerateSeparateFiles)
                        w?.WriteLine("#endregion\n"); // line 509
                }
                #endregion Database context (Line 203 to 509)

                Console.WriteLine("Database context factory...");
                #region Database context factory (Line 511 to 532)
                if (Settings.ElementsToGenerate.HasFlag(Elements.Context) && Settings.AddIDbContextFactory)
                {
                    if (Settings.GenerateSeparateFiles)
                        StartNewFile(Settings.DbContextName + "Factory" + Settings.FileExtension);
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("#region Database context factory\n"); // line 517
                    w?.WriteLine($@"
                        { Settings.DbContextClassModifiers } class { Settings.DbContextName + "Factory" } : System.Data.Entity.Infrastructure.IDbContextFactory<{ Settings.DbContextName }>
                        {{
                            public { Settings.DbContextName } Create()
                            {{
                                return new { Settings.DbContextName }();
                            }}
                        }}");
                    if (!Settings.GenerateSeparateFiles)
                    {
                        w?.WriteLine("\n#endregion"); // line 529
                    }

                }
                #endregion Database context factory (Line 511 to 532)


                Console.WriteLine("Fake Database context...");
                #region Fake Database context (Line 533 to 1002)
                if (Settings.ElementsToGenerate.HasFlag(Elements.Context) && Settings.AddUnitTestingDbContext)
                {
                    if (Settings.GenerateSeparateFiles)
                        StartNewFile("Fake" + Settings.DbContextName + Settings.FileExtension);
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#region Fake Database context\n"); // line 538
                    else foreach (var usingStatement in usingsContext.Distinct().OrderBy(x => x))
                            w?.WriteLine($"using { usingStatement };");
                    if (Settings.IncludeCodeGeneratedAttribute)
                        w?.WriteLine(CodeGeneratedAttribute);

                    Console.WriteLine("548...");
                    #region 548 to 1000
                    using (w?.WithCBlock($"{ Settings.DbContextClassModifiers } class Fake{Settings.DbContextName}{ (string.IsNullOrWhiteSpace(Settings.DbContextInterfaceName) ? "" : " : " + Settings.DbContextInterfaceName)}"))
                    {
                        foreach (Table tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).OrderBy(x => x.NameHumanCase)) // Line 551
                        {
                            w?.WriteLine($"public System.Data.Entity.DbSet<{tbl.NameHumanCaseWithSuffix()}> {Inflector.MakePlural(tbl.NameHumanCase)} {{ get; set; }}");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock($"public Fake{Settings.DbContextName}()"))
                        {
                            w?.WriteLine("_changeTracker = null;");
                            w?.WriteLine("_configuration = null;");
                            w?.WriteLine("_database = null;");
                            w?.WriteLine();

                            foreach (Table tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).OrderBy(x => x.NameHumanCase)) // Line 564
                                w?.WriteLine($@"{Inflector.MakePlural(tbl.NameHumanCase) } = new FakeDbSet<{tbl.NameHumanCaseWithSuffix() }>({ string.Join(", ", tbl.PrimaryKeys.Select(x => "\"" + x.NameHumanCase + "\"")) });");
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine($@"

                            public int SaveChangesCount {{ get; private set; }}
                            public int SaveChanges()
                            {{
                                ++SaveChangesCount;
                                return 1;
                            }}");

                        if (Settings.IsSupportedFrameworkVersion("4.5"))
                        {

                            w?.WriteLine($@"

                                public System.Threading.Tasks.Task<int> SaveChangesAsync()
                                {{
                                    ++SaveChangesCount;
                                    return System.Threading.Tasks.Task<int>.Factory.StartNew(() => 1);
                                }}

                                public System.Threading.Tasks.Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken)
                                {{
                                    ++SaveChangesCount;
                                    return System.Threading.Tasks.Task<int>.Factory.StartNew(() => 1, cancellationToken);
                                }}
                        ");
                        }

                        if (Settings.DbContextClassIsPartial())
                            w?.WriteLine("partial void InitializePartial();");

                        w?.WriteLine(@"
                            protected virtual void Dispose(bool disposing)
                            {
                            }

                            public void Dispose()
                            {
                                Dispose(true);
                            }

                            private System.Data.Entity.Infrastructure.DbChangeTracker _changeTracker;
                            public System.Data.Entity.Infrastructure.DbChangeTracker ChangeTracker { get { return _changeTracker; } }
                            private System.Data.Entity.Infrastructure.DbContextConfiguration _configuration;
                            public System.Data.Entity.Infrastructure.DbContextConfiguration Configuration { get { return _configuration; } }
                            private System.Data.Entity.Database _database;
                            public System.Data.Entity.Database Database { get { return _database; } }
                            public System.Data.Entity.Infrastructure.DbEntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class
                            {
                                throw new System.NotImplementedException();
                            }
                            public System.Data.Entity.Infrastructure.DbEntityEntry Entry(object entity)
                            {
                                throw new System.NotImplementedException();
                            }
                            public System.Collections.Generic.IEnumerable<System.Data.Entity.Validation.DbEntityValidationResult> GetValidationErrors()
                            {
                                throw new System.NotImplementedException();
                            }
                            public System.Data.Entity.DbSet Set(System.Type entityType)
                            {
                                throw new System.NotImplementedException();
                            }
                            public System.Data.Entity.DbSet<TEntity> Set<TEntity>() where TEntity : class
                            {
                                throw new System.NotImplementedException();
                            }
                            public override string ToString()
                            {
                                throw new System.NotImplementedException();
                            }
                            ");


                        if (Settings.StoredProcs.Any()) // Line 639
                        {
                            w?.WriteLine("// Stored Procedures");
                            foreach (StoredProcedure sp in Settings.StoredProcs.Where(s => !s.IsTVF).OrderBy(x => x.NameHumanCase)) // Line 644
                            {
                                string spReturnClassName = WriteStoredProcReturnModelName(sp);
                                string spExecName = WriteStoredProcFunctionName(sp);
                                int returnModelsCount = sp.ReturnModels.Count;
                                #region Lines 649 to 687
                                if (returnModelsCount > 0)
                                {
                                    using (w?.WithCBlock($@"public {WriteStoredProcReturnType(sp) } {WriteStoredProcFunctionName(sp) }({WriteStoredProcFunctionParams(sp, false) })"))
                                    {
                                        w?.WriteLine($@"int procResult;");
                                        w?.WriteLine($@"return {spExecName }({WriteStoredProcFunctionOverloadCall(sp) });");
                                    }

                                    w?.WriteLine("");

                                    using (w?.WithCBlock($@"public {WriteStoredProcReturnType(sp) } {WriteStoredProcFunctionName(sp) }({WriteStoredProcFunctionParams(sp, true) })"))
                                    {
                                        WriteStoredProcFunctionSetSqlParameters(w, sp, true);
                                        w?.WriteLine($"\nprocResult = 0;");
                                        w?.WriteLine($"return new {WriteStoredProcReturnType(sp) }();");
                                    }

                                    if (Settings.IsSupportedFrameworkVersion("4.5") && !StoredProcHasOutParams(sp) && returnModelsCount > 0)
                                    {
                                        w?.WriteLine($@"

                                        public System.Threading.Tasks.Task<{WriteStoredProcReturnType(sp)}> {WriteStoredProcFunctionName(sp) }Async({WriteStoredProcFunctionParams(sp, false) })
                                        {{
                                            int procResult;
                                            return System.Threading.Tasks.Task.FromResult({ spExecName }({WriteStoredProcFunctionOverloadCall(sp) }));
                                        }}
                                        ");
                                    }
                                }
                                else
                                {
                                    using (w?.WithCBlock($@"public int { spExecName }({WriteStoredProcFunctionParams(sp, true) })"))
                                    {
                                        WriteStoredProcFunctionSetSqlParameters(w, sp, true);
                                        w?.WriteLine($@"return 0;");
                                    }


                                    if (Settings.IsSupportedFrameworkVersion("4.5") && !StoredProcHasOutParams(sp) && returnModelsCount > 0)
                                    {
                                        using (w?.WithCBlock($@"public System.Threading.Tasks.Task<int> { spExecName }Async({WriteStoredProcFunctionParams(sp, false) })"))
                                        {
                                            WriteStoredProcFunctionSetSqlParameters(w, sp, true);
                                            w?.WriteLine($@"return System.Threading.Tasks.Task.FromResult(0);");
                                        }
                                    }
                                }
                                #endregion Lines 649 to 687
                            }
                        }
                        #region IncludeTableValuedFunctions (Lines 688 to 705)
                        if (Settings.IncludeTableValuedFunctions)
                        {
                            w?.WriteLine("// Table Valued Functions");
                            foreach (StoredProcedure spTvf in Settings.StoredProcs.Where(s => s.IsTVF).OrderBy(x => x.NameHumanCase))
                            {
                                string spExecNamespTvf = WriteStoredProcFunctionName(spTvf);
                                string spReturnClassName = WriteStoredProcReturnModelName(spTvf);
                                w?.WriteLine($@"
                                    [System.Data.Entity.DbFunction(""{ Settings.DbContextName}"", ""{ spTvf.Name}"")]
                                    public IQueryable<{ spReturnClassName }> { spExecNamespTvf } ({WriteStoredProcFunctionParams(spTvf, false)})
                                    {{
                                        return new System.Collections.Generic.List<{ spReturnClassName }>().AsQueryable();
                                    }}
                                    ");
                            }
                        }
                        #endregion IncludeTableValuedFunctions (Lines 688 to 705)


                    } // end of DbContextName


                    if (Settings.GenerateSeparateFiles)
                        StartNewFile("FakeDbSet" + Settings.FileExtension);
                    if (Settings.GenerateSeparateFiles)
                        w?.WriteLine("using System.Linq;");
                    w?.WriteLine(@"
                        // ************************************************************************
                        // Fake DbSet
                        // Implementing Find:
                        //      The Find method is difficult to implement in a generic fashion. If
                        //      you need to test code that makes use of the Find method it is
                        //      easiest to create a test DbSet for each of the entity types that
                        //      need to support find. You can then write logic to find that
                        //      particular type of entity, as shown below:
                        //      public class FakeBlogDbSet : FakeDbSet<Blog>
                        //      {
                        //          public override Blog Find(params object[] keyValues)
                        //          {
                        //              var id = (int) keyValues.Single();
                        //              return this.SingleOrDefault(b => b.BlogId == id);
                        //          }
                        //      }
                        //      Read more about it here: https://msdn.microsoft.com/en-us/data/dn314431.aspx
                        ");
                    if (Settings.IncludeCodeGeneratedAttribute)
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($@"{ Settings.DbContextClassModifiers } class FakeDbSet<TEntity> : System.Data.Entity.DbSet<TEntity>, IQueryable, System.Collections.Generic.IEnumerable<TEntity>{ (Settings.IsSupportedFrameworkVersion("4.5") ? ", System.Data.Entity.Infrastructure.IDbAsyncEnumerable<TEntity>" : "") } where TEntity : class"))
                    {
                        w?.WriteLine("private readonly System.Reflection.PropertyInfo[] _primaryKeys;");
                        w?.WriteLine("private readonly System.Collections.ObjectModel.ObservableCollection<TEntity> _data;");
                        w?.WriteLine("private readonly IQueryable _query;");

                        w?.WriteLine("");
                        using (w?.WithCBlock("public FakeDbSet()"))
                        {
                            w?.WriteLine("_data = new System.Collections.ObjectModel.ObservableCollection<TEntity>();");
                            w?.WriteLine("_query = _data.AsQueryable();");
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }

                        w?.WriteLine("");
                        using (w?.WithCBlock("public FakeDbSet(params string[] primaryKeys)"))
                        {
                            w?.WriteLine("_primaryKeys = typeof(TEntity).GetProperties().Where(x => primaryKeys.Contains(x.Name)).ToArray();");
                            w?.WriteLine("_data = new System.Collections.ObjectModel.ObservableCollection<TEntity>();");
                            w?.WriteLine("_query = _data.AsQueryable();");
                            if (Settings.DbContextClassIsPartial())
                                w?.WriteLine("InitializePartial();");
                        }


                        w?.WriteLine($@"

                            public override TEntity Find(params object[] keyValues)
                            {{
                                if (_primaryKeys == null)
                                    throw new System.ArgumentException(""No primary keys defined"");
                                if (keyValues.Length != _primaryKeys.Length)
                                    throw new System.ArgumentException(""Incorrect number of keys passed to Find method"");

                                var keyQuery = this.AsQueryable();
                                keyQuery = keyValues
                                    .Select((t, i) => i)
                                    .Aggregate(keyQuery,
                                        (current, x) =>
                                            current.Where(entity => _primaryKeys[x].GetValue(entity, null).Equals(keyValues[x])));

                                return keyQuery.SingleOrDefault();
                            }}
                            ");

                        if (Settings.IsSupportedFrameworkVersion("4.5"))
                        {
                            w?.WriteLine($@"
                            public override System.Threading.Tasks.Task<TEntity> FindAsync(System.Threading.CancellationToken cancellationToken, params object[] keyValues)
                            {{
                                return System.Threading.Tasks.Task<TEntity>.Factory.StartNew(() => Find(keyValues), cancellationToken);
                            }}

                            public override System.Threading.Tasks.Task<TEntity> FindAsync(params object[] keyValues)
                            {{
                                return System.Threading.Tasks.Task<TEntity>.Factory.StartNew(() => Find(keyValues));
                            }}
");

                        }

                        w?.WriteLine($@"
                            public override System.Collections.Generic.IEnumerable<TEntity> AddRange(System.Collections.Generic.IEnumerable<TEntity> entities)
                            {{
                                if (entities == null) throw new System.ArgumentNullException(""entities"");
                                var items = entities.ToList();
                                foreach (var entity in items)
                                {{
                                    _data.Add(entity);
                                }}
                                return items;
                            }}

                            public override TEntity Add(TEntity item)
                            {{
                                if (item == null) throw new System.ArgumentNullException(""item"");
                                _data.Add(item);
                                return item;
                            }}

                            public override System.Collections.Generic.IEnumerable<TEntity> RemoveRange(System.Collections.Generic.IEnumerable<TEntity> entities)
                            {{
                                if (entities == null) throw new System.ArgumentNullException(""entities"");
                                var items = entities.ToList();
                                foreach (var entity in items)
                                {{
                                    _data.Remove(entity);
                                }}
                                return items;
                            }}

                            public override TEntity Remove(TEntity item)
                            {{
                                if (item == null) throw new System.ArgumentNullException(""item"");
                                _data.Remove(item);
                                return item;
                            }}

                            public override TEntity Attach(TEntity item)
                            {{
                                if (item == null) throw new System.ArgumentNullException(""item"");
                                _data.Add(item);
                                return item;
                            }}

                            public override TEntity Create()
                            {{
                                return System.Activator.CreateInstance<TEntity>();
                            }}

                            public override TDerivedEntity Create<TDerivedEntity>()
                            {{
                                return System.Activator.CreateInstance<TDerivedEntity>();
                            }}

                            public override System.Collections.ObjectModel.ObservableCollection<TEntity> Local
                            {{
                                get {{ return _data; }}
                            }}

                            System.Type IQueryable.ElementType
                            {{
                                get {{ return _query.ElementType; }}
                            }}

                            System.Linq.Expressions.Expression IQueryable.Expression
                            {{
                                get {{ return _query.Expression; }}
                            }}

                            IQueryProvider IQueryable.Provider
                            {{
                                get {{ {(Settings.IsSupportedFrameworkVersion("4.5") ? "return new FakeDbAsyncQueryProvider<TEntity>(_query.Provider);" : "_query.Provider;")} }}
                            }}

                            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                            {{
                                return _data.GetEnumerator();
                            }}

                            System.Collections.Generic.IEnumerator<TEntity> System.Collections.Generic.IEnumerable<TEntity>.GetEnumerator()
                            {{
                                return _data.GetEnumerator();
                            }}
                            { (Settings.IsSupportedFrameworkVersion("4.5") ?
                                $@"
                            System.Data.Entity.Infrastructure.IDbAsyncEnumerator<TEntity> System.Data.Entity.Infrastructure.IDbAsyncEnumerable<TEntity>.GetAsyncEnumerator()
                            {{
                                return new FakeDbAsyncEnumerator<TEntity>(_data.GetEnumerator());
                            }}" : "")}

                            { (Settings.DbContextClassIsPartial() ? "partial void InitializePartial();\n" : "") }");
                    } // Line 882
                    w?.WriteLine("");

                    if (Settings.IncludeCodeGeneratedAttribute)
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($@"{ Settings.DbContextClassModifiers } class FakeDbAsyncQueryProvider<TEntity> : System.Data.Entity.Infrastructure.IDbAsyncQueryProvider"))
                    {
                        w?.WriteLine($@"
                            private readonly IQueryProvider _inner;

                            public FakeDbAsyncQueryProvider(IQueryProvider inner)
                            {{
                                _inner = inner;
                            }}

                            public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
                            {{
                                var m = expression as System.Linq.Expressions.MethodCallExpression;
                                if (m != null)
                                {{
                                    var resultType = m.Method.ReturnType; // it shoud be IQueryable<T>
                                    var tElement = resultType.GetGenericArguments()[0];
                                    var queryType = typeof(FakeDbAsyncEnumerable<>).MakeGenericType(tElement);
                                    return (IQueryable) System.Activator.CreateInstance(queryType, expression);
                                }}
                                return new FakeDbAsyncEnumerable<TEntity>(expression);
                            }}

                            public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
                            {{
                                var queryType = typeof(FakeDbAsyncEnumerable<>).MakeGenericType(typeof(TElement));
                                return (IQueryable<TElement>)System.Activator.CreateInstance(queryType, expression);
                            }}

                            public object Execute(System.Linq.Expressions.Expression expression)
                            {{
                                return _inner.Execute(expression);
                            }}

                            public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
                            {{
                                return _inner.Execute<TResult>(expression);
                            }}

                            public System.Threading.Tasks.Task<object> ExecuteAsync(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
                            {{
                                return System.Threading.Tasks.Task.FromResult(Execute(expression));
                            }}

                            public System.Threading.Tasks.Task<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, System.Threading.CancellationToken cancellationToken)
                            {{
                                return System.Threading.Tasks.Task.FromResult(Execute<TResult>(expression));
                            }}");

                    }



                    w?.WriteLine();
                    if (Settings.IncludeCodeGeneratedAttribute) // 937
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($@"{ Settings.DbContextClassModifiers } class FakeDbAsyncEnumerable<T> : EnumerableQuery<T>, System.Data.Entity.Infrastructure.IDbAsyncEnumerable<T>, IQueryable<T>"))
                    {
                        w?.WriteLine($@"
                            public FakeDbAsyncEnumerable(System.Collections.Generic.IEnumerable<T> enumerable) : base(enumerable)
                            {{ }}
                            
                            public FakeDbAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
                            {{ }}
                            
                            public System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T> GetAsyncEnumerator()
                            {{
                                return new FakeDbAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
                            }}
                            
                            System.Data.Entity.Infrastructure.IDbAsyncEnumerator System.Data.Entity.Infrastructure.IDbAsyncEnumerable.GetAsyncEnumerator()
                            {{
                                return GetAsyncEnumerator();
                            }}
                            
                            IQueryProvider IQueryable.Provider
                            {{
                                get {{ return new FakeDbAsyncQueryProvider<T>(this); }}
                            }}");
                    }

                    if (Settings.IncludeCodeGeneratedAttribute) // 937
                        w?.WriteLine(CodeGeneratedAttribute);
                    using (w?.WithCBlock($@"{ Settings.DbContextClassModifiers } class FakeDbAsyncEnumerator<T> : System.Data.Entity.Infrastructure.IDbAsyncEnumerator<T>"))
                    {
                        w?.WriteLine($@"
                            private readonly System.Collections.Generic.IEnumerator<T> _inner;

                            public FakeDbAsyncEnumerator(System.Collections.Generic.IEnumerator<T> inner)
                            {{
                                _inner = inner;
                            }}

                            public void Dispose()
                            {{
                                _inner.Dispose();
                            }}

                            public System.Threading.Tasks.Task<bool> MoveNextAsync(System.Threading.CancellationToken cancellationToken)
                            {{
                                return System.Threading.Tasks.Task.FromResult(_inner.MoveNext());
                            }}

                            public T Current
                            {{
                                get {{ return _inner.Current; }}
                            }}

                            object System.Data.Entity.Infrastructure.IDbAsyncEnumerator.Current
                            {{
                                get {{ return Current; }}
                            }}");
                    }


                    if (Settings.ElementsToGenerate.HasFlag(Elements.Context) && !Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#endregion"); // line 1000
                    #endregion
                }
                #endregion Fake Database context (Line 533 to 1002)

                Console.WriteLine("POCO classes...");
                #region POCO classes (Line 1003 to 1112)
                if (Settings.ElementsToGenerate.HasFlag(Elements.Poco))
                {
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#region POCO classes\n"); // line 1005
                    foreach (Table tbl in Settings.Tables.Where(t => !t.IsMapping).OrderBy(x => x.NameHumanCase))
                    {
                        if (Settings.GenerateSeparateFiles)
                            StartNewFile(tbl.NameHumanCaseWithSuffix() + Settings.FileExtension);
                        if (!tbl.HasPrimaryKey)
                        {
                            w?.WriteLine($"// The table '{tbl.Name}' is not usable by entity framework because it");
                            w?.WriteLine($"// does not have a primary key. It is listed here for completeness.");
                        }
                        if (Settings.IncludeComments != CommentsStyle.None)
                            w?.WriteLine($"// {tbl.Name}");
                        WritePocoClassExtendedComments(w, tbl); // Line 1019
                        WritePocoClassAttributes(w, tbl); // Line 1020
                        if (Settings.IncludeCodeGeneratedAttribute)
                            w?.WriteLine(CodeGeneratedAttribute);
                        using (w?.WithCBlock($"{ Settings.EntityClassesModifiers } class {tbl.NameHumanCaseWithSuffix()}{this.WritePocoBaseClasses?.Invoke(tbl)}"))
                        {
                            this.WritePocoBaseClassBody(w, tbl); // Line 1025
                            foreach (Column col in tbl.Columns.OrderBy(x => x.Ordinal).Where(x => !x.Hidden))
                                this.WritePocoColumn(w, col);
                            //Console.WriteLine("ReverseNavigationProperty...");
                            #region ReverseNavigationProperty (Line 1032 to 1055)
                            if (tbl.ReverseNavigationProperty.Count() > 0)
                            {
                                w?.WriteLine("");
                                if (Settings.IncludeComments != CommentsStyle.None)
                                    w?.WriteLine($"// Reverse navigation\n");
                                foreach (var s in tbl.ReverseNavigationProperty.OrderBy(x => x.Definition))
                                {
                                    if (Settings.IncludeComments != CommentsStyle.None)
                                    {
                                        w?.WriteLine($"/// <summary>");
                                        w?.WriteLine($"/// {s.Comments ?? "" }");
                                        w?.WriteLine($"/// </summary>");
                                    }
                                    foreach (var rnpda in Settings.AdditionalReverseNavigationsDataAnnotations)
                                        w?.WriteLine($"[{rnpda }]");
                                    if (s.AdditionalDataAnnotations != null)
                                    {
                                        foreach (var fkda in s.AdditionalDataAnnotations)
                                        {
                                            w?.WriteLine($"[{fkda }]");
                                        }
                                    }
                                    w?.WriteLine($"{s.Definition }");
                                    w?.WriteLine();
                                }
                            }
                            #endregion ReverseNavigationProperty (Line 1032 to 1055)

                            //Console.WriteLine("ForeignKeys...");
                            #region ForeignKeys - (Line 1056 to 1077)
                            if (tbl.HasForeignKey)
                            {
                                if (Settings.IncludeComments != CommentsStyle.None && tbl.Columns.SelectMany(x => x.EntityFk).Any())
                                    w?.WriteLine("\n// Foreign keys\n");
                                foreach (var entityFk in tbl.Columns.SelectMany(x => x.EntityFk).OrderBy(o => o.Definition))
                                {
                                    if (Settings.IncludeComments != CommentsStyle.None)
                                    {
                                        w?.WriteLine($"/// <summary>");
                                        w?.WriteLine($"/// {entityFk.Comments ?? "" }");
                                        w?.WriteLine($"/// </summary>");
                                    }
                                    foreach (var fkda in Settings.AdditionalForeignKeysDataAnnotations)
                                        w?.WriteLine($"[{fkda }]");
                                    if (entityFk.AdditionalDataAnnotations != null)
                                    {
                                        foreach (var fkda in entityFk.AdditionalDataAnnotations)
                                        {
                                            w?.WriteLine($"[{fkda }]");
                                        }
                                    }
                                    w?.WriteLine($"{entityFk.Definition }");
                                    w?.WriteLine();
                                }
                            }
                            #endregion ForeignKeys - (Line 1056 to 1077)

                            //Console.WriteLine("UsePropertyInitializers...");
                            #region POCO UsePropertyInitializers (Line 1079 to 1104)
                            if (!Settings.UsePropertyInitializers)
                            {
                                if (tbl.Columns.Where(c => c.Default != string.Empty && !c.Hidden).Count() > 0 || tbl.ReverseNavigationCtor.Count() > 0 || Settings.EntityClassesArePartial())
                                {
                                    using (w?.WithCBlock($"public {tbl.NameHumanCaseWithSuffix()}()"))
                                    {
                                        foreach (var col in tbl.Columns.OrderBy(x => x.Ordinal).Where(c => c.Default != string.Empty && !c.Hidden))
                                            w?.WriteLine($"{col.NameHumanCase } = {col.Default };");
                                        foreach (string s in tbl.ReverseNavigationCtor)
                                            w?.WriteLine(s);
                                        if (Settings.EntityClassesArePartial())
                                            w?.WriteLine("InitializePartial();");
                                    }
                                    if (Settings.EntityClassesArePartial())
                                        w?.WriteLine("partial void InitializePartial();");
                                }
                            }
                            #endregion POCO UsePropertyInitializers (Line 1079 to 1104)


                        }
                        w?.WriteLine();
                    }
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#endregion\n"); // line 1110
                }
                #endregion POCO classes (Line 1003 to 1112)

                Console.WriteLine("POCO Configuration...");
                #region POCO Configuration (Line 1113 to 1178)
                if (Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration))
                {
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#region POCO Configuration\n"); // line 1115
                    foreach (var tbl in Settings.Tables.Where(t => !t.IsMapping && t.HasPrimaryKey).OrderBy(x => x.NameHumanCase))
                    {
                        if (Settings.GenerateSeparateFiles)
                            StartNewFile(tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName + Settings.FileExtension);
                        if (Settings.IncludeComments != CommentsStyle.None)
                            w?.WriteLine($"// {tbl.Name}");
                        if (Settings.IncludeCodeGeneratedAttribute)
                            w?.WriteLine(CodeGeneratedAttribute);
                        using (w?.WithCBlock($"{ Settings.ConfigurationClassesModifiers } class {tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName} : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<{tbl.NameHumanCaseWithSuffix()}>"))
                        {
                            using (w?.WithCBlock($"public {tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName}() : this(\"{ tbl.Schema ?? ""}\")"))
                            {
                            }
                            w?.WriteLine("");

                            using (w?.WithCBlock($"public {tbl.NameHumanCaseWithSuffix() + Settings.ConfigurationClassName}(string schema)"))
                            {
                                if (!Settings.UseDataAnnotations)
                                {
                                    if (!string.IsNullOrEmpty(tbl.Schema))
                                        w?.WriteLine($"ToTable(\"{ tbl.Name}\", schema);");
                                    else
                                        w?.WriteLine($"ToTable(\"{ tbl.Name}\");");
                                }
                                if (!Settings.UseDataAnnotations)
                                    w?.WriteLine($"HasKey({tbl.PrimaryKeyNameHumanCase()});\n");
                                foreach (var col in tbl.Columns.Where(x => !x.Hidden && !string.IsNullOrEmpty(x.Config)).OrderBy(x => x.Ordinal))
                                    w?.WriteLine(col.Config);

                                //Console.WriteLine("ForeignKeys 1151 ...");
                                #region ForeignKeys (Line 1151 to 1160)
                                if (tbl.HasForeignKey)
                                {
                                    if (Settings.IncludeComments != CommentsStyle.None && tbl.Columns.SelectMany(x => x.ConfigFk).Any())
                                        w?.WriteLine($"\n// Foreign keys");
                                    foreach (var configFk in tbl.Columns.SelectMany(x => x.ConfigFk).OrderBy(o => o))
                                    {
                                        w?.WriteLine(configFk);
                                    }
                                }
                                #endregion ForeignKeys (Line 1151 to 1160)
                                foreach (string s in tbl.MappingConfiguration)
                                    w?.WriteLine(s);
                                if (Settings.DbContextClassIsPartial())
                                    w?.WriteLine("InitializePartial();");
                            }
                            if (Settings.ConfigurationClassesArePartial())
                                w?.WriteLine("partial void InitializePartial();");
                        } // Line 1172
                        w?.WriteLine("");
                    }
                } // Line 1174
                if (Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration) && !Settings.GenerateSeparateFiles)
                    w?.WriteLine("\n#endregion\n"); // line 1176
                #endregion POCO Configuration (Line 1113 to 1178)

                Console.WriteLine("Stored procedure return models...");
                #region Stored procedure return models (Line 1179 to 1124)
                if (Settings.StoredProcs.Any() && Settings.ElementsToGenerate.HasFlag(Elements.Poco))
                {
                    if (!Settings.GenerateSeparateFiles)
                        w?.WriteLine("\n#region Stored procedure return models\n"); // line 1181
                    foreach (var sp in Settings.StoredProcs.Where(x => x.ReturnModels.Count > 0 && x.ReturnModels.Any(returnColumns => returnColumns.Any()) && !Settings.StoredProcedureReturnTypes.ContainsKey(x.NameHumanCase) && !Settings.StoredProcedureReturnTypes.ContainsKey(x.Name)).OrderBy(x => x.NameHumanCase))
                    {
                        string spReturnClassName = WriteStoredProcReturnModelName(sp);
                        if (Settings.GenerateSeparateFiles)
                            StartNewFile(spReturnClassName + Settings.FileExtension);
                        if (Settings.IncludeCodeGeneratedAttribute)
                            w?.WriteLine(CodeGeneratedAttribute);
                        using (w?.WithCBlock($"{Settings.ResultClassModifiers } class { spReturnClassName }"))
                        {
                            var returnModelCount = sp.ReturnModels.Count;
                            if (returnModelCount < 2)
                            {
                                foreach (var returnColumn in sp.ReturnModels.First())
                                    w?.WriteLine(WriteStoredProcReturnColumn(returnColumn));
                            }
                            else
                            {
                                int model = 0;
                                foreach (var returnModel in sp.ReturnModels)
                                {
                                    model++;
                                    using (w?.WithCBlock($"public class ResultSetModel{ model }"))
                                    {
                                        foreach (var returnColumn in returnModel)
                                            w?.WriteLine(WriteStoredProcReturnColumn(returnColumn));
                                    }
                                    w?.WriteLine($"public System.Collections.Generic.List<ResultSetModel{ model }> ResultSet{ model };");
                                }

                            }
                        }
                        w?.WriteLine();
                    }
                }
                if (Settings.StoredProcs.Any() && Settings.ElementsToGenerate.HasFlag(Elements.Poco) && !Settings.GenerateSeparateFiles)
                    w?.WriteLine("\n#endregion\n"); // line 1222
                #endregion Stored procedure return models (Line 1179 to 1124)

                FinishCurrentFile();
            }
            //_output?.WriteLine("}"); // this is in FinishCurrentFile()
            #endregion Namespace / whole file (Line 31 to 1226)
            //_output?.WriteLine("// </auto-generated>"); // this is in FinishCurrentFile()

            #endregion
            #endregion
        }

        #region All this code mostly came from Simon Hughes T4 templates (with minor adjustments) - EF.Reverse.POCO.Core.ttinclude - see https://github.com/sjh37/EntityFramework-Reverse-POCO-Code-First-Generator

        const string CodeGeneratedAttribute = "[System.CodeDom.Compiler.GeneratedCode(\"EF.Reverse.POCO.Generator\", \"2.37.4.0\")]";
        //const string DataDirectory = "|DataDirectory|";


        public static void ArgumentNotNull<T>(T arg, string name) where T : class
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(CultureInfo.CurrentCulture, format, args));
        }

        public void WriteLine(string message)
        {
            LogToOutput(message);
            //base.WriteLine(message);
        }

        public void Warning(string message)
        {
            LogToOutput(string.Format(CultureInfo.CurrentCulture, "Warning: {0}", message));
            //base.Warning(message);
        }
        public void Error(string message)
        {
            LogToOutput(string.Format(CultureInfo.CurrentCulture, "Error: {0}", message));
            throw new Exception(message);
            //base.Error(message);
        }

        private void LogToOutput(string message)
        {
            this.w?.WriteLine(message);
        }


        //private static string ZapPassword()
        //{
        //    var rx = new Regex("password=[^\";]*", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase);
        //    return rx.Replace(Settings.ConnectionString, "password=**zapped**;");
        //}

        public void PrintError(String message, Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            while (ex != null)
            {
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.GetType().FullName);
                sb.AppendLine(ex.StackTrace);
                sb.AppendLine();
                ex = ex.InnerException;
            }
            String report = sb.ToString();

            Warning(message + " " + report);
            WriteLine("");
            WriteLine("// -----------------------------------------------------------------------------------------");
            WriteLine("// " + message);
            WriteLine("// -----------------------------------------------------------------------------------------");
            WriteLine(report);
            WriteLine("");
        }

        /*
        private DbProviderFactory GetDbProviderFactory()
        {
            WriteLine("// ------------------------------------------------------------------------------------------------");
            WriteLine("// This code was generated by EntityFramework Reverse POCO Generator (http://www.reversepoco.com/).");
            WriteLine("// Created by Simon Hughes (https://about.me/simon.hughes).");
            WriteLine("//");
            WriteLine("// Do not make changes directly to this file - edit the template instead.");
            WriteLine("//");
            if (Settings.IncludeConnectionSettingComments)
            {
                WriteLine("// The following connection settings were used to generate this file:");
                if (!string.IsNullOrEmpty(Settings.ConnectionStringName)) // && !string.IsNullOrEmpty(Settings.ConfigFilePath))
                {
                    //var solutionPath = Path.GetDirectoryName(GetSolution().FileName) + "\\";
                    //WriteLine("//     Configuration file:     \"{0}\"", Settings.ConfigFilePath.Replace(solutionPath, string.Empty));
                    WriteLine("//     Connection String Name: \"{0}\"", Settings.ConnectionStringName);
                }
                WriteLine("//     Connection String:      \"{0}\"", ZapPassword());
                WriteLine("// ------------------------------------------------------------------------------------------------");
            }

            if (!string.IsNullOrEmpty(Settings.ProviderName))
            {
                try
                {
                    return DbProviderFactories.GetFactory(Settings.ProviderName);
                }
                catch (Exception x)
                {
                    PrintError("Failed to load provider \"" + Settings.ProviderName + "\".", x);
                    //throw;
                }
            }
            else
            {
                Warning("Failed to find providerName in the connection string");
                WriteLine("");
                WriteLine("// ------------------------------------------------------------------------------------------------");
                WriteLine("//  Failed to find providerName in the connection string");
                WriteLine("// ------------------------------------------------------------------------------------------------");
                WriteLine("");
            }
            return null;
            //throw new Exception("Failed to find providerName in the connection string");
        }

        private DbProviderFactory TryGetDbProviderFactory()
        {
            try
            {
                return DbProviderFactories.GetFactory(Settings.ProviderName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        */
        private bool IsSqlCeConnection(DbConnection connection)
        {
            if (connection.GetType().Name.ToLower() == "sqlceconnection")
                return true;
            return false;
        }

        private Tables LoadTables()
        {
            if (!(Settings.ElementsToGenerate.HasFlag(Elements.Poco) ||
                                    Settings.ElementsToGenerate.HasFlag(Elements.Context) ||
                                    Settings.ElementsToGenerate.HasFlag(Elements.UnitOfWork) ||
                                    Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration)))
                return new Tables();

            try
            {
                using (var conn = _createConnection())
                {
                    if (conn == null)
                        return new Tables();

                    conn.Open();

                    Settings.IsSqlCe = IsSqlCeConnection(conn);

                    if (Settings.IsSqlCe)
                        Settings.PrependSchemaName = false;

                    var reader = new SqlServerSchemaReader(conn, w);
                    var tables = reader.ReadSchema();
                    var fkList = reader.ReadForeignKeys();
                    reader.IdentifyForeignKeys(fkList, tables);

                    foreach (var t in tables)
                    {
                        if (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent)
                            t.SetupDataAnnotations();
                        t.Suffix = Settings.TableSuffix;
                    }

                    if (Settings.AddForeignKeys != null) Settings.AddForeignKeys(fkList, tables);

                    // Work out if there are any foreign key relationship naming clashes
                    reader.ProcessForeignKeys(fkList, tables, true);
                    if (Settings.UseMappingTables)
                        tables.IdentifyMappingTables(fkList, true);

                    // Now we know our foreign key relationships and have worked out if there are any name clashes,
                    // re-map again with intelligently named relationships.
                    tables.ResetNavigationProperties();

                    reader.ProcessForeignKeys(fkList, tables, false);
                    if (Settings.UseMappingTables)
                        tables.IdentifyMappingTables(fkList, false);

                    conn.Close();
                    return tables;
                }
            }
            catch (Exception x)
            {
                PrintError("Failed to read database schema in LoadTables().", x);
                return new Tables();
            }
        }

        /// <summary>AddRelationship overload for single-column foreign-keys.</summary>
        public static void AddRelationship(List<ForeignKey> fkList, Tables tablesAndViews, String name, String pkSchema, String pkTable, String pkColumn, String fkSchema, String fkTable, String fkColumn)
        {
            AddRelationship(fkList, tablesAndViews, name, pkSchema, pkTable, new String[] { pkColumn }, fkSchema, fkTable, new String[] { fkColumn });
        }

        public static void AddRelationship(List<ForeignKey> fkList, Tables tablesAndViews, String relationshipName, String pkSchema, String pkTableName, String[] pkColumns, String fkSchema, String fkTableName, String[] fkColumns)
        {
            // Argument validation:
            if (fkList == null) throw new ArgumentNullException("fkList");
            if (tablesAndViews == null) throw new ArgumentNullException("tablesAndViews");
            if (string.IsNullOrEmpty(relationshipName)) throw new ArgumentNullException("relationshipName");
            if (string.IsNullOrEmpty(pkSchema)) throw new ArgumentNullException("pkSchema");
            if (string.IsNullOrEmpty(pkTableName)) throw new ArgumentNullException("pkTableName");
            if (pkColumns == null) throw new ArgumentNullException("pkColumns");
            if (pkColumns.Length == 0 || pkColumns.Any(s => string.IsNullOrEmpty(s))) throw new ArgumentException("Invalid primary-key columns: No primary-key column names are specified, or at least one primary-key column name is empty.", "pkColumns");
            if (string.IsNullOrEmpty(fkSchema)) throw new ArgumentNullException("fkSchema");
            if (string.IsNullOrEmpty(fkTableName)) throw new ArgumentNullException("fkTableName");
            if (fkColumns == null) throw new ArgumentNullException("fkColumns");
            if (fkColumns.Length != pkColumns.Length || fkColumns.Any(s => string.IsNullOrEmpty(s))) throw new ArgumentException("Invalid foreign-key columns:Foreign-key column list has a different number of columns than the primary-key column list, or at least one foreign-key column name is empty.", "pkColumns");

            //////////////////

            Table pkTable = tablesAndViews.GetTable(pkTableName, pkSchema);
            if (pkTable == null) throw new ArgumentException("Couldn't find table " + pkSchema + "." + pkTableName);

            Table fkTable = tablesAndViews.GetTable(fkTableName, fkSchema);
            if (fkTable == null) throw new ArgumentException("Couldn't find table " + fkSchema + "." + fkTableName);

            // Ensure all columns exist:
            foreach (String pkCol in pkColumns)
            {
                if (pkTable.Columns.SingleOrDefault(c => c.Name == pkCol) == null) throw new ArgumentException("The relationship primary-key column \"" + pkCol + "\" does not exist in table or view " + pkSchema + "." + pkTableName);
            }
            foreach (String fkCol in fkColumns)
            {
                if (fkTable.Columns.SingleOrDefault(c => c.Name == fkCol) == null) throw new ArgumentException("The relationship foreign-key column \"" + fkCol + "\" does not exist in table or view " + fkSchema + "." + fkTableName);
            }

            for (int i = 0; i < pkColumns.Length; i++)
            {
                String pkc = pkColumns[i];
                String fkc = fkColumns[i];

                String pkTableNameFiltered = Settings.TableRename(pkTableName, pkSchema, pkTable.IsView); // TODO: This can probably be done-away with. Is `AddRelationship` called before or after table.NameFiltered is set?

                ForeignKey fk = new ForeignKey(
                    fkTableName: fkTable.Name,
                    fkSchema: fkSchema,
                    pkTableName: pkTable.Name,
                    pkSchema: pkSchema,
                    fkColumn: fkc,
                    pkColumn: pkc,
                    constraintName: "AddRelationship: " + relationshipName,
                    pkTableNameFiltered: pkTableNameFiltered,
                    ordinal: Int32.MaxValue,
                    cascadeOnDelete: false,
                    isNotEnforced: false
                );
                fk.IncludeReverseNavigation = true;

                fkList.Add(fk);
                fkTable.HasForeignKey = true;
            }
        }

        private List<StoredProcedure> LoadStoredProcs()
        {
            if (!Settings.IncludeStoredProcedures)
                return new List<StoredProcedure>();

            try
            {
                using (var conn = _createConnection())
                {
                    if (conn == null)
                        return new List<StoredProcedure>();

                    conn.Open();

                    if (Settings.IsSqlCe)
                        return new List<StoredProcedure>();

                    var reader = new SqlServerSchemaReader(conn, w);
                    var storedProcs = reader.ReadStoredProcs();
                    conn.Close();

                    // Remove unrequired stored procs
                    for (int i = storedProcs.Count - 1; i >= 0; i--)
                    {
                        if (Settings.SchemaFilterInclude != null && !Settings.SchemaFilterInclude.IsMatch(storedProcs[i].Schema))
                        {
                            storedProcs.RemoveAt(i);
                            continue;
                        }
                        if (Settings.StoredProcedureFilterInclude != null && !Settings.StoredProcedureFilterInclude.IsMatch(storedProcs[i].Name))
                        {
                            storedProcs.RemoveAt(i);
                            continue;
                        }
                        if (!Settings.StoredProcedureFilter(storedProcs[i]))
                        {
                            storedProcs.RemoveAt(i);
                            continue;
                        }
                    }

                    using (var sqlConnection = (System.Data.SqlClient.SqlConnection) _createConnection())
                    {
                        foreach (var proc in storedProcs)
                            reader.ReadStoredProcReturnObject(sqlConnection, proc);
                    }

                    // Remove stored procs where the return model type contains spaces and cannot be mapped
                    // Also need to remove any TVF functions with parameters that are non scalar types, such as DataTable
                    var validStoredProcedures = new List<StoredProcedure>();
                    foreach (var sp in storedProcs)
                    {
                        if (!sp.ReturnModels.Any())
                        {
                            validStoredProcedures.Add(sp);
                            continue;
                        }

                        if (sp.ReturnModels.Any(returnColumns => returnColumns.Any(c => c.ColumnName.Contains(" "))))
                            continue;

                        if (sp.IsTVF && sp.Parameters.Any(c => c.PropertyType == "System.Data.DataTable"))
                            continue;

                        validStoredProcedures.Add(sp);
                    }
                    return validStoredProcedures;
                }
            }
            catch (Exception ex)
            {
                PrintError("Failed to read database schema for stored procedures.", ex);
                return new List<StoredProcedure>();
            }
        }


        // ***********************************************************************
        // ** Stored procedure callbacks

        public static readonly Func<StoredProcedure, string> WriteStoredProcFunctionName = sp => sp.NameHumanCase;

        public static readonly Func<StoredProcedure, bool> StoredProcHasOutParams = (sp) =>
        {
            return sp.Parameters.Any(x => x.Mode != StoredProcedureParameterMode.In);
        };

        public static readonly Func<StoredProcedure, bool, string> WriteStoredProcFunctionParams = (sp, includeProcResult) =>
        {
            var sb = new StringBuilder();
            int n = 1;
            int count = sp.Parameters.Count;
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.AppendFormat("{0}{1}{2} {3}{4}",
                    p.Mode == StoredProcedureParameterMode.In ? "" : "out ",
                    p.PropertyType,
                    SqlServerSchemaReader.NotNullable.Contains(p.PropertyType.ToLower()) ? string.Empty : "?",
                    p.NameHumanCase,
                    (n++ < count) ? ", " : string.Empty);
            }
            if (includeProcResult && sp.ReturnModels.Count > 0 && sp.ReturnModels.First().Count > 0)
                sb.AppendFormat((sp.Parameters.Count > 0 ? ", " : "") + "out int procResult");
            return sb.ToString();
        };

        public static readonly Func<StoredProcedure, string> WriteStoredProcFunctionOverloadCall = (sp) =>
        {
            var sb = new StringBuilder();
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.AppendFormat("{0}{1}, ",
                    p.Mode == StoredProcedureParameterMode.In ? "" : "out ",
                    p.NameHumanCase);
            }
            sb.Append("out procResult");
            return sb.ToString();
        };

        public static readonly Func<StoredProcedure, string> WriteStoredProcFunctionSqlAtParams = sp =>
        {
            var sb = new StringBuilder();
            int n = 1;
            int count = sp.Parameters.Count;
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.AppendFormat("{0}{1}{2}",
                    p.Name,
                    p.Mode == StoredProcedureParameterMode.In ? string.Empty : " OUTPUT",
                    (n++ < count) ? ", " : string.Empty);
            }
            return sb.ToString();
        };

        public static readonly Func<StoredProcedureParameter, string> WriteStoredProcSqlParameterName = p => p.NameHumanCase + "Param";

        public static readonly Action<CodegenTextWriter, StoredProcedure, bool> WriteStoredProcFunctionDeclareSqlParameter = (o, sp, includeProcResult) =>
        {
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                var isNullable = !SqlServerSchemaReader.NotNullable.Contains(p.PropertyType.ToLower());
                var getValueOrDefault = isNullable ? ".GetValueOrDefault()" : string.Empty;
                var isGeography = p.PropertyType == "System.Data.Entity.Spatial.DbGeography";

                o?.WriteLine(
                    string.Format("var {0} = new System.Data.SqlClient.SqlParameter", WriteStoredProcSqlParameterName(p))
                    + string.Format(" {{ ParameterName = \"{0}\", ", p.Name)
                    + (isGeography ? "UdtTypeName = \"geography\"" : string.Format("SqlDbType = System.Data.SqlDbType.{0}", p.SqlDbType))
                    + ", Direction = System.Data.ParameterDirection."
                    + (p.Mode == StoredProcedureParameterMode.In ? "Input" : "Output")
                    + (p.Mode == StoredProcedureParameterMode.In
                        ? ", Value = " + (isGeography
                            ? string.Format("Microsoft.SqlServer.Types.SqlGeography.Parse({0}.AsText())", p.NameHumanCase)
                              : p.NameHumanCase + getValueOrDefault)
                        : string.Empty)
                    + (p.MaxLength != 0 ? ", Size = " + p.MaxLength : string.Empty)
                    + ((p.Precision > 0 || p.Scale > 0) ? ", Precision = " + p.Precision + ", Scale = " + p.Scale : string.Empty)
                    + (p.PropertyType.ToLower().Contains("datatable") ? ", TypeName = \"" + p.UserDefinedTypeName + "\"" : string.Empty)
                    + " };");

                if (p.Mode == StoredProcedureParameterMode.In)
                {
                    o?.WriteLine(
                        isNullable
                        ? @"
                                if (!{0}.HasValue)
                                    {0}Param.Value = System.DBNull.Value;"
                        : @"
                                if ({0}Param.Value == null)
                                    {0}Param.Value = System.DBNull.Value;",
                    p.NameHumanCase);
                    o?.WriteLine();
                }
            }
            if (includeProcResult && sp.ReturnModels.Count < 2)
                o?.WriteLine("var procResultParam = new System.Data.SqlClient.SqlParameter { ParameterName = \"@procResult\", SqlDbType = System.Data.SqlDbType.Int, Direction = System.Data.ParameterDirection.Output };");
        };

        public static readonly Func<StoredProcedure, string> WriteTableValuedFunctionDeclareSqlParameter = sp =>
        {
            var sb = new StringBuilder();
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.AppendLine(string.Format("var {0}Param = new System.Data.Entity.Core.Objects.ObjectParameter(\"{1}\", typeof({2})) {{ Value = (object){3} }};",
                    p.NameHumanCase,
                    p.Name.Substring(1),
                    p.PropertyType,
                    p.NameHumanCase + (p.Mode == StoredProcedureParameterMode.In && SqlServerSchemaReader.NotNullable.Contains(p.PropertyType.ToLowerInvariant()) ? string.Empty : " ?? System.DBNull.Value")));
            }
            return sb.ToString();
        };

        public static readonly Func<StoredProcedure, bool, string> WriteStoredProcFunctionSqlParameterAnonymousArray = (sp, includeProcResultParam) =>
        {
            var sb = new StringBuilder();
            bool hasParam = false;
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.Append(string.Format("{0}Param, ", p.NameHumanCase));
                hasParam = true;
            }
            if (includeProcResultParam)
                sb.Append("procResultParam");
            else if (hasParam)
                sb.Remove(sb.Length - 2, 2);
            return sb.ToString();
        };

        public static readonly Func<StoredProcedure, string> WriteTableValuedFunctionSqlParameterAnonymousArray = sp =>
        {
            if (sp.Parameters.Count == 0)
                return "new System.Data.Entity.Core.Objects.ObjectParameter[] { }";
            var sb = new StringBuilder();
            foreach (var p in sp.Parameters.OrderBy(x => x.Ordinal))
            {
                sb.Append(string.Format("{0}Param, ", p.NameHumanCase));
            }
            return sb.ToString().Substring(0, sb.Length - 2);
        };

        public static readonly Action<CodegenTextWriter, StoredProcedure, bool> WriteStoredProcFunctionSetSqlParameters = (o, sp, isFake) =>
        {
            foreach (var p in sp.Parameters.Where(x => x.Mode != StoredProcedureParameterMode.In).OrderBy(x => x.Ordinal))
            {
                var Default = string.Format("default({0})", p.PropertyType);
                var notNullable = SqlServerSchemaReader.NotNullable.Contains(p.PropertyType.ToLower());

                if (isFake)
                    o?.WriteLine(string.Format("{0} = {1};", p.NameHumanCase, Default));
                else
                {
                    o?.WriteLine(string.Format("if (IsSqlParameterNull({0}Param))", p.NameHumanCase));
                    o?.WriteLine(string.Format("    {0} = {1};", p.NameHumanCase, notNullable ? Default : "null"));
                    o?.WriteLine("else");
                    o?.WriteLine(string.Format("    {0} = ({1}) {2}Param.Value;", p.NameHumanCase, p.PropertyType, p.NameHumanCase));
                }
            }
        };

        public static readonly Func<StoredProcedure, string> WriteStoredProcReturnModelName = sp =>
        {
            if (Settings.StoredProcedureReturnTypes.ContainsKey(sp.NameHumanCase))
                return Settings.StoredProcedureReturnTypes[sp.NameHumanCase];
            if (Settings.StoredProcedureReturnTypes.ContainsKey(sp.Name))
                return Settings.StoredProcedureReturnTypes[sp.Name];

            var name = string.Format("{0}ReturnModel", sp.NameHumanCase);
            if (Settings.StoredProcedureReturnModelRename != null)
            {
                var customName = Settings.StoredProcedureReturnModelRename(name, sp);
                if (!string.IsNullOrEmpty(customName))
                    name = customName;
            }

            return name;
        };

        public static readonly Func<DataColumn, string> WriteStoredProcReturnColumn = col =>
        {
            var columnName = SqlServerSchemaReader.ReservedKeywords.Contains(col.ColumnName) ? "@" + col.ColumnName : col.ColumnName;

            return string.Format("public {0} {1} {{ get; set; }}",
                StoredProcedure.WrapTypeIfNullable(
                    (col.DataType.Name.Equals("SqlHierarchyId") ? "Microsoft.SqlServer.Types." : col.DataType.Namespace + ".") +
                    col.DataType.Name, col),
                columnName);
        };

        public static readonly Func<StoredProcedure, string> WriteStoredProcReturnType = (sp) =>
        {
            var returnModelCount = sp.ReturnModels.Count;
            if (returnModelCount == 0)
                return "int";

            var spReturnClassName = WriteStoredProcReturnModelName(sp);
            return (returnModelCount == 1) ? string.Format("System.Collections.Generic.List<{0}>", spReturnClassName) : spReturnClassName;
        };

        /// <summary>
        /// Helper class in making dynamic class definitions easier.
        /// </summary>
        public sealed class BaseClassMaker
        {
            private string _typeName;
            private StringBuilder _interfaces;

            public BaseClassMaker(string baseClassName = null)
            {
                SetBaseClassName(baseClassName);
            }

            /// <summary>
            /// Sets the base-class name.
            /// </summary>
            public void SetBaseClassName(string typeName)
            {
                _typeName = typeName;
            }

            /// <summary>
            /// Appends additional implemented interface.
            /// </summary>
            public bool AddInterface(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                    return false;

                if (_interfaces == null)
                {
                    _interfaces = new StringBuilder();
                }
                else
                {
                    if (_interfaces.Length > 0)
                    {
                        _interfaces.Append(", ");
                    }
                }

                _interfaces.Append(typeName);
                return true;
            }

            /// <summary>
            /// Conditionally appends additional implemented interface.
            /// </summary>
            public bool AddInterface(string interfaceName, bool condition)
            {
                if (condition)
                {
                    return AddInterface(interfaceName);
                }

                return false;
            }

            public override string ToString()
            {
                var hasInterfaces = _interfaces != null && _interfaces.Length > 0;

                if (string.IsNullOrEmpty(_typeName))
                {
                    return hasInterfaces ? " : " + _interfaces : string.Empty;
                }

                return hasInterfaces ? string.Concat(" : ", _typeName, ", ", _interfaces) : " : " + _typeName;
            }
        }

        #endregion


        #region StartNewFile / FinishCurrentFile - with header and footer
        void StartNewFile(string path, bool writeHeader = true)
        {
            FinishCurrentFile();
            CodegenOutputFile outputFile = _context.GetOutputFile(path, OutputFileType.Compile);
            w = outputFile.Writer;
            if (writeHeader)
                this.WriteFileHeader();
        }

        /// <summary>
        /// This is basically for writing header, namespace, and indenting
        /// </summary>
        void WriteFileHeader()
        {
            w?.WriteLine($@"
                // <auto-generated>
                // ReSharper disable ConvertPropertyToExpressionBody
                // ReSharper disable DoNotCallOverridableMethodsInConstructor
                // ReSharper disable EmptyNamespace
                // ReSharper disable InconsistentNaming
                // ReSharper disable PartialMethodWithSinglePart
                // ReSharper disable PartialTypeWithSinglePart
                // ReSharper disable RedundantNameQualifier
                // ReSharper disable RedundantOverridenMember
                // ReSharper disable UseNameofExpression
                // TargetFrameworkVersion = { Settings.TargetFrameworkVersion }
                #pragma warning disable 1591    //  Ignore ""Missing XML Comment"" warning
                
                ");

            if (Settings.ElementsToGenerate.HasFlag(Elements.Poco) || Settings.ElementsToGenerate.HasFlag(Elements.PocoConfiguration)) // Line 20
            {
                if (Settings.UseDataAnnotations || Settings.UseDataAnnotationsWithFluent)
                {
                    w?.WriteLine("using System.ComponentModel.DataAnnotations;");
                }
                if (Settings.UseDataAnnotations)
                {
                    w?.WriteLine("using System.ComponentModel.DataAnnotations.Schema;");
                }
            }

            w?.WriteLine($"namespace { Settings.Namespace }"); // Line 31 // this is in StartNewFile()
            w?.WriteLine("{"); // this is in StartNewFile()
            w?.IncreaseIndent();
            foreach (var usingStatement in usingsAll.Distinct().OrderBy(x => x))
                w?.WriteLine($"using { usingStatement };");
            w?.WriteLine();
        }

        void FinishCurrentFile()
        {
            if (this.w == null)
                return;
            w?.DecreaseIndent();
            w?.WriteLine("}");
            w?.WriteLine("// </auto-generated>");
            //w?.Flush();
            w.Dispose();
            w = null;
        }

        #endregion
    }
}
