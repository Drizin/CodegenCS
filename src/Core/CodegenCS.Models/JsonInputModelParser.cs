using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodegenCS.Models
{
    /// <summary>
    /// Parser for JsonInputModels
    /// </summary>
    public class JsonInputModelParser
    {
        /// <summary>
        /// Returns if a given model is valid according to Json Schema
        /// </summary>
        public async Task<List<ValidationError>> ValidateSchemaAsync(string jsonSchema, string input)
        {
            JsonSchema schema = await JsonSchema.FromJsonAsync(jsonSchema);
            JObject jObject = JObject.Parse(input);
            List<ValidationError> errors = new JsonSchemaValidator().Validate(input, schema).ToList();

            errors.ForEach(error => System.Diagnostics.Debug.WriteLine($"Error {error.Kind} on line {error.LineNumber}"));

            return errors;
        }
    }
}
