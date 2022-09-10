using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class ForeignKey
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
        public string PrimaryKeyName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
        public string PKTableSchema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
        public string PKTableName { get; set; }


        public string ForeignKeyConstraintName { get; set; }

        public string ForeignKeyDescription { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
        public string FKTableSchema { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)] // when constraint is serialized under parent table we don't need to serialize redundant attributes
        public string FKTableName { get; set; }


        /// <summary>
        /// NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
        /// </summary>
        public string OnDeleteCascade { get; set; }

        /// <summary>
        /// NO_ACTION, CASCADE, SET_NULL, SET_DEFAULT
        /// </summary>
        public string OnUpdateCascade { get; set; }

        public bool IsSystemNamed { get; set; }

        public bool IsNotEnforced { get; set; }

        public List<ForeignKeyMember> Columns { get; set; }
    }
#if DLL
}
#endif