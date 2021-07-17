extern alias NewtonsoftJsonSchema;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using NewtonsoftJsonSchema::Newtonsoft.Json.Schema;

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
            JSchema schema = JSchema.Parse(jsonSchema);
            JObject jObject = JObject.Parse(input);
            List<ValidationError> errors = new List<ValidationError>();
            jObject.Validate(schema, (sender, e) => 
            {
                errors.Add(e.ValidationError);
                System.Diagnostics.Debug.WriteLine($"Error {e.Message} on line {e.ValidationError.LineNumber}");
            });
            return errors;
        }
    }
}
