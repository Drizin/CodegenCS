using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;
using System.Collections.Generic;
using System.Linq;

namespace CodegenCS.InputModels
{
    /// <summary>
    /// Parser for JsonInputModels
    /// </summary>
    public class JsonInputModelParser

    {
        /// <summary>
        /// Returns if a given model is valid according to Json Schema
        /// </summary>
        public List<ValidationError> ValidateSchema(string jsonSchema, string input)
        {
            JsonSchema schema = JsonSchema.FromSampleJson(jsonSchema);
            JObject jObject = JObject.Parse(input);
            List<ValidationError> errors = new JsonSchemaValidator().Validate(input, schema).ToList();

            errors.ForEach(error => System.Diagnostics.Debug.WriteLine($"Error {error.Kind} on line {error.LineNumber}"));

            return errors;
        }
    }
}
