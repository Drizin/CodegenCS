using CodegenCS.InputModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class DatabaseSchema : IJsonInputModel
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

        public static DatabaseSchema TryParse(string input)
        {
            var validationErrors = new JsonInputModelParser().ValidateSchema(_jsonSchema.Value, input);

            // ignore these irrelevant errors from previous versions
            // TODO: Clearify how to handle this without "message"
            //validationErrors.RemoveAll(v => v.Message.StartsWith("Property 'Id' has not been defined and the schema does not allow additional properties."));
            //validationErrors.RemoveAll(v => v.Message.StartsWith("Property 'Schema' has not been defined and the schema does not allow additional properties."));
            //validationErrors.RemoveAll(v => v.Message.StartsWith("Required properties are missing from object: $schema."));

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
#if DLL
}
#endif