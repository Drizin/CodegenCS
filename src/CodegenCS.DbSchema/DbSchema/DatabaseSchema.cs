using System;
using System.Collections.Generic;
using System.Text;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class DatabaseSchema
    {
        public DateTimeOffset LastRefreshed { get; set; }
        public List<Table> Tables { get; set; }
    }
#if DLL
}
#endif