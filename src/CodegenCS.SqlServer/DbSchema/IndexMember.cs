using Newtonsoft.Json;
using System;
using System.Collections.Generic;

public class IndexMember
{
    public string ColumnName { get; set; }

    public int IndexOrdinalPosition { get; set; }

    public bool IsDescendingKey { get; set; }
    public bool IsIncludedColumn { get; set; }
}
