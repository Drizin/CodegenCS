using System;
using System.Collections.Generic;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class Table
    {
        public string Database { get; set; }
        public string TableSchema { get; set; }
        public string TableName { get; set; }

        /// <summary>
        /// Can be "TABLE" or "VIEW"
        /// </summary>
        public string TableType { get; set; }

        public string TableDescription { get; set; }

        public List<Column> Columns { get; set; } = new List<Column>();

        /// <summary>
        /// FKs which point from THIS (Child) table to the primary key of OTHER (Parent) tables
        /// </summary>
        public List<ForeignKey> ForeignKeys { get; set; } = new List<ForeignKey>();

        /// <summary>
        /// FKs which point from OTHER (Child) tables to the primary key of THIS (Parent) table
        /// </summary>
        public List<ForeignKey> ChildForeignKeys { get; set; } = new List<ForeignKey>();

        public string PrimaryKeyName { get; set; }

        public bool PrimaryKeyIsClustered { get; set; }

        public List<Index> Indexes { get; set; } = new List<Index>();

    }
#if DLL
}
#endif