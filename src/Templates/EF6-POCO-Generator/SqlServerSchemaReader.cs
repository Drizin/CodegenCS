using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EF6POCOGenerator
{
    public class SqlServerSchemaReader : SchemaReader
    {
        private static string _sqlDatabaseEdition, _sqlDatabaseEngineEdition, _sqlDatabaseProductVersion;
        private static int _sqlDatabaseProductMajorVersion;

        private const string TableSQL = @"
SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#Columns')     IS NOT NULL DROP TABLE #Columns;
IF OBJECT_ID('tempdb..#PrimaryKeys') IS NOT NULL DROP TABLE #PrimaryKeys;
IF OBJECT_ID('tempdb..#ForeignKeys') IS NOT NULL DROP TABLE #ForeignKeys;

SELECT
    c.TABLE_SCHEMA,
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.ORDINAL_POSITION,
    c.COLUMN_DEFAULT,
    sc.IS_NULLABLE,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.DATETIME_PRECISION,

    ss.schema_id,
    st.object_id AS table_object_id,
    sv.object_id AS view_object_id,

    sc.is_identity,
    sc.is_rowguidcol,
    sc.is_computed, -- Computed columns are read-only, do not confuse it with a column with a DEFAULT expression (which can be re-assigned). See the IsStoreGenerated attribute.
    CONVERT( tinyint, [sc].[generated_always_type] ) AS generated_always_type -- SQL Server 2016 (13.x) or later. 0 = Not generated, 1 = AS_ROW_START, 2 = AS_ROW_END

INTO
    #Columns
FROM
    INFORMATION_SCHEMA.COLUMNS c

    INNER JOIN sys.schemas AS ss ON c.TABLE_SCHEMA = ss.[name]
    LEFT OUTER JOIN sys.tables AS st ON st.schema_id = ss.schema_id AND st.[name] = c.TABLE_NAME
    LEFT OUTER JOIN sys.views AS sv ON sv.schema_id = ss.schema_id AND sv.[name] = c.TABLE_NAME
    INNER JOIN sys.all_columns AS sc ON sc.object_id = COALESCE( st.object_id, sv.object_id ) AND c.COLUMN_NAME = sc.[name]

WHERE
   c.TABLE_NAME NOT IN ('EdmMetadata', '__MigrationHistory', '__RefactorLog', 'sysdiagrams')


CREATE NONCLUSTERED INDEX IX_EfPoco_Columns
    ON dbo.#Columns (TABLE_NAME)
    INCLUDE (
        TABLE_SCHEMA,COLUMN_NAME,ORDINAL_POSITION,COLUMN_DEFAULT,IS_NULLABLE,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,NUMERIC_PRECISION,NUMERIC_SCALE,DATETIME_PRECISION,
        schema_id, table_object_id, view_object_id,
        is_identity,is_rowguidcol,is_computed,generated_always_type
    );

-----------

SELECT
    u.TABLE_SCHEMA,
    u.TABLE_NAME,
    u.COLUMN_NAME,
    u.ORDINAL_POSITION
INTO
    #PrimaryKeys
FROM
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON
        u.TABLE_SCHEMA COLLATE DATABASE_DEFAULT = tc.CONSTRAINT_SCHEMA COLLATE DATABASE_DEFAULT
        AND
        u.TABLE_NAME = tc.TABLE_NAME
        AND
        u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
WHERE
    CONSTRAINT_TYPE = 'PRIMARY KEY';

SELECT DISTINCT
    u.TABLE_SCHEMA,
    u.TABLE_NAME,
    u.COLUMN_NAME
INTO
    #ForeignKeys
FROM
    INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
    INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON
        u.TABLE_SCHEMA COLLATE DATABASE_DEFAULT = tc.CONSTRAINT_SCHEMA COLLATE DATABASE_DEFAULT
        AND
        u.TABLE_NAME = tc.TABLE_NAME
        AND
        u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
WHERE
    CONSTRAINT_TYPE = 'FOREIGN KEY';

--------------------------

SELECT
    c.TABLE_SCHEMA AS SchemaName,
    c.TABLE_NAME AS TableName,
    t.TABLE_TYPE AS TableType,
    CONVERT( tinyint, ISNULL( tt.temporal_type, 0 ) ) AS TableTemporalType,

    c.ORDINAL_POSITION AS Ordinal,
    c.COLUMN_NAME AS ColumnName,
    c.IS_NULLABLE AS IsNullable,
    DATA_TYPE AS TypeName,
    ISNULL(CHARACTER_MAXIMUM_LENGTH, 0) AS [MaxLength],
    CAST(ISNULL(NUMERIC_PRECISION, 0) AS INT) AS [Precision],
    ISNULL(COLUMN_DEFAULT, '') AS [Default],
    CAST(ISNULL(DATETIME_PRECISION, 0) AS INT) AS DateTimePrecision,
    ISNULL(NUMERIC_SCALE, 0) AS Scale,

    c.is_identity AS IsIdentity,
    c.is_rowguidcol AS IsRowGuid,
    c.is_computed AS IsComputed,
    c.generated_always_type AS GeneratedAlwaysType,

    CONVERT( bit,
        CASE WHEN
            c.is_identity = 1 OR
            c.is_rowguidcol = 1 OR
            c.is_computed = 1 OR
            c.generated_always_type <> 0 OR
            c.DATA_TYPE IN ( 'rowversion', 'timestamp' ) OR
            ( c.DATA_TYPE = 'uniqueidentifier' AND c.COLUMN_DEFAULT LIKE '%newsequentialid%' )
            THEN 1
        ELSE
            0
        END
    ) AS IsStoreGenerated,

    CONVERT( bit, ISNULL( pk.ORDINAL_POSITION, 0 ) ) AS PrimaryKey,
    ISNULL(pk.ORDINAL_POSITION, 0) PrimaryKeyOrdinal,
    CONVERT( bit, CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END ) AS IsForeignKey

FROM
    #Columns c

    LEFT OUTER JOIN #PrimaryKeys pk ON
        c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND
        c.TABLE_NAME   = pk.TABLE_NAME AND
        c.COLUMN_NAME  = pk.COLUMN_NAME

    LEFT OUTER JOIN #ForeignKeys fk ON
        c.TABLE_SCHEMA = fk.TABLE_SCHEMA AND
        c.TABLE_NAME   = fk.TABLE_NAME AND
        c.COLUMN_NAME  = fk.COLUMN_NAME

    INNER JOIN INFORMATION_SCHEMA.TABLES t ON
        c.TABLE_SCHEMA COLLATE DATABASE_DEFAULT = t.TABLE_SCHEMA COLLATE DATABASE_DEFAULT AND
        c.TABLE_NAME   COLLATE DATABASE_DEFAULT = t.TABLE_NAME   COLLATE DATABASE_DEFAULT

    LEFT OUTER JOIN
    (
        SELECT
            st.object_id,
            [st].[temporal_type] AS temporal_type
        FROM
            sys.tables AS st
    ) AS tt ON c.table_object_id = tt.object_id
";

        private const string SynonymTableSQLSetup = @"
SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#SynonymDetails') IS NOT NULL DROP TABLE #SynonymDetails;
IF OBJECT_ID('tempdb..#SynonymTargets') IS NOT NULL DROP TABLE #SynonymTargets;

-- Synonyms
-- Create the #SynonymDetails temp table structure for later use
SELECT TOP (0)
    sc.name AS SchemaName,
    sn.name AS TableName,
    'SN' AS TableType,
    CONVERT( tinyint, 0 ) AS TableTemporalType,
    COLUMNPROPERTY(c.object_id, c.name, 'ordinal') AS Ordinal,
    c.name AS ColumnName,
    c.is_nullable AS IsNullable,
    ISNULL(TYPE_NAME(c.system_type_id), t.name) AS TypeName,
    ISNULL(COLUMNPROPERTY(c.object_id, c.name, 'charmaxlen'), 0) AS MaxLength,
    CAST(ISNULL(CONVERT(TINYINT, CASE WHEN c.system_type_id IN (48, 52, 56, 59, 60, 62, 106, 108, 122, 127) THEN c.precision END), 0) AS INT) AS Precision,
    ISNULL(CONVERT(NVARCHAR(4000), OBJECT_DEFINITION(c.default_object_id)), '') AS [Default],
    CAST(ISNULL(CONVERT(SMALLINT, CASE WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN ODBCSCALE(c.system_type_id, c.scale) END), 0) AS INT) AS DateTimePrecision,
    ISNULL(CONVERT(INT, CASE WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN NULL ELSE ODBCSCALE(c.system_type_id, c.scale) END), 0) AS Scale,
    c.is_identity AS IsIdentity,
    c.is_rowguidcol AS IsRowGuid,
    c.is_computed AS IsComputed,
    CONVERT( tinyint, [c].[generated_always_type] ) AS GeneratedAlwaysType,
    CAST(CASE
        WHEN COLUMNPROPERTY(OBJECT_ID(QUOTENAME(sc.NAME) + '.' + QUOTENAME(o.NAME)), c.NAME, 'IsIdentity') = 1 THEN 1
        WHEN COLUMNPROPERTY(OBJECT_ID(QUOTENAME(sc.NAME) + '.' + QUOTENAME(o.NAME)), c.NAME, 'IsComputed') = 1 THEN 1
        WHEN COLUMNPROPERTY(OBJECT_ID(QUOTENAME(sc.NAME) + '.' + QUOTENAME(o.NAME)), c.NAME, 'GeneratedAlwaysType') > 0 THEN 1
        WHEN ISNULL(TYPE_NAME(c.system_type_id), t.NAME) = 'TIMESTAMP' THEN 1
        WHEN ISNULL(TYPE_NAME(c.system_type_id), t.NAME) = 'UNIQUEIDENTIFIER' AND LOWER(ISNULL(CONVERT(NVARCHAR(4000), OBJECT_DEFINITION(c.default_object_id)), '')) LIKE '%newsequentialid%' THEN 1
        ELSE 0
    END AS BIT) AS IsStoreGenerated,
    CAST(CASE WHEN pk.ORDINAL_POSITION IS NULL THEN 0 ELSE 1 END AS BIT) AS PrimaryKey,
    ISNULL(pk.ORDINAL_POSITION, 0) PrimaryKeyOrdinal,
    CAST(CASE WHEN fk.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS BIT) AS IsForeignKey
INTO
    #SynonymDetails
FROM
    sys.synonyms sn
    INNER JOIN sys.COLUMNS c ON c.[object_id] = OBJECT_ID(sn.base_object_name)
    INNER JOIN sys.schemas sc ON sc.[schema_id] = sn.[schema_id]
    LEFT JOIN sys.types t ON c.user_type_id = t.user_type_id
    INNER JOIN sys.objects o ON c.[object_id] = o.[object_id]
    LEFT OUTER JOIN
    (
        SELECT
            u.TABLE_SCHEMA,
            u.TABLE_NAME,
            u.COLUMN_NAME,
            u.ORDINAL_POSITION
        FROM
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON u.TABLE_SCHEMA = tc.CONSTRAINT_SCHEMA AND u.TABLE_NAME = tc.TABLE_NAME AND u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
        WHERE
            CONSTRAINT_TYPE = 'PRIMARY KEY'
    ) pk
        ON sc.NAME = pk.TABLE_SCHEMA AND sn.name = pk.TABLE_NAME AND c.name = pk.COLUMN_NAME

    LEFT OUTER JOIN
    (
        SELECT DISTINCT
            u.TABLE_SCHEMA,
            u.TABLE_NAME,
            u.COLUMN_NAME
        FROM
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON u.TABLE_SCHEMA = tc.CONSTRAINT_SCHEMA AND u.TABLE_NAME = tc.TABLE_NAME AND u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
        WHERE
            CONSTRAINT_TYPE = 'FOREIGN KEY'
    ) fk
        ON sc.NAME = fk.TABLE_SCHEMA AND sn.name = fk.TABLE_NAME AND c.name = fk.COLUMN_NAME;

DECLARE @synonymDetailsQueryTemplate nvarchar(max) = 'USE [@synonmymDatabaseName];
INSERT INTO #SynonymDetails (
    SchemaName, TableName, TableType, TableTemporalType, Ordinal, ColumnName, IsNullable, TypeName, [MaxLength], [Precision], [Default], DateTimePrecision, Scale,
    IsIdentity, IsRowGuid, IsComputed, GeneratedAlwaysType, IsStoreGenerated, PrimaryKey, PrimaryKeyOrdinal, IsForeignKey
)
SELECT
    st.SynonymSchemaName AS SchemaName,
    st.SynonymName AS TableName,
    ''SN'' AS TableType,
    CONVERT( tinyint, ISNULL( tt.temporal_type, 0 ) ) AS TableTemporalType,

    COLUMNPROPERTY(c.object_id, c.name, ''ordinal'') AS Ordinal,
    c.name AS ColumnName,
    c.is_nullable AS IsNullable,
    ISNULL(TYPE_NAME(c.system_type_id), t.name) AS TypeName,
    ISNULL(COLUMNPROPERTY(c.object_id, c.name, ''charmaxlen''), 0) AS [MaxLength],
    CAST(ISNULL(CONVERT(TINYINT, CASE WHEN c.system_type_id IN (48, 52, 56, 59, 60, 62, 106, 108, 122, 127) THEN c.precision END), 0) AS INT) AS [Precision],
    ISNULL(CONVERT(NVARCHAR(4000), OBJECT_DEFINITION(c.default_object_id)), '''') AS [Default],
    CAST(ISNULL(CONVERT(SMALLINT, CASE WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN ODBCSCALE(c.system_type_id, c.scale) END), 0) AS INT) AS DateTimePrecision,
    ISNULL(CONVERT(INT, CASE WHEN c.system_type_id IN (40, 41, 42, 43, 58, 61) THEN NULL ELSE ODBCSCALE(c.system_type_id, c.scale) END), 0) AS Scale,

    c.is_identity AS IsIdentity,
    c.is_rowguidcol AS IsRowGuid,
    c.is_computed AS IsComputed,
    CONVERT( tinyint, [c].[generated_always_type] ) AS GeneratedAlwaysType,

    CONVERT( bit,
        CASE
            WHEN
                c.is_identity = 1 OR
                c.is_rowguidcol = 1 OR
                c.is_computed = 1 OR
                [c].[generated_always_type] <> 0 OR
                t.name IN ( ''rowversion'', ''timestamp'' ) OR
                ( t.name = ''uniqueidentifier'' AND sd.definition LIKE ''%newsequentialid%'' )
                THEN 1
            ELSE 0
        END
    ) AS IsStoreGenerated,

    CAST(CASE WHEN pk.ORDINAL_POSITION IS NULL THEN 0  ELSE 1 END AS BIT) AS PrimaryKey,
    ISNULL(pk.ORDINAL_POSITION, 0) PrimaryKeyOrdinal,
    CAST(CASE WHEN fk.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS BIT) AS IsForeignKey
FROM
    #SynonymTargets st
    
    INNER JOIN sys.columns c ON c.[object_id] = st.base_object_id
    
    LEFT JOIN sys.types t ON c.user_type_id = t.user_type_id

    LEFT OUTER JOIN sys.default_constraints sd ON c.default_object_id = sd.object_id
    
    INNER JOIN sys.objects o ON c.[object_id] = o.[object_id]
    
    LEFT OUTER JOIN
    (
        SELECT
            u.TABLE_SCHEMA,
            u.TABLE_NAME,
            u.COLUMN_NAME,
            u.ORDINAL_POSITION
        FROM
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON u.TABLE_SCHEMA = tc.CONSTRAINT_SCHEMA AND u.TABLE_NAME = tc.TABLE_NAME AND u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
        WHERE
            CONSTRAINT_TYPE = ''PRIMARY KEY''
    ) AS pk ON
        st.SchemaName = pk.TABLE_SCHEMA AND
        st.ObjectName = pk.TABLE_NAME AND
        c.name        = pk.COLUMN_NAME
    
    LEFT OUTER JOIN
    (
        SELECT DISTINCT
            u.TABLE_SCHEMA,
            u.TABLE_NAME,
            u.COLUMN_NAME
        FROM
            INFORMATION_SCHEMA.KEY_COLUMN_USAGE u
            INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON
                u.TABLE_SCHEMA = tc.CONSTRAINT_SCHEMA AND
                u.TABLE_NAME = tc.TABLE_NAME AND
                u.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
        WHERE
            CONSTRAINT_TYPE = ''FOREIGN KEY''
    ) AS fk ON
        st.SchemaName = fk.TABLE_SCHEMA AND
        st.ObjectName = fk.TABLE_NAME AND
        c.name = fk.COLUMN_NAME

    LEFT OUTER JOIN
    (
        SELECT
            st.object_id,
            [st].[temporal_type] AS temporal_type
        FROM
            sys.tables AS st
    ) AS tt ON c.object_id = tt.object_id

WHERE
    st.DatabaseName = @synonmymDatabaseName;
'

-- Pull details about the synonym target from each database being referenced
SELECT
    sc.name AS SynonymSchemaName,
    sn.name AS SynonymName,
    sn.object_id,
    sn.base_object_name,
    OBJECT_ID(sn.base_object_name) AS base_object_id,
    PARSENAME(sn.base_object_name, 1) AS ObjectName,
    ISNULL(PARSENAME(sn.base_object_name, 2), sc.name) AS SchemaName,
    ISNULL(PARSENAME(sn.base_object_name, 3), DB_NAME()) AS DatabaseName,
    PARSENAME(sn.base_object_name, 4) AS ServerName
INTO
    #SynonymTargets
FROM
    sys.synonyms sn
    INNER JOIN sys.schemas sc ON sc.schema_id = sn.schema_id
WHERE
    ISNULL(PARSENAME(sn.base_object_name, 4), @@SERVERNAME) = @@SERVERNAME; -- Only populate info from current server

-- Loop through synonyms and populate #SynonymDetails
DECLARE @synonmymDatabaseName sysname = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
DECLARE @synonmymDetailsSelect nvarchar(max)
WHILE ( @synonmymDatabaseName IS NOT NULL)
BEGIN
    SET @synonmymDetailsSelect = REPLACE(@synonymDetailsQueryTemplate, '[@synonmymDatabaseName]', '[' + @synonmymDatabaseName + ']')
    --SELECT @synonmymDetailsSelect
    EXEC sp_executesql @stmt=@synonmymDetailsSelect, @params=N'@synonmymDatabaseName sysname', @synonmymDatabaseName=@synonmymDatabaseName
    DELETE FROM #SynonymTargets WHERE DatabaseName = @synonmymDatabaseName
    SET @synonmymDatabaseName = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
END
SET NOCOUNT OFF;
";

        private const string SynonymTableSQL = @"
UNION
-- Synonyms
SELECT
    SchemaName,
    TableName,
    TableType,
    CONVERT( tinyint, 0 ) AS TableTemporalType,

    Ordinal,
    ColumnName,
    IsNullable,
    TypeName,
    [MaxLength],
    [Precision],
    [Default],
    DateTimePrecision,
    Scale,

    IsIdentity,
    IsRowGuid,
    IsComputed,
    GeneratedAlwaysType,

    IsStoreGenerated,
    PrimaryKey,
    PrimaryKeyOrdinal,
    IsForeignKey
FROM
    #SynonymDetails";

        private const string ForeignKeySQL = @"
SELECT  fkData.FK_Table,
        fkData.FK_Column,
        fkData.PK_Table,
        fkData.PK_Column,
        fkData.Constraint_Name,
        fkData.fkSchema,
        fkData.pkSchema,
        fkData.primarykey,
        fkData.ORDINAL_POSITION,
        fkData.CascadeOnDelete,
        fkData.IsNotEnforced
FROM    (SELECT FK.name AS FK_Table,
                FkCol.name AS FK_Column,
                PK.name AS PK_Table,
                PkCol.name AS PK_Column,
                OBJECT_NAME(f.object_id) AS Constraint_Name,
                SCHEMA_NAME(FK.schema_id) AS fkSchema,
                SCHEMA_NAME(PK.schema_id) AS pkSchema,
                PkCol.name AS primarykey,
                k.constraint_column_id AS ORDINAL_POSITION,
                CASE WHEN f.delete_referential_action = 1 THEN 1
                     ELSE 0
                END AS CascadeOnDelete,
                f.is_disabled AS IsNotEnforced,
                ROW_NUMBER() OVER (PARTITION BY FK.name, FkCol.name, PK.name, PkCol.name, SCHEMA_NAME(FK.schema_id), SCHEMA_NAME(PK.schema_id) ORDER BY f.object_id) AS n
         FROM   sys.objects AS PK
                INNER JOIN sys.foreign_keys AS f
                    INNER JOIN sys.foreign_key_columns AS k
                        ON k.constraint_object_id = f.object_id
                    INNER JOIN sys.indexes AS i
                        ON f.referenced_object_id = i.object_id
                           AND f.key_index_id = i.index_id
                    ON PK.object_id = f.referenced_object_id
                INNER JOIN sys.objects AS FK
                    ON f.parent_object_id = FK.object_id
                INNER JOIN sys.columns AS PkCol
                    ON f.referenced_object_id = PkCol.object_id
                       AND k.referenced_column_id = PkCol.column_id
                INNER JOIN sys.columns AS FkCol
                    ON f.parent_object_id = FkCol.object_id
                       AND k.parent_column_id = FkCol.column_id) fkData
WHERE   fkData.n = 1 -- Remove duplicate FK's";

        private const string SynonymForeignKeySQLSetup = @"
SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#SynonymFkDetails') IS NOT NULL DROP TABLE #SynonymFkDetails;
IF OBJECT_ID('tempdb..#SynonymTargets') IS NOT NULL DROP TABLE #SynonymTargets;

-- Create the #SynonymFkDetails temp table structure for later use
SELECT  FK.name AS FK_Table,
        FkCol.name AS FK_Column,
        PK.name AS PK_Table,
        PkCol.name AS PK_Column,
        OBJECT_NAME(f.object_id) AS Constraint_Name,
        SCHEMA_NAME(FK.schema_id) AS fkSchema,
        SCHEMA_NAME(PK.schema_id) AS pkSchema,
        PkCol.name AS primarykey,
        k.constraint_column_id AS ORDINAL_POSITION,
        CASE WHEN f.delete_referential_action = 1 THEN 1 ELSE 0 END as CascadeOnDelete,
        f.is_disabled AS IsNotEnforced
INTO    #SynonymFkDetails
FROM    sys.objects AS PK
        INNER JOIN sys.foreign_keys AS f
            INNER JOIN sys.foreign_key_columns AS k
                ON k.constraint_object_id = f.object_id
            INNER JOIN sys.indexes AS i
                ON f.referenced_object_id = i.object_id
                   AND f.key_index_id = i.index_id
            ON PK.object_id = f.referenced_object_id
        INNER JOIN sys.objects AS FK
            ON f.parent_object_id = FK.object_id
        INNER JOIN sys.columns AS PkCol
            ON f.referenced_object_id = PkCol.object_id
               AND k.referenced_column_id = PkCol.column_id
        INNER JOIN sys.columns AS FkCol
            ON f.parent_object_id = FkCol.object_id
               AND k.parent_column_id = FkCol.column_id
ORDER BY FK_Table, FK_Column

-- Get all databases referenced by synonyms.
SELECT DISTINCT PARSENAME(sn.base_object_name, 3) AS DatabaseName
INTO #SynonymTargets
FROM sys.synonyms sn
WHERE PARSENAME(sn.base_object_name, 3) != DB_NAME()
AND ISNULL(PARSENAME(sn.base_object_name, 4), @@SERVERNAME) = @@SERVERNAME -- Only populate info from current server
ORDER BY PARSENAME(sn.base_object_name, 3)

-- Create a query to execute for each referenced database
DECLARE @synonymFkDetailsQuery nvarchar(max) =
'
INSERT INTO #SynonymFkDetails (FK_Table, FK_Column, PK_Table, PK_Column, Constraint_Name, fkSchema, pkSchema, primarykey, ORDINAL_POSITION,
                             CascadeOnDelete, IsNotEnforced)
SELECT  FK.name AS FK_Table,
        FkCol.name AS FK_Column,
        PK.name AS PK_Table,
        PkCol.name AS PK_Column,
        OBJECT_NAME(f.object_id) AS Constraint_Name,
        SCHEMA_NAME(FK.schema_id) AS fkSchema,
        SCHEMA_NAME(PK.schema_id) AS pkSchema,
        PkCol.name AS primarykey,
        k.constraint_column_id AS ORDINAL_POSITION,
        CASE WHEN f.delete_referential_action = 1 THEN 1 ELSE 0 END as CascadeOnDelete,
        f.is_disabled AS IsNotEnforced
FROM    sys.objects AS PK
        INNER JOIN sys.foreign_keys AS f
            INNER JOIN sys.foreign_key_columns AS k
                ON k.constraint_object_id = f.object_id
            INNER JOIN sys.indexes AS i
                ON f.referenced_object_id = i.object_id
                   AND f.key_index_id = i.index_id
            ON PK.object_id = f.referenced_object_id
        INNER JOIN sys.objects AS FK
            ON f.parent_object_id = FK.object_id
        INNER JOIN sys.columns AS PkCol
            ON f.referenced_object_id = PkCol.object_id
               AND k.referenced_column_id = PkCol.column_id
        INNER JOIN sys.columns AS FkCol
            ON f.parent_object_id = FkCol.object_id
               AND k.parent_column_id = FkCol.column_id
ORDER BY FK_Table, FK_Column;
'

-- Loop through referenced databases and populate #SynonymFkDetails
DECLARE @synonmymDatabaseName sysname = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
DECLARE @synonymFkDetailsQueryWithDb nvarchar(max)
WHILE (@synonmymDatabaseName IS NOT NULL)
BEGIN
    SET @synonymFkDetailsQueryWithDb = 'USE [' + @synonmymDatabaseName + '] ' + @synonymFkDetailsQuery
    EXEC sp_executesql @stmt=@synonymFkDetailsQueryWithDb
    DELETE FROM #SynonymTargets WHERE DatabaseName = @synonmymDatabaseName
    SET @synonmymDatabaseName = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
END

SET NOCOUNT OFF;
";

        private const string SynonymForeignKeySQL = @"
UNION
-- Synonyms
SELECT FK_Table, FK_Column, PK_Table, PK_Column, Constraint_Name, fkSchema, pkSchema, primarykey, ORDINAL_POSITION,
       CascadeOnDelete, IsNotEnforced FROM #SynonymFkDetails";

        private const string ExtendedPropertySQL = @"
SELECT  s.name AS [schema],
    t.name AS [table],
    c.name AS [column],
    value AS [property]
FROM    sys.extended_properties AS ep
    INNER JOIN sys.tables AS t
        ON ep.major_id = t.object_id
    INNER JOIN sys.schemas AS s
        ON s.schema_id = t.schema_id
    LEFT JOIN sys.columns AS c
        ON ep.major_id = c.object_id
            AND ep.minor_id = c.column_id
WHERE   class = 1
ORDER BY t.name";

        private const string ExtendedPropertyTableExistsSQLCE = @"
SELECT  1
FROM    INFORMATION_SCHEMA.TABLES
WHERE   TABLE_NAME = '__ExtendedProperties'";

        private const string ExtendedPropertySQLCE = @"
SELECT  '' AS [schema],
    [ObjectName] AS [column],
    [ParentName] AS [table],
    [Value] AS [property]
FROM    [__ExtendedProperties]";

        private const string TableSQLCE = @"
SELECT  '' AS SchemaName,
    c.TABLE_NAME AS TableName,
    'BASE TABLE' AS TableType,
    CONVERT( tinyint, 0 ) AS TableTemporalType,
    c.ORDINAL_POSITION AS Ordinal,
    c.COLUMN_NAME AS ColumnName,
    CAST(CASE WHEN c.IS_NULLABLE = N'YES' THEN 1 ELSE 0 END AS BIT) AS IsNullable,
    CASE WHEN c.DATA_TYPE = N'rowversion' THEN 'timestamp' ELSE c.DATA_TYPE END AS TypeName,
    CASE WHEN c.CHARACTER_MAXIMUM_LENGTH IS NOT NULL THEN c.CHARACTER_MAXIMUM_LENGTH ELSE 0 END AS MaxLength,
    CASE WHEN c.NUMERIC_PRECISION IS NOT NULL THEN c.NUMERIC_PRECISION ELSE 0 END AS Precision,
    c.COLUMN_DEFAULT AS [Default],
    CASE WHEN c.DATA_TYPE = N'datetime' THEN 0 ELSE 0 END AS DateTimePrecision,
    CASE WHEN c.DATA_TYPE = N'datetime' THEN 0 WHEN c.NUMERIC_SCALE IS NOT NULL THEN c.NUMERIC_SCALE ELSE 0 END AS Scale,

    CAST(CASE WHEN c.AUTOINC_INCREMENT > 0 THEN 1 ELSE 0 END AS BIT) AS IsIdentity,
    CONVERT( bit, 0 ) as IsComputed,
    CONVERT( bit, 0 ) as IsRowGuid,
    CONVERT( tinyint, 0 ) AS GeneratedAlwaysType,
    CAST(CASE WHEN c.DATA_TYPE = N'rowversion' THEN 1 ELSE 0 END AS BIT) AS IsStoreGenerated,
    0 AS PrimaryKeyOrdinal,
    CAST(CASE WHEN u.TABLE_NAME IS NULL THEN 0 ELSE 1 END AS BIT) AS PrimaryKey,
    CONVERT( bit, 0 ) as IsForeignKey
FROM
    INFORMATION_SCHEMA.COLUMNS c
    INNER JOIN INFORMATION_SCHEMA.TABLES t ON c.TABLE_NAME = t.TABLE_NAME
    LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons ON cons.TABLE_NAME = c.TABLE_NAME
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS u ON
        cons.CONSTRAINT_NAME = u.CONSTRAINT_NAME AND
        u.TABLE_NAME = c.TABLE_NAME AND
        u.COLUMN_NAME = c.COLUMN_NAME
WHERE
    t.TABLE_TYPE <> N'SYSTEM TABLE' AND
    cons.CONSTRAINT_TYPE = 'PRIMARY KEY'
ORDER BY
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.ORDINAL_POSITION";

        private const string ForeignKeySQLCE = @"
SELECT DISTINCT
    FK.TABLE_NAME AS FK_Table,
    FK.COLUMN_NAME AS FK_Column,
    PK.TABLE_NAME AS PK_Table,
    PK.COLUMN_NAME AS PK_Column,
    FK.CONSTRAINT_NAME AS Constraint_Name,
    '' AS fkSchema,
    '' AS pkSchema,
    PT.COLUMN_NAME AS primarykey,
    FK.ORDINAL_POSITION,
    CASE WHEN C.DELETE_RULE = 'CASCADE' THEN 1 ELSE 0 END AS CascadeOnDelete,
    CAST(0 AS BIT) AS IsNotEnforced
FROM    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS C
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS FK
        ON FK.CONSTRAINT_NAME = C.CONSTRAINT_NAME
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS PK
        ON PK.CONSTRAINT_NAME = C.UNIQUE_CONSTRAINT_NAME
            AND PK.ORDINAL_POSITION = FK.ORDINAL_POSITION
    INNER JOIN (
                SELECT  i1.TABLE_NAME,
                        i2.COLUMN_NAME
                FROM    INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2
                            ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                WHERE   i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                ) PT
        ON PT.TABLE_NAME = PK.TABLE_NAME
WHERE   PT.COLUMN_NAME = PK.COLUMN_NAME
ORDER BY FK.TABLE_NAME, FK.COLUMN_NAME";

        private const string StoredProcedureSQL = @"
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + '.' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = 'PROCEDURE'
        AND (
             P.IS_RESULT = 'NO'
             OR P.IS_RESULT IS NULL
            )
        AND R.SPECIFIC_SCHEMA + R.SPECIFIC_NAME IN (
            SELECT  SCHEMA_NAME(sp.schema_id) + sp.name
            FROM    sys.all_objects AS sp
                    LEFT OUTER JOIN sys.all_sql_modules AS sm
                        ON sm.object_id = sp.object_id
            WHERE   sp.type = 'P'
                    AND (CAST(CASE WHEN sp.is_ms_shipped = 1 THEN 1
                                   WHEN (
                                         SELECT major_id
                                         FROM   sys.extended_properties
                                         WHERE  major_id = sp.object_id
                                                AND minor_id = 0
                                                AND class = 1
                                                AND name = N'microsoft_database_tools_support'
                                        ) IS NOT NULL THEN 1
                                   ELSE 0
                              END AS BIT) = 0))

UNION ALL
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + '.' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = 'FUNCTION'
        AND R.DATA_TYPE = 'TABLE'";

        private const string StoredProcedureSQLAzure = @"
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + '.' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = 'PROCEDURE'
        AND (
             P.IS_RESULT = 'NO'
             OR P.IS_RESULT IS NULL
            )
        AND R.SPECIFIC_SCHEMA + R.SPECIFIC_NAME IN (
            SELECT  SCHEMA_NAME(sp.schema_id) + sp.name
            FROM    sys.all_objects AS sp
                    LEFT OUTER JOIN sys.all_sql_modules AS sm
                        ON sm.object_id = sp.object_id
            WHERE   sp.type = 'P'
                    AND sp.is_ms_shipped = 0)
UNION ALL
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + '.' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = 'FUNCTION'
        AND R.DATA_TYPE = 'TABLE'
ORDER BY R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        P.ORDINAL_POSITION";

        private const string SynonymStoredProcedureSQLSetup = @"
SET NOCOUNT ON;
IF OBJECT_ID('tempdb..#SynonymStoredProcedureDetails') IS NOT NULL DROP TABLE #SynonymStoredProcedureDetails;
IF OBJECT_ID('tempdb..#SynonymTargets') IS NOT NULL DROP TABLE #SynonymTargets;

-- Create the ##SynonymStoredProcedureDetails temp table structure for later use
SELECT  TOP (0) R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + '.' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
INTO    #SynonymStoredProcedureDetails
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = 'PROCEDURE'
        AND (
             P.IS_RESULT = 'NO'
             OR P.IS_RESULT IS NULL
            )
        AND R.SPECIFIC_SCHEMA + R.SPECIFIC_NAME IN (
            SELECT  SCHEMA_NAME(sp.schema_id) + sp.name
            FROM    sys.all_objects AS sp
                    LEFT OUTER JOIN sys.all_sql_modules AS sm
                        ON sm.object_id = sp.object_id
            WHERE   sp.type = 'P'
                    AND (CAST(CASE WHEN sp.is_ms_shipped = 1 THEN 1
                                   WHEN (
                                         SELECT major_id
                                         FROM   sys.extended_properties
                                         WHERE  major_id = sp.object_id
                                                AND minor_id = 0
                                                AND class = 1
                                                AND name = N'microsoft_database_tools_support'
                                        ) IS NOT NULL THEN 1
                                   ELSE 0
                              END AS BIT) = 0))

-- Get all databases referenced by synonyms.
SELECT DISTINCT PARSENAME(sn.base_object_name, 3) AS DatabaseName
INTO #SynonymTargets
FROM sys.synonyms sn
WHERE PARSENAME(sn.base_object_name, 3) != DB_NAME()
AND ISNULL(PARSENAME(sn.base_object_name, 4), @@SERVERNAME) = @@SERVERNAME -- Only populate info from current server
ORDER BY PARSENAME(sn.base_object_name, 3)

-- Create a query to execute for each referenced database
DECLARE @synonymStoredProcedureDetailsQuery nvarchar(max) =
'
INSERT INTO #SynonymStoredProcedureDetails (SPECIFIC_SCHEMA, SPECIFIC_NAME, ROUTINE_TYPE, ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME, DATA_TYPE
                                            , CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION, USER_DEFINED_TYPE)
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + ''.'' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = ''PROCEDURE''
        AND (
             P.IS_RESULT = ''NO''
             OR P.IS_RESULT IS NULL
            )
        AND R.SPECIFIC_SCHEMA + R.SPECIFIC_NAME IN (
            SELECT  SCHEMA_NAME(sp.schema_id) + sp.name
            FROM    sys.all_objects AS sp
                    LEFT OUTER JOIN sys.all_sql_modules AS sm
                        ON sm.object_id = sp.object_id
            WHERE   sp.type = ''P''
                    AND (CAST(CASE WHEN sp.is_ms_shipped = 1 THEN 1
                                   WHEN (
                                         SELECT major_id
                                         FROM   sys.extended_properties
                                         WHERE  major_id = sp.object_id
                                                AND minor_id = 0
                                                AND class = 1
                                                AND name = N''microsoft_database_tools_support''
                                        ) IS NOT NULL THEN 1
                                   ELSE 0
                              END AS BIT) = 0))

UNION ALL
SELECT  R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        R.ROUTINE_TYPE,
        P.ORDINAL_POSITION,
        P.PARAMETER_MODE,
        P.PARAMETER_NAME,
        P.DATA_TYPE,
        ISNULL(P.CHARACTER_MAXIMUM_LENGTH, 0) AS CHARACTER_MAXIMUM_LENGTH,
        ISNULL(P.NUMERIC_PRECISION, 0) AS NUMERIC_PRECISION,
        ISNULL(P.NUMERIC_SCALE, 0) AS NUMERIC_SCALE,
        ISNULL(P.DATETIME_PRECISION, 0) AS DATETIME_PRECISION,
        P.USER_DEFINED_TYPE_SCHEMA + ''.'' + P.USER_DEFINED_TYPE_NAME AS USER_DEFINED_TYPE
FROM    INFORMATION_SCHEMA.ROUTINES R
        LEFT OUTER JOIN INFORMATION_SCHEMA.PARAMETERS P
            ON P.SPECIFIC_SCHEMA = R.SPECIFIC_SCHEMA
               AND P.SPECIFIC_NAME = R.SPECIFIC_NAME
WHERE   R.ROUTINE_TYPE = ''FUNCTION''
        AND R.DATA_TYPE = ''TABLE''
ORDER BY R.SPECIFIC_SCHEMA,
        R.SPECIFIC_NAME,
        P.ORDINAL_POSITION
'

-- Loop through referenced databases and populate #SynonymStoredProcedureDetails
DECLARE @synonmymDatabaseName sysname = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
DECLARE @synonymStoredProcedureDetailsQueryWithDb nvarchar(max)
WHILE (@synonmymDatabaseName IS NOT NULL)
BEGIN
    SET @synonymStoredProcedureDetailsQueryWithDb = 'USE [' + @synonmymDatabaseName + '] ' + @synonymStoredProcedureDetailsQuery
    EXEC sp_executesql @stmt=@synonymStoredProcedureDetailsQueryWithDb
    DELETE FROM #SynonymTargets WHERE DatabaseName = @synonmymDatabaseName
    SET @synonmymDatabaseName = (SELECT TOP (1) DatabaseName FROM #SynonymTargets)
END

SET NOCOUNT OFF;
";

        private const string SynonymStoredProcedureSQL = @"
UNION
-- Synonyms
SELECT SPECIFIC_SCHEMA, SPECIFIC_NAME, ROUTINE_TYPE, ORDINAL_POSITION, PARAMETER_MODE, PARAMETER_NAME, DATA_TYPE
    , CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE, DATETIME_PRECISION, USER_DEFINED_TYPE FROM #SynonymStoredProcedureDetails";

        private const string IndexSQL = @"
SELECT  SCHEMA_NAME(t.schema_id) AS TableSchema,
    t.name AS TableName,
    ind.name AS IndexName,
    ic.key_ordinal AS KeyOrdinal,
    col.name AS ColumnName,
    ind.is_unique AS IsUnique,
    ind.is_primary_key AS IsPrimaryKey,
    ind.is_unique_constraint AS IsUniqueConstraint,
    CASE WHEN ind.[type] = 1 AND ind.is_primary_key = 1 THEN 1 ELSE 0 END AS IsClustered,
    (
        SELECT COUNT(1)
        FROM   sys.index_columns i
        WHERE  i.object_id = ind.object_id
            AND i.index_id = ind.index_id
    ) AS ColumnCount
FROM    sys.tables t
    INNER JOIN sys.indexes ind
        ON ind.object_id = t.object_id
    INNER JOIN sys.index_columns ic
        ON ind.object_id = ic.object_id
            AND ind.index_id = ic.index_id
    INNER JOIN sys.columns col
        ON ic.object_id = col.object_id
            AND ic.column_id = col.column_id
WHERE   t.is_ms_shipped = 0
    AND ind.ignore_dup_key = 0
    AND ic.key_ordinal > 0
    AND t.name NOT LIKE 'sysdiagram%'";

        public SqlServerSchemaReader(DbConnection connection, System.IO.TextWriter writer)
            : base(connection, writer)
        {
        }

        private static string IncludeQueryTraceOn9481()
        {
            if (Settings.IncludeQueryTraceOn9481Flag)
                return @"
OPTION (QUERYTRACEON 9481)";
            return string.Empty;
        }

        private void ReadDatabaseEdition()
        {
            if (Settings.IsSqlCe || !string.IsNullOrEmpty(_sqlDatabaseEdition))
                return;

            if (Cmd == null)
                return;

            Cmd.CommandText = @"
SELECT  SERVERPROPERTY('Edition') AS Edition,
        CASE SERVERPROPERTY('EngineEdition')
          WHEN 1 THEN 'Personal'
          WHEN 2 THEN 'Standard'
          WHEN 3 THEN 'Enterprise'
          WHEN 4 THEN 'Express'
          WHEN 5 THEN 'Azure'
          ELSE 'Unknown'
        END AS EngineEdition,
        SERVERPROPERTY('productversion') AS ProductVersion;";

            Cmd.CommandTimeout = Settings.CommandTimeout;

            using (DbDataReader rdr = Cmd.ExecuteReader())
            {
                if (rdr.Read())
                {
                    _sqlDatabaseEdition = rdr["Edition"].ToString();
                    _sqlDatabaseEngineEdition = rdr["EngineEdition"].ToString();
                    _sqlDatabaseProductVersion = rdr["ProductVersion"].ToString();
                    _sqlDatabaseProductMajorVersion = int.Parse(_sqlDatabaseProductVersion.Substring(0, 2).Replace(".", string.Empty));
                }
            }
        }

        private void WriteConnectionSettingComments()
        {
            if (Settings.IncludeConnectionSettingComments)
            {
                if (Settings.IsSqlCe)
                {
                    WriteLine("// Database Edition : SqlCE");
                }
                else
                {
                    WriteLine("// Database Edition        : " + _sqlDatabaseEdition);
                    WriteLine("// Database Engine Edition : " + _sqlDatabaseEngineEdition);
                    WriteLine("// Database Version        : " + _sqlDatabaseProductVersion);
                }
                WriteLine("");
            }
        }

        private bool IsAzure()
        {
            return _sqlDatabaseEngineEdition == "Azure";
        }

        private String GetReadSchemaSql()
        {
            if (Settings.IsSqlCe)
            {
                return TableSQLCE;
            }

            String sql;
            if (Settings.IncludeSynonyms)
            {
                sql = SynonymTableSQLSetup + TableSQL + SynonymTableSQL + IncludeQueryTraceOn9481();
            }
            else
            {
                sql = TableSQL + IncludeQueryTraceOn9481();
            }

            ReadDatabaseEdition();
            var temporalTableSupport = _sqlDatabaseProductMajorVersion >= 13;
            if (!temporalTableSupport)
            {
                // Replace the column names (only present in SQL Server 2016 or later) with literal constants so the query works with older versions of SQL Server.
                sql = sql
                    .Replace("[sc].[generated_always_type]", "0")
                    .Replace("[c].[generated_always_type]", "0")
                    .Replace("[st].[temporal_type]", "0");
            }

            return sql;
        }

        public override Tables ReadSchema()
        {
            ReadDatabaseEdition();
            WriteConnectionSettingComments();

            var result = new Tables();
            if (Cmd == null)
                return result;

            Cmd.CommandText = this.GetReadSchemaSql();

            if (!Settings.IsSqlCe) Cmd.CommandTimeout = Settings.CommandTimeout;

            using (var rdr = Cmd.ExecuteReader())
            {
                var rxClean = new Regex("^(event|Equals|GetHashCode|GetType|ToString|repo|Save|IsNew|Insert|Update|Delete|Exists|SingleOrDefault|Single|First|FirstOrDefault|Fetch|Page|Query)$");
                var lastTable = string.Empty;
                Table table = null;
                while (rdr.Read())
                {
                    string schema = rdr["SchemaName"].ToString().Trim();
                    if (IsFilterExcluded(Settings.SchemaFilterExclude, Settings.SchemaFilterInclude, schema))
                        continue;

                    string tableName = rdr["TableName"].ToString().Trim();
                    if (IsFilterExcluded(Settings.TableFilterExclude, Settings.TableFilterInclude, tableName))
                        continue;

                    if (lastTable != tableName || table == null)
                    {
                        // The data from the database is not sorted
                        table = result.Find(x => x.Name == tableName && x.Schema == schema);
                        if (table == null)
                        {
                            String tableType = ((String)rdr["TableType"]).Trim();
                            TableTemporalType tableTemporalType = (TableTemporalType)(Byte)rdr["TableTemporalType"];

                            table = new Table
                            {
                                Name = tableName,
                                Schema = schema,
                                IsView = string.Compare(tableType, "View", StringComparison.OrdinalIgnoreCase) == 0,
                                TemporalType = tableTemporalType,

                                // Will be set later
                                HasForeignKey = false,
                                HasNullableColumns = false
                            };

                            if (!Settings.IncludeViews && table.IsView)
                                continue;

                            tableName = Settings.TableRename(tableName, schema, table.IsView);
                            if (IsFilterExcluded(Settings.TableFilterExclude, null, tableName)) // Retest exclusion filter after table rename
                                continue;

                            // Handle table names with underscores - singularise just the last word
                            table.ClassName = Inflector.MakeSingular(CleanUp(tableName));
                            var titleCase = (Settings.UsePascalCase ? Inflector.ToTitleCase(table.ClassName) : table.ClassName).Replace(" ", "").Replace("$", "").Replace(".", "");
                            table.NameHumanCase = titleCase;

                            if (Settings.PrependSchemaName && string.Compare(table.Schema, "dbo", StringComparison.OrdinalIgnoreCase) != 0)
                                table.NameHumanCase = table.Schema + "_" + table.NameHumanCase;

                            // Check for table or C# name clashes
                            if (ReservedKeywords.Contains(table.NameHumanCase) ||
                                (Settings.UsePascalCase && result.Find(x => x.NameHumanCase == table.NameHumanCase) != null))
                            {
                                table.NameHumanCase += "1";
                            }

                            if (!Settings.TableFilter(table))
                                continue;

                            result.Add(table);
                        }
                    }

                    var col = CreateColumn(rdr, rxClean, table, Settings.ColumnFilterExclude);
                    if (col != null)
                        table.Columns.Add(col);
                }
            }
            // Check for property name clashes in columns
            foreach (Column c in result.SelectMany(tbl => tbl.Columns.Where(c => tbl.Columns.FindAll(x => x.NameHumanCase == c.NameHumanCase).Count > 1)))
            {
                int n = 1;
                var original = c.NameHumanCase;
                c.NameHumanCase = original + n++;

                // Check if the above resolved the name clash, if not, use next value
                while (c.ParentTable.Columns.Count(c2 => c2.NameHumanCase == c.NameHumanCase) > 1)
                    c.NameHumanCase = original + n++;
            }

            if (Settings.IncludeExtendedPropertyComments != CommentsStyle.None)
                ReadExtendedProperties(result, Settings.IncludeExtendedPropertyComments == CommentsStyle.InSummaryBlock);

            ReadIndexes(result);

            foreach (Table tbl in result)
            {
                if (tbl.IsView && Settings.ViewProcessing != null) Settings.ViewProcessing(tbl);
                tbl.SetPrimaryKeys();
                foreach (Column c in tbl.Columns)
                    Settings.UpdateColumn(c, tbl);
                tbl.Columns.ForEach(x => x.SetupEntityAndConfig());
            }

            return result;
        }

        public override List<ForeignKey> ReadForeignKeys()
        {
            var fkList = new List<ForeignKey>();
            if (Cmd == null)
                return fkList;

            Cmd.CommandText = ForeignKeySQL + IncludeQueryTraceOn9481();

            if (Settings.IncludeSynonyms)
                Cmd.CommandText = SynonymForeignKeySQLSetup + ForeignKeySQL + SynonymForeignKeySQL + IncludeQueryTraceOn9481();

            if (Cmd.GetType().Name == "SqlCeCommand")
                Cmd.CommandText = ForeignKeySQLCE;
            else
                Cmd.CommandTimeout = Settings.CommandTimeout;

            using (DbDataReader rdr = Cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    string fkTableName = rdr["FK_Table"].ToString();
                    string fkSchema = rdr["fkSchema"].ToString();
                    string pkTableName = rdr["PK_Table"].ToString();
                    string pkSchema = rdr["pkSchema"].ToString();
                    string fkColumn = rdr["FK_Column"].ToString();
                    string pkColumn = rdr["PK_Column"].ToString();
                    string constraintName = rdr["Constraint_Name"].ToString();
                    int ordinal = (int)rdr["ORDINAL_POSITION"];
                    bool cascadeOnDelete = ((int)rdr["CascadeOnDelete"]) == 1;
                    bool isNotEnforced = (bool)rdr["IsNotEnforced"];

                    string pkTableNameFiltered = Settings.TableRename(pkTableName, pkSchema, false);

                    var fk = new ForeignKey(fkTableName, fkSchema, pkTableName, pkSchema, fkColumn, pkColumn, constraintName, pkTableNameFiltered, ordinal, cascadeOnDelete, isNotEnforced);

                    var filteredFk = Settings.ForeignKeyFilter(fk);
                    if (filteredFk != null)
                        fkList.Add(filteredFk);
                }
            }

            return fkList;
        }

        // When a table has no primary keys, all the NOT NULL columns are set as being the primary key.
        // This function reads the unique indexes for a table, and correctly sets the columns being used as primary keys.
        public override void ReadIndexes(Tables tables)
        {
            if (Cmd == null)
                return;

            if (Cmd.GetType().Name == "SqlCeCommand")
                return;

            Cmd.CommandText = IndexSQL + IncludeQueryTraceOn9481();
            Cmd.CommandTimeout = Settings.CommandTimeout;

            var list = new List<Index>();
            using (DbDataReader rdr = Cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    var index = new Index
                    {
                        Schema = rdr["TableSchema"].ToString().Trim(),
                        TableName = rdr["TableName"].ToString().Trim(),
                        IndexName = rdr["IndexName"].ToString().Trim(),
                        KeyOrdinal = (byte)rdr["KeyOrdinal"],
                        ColumnName = rdr["ColumnName"].ToString().Trim(),
                        ColumnCount = (int)rdr["ColumnCount"],
                        IsUnique = (bool)rdr["IsUnique"],
                        IsPrimaryKey = (bool)rdr["IsPrimaryKey"],
                        IsUniqueConstraint = (bool)rdr["IsUniqueConstraint"],
                        IsClustered = ((int)rdr["IsClustered"]) == 1
                    };

                    list.Add(index);
                }
            }

            Table t = null;
            var indexTables = list
                .Select(x => new { x.Schema, x.TableName })
                .Distinct();

            foreach (var indexTable in indexTables)
            {
                // Lookup table
                if (t == null || t.Name != indexTable.TableName || t.Schema != indexTable.Schema)
                    t = tables.Find(x => x.Name == indexTable.TableName && x.Schema == indexTable.Schema);

                if (t == null)
                    continue;

                // Find indexes for table
                var indexes =
                    list.Where(x => x.Schema == indexTable.Schema && x.TableName == indexTable.TableName)
                        .OrderBy(o => o.ColumnCount)
                        .ThenBy(o => o.KeyOrdinal)
                        .ToList();

                // Set index on column
                foreach (var index in indexes)
                {
                    var col = t.Columns.Find(x => x.Name == index.ColumnName);
                    if (col == null)
                        continue;

                    col.Indexes.Add(index);

                    col.IsPrimaryKey = col.IsPrimaryKey || index.IsPrimaryKey;
                    col.IsUniqueConstraint = col.IsUniqueConstraint || (index.IsUniqueConstraint && index.ColumnCount == 1);
                    col.IsUnique = col.IsUnique || (index.IsUnique && index.ColumnCount == 1);
                }

                // Check if table has any primary keys
                if (t.PrimaryKeys.Any())
                    continue; // Already has a primary key, ignore this unique index / constraint

                // Find unique indexes for table
                var uniqueIndexKeys = indexes
                    .Where(x => x.IsUnique || x.IsPrimaryKey || x.IsUniqueConstraint)
                    .OrderBy(o => o.ColumnCount)
                    .ThenBy(o => o.KeyOrdinal);

                // Process only the first index with the lowest unique column count
                string indexName = null;
                foreach (var key in uniqueIndexKeys)
                {
                    if (indexName == null)
                        indexName = key.IndexName;

                    if (indexName != key.IndexName)
                        break; // First unique index with lowest column count has been processed, exit.

                    var col = t.Columns.Find(x => x.Name == key.ColumnName);
                    if (col != null && !col.IsNullable && !col.Hidden && !col.IsPrimaryKey)
                    {
                        col.IsPrimaryKey = true;
                        col.IsUniqueConstraint = true;
                        col.IsUnique = true;
                        col.UniqueIndexName = indexName;
                    }
                }
            }
        }

        public override void ReadExtendedProperties(Tables tables, bool commentsInSummaryBlock)
        {
            if (Cmd == null)
                return;

            if (Cmd.GetType().Name == "SqlCeCommand")
            {
                Cmd.CommandText = ExtendedPropertyTableExistsSQLCE;
                var obj = Cmd.ExecuteScalar();
                if (obj == null)
                    return;

                Cmd.CommandText = ExtendedPropertySQLCE;
            }
            else
            {
                if (IsAzure())
                    return;

                Cmd.CommandText = ExtendedPropertySQL + IncludeQueryTraceOn9481();
                Cmd.CommandTimeout = Settings.CommandTimeout;
            }

            using (DbDataReader rdr = Cmd.ExecuteReader())
            {
                Table t = null;
                while (rdr.Read())
                {
                    string schema = rdr["schema"].ToString().Trim();
                    string tableName = rdr["table"].ToString().Trim();
                    string column = rdr["column"].ToString().Trim();
                    string extendedProperty = rdr["property"].ToString().Trim();

                    if (string.IsNullOrEmpty(extendedProperty))
                        continue;

                    if (t == null || t.Name != tableName || t.Schema != schema)
                        t = tables.Find(x => x.Name == tableName && x.Schema == schema);

                    if (t != null)
                    {
                        if (string.IsNullOrEmpty(column))
                        {
                            // Table level extended comment
                            t.ExtendedProperty = Regex.Replace(extendedProperty, "[\r\n]+", "\r\n    /// ");
                        }
                        else
                        {
                            var col = t.Columns.Find(x => x.Name == column);
                            if (col != null)
                            {
                                // Column level extended comment
                                if (commentsInSummaryBlock)
                                    col.ExtendedProperty = Regex.Replace(extendedProperty, "[\r\n]+", "\r\n        /// ");
                                else
                                    col.ExtendedProperty = Regex.Replace(Regex.Replace(extendedProperty, "[\r\n]+", " "), "\\s+", " ");
                            }
                        }
                    }
                }
            }
        }

        public override List<StoredProcedure> ReadStoredProcs()
        {
            var result = new List<StoredProcedure>();
            if (Cmd == null)
                return result;

            if (Cmd.GetType().Name == "SqlCeCommand")
                return result;

            if (IsAzure())
                Cmd.CommandText = StoredProcedureSQLAzure + IncludeQueryTraceOn9481();
            else if (Settings.IncludeSynonyms)
                Cmd.CommandText = SynonymStoredProcedureSQLSetup + StoredProcedureSQL + SynonymStoredProcedureSQL + IncludeQueryTraceOn9481();
            else
                Cmd.CommandText = StoredProcedureSQL + IncludeQueryTraceOn9481();

            Cmd.CommandTimeout = Settings.CommandTimeout;

            using (DbDataReader rdr = Cmd.ExecuteReader())
            {
                var lastSp = string.Empty;
                StoredProcedure sp = null;
                while (rdr.Read())
                {
                    var spType = rdr["ROUTINE_TYPE"].ToString().Trim().ToUpper();
                    var isTVF = (spType == "FUNCTION");
                    if (isTVF && !Settings.IncludeTableValuedFunctions)
                        continue;

                    string schema = rdr["SPECIFIC_SCHEMA"].ToString().Trim();
                    if (Settings.SchemaFilterExclude != null && Settings.SchemaFilterExclude.IsMatch(schema))
                        continue;

                    string spName = rdr["SPECIFIC_NAME"].ToString().Trim();
                    var fullname = schema + "." + spName;
                    if (Settings.StoredProcedureFilterExclude != null && (Settings.StoredProcedureFilterExclude.IsMatch(spName) || Settings.StoredProcedureFilterExclude.IsMatch(fullname)))
                        continue;

                    if (lastSp != fullname || sp == null)
                    {
                        lastSp = fullname;
                        sp = new StoredProcedure
                        {
                            IsTVF = isTVF,
                            Name = spName,
                            NameHumanCase = (Settings.UsePascalCase ? Inflector.ToTitleCase(spName) : spName).Replace(" ", "").Replace("$", ""),
                            Schema = schema
                        };
                        sp.NameHumanCase = CleanUp(sp.NameHumanCase);
                        if ((string.Compare(schema, "dbo", StringComparison.OrdinalIgnoreCase) != 0) && Settings.PrependSchemaName)
                            sp.NameHumanCase = schema + "_" + sp.NameHumanCase;

                        sp.NameHumanCase = Settings.StoredProcedureRename(sp);
                        if (Settings.StoredProcedureFilterExclude != null && (Settings.StoredProcedureFilterExclude.IsMatch(sp.NameHumanCase) || Settings.StoredProcedureFilterExclude.IsMatch(schema + "." + sp.NameHumanCase)))
                            continue;

                        result.Add(sp);
                    }

                    if (rdr["DATA_TYPE"] != null && rdr["DATA_TYPE"] != DBNull.Value)
                    {
                        var typename = rdr["DATA_TYPE"].ToString().Trim().ToLower();
                        var scale = (int)rdr["NUMERIC_SCALE"];
                        var precision = (int)((byte)rdr["NUMERIC_PRECISION"]);
                        var parameterMode = rdr["PARAMETER_MODE"].ToString().Trim().ToUpper();

                        var param = new StoredProcedureParameter
                        {
                            Ordinal = (int)rdr["ORDINAL_POSITION"],
                            Mode = parameterMode == "IN" ? StoredProcedureParameterMode.In : StoredProcedureParameterMode.InOut,
                            Name = rdr["PARAMETER_NAME"].ToString().Trim(),
                            SqlDbType = GetSqlDbType(typename),
                            PropertyType = GetPropertyType(typename),
                            DateTimePrecision = (short)rdr["DATETIME_PRECISION"],
                            MaxLength = (int)rdr["CHARACTER_MAXIMUM_LENGTH"],
                            Precision = precision,
                            Scale = scale,
                            UserDefinedTypeName = rdr["USER_DEFINED_TYPE"].ToString().Trim()
                        };

                        var clean = CleanUp(param.Name.Replace("@", ""));
                        if (!string.IsNullOrEmpty(clean))
                        {
                            param.NameHumanCase = Inflector.MakeInitialLower((Settings.UsePascalCase ? Inflector.ToTitleCase(clean) : clean).Replace(" ", ""));

                            if (ReservedKeywords.Contains(param.NameHumanCase))
                                param.NameHumanCase = "@" + param.NameHumanCase;

                            sp.Parameters.Add(param);
                        }
                    }
                }
            }
            return result;
        }

        public void ReadStoredProcReturnObject(SqlConnection sqlConnection, StoredProcedure proc)
        {
            try
            {
                const string structured = "Structured";
                var sb = new StringBuilder();
                sb.AppendLine();
                sb.AppendLine("SET FMTONLY OFF; SET FMTONLY ON;");
                if (proc.IsTVF)
                {
                    foreach (var param in proc.Parameters.Where(x => x.SqlDbType.Equals(structured, StringComparison.InvariantCultureIgnoreCase)))
                        sb.AppendLine(string.Format("DECLARE {0} {1};", param.Name, param.UserDefinedTypeName));

                    sb.Append(string.Format("SELECT * FROM [{0}].[{1}](", proc.Schema, proc.Name));
                    foreach (var param in proc.Parameters)
                        sb.Append(string.Format("{0}, ", param.SqlDbType.Equals(structured, StringComparison.InvariantCultureIgnoreCase) ? param.Name : "default"));

                    if (proc.Parameters.Count > 0)
                        sb.Length -= 2;

                    sb.AppendLine(");");
                }
                else
                {
                    foreach (var param in proc.Parameters)
                        sb.AppendLine(string.Format("DECLARE {0} {1};", param.Name, param.SqlDbType.Equals(structured, StringComparison.InvariantCultureIgnoreCase) ? param.UserDefinedTypeName : param.SqlDbType));

                    sb.Append(string.Format("exec [{0}].[{1}] ", proc.Schema, proc.Name));
                    foreach (var param in proc.Parameters)
                        sb.Append(string.Format("{0}, ", param.Name));

                    if (proc.Parameters.Count > 0)
                        sb.Length -= 2;

                    sb.AppendLine(";");
                }
                sb.AppendLine("SET FMTONLY OFF; SET FMTONLY OFF;");

                var ds = new DataSet();
                using (var sqlAdapter = new SqlDataAdapter(sb.ToString(), sqlConnection))
                {
                    if (sqlConnection.State != ConnectionState.Open)
                        sqlConnection.Open();
                    sqlAdapter.SelectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
                    sqlConnection.Close();
                    sqlAdapter.FillSchema(ds, SchemaType.Source, "MyTable");
                }

                // Tidy up parameters
                foreach (var p in proc.Parameters)
                    p.NameHumanCase = Regex.Replace(p.NameHumanCase, @"[^A-Za-z0-9@\s]*", "");

                for (var count = 0; count < ds.Tables.Count; count++)
                {
                    proc.ReturnModels.Add(ds.Tables[count].Columns.Cast<DataColumn>().ToList());
                }
            }
            catch (Exception)
            {
                // Stored procedure does not have a return type
            }
        }

        public override void ProcessForeignKeys(List<ForeignKey> fkList, Tables tables, bool checkForFkNameClashes)
        {
            var constraints = fkList.Select(x => x.FkSchema + "." + x.ConstraintName).Distinct();
            var doubleForeignTableRefcontraints = fkList.GroupBy(x => new { x.FkTableName, x.PkTableName }).Where(g => g.Count() > 1).SelectMany(g => g.Select(x => string.Format("{0}.{1}", x.FkSchema, x.ConstraintName))).Distinct();
            foreach (var constraint in constraints)
            {
                var foreignKeys = fkList
                    .Where(x => string.Format("{0}.{1}", x.FkSchema, x.ConstraintName).Equals(constraint, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();

                var foreignKey = foreignKeys.First();
                Table fkTable = tables.GetTable(foreignKey.FkTableName, foreignKey.FkSchema);
                if (fkTable == null || fkTable.IsMapping || !fkTable.HasForeignKey)
                    continue;

                Table pkTable = tables.GetTable(foreignKey.PkTableName, foreignKey.PkSchema);
                if (pkTable == null || pkTable.IsMapping)
                    continue;

                var fkCols = foreignKeys.Select(x => new
                {
                    fk = x,
                    col = fkTable.Columns.Find(n => string.Equals(n.Name, x.FkColumn, StringComparison.InvariantCultureIgnoreCase))
                })
                    .Where(x => x.col != null)
                    .OrderBy(o => o.fk.Ordinal)
                    .ToList();

                if (!fkCols.Any())
                    continue;

                //if(EF6)
                {
                    // Check FK has same number of columns as the primary key it points to
                    var pks = pkTable.PrimaryKeys.OrderBy(x => x.PropertyType).ThenBy(y => y.Name).ToArray();
                    var cols = fkCols.Select(x => x.col).OrderBy(x => x.PropertyType).ThenBy(y => y.Name).ToArray();
                    if (pks.Length != cols.Length)
                        continue;

                    // EF6 - Cannot have a FK to a non-primary key
                    if (pks.Where((pk, ef6Check) => pk.PropertyType != cols[ef6Check].PropertyType).Any())
                        continue;
                }

                var pkCols = foreignKeys.Select(x => pkTable.Columns.Find(n => string.Equals(n.Name, x.PkColumn, StringComparison.InvariantCultureIgnoreCase)))
                                        .Where(x => x != null)
                                        .OrderBy(o => o.Ordinal)
                                        .ToList();

                if (!pkCols.Any())
                    continue;

                // EF6 - Cannot have a FK to a non-primary key
                if (!pkCols.All(c => c.IsPrimaryKey))
                    continue;

                var relationship = CalcRelationship(pkTable, fkTable, fkCols.Select(c => c.col).ToList(), pkCols);
                if (relationship == Relationship.DoNotUse)
                    continue;

                if (fkCols.All(x => !x.col.IsNullable && !x.col.Hidden) && pkCols.All(x => x.IsPrimaryKey || x.IsUnique))
                {
                    foreach (var fk in fkCols)
                        fk.fk.IncludeRequiredAttribute = true;
                }

                foreignKey = Settings.ForeignKeyProcessing(foreignKeys, fkTable, pkTable, fkCols.Any(x => x.col.IsNullable));

                string pkTableHumanCaseWithSuffix = foreignKey.PkTableHumanCase(pkTable.Suffix);
                string pkTableHumanCase = foreignKey.PkTableHumanCase(null);
                string pkPropName = fkTable.GetUniqueColumnName(pkTableHumanCase, foreignKey, checkForFkNameClashes, true, Relationship.ManyToOne);
                bool fkMakePropNameSingular = (relationship == Relationship.OneToOne);
                string fkPropName = pkTable.GetUniqueColumnName(fkTable.NameHumanCase, foreignKey, checkForFkNameClashes, fkMakePropNameSingular, Relationship.OneToMany);

                var dataAnnotation = string.Empty;
                if (Settings.UseDataAnnotationsWithFluent && !Settings.UseDataAnnotations)
                {
                    dataAnnotation = foreignKey.IncludeRequiredAttribute ? "[Required] " : string.Empty;
                }
                else if (Settings.UseDataAnnotations)
                {
                    dataAnnotation = string.Format("[ForeignKey(\"{0}\"){1}] ",
                        string.Join(", ", fkCols.Select(x => x.col.NameHumanCase).Distinct().ToArray()),
                        foreignKey.IncludeRequiredAttribute ? ", Required" : string.Empty
                    );

                    if (!checkForFkNameClashes &&
                        relationship == Relationship.OneToOne &&
                        foreignKey.IncludeReverseNavigation &&
                        fkCols.All(x => x.col.IsPrimaryKey))
                    {
                        var principalEndAttribute = string.Format("ForeignKey(\"{0}\")", pkPropName);
                        foreach (var fk in fkCols)
                        {
                            if (!fk.col.DataAnnotations.Contains(principalEndAttribute))
                                fk.col.DataAnnotations.Add(principalEndAttribute);
                        }
                    }
                }

                var fkd = new PropertyAndComments
                {
                    AdditionalDataAnnotations = Settings.ForeignKeyAnnotationsProcessing(fkTable, pkTable, pkPropName, fkPropName),
                    Definition = string.Format("{0}public {1}{2} {3} {4}{5}", dataAnnotation,
                        Table.GetLazyLoadingMarker(),
                        pkTableHumanCaseWithSuffix,
                        pkPropName,
                        "{ get; set; }",
                        Settings.IncludeComments != CommentsStyle.None ? " // " + foreignKey.ConstraintName : string.Empty),
                    Comments = string.Format("Parent {0} pointed by [{1}].({2}) ({3})",
                        pkTableHumanCase,
                        fkTable.Name,
                        string.Join(", ", fkCols.Select(x => "[" + x.col.NameHumanCase + "]").Distinct().ToArray()),
                        foreignKey.ConstraintName)
                };

                var firstFKCol = fkCols.First();
                firstFKCol.col.EntityFk.Add(fkd);

                string manyToManyMapping, mapKey;
                if (foreignKeys.Count > 1)
                {
                    manyToManyMapping = string.Format("c => new {{ {0} }}", string.Join(", ", fkCols.Select(x => "c." + x.col.NameHumanCase).Distinct().ToArray()));
                    mapKey = string.Format("{0}", string.Join(",", fkCols.Select(x => "\"" + x.col.Name + "\"").Distinct().ToArray()));
                }
                else
                {
                    manyToManyMapping = string.Format("c => c.{0}", firstFKCol.col.NameHumanCase);
                    mapKey = string.Format("\"{0}\"", firstFKCol.col.Name);
                }

                if (!Settings.UseDataAnnotations || (doubleForeignTableRefcontraints.Any(x => x.Equals(constraint, StringComparison.InvariantCultureIgnoreCase)) && relationship == Relationship.ManyToOne))
                {
                    List<Column> fkCols2 = fkCols.Select(c => c.col).ToList();

                    string rel = GetRelationship(relationship, fkCols2, pkCols, pkPropName, fkPropName, manyToManyMapping, mapKey, foreignKey.CascadeOnDelete, foreignKey.IncludeReverseNavigation, foreignKey.IsNotEnforced);
                    string com = Settings.IncludeComments != CommentsStyle.None ? " // " + foreignKey.ConstraintName : string.Empty;
                    firstFKCol.col.ConfigFk.Add(string.Format("{0};{1}", rel, com));
                }

                if (foreignKey.IncludeReverseNavigation)
                {
                    pkTable.AddReverseNavigation(relationship, pkTableHumanCase, fkTable, fkPropName, string.Format("{0}.{1}", fkTable.Name, foreignKey.ConstraintName), foreignKeys);
                }
            }
        }

        public override void IdentifyForeignKeys(List<ForeignKey> fkList, Tables tables)
        {
            foreach (var foreignKey in fkList)
            {
                Table fkTable = tables.GetTable(foreignKey.FkTableName, foreignKey.FkSchema);
                if (fkTable == null)
                    continue;   // Could be filtered out

                Table pkTable = tables.GetTable(foreignKey.PkTableName, foreignKey.PkSchema);
                if (pkTable == null)
                    continue;   // Could be filtered out

                Column fkCol = fkTable.Columns.Find(n => string.Equals(n.Name, foreignKey.FkColumn, StringComparison.InvariantCultureIgnoreCase));
                if (fkCol == null)
                    continue;   // Could not find fk column

                Column pkCol = pkTable.Columns.Find(n => string.Equals(n.Name, foreignKey.PkColumn, StringComparison.InvariantCultureIgnoreCase));
                if (pkCol == null)
                    continue;   // Could not find pk column

                fkTable.HasForeignKey = true;
            }
        }

        private static string GetRelationship(Relationship relationship, IList<Column> fkCols, IList<Column> pkCols, string pkPropName, string fkPropName, string manyToManyMapping, string mapKey, bool cascadeOnDelete, bool includeReverseNavigation, bool isNotEnforced)
        {
            string hasMethod = GetHasMethod(relationship, fkCols, pkCols, isNotEnforced);
            string withMethod = GetWithMethod(relationship, fkCols, fkPropName, manyToManyMapping, mapKey, includeReverseNavigation);

            return string.Format("{0}(a => a.{1}){2}{3}",
                hasMethod,
                pkPropName,
                withMethod,
                cascadeOnDelete ? string.Empty : ".WillCascadeOnDelete(false)");
        }

        // HasOptional
        // HasRequired
        // HasMany
        private static string GetHasMethod(Relationship relationship, IList<Column> fkCols, IList<Column> pkCols, bool isNotEnforced)
        {
            bool withMany = (relationship == Relationship.ManyToOne || relationship == Relationship.ManyToMany);
            bool fkIsNullable = fkCols.Any(c => c.IsNullable);
            bool pkIsUnique = pkCols.Any(c => c.IsUnique || c.IsUniqueConstraint || c.IsPrimaryKey);

            if (withMany || pkIsUnique)
            {
                if (fkIsNullable || isNotEnforced)
                {
                    return "HasOptional";
                }
                else
                {
                    return "HasRequired";
                }
            }
            else
            {
                return "HasMany";
            }

        }

        // WithOptional
        // WithRequired
        // WithMany
        // WithRequiredPrincipal
        // WithRequiredDependent
        private static string GetWithMethod(Relationship relationship, IList<Column> fkCols, string fkPropName, string manyToManyMapping, string mapKey, bool includeReverseNavigation)
        {
            string withParam = includeReverseNavigation ? string.Format("b => b.{0}", fkPropName) : string.Empty;
            switch (relationship)
            {
                case Relationship.OneToOne:
                    return string.Format(".WithOptional({0})", withParam);

                case Relationship.OneToMany:
                    return string.Format(".WithRequiredDependent({0})", withParam);

                case Relationship.ManyToOne:
                    if (!fkCols.Any(c => c.Hidden))
                        return string.Format(".WithMany({0}).HasForeignKey({1})", withParam, manyToManyMapping);   // Foreign Key Association
                    return string.Format(".WithMany({0}).Map(c => c.MapKey({1}))", withParam, mapKey);  // Independent Association

                case Relationship.ManyToMany:
                    return string.Format(".WithMany({0}).HasForeignKey({1})", withParam, manyToManyMapping);

                default:
                    throw new ArgumentOutOfRangeException("relationship");
            }
        }

        private static Column CreateColumn(IDataRecord rdr, Regex rxClean, Table table, Regex columnFilterExclude)
        {
            if (rdr == null)
                throw new ArgumentNullException("rdr");

            string typename = rdr["TypeName"].ToString().Trim().ToLower();
            int rdrScale = (int)rdr["Scale"];
            bool rdrIsNullable = (bool)rdr["IsNullable"];
            int rdrMaxLength = (int)rdr["MaxLength"];
            int rdrDtp = (int)rdr["DateTimePrecision"];
            int rdrPrecision = (int)rdr["Precision"];
            bool rdrIsIdentity = (bool)rdr["IsIdentity"];
            bool rdrIsComputed = (bool)rdr["IsComputed"];
            bool rdrIsRowGuid = (bool)rdr["IsRowGuid"];
            byte rdrGat = (byte)rdr["GeneratedAlwaysType"];
            bool rdrIsg = (bool)rdr["IsStoreGenerated"];
            int rdrPko = (int)rdr["PrimaryKeyOrdinal"];
            bool rdrIsPk = (bool)rdr["PrimaryKey"];
            bool rdrIsFk = (bool)rdr["IsForeignKey"];

            var col = new Column
            {
                Ordinal = (int)rdr["Ordinal"],
                Name = rdr["ColumnName"].ToString().Trim(),
                IsNullable = rdrIsNullable,
                PropertyType = GetPropertyType(typename),
                SqlPropertyType = typename,
                MaxLength = rdrMaxLength,
                Precision = rdrPrecision,
                Default = rdr["Default"].ToString().Trim(),
                DateTimePrecision = rdrDtp,
                Scale = rdrScale,

                IsIdentity = rdrIsIdentity,
                IsRowGuid = rdrIsRowGuid,
                IsComputed = rdrIsComputed,
                GeneratedAlwaysType = (ColumnGeneratedAlwaysType)rdrGat,
                IsStoreGenerated = rdrIsg,

                IsPrimaryKey = rdrIsPk,
                PrimaryKeyOrdinal = rdrPko,
                IsForeignKey = rdrIsFk,
                ParentTable = table
            };

            if (col.MaxLength == -1 && (col.SqlPropertyType.EndsWith("varchar", StringComparison.InvariantCultureIgnoreCase) || col.SqlPropertyType.EndsWith("varbinary", StringComparison.InvariantCultureIgnoreCase)))
                col.SqlPropertyType += "(max)";

            if (col.IsPrimaryKey && !col.IsIdentity && col.IsStoreGenerated && typename == "uniqueidentifier")
            {
                col.IsStoreGenerated = false;
                col.IsIdentity = true;
            }

            var fullName = string.Format("{0}.{1}.{2}", table.Schema, table.Name, col.Name);
            if (columnFilterExclude != null && !col.IsPrimaryKey && (columnFilterExclude.IsMatch(col.Name) || columnFilterExclude.IsMatch(fullName)))
                col.Hidden = true;

            col.IsFixedLength = (typename == "char" || typename == "nchar");
            col.IsUnicode = !(typename == "char" || typename == "varchar" || typename == "text");
            col.IsMaxLength = (typename == "ntext");

            col.IsRowVersion = col.IsStoreGenerated && !col.IsNullable && typename == "timestamp";
            if (col.IsRowVersion)
                col.MaxLength = 8;

            if (typename == "hierarchyid")
                col.MaxLength = 0;

            col.CleanUpDefault();
            col.NameHumanCase = CleanUp(col.Name);
            col.NameHumanCase = rxClean.Replace(col.NameHumanCase, "_$1");

            if (ReservedKeywords.Contains(col.NameHumanCase))
                col.NameHumanCase = "@" + col.NameHumanCase;

            col.DisplayName = ToDisplayName(col.Name);

            var titleCase = (Settings.UsePascalCase ? Inflector.ToTitleCase(col.NameHumanCase) : col.NameHumanCase).Replace(" ", "");
            if (titleCase != string.Empty)
                col.NameHumanCase = titleCase;

            // Make sure property name doesn't clash with class name
            if (col.NameHumanCase == table.NameHumanCase)
                col.NameHumanCase = col.NameHumanCase + "_";

            if (char.IsDigit(col.NameHumanCase[0]))
                col.NameHumanCase = "_" + col.NameHumanCase;

            table.HasNullableColumns = col.IsNullable2(); // TODO: doesn't look correct. but doesn't seem to be used at all

            // If PropertyType is empty, return null. Most likely ignoring a column due to legacy (such as OData not supporting spatial types)
            if (string.IsNullOrEmpty(col.PropertyType))
                return null;

            return col;
        }

        private static string GetSqlDbType(string sqlType)
        {
            var sysType = "VarChar";
            switch (sqlType)
            {
                case "hierarchyid":
                    sysType = "VarChar";
                    break;

                case "bigint":
                    sysType = "BigInt";
                    break;

                case "binary":
                    sysType = "Binary";
                    break;

                case "bit":
                    sysType = "Bit";
                    break;

                case "char":
                    sysType = "Char";
                    break;

                case "datetime":
                    sysType = "DateTime";
                    break;

                case "decimal":
                case "numeric":
                    sysType = "Decimal";
                    break;

                case "float":
                    sysType = "Float";
                    break;

                case "image":
                    sysType = "Image";
                    break;

                case "int":
                    sysType = "Int";
                    break;

                case "money":
                    sysType = "Money";
                    break;

                case "nchar":
                    sysType = "NChar";
                    break;

                case "ntext":
                    sysType = "NText";
                    break;

                case "nvarchar":
                    sysType = "NVarChar";
                    break;

                case "real":
                    sysType = "Real";
                    break;

                case "uniqueidentifier":
                    sysType = "UniqueIdentifier";
                    break;

                case "smalldatetime":
                    sysType = "SmallDateTime";
                    break;

                case "smallint":
                    sysType = "SmallInt";
                    break;

                case "smallmoney":
                    sysType = "SmallMoney";
                    break;

                case "text":
                    sysType = "Text";
                    break;

                case "timestamp":
                    sysType = "Timestamp";
                    break;

                case "tinyint":
                    sysType = "TinyInt";
                    break;

                case "varbinary":
                    sysType = "VarBinary";
                    break;

                case "varchar":
                    sysType = "VarChar";
                    break;

                case "variant":
                    sysType = "Variant";
                    break;

                case "xml":
                    sysType = "Xml";
                    break;

                case "udt":
                    sysType = "Udt";
                    break;

                case "table type":
                case "structured":
                    sysType = "Structured";
                    break;

                case "date":
                    sysType = "Date";
                    break;

                case "time":
                    sysType = "Time";
                    break;

                case "datetime2":
                    sysType = "DateTime2";
                    break;

                case "datetimeoffset":
                    sysType = "DateTimeOffset";
                    break;
            }
            return sysType;
        }

        private static string GetPropertyType(string sqlType)
        {
            var sysType = "string";
            switch (sqlType)
            {
                case "hierarchyid":
                    sysType = "System.Data.Entity.Hierarchy.HierarchyId";
                    break;
                case "bigint":
                    sysType = "long";
                    break;
                case "smallint":
                    sysType = "short";
                    break;
                case "int":
                    sysType = "int";
                    break;
                case "uniqueidentifier":
                    sysType = "System.Guid";
                    break;
                case "smalldatetime":
                case "datetime":
                case "datetime2":
                case "date":
                    sysType = "System.DateTime";
                    break;
                case "datetimeoffset":
                    sysType = "System.DateTimeOffset";
                    break;
                case "table type":
                    sysType = "System.Data.DataTable";
                    break;
                case "time":
                    sysType = "System.TimeSpan";
                    break;
                case "float":
                    sysType = "double";
                    break;
                case "real":
                    sysType = "float";
                    break;
                case "numeric":
                case "smallmoney":
                case "decimal":
                case "money":
                    sysType = "decimal";
                    break;
                case "tinyint":
                    sysType = "byte";
                    break;
                case "bit":
                    sysType = "bool";
                    break;
                case "image":
                case "binary":
                case "varbinary":
                case "varbinary(max)":
                case "timestamp":
                    sysType = "byte[]";
                    break;
                case "geography":
                    sysType = Settings.DisableGeographyTypes ? "" : "System.Data.Entity.Spatial.DbGeography";
                    break;
                case "geometry":
                    sysType = Settings.DisableGeographyTypes ? "" : "System.Data.Entity.Spatial.DbGeometry";
                    break;
            }
            return sysType;
        }


        private static readonly Regex RxCleanUp = new Regex(@"[^\w\d\s_-]", RegexOptions.Compiled);

        private static readonly Func<string, string> CleanUp = (str) =>
        {
            // Replace punctuation and symbols in variable names as these are not allowed.
            int len = str.Length;
            if (len == 0)
                return str;
            var sb = new StringBuilder();
            bool replacedCharacter = false;
            for (int n = 0; n < len; ++n)
            {
                char c = str[n];
                if (c != '_' && c != '-' && (char.IsSymbol(c) || char.IsPunctuation(c)))
                {
                    int ascii = c;
                    sb.AppendFormat("{0}", ascii);
                    replacedCharacter = true;
                    continue;
                }
                sb.Append(c);
            }
            if (replacedCharacter)
                str = sb.ToString();

            // Remove non alphanumerics
            str = RxCleanUp.Replace(str, "");
            if (char.IsDigit(str[0]))
                str = "C" + str;

            return str;
        };

        private static readonly Func<string, string> ToDisplayName = (str) =>
        {
            if (string.IsNullOrEmpty(str))
                return string.Empty;

            var sb = new StringBuilder();
            str = Regex.Replace(str, @"[^a-zA-Z0-9]", " "); // Anything that is not a letter or digit, convert to a space
            str = Regex.Replace(str, @"[A-Z]{2,}", " $+ "); // Any word that is upper case

            var hasUpperCased = false;
            var lastChar = '\0';
            foreach (var original in str.Trim())
            {
                var c = original;
                if (lastChar == '\0')
                {
                    c = char.ToUpperInvariant(original);
                }
                else
                {
                    var isLetter = char.IsLetter(original);
                    var isDigit = char.IsDigit(original);
                    var isWhiteSpace = !isLetter && !isDigit;

                    // Is this char is different to last time
                    var isDifferent = false;
                    if (isLetter && !char.IsLetter(lastChar))
                        isDifferent = true;
                    else if (isDigit && !char.IsDigit(lastChar))
                        isDifferent = true;
                    else if (char.IsUpper(original) && !char.IsUpper(lastChar))
                        isDifferent = true;

                    if (isDifferent || isWhiteSpace)
                        sb.Append(' '); // Add a space

                    if (hasUpperCased && isLetter)
                        c = char.ToLowerInvariant(original);
                }
                lastChar = original;
                if (!hasUpperCased && char.IsUpper(c))
                    hasUpperCased = true;
                sb.Append(c);
            }
            str = sb.ToString();
            str = Regex.Replace(str, @"\s+", " ").Trim(); // Multiple white space to one space
            str = Regex.Replace(str, @"\bid\b", "ID"); //  Make ID word uppercase
            return str;
        };


        // Calculates the relationship between a child table and it's parent table.
        public static Relationship CalcRelationship(Table parentTable, Table childTable, List<Column> childTableCols, List<Column> parentTableCols)
        {
            if (childTableCols.Count == 1 && parentTableCols.Count == 1)
                return CalcRelationshipSingle(parentTable, childTable, childTableCols.First(), parentTableCols.First());

            // This relationship has multiple composite keys

            // childTable FK columns are exactly the primary key (they are part of primary key, and no other columns are primary keys) //TODO: we could also check if they are an unique index
            bool childTableColumnsAllPrimaryKeys = (childTableCols.Count == childTableCols.Count(x => x.IsPrimaryKey)) && (childTableCols.Count == childTable.PrimaryKeys.Count());

            // parentTable columns are exactly the primary key (they are part of primary key, and no other columns are primary keys) //TODO: we could also check if they are an unique index
            bool parentTableColumnsAllPrimaryKeys = (parentTableCols.Count == parentTableCols.Count(x => x.IsPrimaryKey)) && (parentTableCols.Count == parentTable.PrimaryKeys.Count());

            // childTable FK columns are not only FK but also the whole PK (not only part of PK); parentTable columns are the whole PK (not only part of PK) - so it's 1:1
            if (childTableColumnsAllPrimaryKeys && parentTableColumnsAllPrimaryKeys)
                return Relationship.OneToOne;

            return Relationship.ManyToOne;
        }

        // Calculates the relationship between a child table and it's parent table.
        public static Relationship CalcRelationshipSingle(Table parentTable, Table childTable, Column childTableCol, Column parentTableCol)
        {
            if (!childTableCol.IsPrimaryKey && !childTableCol.IsUniqueConstraint)
                return Relationship.ManyToOne;

            if (!parentTableCol.IsPrimaryKey && !parentTableCol.IsUniqueConstraint)
                return Relationship.ManyToOne;

            if (childTable.PrimaryKeys.Count() != 1)
                return Relationship.ManyToOne;

            if (parentTable.PrimaryKeys.Count() != 1)
                return Relationship.ManyToOne;

            return Relationship.OneToOne;
        }



        public static readonly List<string> ReservedKeywords = new List<string>
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
            "checked", "class", "const", "continue", "decimal", "default", "delegate", "do",
            "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed",
            "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
            "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator",
            "out", "override", "params", "private", "protected", "public", "readonly", "ref",
            "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string",
            "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "volatile", "void", "while"
        };


        //TODO: move this?
        public static readonly List<string> NotNullable = new List<string>
        {
            "string",
            "byte[]",
            "datatable",
            "system.data.datatable",
            "object",
            "microsoft.sqlserver.types.sqlgeography",
            "microsoft.sqlserver.types.sqlgeometry",
            "system.data.entity.spatial.dbgeography",
            "system.data.entity.spatial.dbgeometry",
            "system.data.entity.hierarchy.hierarchyid"
        };


    }
}
