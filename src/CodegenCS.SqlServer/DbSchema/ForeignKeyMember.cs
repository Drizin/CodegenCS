using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

public class ForeignKeyMember
{
    public int PKColumnOrdinalPosition { get; set; }
    public string PKColumnName { get; set; }

    public string FKColumnName { get; set; }
}
