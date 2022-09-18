using CodegenCS.InputModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CodegenCS.DbSchema
{
    public class DatabaseSchema : IJsonInputModel, IValidatableJsonInputModel
    {
        #region IJsonInputModel
        /// <inheritdoc/>
        public string Schema => _schema;
        private static string _schema => "http://codegencs.com/schemas/dbschema/2021-07/dbschema.json";

        static Lazy<string> _jsonSchema = new Lazy<string>(() =>
        {
            var _assembly = Assembly.GetExecutingAssembly();
            var _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("CodegenCS.DbSchema.dbschema.json"));
            return _textStreamReader.ReadToEnd();
        });
        #endregion

        public async static Task<DatabaseSchema> TryParseAsync(string input)
        {
            var validationErrors = await new JsonInputModelParser().ValidateSchemaAsync(_jsonSchema.Value, input);

            // ignore these irrelevant errors from previous versions
            validationErrors.RemoveAll(v => v.Kind == NJsonSchema.Validation.ValidationErrorKind.NoAdditionalPropertiesAllowed && v.Property == "Id");
            validationErrors.RemoveAll(v => v.Kind == NJsonSchema.Validation.ValidationErrorKind.NoAdditionalPropertiesAllowed && v.Property == "Schema");
            validationErrors.RemoveAll(v => v.Kind == NJsonSchema.Validation.ValidationErrorKind.PropertyRequired && v.Property == "$schema");

            if (validationErrors.Any())
                return null;

            var parsed = JsonConvert.DeserializeObject<DatabaseSchema>(input);

            if (!string.IsNullOrEmpty(parsed.Schema) && parsed.Schema != _schema && parsed.Schema != "http://codegencs.com/schemas/dbschema.json")
                throw new Exception($"Invalid schema \"({parsed.Schema})\" - should be \"{_schema}\"");

            return parsed;
        }

        public DateTimeOffset LastRefreshed { get; set; }
        public List<Table> Tables { get; set; }
    }
}
