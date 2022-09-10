using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class ForeignKeyMember
    {
        public int PKColumnOrdinalPosition { get; set; }
        public string PKColumnName { get; set; }

        public string FKColumnName { get; set; }
    }
#if DLL
}
#endif