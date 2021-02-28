using Dapper;
using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema.PostgreSQL
{
#endif

    public class PgsqlSchemaReader
    {
        public Func<IDbConnection> CreateDbConnection { get; set; }

        public PgsqlSchemaReader(Func<IDbConnection> createDbConnection)
        {
            CreateDbConnection = createDbConnection;

            #if !DLL // if this is included in a CSX file we'll have to tricky some assembly resolutions to ignore versions
            // Let Npgsql load ANY version of assemblies
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            #endif
        }

#if !DLL // if this is included in a CSX file we'll have to tricky some assembly resolutions to ignore versions
        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var name = new AssemblyName(args.Name);
            if (name.Name == "System.Threading.Channels")
            {
                return typeof(System.Threading.Channels.Channel).Assembly;
            }
            if (name.Name == "System.Text.Json")
            {
                return typeof(System.Text.Json.JsonDocument).Assembly;
            }
            return null;
        }
#endif


        public void ExportSchemaToJSON(string outputJsonSchema)
        {
            Console.WriteLine("Reading Database...");
            using (var cn = CreateDbConnection())
            {
                Console.WriteLine("cn.Query<Table>...");
                var tables = cn.Query<Table>(@"
                    SELECT
                        current_database() as Database,
                        nsp.nspname as TableSchema, 
                           cls.relname as TableName, 
                           rol.rolname as owner, 
                           case cls.relkind
                             when 'r' then 'TABLE'
                             when 'm' then 'MATERIALIZED_VIEW'
                             when 'i' then 'INDEX'
                             when 'S' then 'SEQUENCE'
                             when 'v' then 'VIEW'
                             when 'c' then 'TYPE'
                             else cls.relkind::text
                           end as TableType,
                           obj_description(cls.oid) as TableDescription,
                           tco.constraint_name as PrimaryKeyName,
                           1 as PrimaryKeyIsClustered
                    from pg_class cls
                      join pg_roles rol on rol.oid = cls.relowner
                      join pg_namespace nsp on nsp.oid = cls.relnamespace
                      left join information_schema.table_constraints tco on nsp.nspname=tco.table_schema and cls.relname=tco.table_name and tco.constraint_type = 'PRIMARY KEY'
                    where nsp.nspname not in ('information_schema', 'pg_catalog')
                      and nsp.nspname not like 'pg_toast%'
                      --and rol.rolname = current_user  --- remove this if you want to see all objects
                      and cls.relkind IN ('r','v')
                    order by nsp.nspname, cls.relname;
                ").AsList();

                Console.WriteLine("cn.Query<ColumnTmp>...");
                var allColumns = cn.Query<ColumnTmp>(@"
                    DROP TABLE IF EXISTS tmpForeignKeyColumns;

                    select DISTINCT kcu.table_schema, kcu.table_name, kcu.column_name
                    INTO TEMP tmpForeignKeyColumns
                    from information_schema.table_constraints tco
                    join information_schema.key_column_usage kcu 
                         on kcu.constraint_name = tco.constraint_name
                         and kcu.constraint_schema = tco.constraint_schema
                         and kcu.constraint_name = tco.constraint_name
                    where tco.constraint_type = 'FOREIGN KEY'
                    order by kcu.table_schema, kcu.table_name, kcu.column_name;


                    SELECT 
                    current_database() as Database,
                    c.table_schema as TableSchema,
                    c.table_name as TableName,
                    c.column_name as ColumnName,
                    c.ordinal_position as OrdinalPosition,
                    c.column_default as DefaultSetting,
                    case when c.is_nullable='YES' then 1 else 0 end as IsNullable,
                    c.data_type as SqlDataType,
                    c.character_maximum_length as MaxLength,
                    c.datetime_precision as DateTimePrecision,
                    c.numeric_scale as NumericScale,
                    c.numeric_precision as NumericPrecision,
                    case when c.is_identity='YES' then 1 else 0 end as IsIdentity,
                    case when c.generation_expression IS NOT NULL then 1 else 0 end as IsComputed,
                    0 as IsRowGuid, -- no such thing in pgsql?
                    case when pkcol.ordinal_position is NULL then 0 else 1 end as IsPrimaryKeyMember,
                    pkcol.ordinal_position as PrimaryKeyOrdinalPosition,
                    case when fkcol.column_name is NULL then 0 else 1 end as IsForeignKeyMember,
                    NULL as ColumnDescription --TODO: obj_description?
                    ,* 
                    FROM information_schema.columns c
                    left join information_schema.table_constraints tco on c.table_schema=tco.table_schema and c.table_name=tco.table_name and tco.constraint_type = 'PRIMARY KEY'

                    left join information_schema.key_column_usage pkcol
                         on pkcol.constraint_name = tco.constraint_name
                         and pkcol.constraint_schema = tco.constraint_schema
                         and pkcol.constraint_name = tco.constraint_name
                         and pkcol.column_name = c.column_name
                    left join tmpForeignKeyColumns fkcol
                         on fkcol.table_schema = c.table_schema
                         and fkcol.table_name = c.table_name
                         and fkcol.column_name = c.column_name
                    WHERE c.table_name<>'tmpforeignkeycolumns'
                    ORDER BY 1,2,3,OrdinalPosition ASC
            ").AsList();

                Console.WriteLine("cn.Query<ForeignKey>...");
                var fks = cn.Query<ForeignKey>(@"
                    SELECT DISTINCT
                        pkt.constraint_name as PrimaryKeyName,
                        tc.table_schema AS PKTableSchema,
                        ccu.table_name AS PKTableName,
                        tc.constraint_name as ForeignKeyConstraintName, 
                        NULL as ForeignKeyDescription, --TODO: obj_description
                        tc.table_schema as FKTableSchema, 
                        tc.table_name as FKTableName,
                        NULL as OnDeleteCascade,
                        NULL as OnUpdateCascade,
                        NULL as IsSystemNamed, --TODO: do we know this?
                        case when tc.enforced='NO' then 1 else 0 end as IsNotEnforced

                    FROM 
                        information_schema.table_constraints AS tc 
                        JOIN information_schema.key_column_usage AS kcu
                          ON tc.constraint_name = kcu.constraint_name
                          AND tc.table_schema = kcu.table_schema
                        JOIN information_schema.constraint_column_usage AS ccu
                          ON ccu.constraint_name = tc.constraint_name
                          AND ccu.table_schema = tc.table_schema
                        left join information_schema.table_constraints pkt on ccu.table_schema=pkt.table_schema and ccu.table_name=pkt.table_name and pkt.constraint_type = 'PRIMARY KEY'
                    WHERE tc.constraint_type = 'FOREIGN KEY';
            ").AsList();

                Console.WriteLine("cn.Query<ForeignKeyMemberTmp>...");
                var fkCols = cn.Query<ForeignKeyMemberTmp>(@"
                    SELECT 
                        pkt.constraint_name as PrimaryKeyName,
                        ccu.table_schema AS PKTableSchema,
                        ccu.table_name AS PKTableName,
                        tc.constraint_name as ForeignKeyConstraintName, 
                        tc.table_schema as FKTableSchema, 
                        tc.table_name as FKTableName,
                        kcu.column_name as FKColumnName, 
                        ccu.column_name AS PKColumnName,
                        kcu.position_in_unique_constraint as PKColumnOrdinalPosition,
                        kcu.ordinal_position as FKColumnOrdinalPosition
                    FROM 
                        information_schema.table_constraints AS tc 
                        JOIN information_schema.key_column_usage AS kcu
                          ON tc.constraint_name = kcu.constraint_name
                          AND tc.table_schema = kcu.table_schema
                        JOIN information_schema.constraint_column_usage AS ccu
                          ON ccu.constraint_name = tc.constraint_name
                          AND ccu.table_schema = tc.table_schema
                        left join information_schema.table_constraints pkt on ccu.table_schema=pkt.table_schema and ccu.table_name=pkt.table_name and pkt.constraint_type = 'PRIMARY KEY'
                    WHERE tc.constraint_type = 'FOREIGN KEY';
            ").AsList();

                Console.WriteLine("cn.Query<IndexTmp>...");
                var indexes = cn.Query<IndexTmp>(@"

                    select DISTINCT
                        ns.nspname as TableSchema,
                        t.relname as TableName,
                        i.relname as IndexName,
                        NULL as IndexId,
                        case when ix.indisclustered then 'CLUSTERED' else 'NONCLUSTERED' end as PhysicalType, 
                        CASE 
                            WHEN ix.indisprimary THEN 'PRIMARY_KEY' 
                            WHEN ix.indisunique THEN 'UNIQUE_INDEX' 
                            ELSE '??'
                        END as LogicalType, --TODO: NON_UNIQUE_INDEX? UNIQUE_CONSTRAINT?
                        case when ix.indoption[array_position(ix.indkey, a.attnum)] & 1 = 1 then 1 else 0 end as IsDescendingKey,
                        case when ix.indisprimary then 1 else 0 end as IsPrimaryKey, 
                        case when ix.indisunique then 1 else 0 end as IsUnique, 
                        0 as IsUniqueConstraint, -- Does PGSQL has this?
                        NULL as IndexDescription, --TODO: obj_description
                        0 as IsIncludedColumn -- where is this stored?

                    from
                        pg_class t,
                        pg_index ix,
                        pg_class i,
                        pg_attribute a,
                        pg_namespace ns
                    where
                        t.oid = ix.indrelid
                        and i.oid = ix.indexrelid
                        and a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
                        and t.relkind = 'r'
                        and t.relnamespace = ns.oid
                        and ns.nspname<>'pg_catalog'
                    order by
                        1,2,3;
            ");

                Console.WriteLine("cn.Query<IndexMemberTmp>...");
                var indexesCols = cn.Query<IndexMemberTmp>(@"
                    select
                        ns.nspname as TableSchema,
                        t.relname as TableName,
                        i.relname as IndexName,
                        NULL as IndexId,
                        a.attname as ColumnName,
                        array_position(ix.indkey, a.attnum) as IndexOrdinalPosition,
                        case when ix.indoption[array_position(ix.indkey, a.attnum)] & 1 = 1 then 1 else 0 end as IsDescendingKey,
                        0 as IsIncludedColumn -- where is this stored?
                    from
                        pg_class t,
                        pg_index ix,
                        pg_class i,
                        pg_attribute a,
                        pg_namespace ns
                    where
                        t.oid = ix.indrelid
                        and i.oid = ix.indexrelid
                        and a.attrelid = t.oid and a.attnum = ANY(ix.indkey)
                        and t.relkind = 'r'
                        and t.relnamespace = ns.oid
                        and ns.nspname<>'pg_catalog'
                    order by
                        1,2,3,IndexOrdinalPosition;
            ");


                foreach (var fk in fks)
                {
                    fk.Columns = fkCols.Where(c => c.ForeignKeyConstraintName == fk.ForeignKeyConstraintName && c.FKTableSchema == fk.FKTableSchema)
                        .OrderBy(c => c.PKColumnOrdinalPosition)
                        .Select(c => Map<ForeignKeyMember, ForeignKeyMemberTmp>(c))
                        .ToList();
                }

                foreach (var index in indexes)
                {
                    index.Columns = indexesCols.Where(c => c.TableSchema == index.TableSchema && c.TableName == index.TableName && c.IndexName == index.IndexName)
                        .OrderBy(c => c.IndexOrdinalPosition)
                        .Select(c => Map<IndexMember, IndexMemberTmp>(c))
                        .ToList();
                }

                foreach (var table in tables)
                {
                    table.Columns = allColumns.Where(c => c.TableSchema == table.TableSchema && c.TableName == table.TableName).Select(c => Map<Column, ColumnTmp>(c)).ToList();
                    foreach (var column in table.Columns)
                    {
                        column.ClrType = GetClrType(table, column);
                    }

                    // We copy FKs and remove redundant properties of the parent object (table) which we're attaching this FK into
                    table.ForeignKeys = Clone(fks.Where(fk => fk.FKTableSchema == table.TableSchema && fk.FKTableName == table.TableName).ToList());
                    table.ForeignKeys.ForEach(fk => { fk.FKTableSchema = null; fk.FKTableName = null; });

                    // We copy FKs and remove redundant properties of the parent object (table) which we're attaching this FK into
                    table.ChildForeignKeys = Clone(fks.Where(fk => fk.PKTableSchema == table.TableSchema && fk.PKTableName == table.TableName).ToList());
                    table.ChildForeignKeys.ForEach(fk => { fk.PKTableSchema = null; fk.PKTableName = null; });

                    table.Indexes = indexes.Where(i => i.TableSchema == table.TableSchema && i.TableName == table.TableName)
                        .Select(i => Map<Index, IndexTmp>(i))
                        .ToList();
                }

                DatabaseSchema schema = new DatabaseSchema()
                {
                    LastRefreshed = DateTimeOffset.Now,
                    Tables = tables,
                };

                Console.WriteLine($"Saving into {outputJsonSchema}...");
                File.WriteAllText(outputJsonSchema, JsonConvert.SerializeObject(schema, Newtonsoft.Json.Formatting.Indented));
            }

            Console.WriteLine("Success!");
        }

        string GetClrType(Table table, Column column)
        {
            string sqlDataType = column.SqlDataType;
            switch (sqlDataType)
            {
                case "bigint":
                    return typeof(long).FullName;
                case "smallint":
                    return typeof(short).FullName;
                case "int":
                case "integer":
                    return typeof(int).FullName;
                case "uniqueidentifier":
                case "uuid":
                    return typeof(Guid).FullName;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "date":
                case "time":
                case "timestamp without time zone":
                    return typeof(DateTime).FullName;
                case "datetimeoffset":
                    return typeof(DateTimeOffset).FullName;
                case "float":
                case "double precision":
                    return typeof(double).FullName;
                case "real":
                    return typeof(float).FullName;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    return typeof(decimal).FullName;
                case "tinyint":
                    return typeof(byte).FullName;
                case "bit":
                case "boolean":
                    return typeof(bool).FullName;
                case "image":
                case "binary":
                case "varbinary":
                case "timestamp":
                case "bytea":
                    return typeof(byte[]).FullName;
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                case "text":
                case "ntext":
                case "xml":
                case "character varying":
                case "character":
                    return typeof(string).FullName;
                case "time without time zone":
                    return typeof(TimeSpan).FullName;
                default:
                    Console.WriteLine($"Unknown sqlDataType for {table.TableName}.{column.ColumnName}: {sqlDataType}");
                    return null;

                // Vendor-specific types
            }
        }

        public static T Clone<T>(T source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }
        public static T Map<T, S>(S source)
        {
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<T>(serialized);
        }

#region Temporary Classes used just for Bulk Loads
        class ColumnTmp : Column
        {
            public string Database { get; set; }
            public string TableSchema { get; set; }
            public string TableName { get; set; }

        }
        class IndexTmp : Index
        {
            public string Database { get; set; }

            public string TableSchema { get; set; }

            public string TableName { get; set; }
        }
        class ForeignKeyMemberTmp : ForeignKeyMember
        {
            public string ForeignKeyConstraintName { get; set; }
            public string FKTableSchema { get; set; }
        }
        class IndexMemberTmp : IndexMember
        {
            public string Database { get; set; }
            public string TableSchema { get; set; }
            public string TableName { get; set; }
            public string IndexName { get; set; }
            public int IndexId { get; set; }

        }
#endregion

        public void DebugError()
        {
            try
            {
                var cn2 = CreateDbConnection();
                cn2.Open();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.InnerException.Message);
            }
        }


    }
#if DLL
}
#endif