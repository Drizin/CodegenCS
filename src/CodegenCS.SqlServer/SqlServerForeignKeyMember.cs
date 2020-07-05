using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public class SqlServerForeignKeyMember
{
    [JsonIgnore] // only used in-memory to associate column with parent constraint
    public string ForeignKeyConstraintName { get; set; }
    [JsonIgnore] // only used in-memory to associate column with parent constraint
    public string FKTableSchema { get; set; }

    public int PKColumnOrdinalPosition { get; set; }
    public string PKColumnName { get; set; }

    public string FKColumnName { get; set; }
}
