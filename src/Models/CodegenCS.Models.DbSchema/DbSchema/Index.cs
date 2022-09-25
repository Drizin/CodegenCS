using System;
using System.Collections.Generic;

namespace CodegenCS.Models.DbSchema
{
    public class Index
    {
        public string IndexName { get; set; }

        public int IndexId { get; set; }

        /// <summary>
        /// CLUSTERED
        /// NONCLUSTERED
        /// XML
        /// HEAP
        /// SPATIAL
        /// CLUSTERED COLUMNSTORE (SQL Server 2014 (12.x) and later)
        /// NONCLUSTERED COLUMNSTORE (SQL Server 2012 (11.x) and later)
        /// NONCLUSTERED HASH (SQL Server 2014 (12.x) and later)
        /// </summary>
        public string PhysicalType { get; set; }

        /// <summary>
        /// PRIMARY_KEY
        /// UNIQUE_INDEX
        /// UNIQUE_CONSTRAINT
        /// NON_UNIQUE_INDEX
        /// </summary>
        public string LogicalType { get; set; }


        [Obsolete("Please prefer using LogicalType property")]
        public bool IsPrimaryKey { get; set; }

        [Obsolete("Please prefer using LogicalType property")]
        public bool IsUnique { get; set; }

        [Obsolete("Please prefer using LogicalType property. Unique Constraints are maintained through a unique Index (IsUnique is also true)")]
        public bool IsUniqueConstraint { get; set; }

        public string IndexDescription { get; set; }

        public List<IndexMember> Columns { get; set; }

        public override string ToString() => IndexName; // If someone renders the object (index) instead of using the right property

    }
}
