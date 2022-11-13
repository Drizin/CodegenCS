using CodegenCS.Models;
using CodegenCS.Models.NSwagAdapter;
using System;
using System.Collections.Generic;
using System.Text;

namespace CodegenCS
{
    public class ModelFactoryBuilder
    {
        public static IModelFactory GetModelFactory()
        {
            return new ModelFactory(new List<IInputModelAdapter>() { new OpenApiDocumentAdapter() });
        }
    }
}
