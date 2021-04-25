using CodegenCS.InputModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

#if DLL // if this is included in a CSX file we don't want namespaces, because most Roslyn engines don't play well with namespaces
namespace CodegenCS.DbSchema
{
#endif
    public class DatabaseSchema : IJsonInputModel
    {
        #region IJsonInputModel
        /// <inheritdoc/>
        public string Id => "http://codegencs.com/schemas/dbschema.json";

        public string Schema => _jsonSchema.Value;
        static Lazy<string> _jsonSchema = new Lazy<string>(() =>
        {
            var _assembly = Assembly.GetExecutingAssembly();
            var _textStreamReader = new StreamReader(_assembly.GetManifestResourceStream("CodegenCS.DbSchema.dbschema.json"));
            return _textStreamReader.ReadToEnd();
        });
        #endregion

        public static DatabaseSchema TryParse(string input)
        {
            if (new JsonInputModelParser().IsValid(_jsonSchema.Value, input))
                return Newtonsoft.Json.JsonConvert.DeserializeObject<DatabaseSchema>(input);
            return null;
        }

        public DateTimeOffset LastRefreshed { get; set; }
        public List<Table> Tables { get; set; }
    }
#if DLL
}
#endif