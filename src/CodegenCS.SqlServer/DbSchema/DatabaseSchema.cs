using System;
using System.Collections.Generic;
using System.Text;

public class DatabaseSchema
{
    public DateTimeOffset LastRefreshed { get; set; }
    public List<Table> Tables { get; set; }
}
