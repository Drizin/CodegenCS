using NSwag;
using System;
using System.Threading.Tasks;

namespace CodegenCS.Models.NSwagAdapter
{
    public class OpenApiDocumentAdapter : IInputModelAdapter
    {
        public bool CanLoadType(Type targetType)
        {
            return targetType == typeof(OpenApiDocument);
        }

        public async Task<object> LoadFromContentAsync(string content, string filePath)
        {
            var document = await OpenApiDocument.FromJsonAsync(content);
            return document;
        }
    }
}
