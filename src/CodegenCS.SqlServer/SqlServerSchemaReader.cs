using Dapper;
using System;
using System.Data;
using System.IO;
using System.Linq;

public class SqlServerSchemaReader
{
    public Func<IDbConnection> CreateDbConnection { get; set; }

    public SqlServerSchemaReader(Func<IDbConnection> createDbConnection)
    {
        CreateDbConnection = createDbConnection;
    }

    public void ExportSchemaToJSON(string outputJsonSchema)
    {
        Console.WriteLine("Reading Database...");

        using (var cn = CreateDbConnection())
        {
            var tables = cn.Query<SqlServerTable>(@"
                SELECT 
                    t.TABLE_CATALOG as [Database], 
                    t.TABLE_SCHEMA as [TableSchema], 
                    t.TABLE_NAME as [TableName], 
                    CASE WHEN t.TABLE_TYPE='VIEW' THEN 'VIEW' ELSE 'TABLE' END as [TableType],
                    ep.value as [TableDescription]
		        FROM  INFORMATION_SCHEMA.TABLES t
		        INNER JOIN sys.schemas sc ON t.TABLE_SCHEMA = sc.[name]
		        LEFT OUTER JOIN sys.objects so ON so.schema_id = so.schema_id AND so.[name] = t.TABLE_NAME AND so.[Type] IN ('U','V')
                LEFT OUTER JOIN sys.extended_properties ep ON ep.name='MS_Description' AND  ep.class = 1  AND ep.major_id = so.object_id AND ep.minor_id = 0
		        WHERE t.TABLE_TYPE='BASE TABLE' OR t.TABLE_TYPE='VIEW'
		        ORDER BY t.TABLE_SCHEMA, t.TABLE_TYPE, t.TABLE_NAME
            ").AsList();

            // Based on PetaPoco T4 Templates (https://github.com/CollaboratingPlatypus/PetaPoco/blob/development/T4Templates/PetaPoco.Core.ttinclude)
            var allColumns = cn.Query<SqlServerColumn>(@"
                IF OBJECT_ID('tempdb..#PrimaryKeyColumns') IS NOT NULL DROP TABLE #PrimaryKeyColumns;
                SELECT cu.TABLE_SCHEMA, cu.TABLE_NAME, cu.COLUMN_NAME, cu.ORDINAL_POSITION INTO #PrimaryKeyColumns
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON cu.TABLE_SCHEMA COLLATE DATABASE_DEFAULT = tc.CONSTRAINT_SCHEMA COLLATE DATABASE_DEFAULT AND cu.TABLE_NAME = tc.TABLE_NAME AND cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY';

                IF OBJECT_ID('tempdb..#ForeignKeyColumns') IS NOT NULL DROP TABLE #ForeignKeyColumns;
                SELECT DISTINCT cu.TABLE_SCHEMA, cu.TABLE_NAME, cu.COLUMN_NAME INTO #ForeignKeyColumns
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON cu.TABLE_SCHEMA COLLATE DATABASE_DEFAULT = tc.CONSTRAINT_SCHEMA COLLATE DATABASE_DEFAULT AND cu.TABLE_NAME = tc.TABLE_NAME AND cu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY';

                SELECT 
			        c.TABLE_CATALOG as [Database],
			        c.TABLE_SCHEMA as [TableSchema], 
			        c.TABLE_NAME as TableName, 
			        c.COLUMN_NAME as ColumnName, 
			        c.ORDINAL_POSITION as OrdinalPosition, 
			        c.COLUMN_DEFAULT as DefaultSetting, 
			        sc.is_nullable as IsNullable, -- c.IS_NULLABLE as IsNullable, 
			        c.DATA_TYPE as SqlDataType, 
			        c.CHARACTER_MAXIMUM_LENGTH as MaxLength, 
			        c.DATETIME_PRECISION as DateTimePrecision,
			        c.NUMERIC_SCALE as [NumericScale],
			        c.NUMERIC_PRECISION as [NumericPrecision],
			        sc.is_identity as IsIdentity, 
			        sc.is_computed as IsComputed, 
			        sc.is_rowguidcol as IsRowGuid,
                    CONVERT( bit, ISNULL( pk.ORDINAL_POSITION, 0 ) ) as [IsPrimaryKeyMember],
                    pk.ORDINAL_POSITION as [PrimaryKeyOrdinalPosition],
                    CONVERT( bit, CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END ) as [IsForeignKeyMember],
                    ep.value as [ColumnDescription]
		        FROM  INFORMATION_SCHEMA.COLUMNS c
		        INNER JOIN sys.schemas ss ON c.TABLE_SCHEMA = ss.[name]
		        LEFT OUTER JOIN sys.tables st ON st.schema_id = ss.schema_id AND st.[name] = c.TABLE_NAME
		        LEFT OUTER JOIN sys.views sv ON sv.schema_id = ss.schema_id AND sv.[name] = c.TABLE_NAME
		        INNER JOIN sys.all_columns sc ON sc.object_id = COALESCE( st.object_id, sv.object_id ) AND c.COLUMN_NAME = sc.[name]
                LEFT OUTER JOIN sys.extended_properties ep ON ep.name='MS_Description' AND  ep.class = 1  AND ep.major_id = st.object_id AND ep.minor_id = sc.column_id

		        LEFT OUTER JOIN #PrimaryKeyColumns pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME  = pk.COLUMN_NAME
		        LEFT OUTER JOIN #ForeignKeyColumns fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA AND c.TABLE_NAME = fk.TABLE_NAME AND c.COLUMN_NAME  = fk.COLUMN_NAME

		        ORDER BY 1,2,3,OrdinalPosition ASC
            ").AsList();

            var fks = cn.Query<SqlServerForeignKey>(@"
                SELECT 
                    i.name as PrimaryKeyName,
	                pksch.name as PKTableSchema,
                    pk.name as PKTableName,

                    f.name as [ForeignKeyConstraintName],
	                ep.value as [ForeignKeyDescription],
                    fksch.name as FKTableSchema,
	                fk.name as FKTableName,

	                f.delete_referential_action_desc as [OnDeleteCascade], -- NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
	                f.update_referential_action_desc as [OnUpdateCascade], -- NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
	                f.is_system_named as [IsSystemNamed],
                    f.is_disabled as [IsNotEnforced]
                    --,k.constraint_column_id as PKColumnOrdinalPosition,
                    --pkCols.name as PKColumnName,
                    --fkCols.name as FKColumnName

                FROM   
	                sys.objects pk
                    INNER JOIN sys.foreign_keys as f ON pk.object_id = f.referenced_object_id
                    --INNER JOIN sys.foreign_key_columns as k ON k.constraint_object_id = f.object_id
                    INNER JOIN sys.indexes as i ON f.referenced_object_id = i.object_id AND f.key_index_id = i.index_id
                    INNER JOIN sys.objects fk ON f.parent_object_id = fk.object_id
                    --INNER JOIN sys.columns as pkCols ON f.referenced_object_id = pkCols.object_id AND k.referenced_column_id = pkCols.column_id
                    --INNER JOIN sys.columns as fkCols ON f.parent_object_id = fkCols.object_id AND k.parent_column_id = fkCols.column_id
	                INNER JOIN sys.schemas pksch ON pk.schema_id = pksch.schema_id
	                INNER JOIN sys.schemas fksch ON fk.schema_id = fksch.schema_id
	                LEFT OUTER JOIN sys.extended_properties ep ON ep.name='MS_Description' AND  ep.class = 1  AND ep.major_id = f.object_id AND ep.minor_id = 0
            ").AsList();

            var fkCols = cn.Query<SqlServerForeignKeyMember>(@"
                SELECT 
                    f.name as [ForeignKeyConstraintName],
                    fksch.name as FKTableSchema,
                    k.constraint_column_id as PKColumnOrdinalPosition,
                    pkCols.name as PKColumnName,
                    fkCols.name as FKColumnName

                FROM
	                sys.objects pk
                    INNER JOIN sys.foreign_keys as f ON pk.object_id = f.referenced_object_id
                    INNER JOIN sys.foreign_key_columns as k ON k.constraint_object_id = f.object_id
                    INNER JOIN sys.indexes as i ON f.referenced_object_id = i.object_id AND f.key_index_id = i.index_id
                    INNER JOIN sys.objects fk ON f.parent_object_id = fk.object_id
                    INNER JOIN sys.columns as pkCols ON f.referenced_object_id = pkCols.object_id AND k.referenced_column_id = pkCols.column_id
                    INNER JOIN sys.columns as fkCols ON f.parent_object_id = fkCols.object_id AND k.parent_column_id = fkCols.column_id
	                INNER JOIN sys.schemas pksch ON pk.schema_id = pksch.schema_id
	                INNER JOIN sys.schemas fksch ON fk.schema_id = fksch.schema_id
            ").AsList();
            foreach (var fk in fks)
            {
                fk.Columns = fkCols.Where(c => c.ForeignKeyConstraintName == fk.ForeignKeyConstraintName && c.FKTableSchema == fk.FKTableSchema).OrderBy(c => c.PKColumnOrdinalPosition).ToList();
            }

            foreach (var table in tables)
            {
                table.Columns = allColumns.Where(c => c.TableSchema == table.TableSchema && c.TableName == table.TableName).ToList();
                foreach(var column in table.Columns)
                {
                    column.ClrType = GetClrType(table, column)?.FullName;
                }
                table.Columns.ForEach(c => { c.Database = null; c.TableSchema = null; c.TableName = null; });

                table.ForeignKeys = fks.Where(fk => fk.PKTableSchema == table.TableSchema && fk.PKTableName == table.TableName).ToList();
                table.ForeignKeys.ForEach(fk => { fk.PKTableSchema = null; fk.PKTableName = null; });

                table.ChildForeignKeys = fks.Where(fk => fk.FKTableSchema == table.TableSchema && fk.FKTableName == table.TableName).ToList();
                table.ChildForeignKeys.ForEach(fk => { fk.FKTableSchema = null; fk.FKTableName = null; });
            }

            SqlServerDatabaseSchema schema = new SqlServerDatabaseSchema()
            {
                LastRefreshed = DateTimeOffset.Now,
                Tables = tables,
            };

            Console.WriteLine($"Saving into {outputJsonSchema}...");
            File.WriteAllText(outputJsonSchema, Newtonsoft.Json.JsonConvert.SerializeObject(schema, Newtonsoft.Json.Formatting.Indented));
        }

        Console.WriteLine("Success!");
    }

    System.Type GetClrType(SqlServerTable table, SqlServerColumn column)
    {
        string sqlDataType = column.SqlDataType;
        switch (sqlDataType)
        {
            case "bigint":
                return typeof(long);
            case "smallint":
                return typeof(short);
            case "int":
                return typeof(int);
            case "uniqueidentifier":
                return typeof(Guid);
            case "smalldatetime":
            case "datetime":
            case "date":
            case "time":
                return typeof(DateTime);
            case "float":
                return typeof(double);
            case "real":
                return typeof(float);
            case "numeric":
            case "smallmoney":
            case "decimal":
            case "money":
                return typeof(decimal);
            case "tinyint":
                return typeof(byte);
            case "bit":
                return typeof(bool);
            case "image":
            case "binary":
            case "varbinary":
            case "timestamp":
                return typeof(byte[]);
            case "nvarchar":
            case "varchar":
            case "nchar":
            case "text":
            case "xml":
                return typeof(string);
            default:
                Console.WriteLine($"Unknown sqlDataType for {table.TableName}.{column.ColumnName}: {sqlDataType}");
                return null;

            // there's no unique mapping for these types (it depends on the ORM)
            // leave these special mappings to the POCO/DbContext generator which is ORM-specific...
            case "hierarchyid":
            case "geography":
                return null; 
        }
    }

}

