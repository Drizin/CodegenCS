using Newtonsoft.Json;
using System.Reflection;

namespace CodegenCS.Models.DbSchema
{
    public class Column
    {
        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }

        public string DefaultSetting { get; set; }

        public bool IsNullable { get; set; }

        public string SqlDataType { get; set; }

        /// <summary>
        /// CLR Type which is equivalent to the SqlDataType
        /// </summary>
        public string ClrType { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? MaxLength { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DateTimePrecision { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NumericScale { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? NumericPrecision { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsComputed { get; set; }

        public bool IsRowGuid { get; set; }

        public bool IsPrimaryKeyMember { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PrimaryKeyOrdinalPosition { get; set; }

        public bool IsForeignKeyMember { get; set; }

        public string ColumnDescription { get; set; }

        public override string ToString() => ColumnName; // If someone renders the column object instead of using the right property
    }
}
