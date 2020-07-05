using System;
using System.Collections.Generic;
using System.Text;

public class SqlServerDatabaseSchema
{
    public DateTimeOffset LastRefreshed { get; set; }
    public List<SqlServerTable> Tables { get; set; }
}
