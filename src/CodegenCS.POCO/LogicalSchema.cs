using System;
using System.Collections.Generic;

/************************************************************************************************
 The JSON serialized schema has only Physical Properties.
 We inherit Physical definitions, and add some new Logical definitions.
 For example: ForeignKeys in a logical model have the "Navigation Property Name".
 And POCOs (mapped 1-to-1 by Entities) track the list of Property Names used by Columns, used by Navigation Properties, etc., 
 to avoid naming conflicts.
************************************************************************************************/

public class LogicalSchema : CodegenCS.DbSchema.DatabaseSchema
{
    public new List<Table> Tables { get; set; }
}
public class Table : CodegenCS.DbSchema.Table
{
    public Dictionary<string, string> ColumnPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> FKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, string> ReverseFKPropertyNames { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public new List<Column> Columns { get; set; } = new List<Column>();
    public new List<ForeignKey> ForeignKeys { get; set; } = new List<ForeignKey>();
    public new List<ForeignKey> ChildForeignKeys { get; set; } = new List<ForeignKey>();
}

public class ForeignKey : CodegenCS.DbSchema.ForeignKey
{
    public string NavigationPropertyName { get; set; }
}
public class Column : CodegenCS.DbSchema.Column
{

}
