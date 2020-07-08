using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class SqlServerIndexMember
{
    [JsonIgnore] // only used in-memory to associate column with parent index
    public string Database { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string TableSchema { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string TableName { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public string IndexName { get; set; }

    [JsonIgnore] // only used in-memory to associate column with parent index
    public int IndexId { get; set; }


    public string ColumnName { get; set; }

    public int IndexOrdinalPosition { get; set; }

    public bool IsDescendingKey { get; set; }
    public bool IsIncludedColumn { get; set; }
}
