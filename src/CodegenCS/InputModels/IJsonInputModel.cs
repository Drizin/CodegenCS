using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS.InputModels
{
    /// <summary>
    /// JSON-based Input Models provide information to be used in Templates (generators)
    /// </summary>
    public interface IJsonInputModel : IInputModel
    {
        /// <summary>
        /// Json Schema which uniquely identifies this model.
        /// Should follow https://json-schema.org/ specs.
        /// Can be a url, can have a version in the url.
        /// E.g. "http://codegencs.com/schemas/dbschema/2021-07/dbschema.json"
        /// </summary>
        [JsonProperty("$schema")]
        string Schema { get; }
    }
}
