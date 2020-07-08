using System;
using System.Collections.Generic;

public class SqlServerTable
{
    public string Database { get; set; }
    public string TableSchema { get; set; }
    public string TableName { get; set; }

    /// <summary>
    /// Can be "TABLE" or "VIEW"
    /// </summary>
    public string TableType { get; set; }

    public string TableDescription { get; set; }

    public List<SqlServerColumn> Columns { get; set; } = new List<SqlServerColumn>();

    /// <summary>
    /// FKs which point from THIS (Child) table to the primary key of OTHER (Parent) tables
    /// </summary>
    public List<SqlServerForeignKey> ForeignKeys { get; set; } = new List<SqlServerForeignKey>();

    /// <summary>
    /// FKs which point from OTHER (Child) tables to the primary key of THIS (Parent) table
    /// </summary>
    public List<SqlServerForeignKey> ChildForeignKeys { get; set; } = new List<SqlServerForeignKey>();

    public string PrimaryKeyName { get; set; }

    public bool PrimaryKeyIsClustered { get; set; }

    public List<SqlServerIndex> Indexes { get; set; } = new List<SqlServerIndex>();

}
