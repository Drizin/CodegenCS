using CodegenCS.Models;
using CodegenCS.Models.NSwagAdapter;
using System.Collections.Generic;


namespace CodegenCS
{
    public class ModelFactoryBuilder
    {
        public static IModelFactory CreateModelFactory(string[] searchPaths)
        {
            return new ModelFactory(searchPaths, new List<IInputModelAdapter>() { new OpenApiDocumentAdapter() });
        }
    }
}
