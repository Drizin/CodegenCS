using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class IndexMember
    {
        public string ColumnName { get; set; }

        public int IndexOrdinalPosition { get; set; }

        public bool IsDescendingKey { get; set; }
        public bool IsIncludedColumn { get; set; }
    }
#if DLL
}
#endif