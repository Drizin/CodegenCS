using Newtonsoft.Json;

namespace CodegenCS.InputModels
{
    /// <summary>
    /// JSON-based Input Models that are loaded by Command-line tool (dotnet-codegencs) (deserialized from a JSON file)
    /// and will be automatically injected into template constructors or entrypoints.
    /// </summary>
    public interface IJsonInputModel : IInputModel
    {
    }

    /// <summary>
    /// JSON-based Input Models that have a Schema definition in a public URL and therefore can be validated.
    /// </summary>
    public interface IValidatableJsonInputModel : IJsonInputModel
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
