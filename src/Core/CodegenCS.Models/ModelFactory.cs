using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static CodegenCS.Utils.TypeUtils;

namespace CodegenCS.Models
{
    /// <inheritdoc />
    public class ModelFactory : IModelFactory
    {
        protected List<IInputModelAdapter> _inputModelAdapters;
        
        public ModelFactory() 
        {
            _inputModelAdapters = new List<IInputModelAdapter>();
        }

        public ModelFactory(List<IInputModelAdapter> inputModelAdapters) 
        {  
            _inputModelAdapters = inputModelAdapters;
        }

        public bool CanCreateModel(Type modelType)
        {
            if (IsAssignableToType(modelType, typeof(IInputModel)))
                return true;
            if (_inputModelAdapters.Any(a => a.CanLoadType(modelType)))
                return true;
            return false;
        }

        public T LoadModelFromFile<T>(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }

        public Task<T> LoadModelFromFileAsync<T>(string filePath)
        {
            return Task.Factory.StartNew(() => JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath)));
        }

        public async Task<object> LoadModelFromFileAsync(Type modelType, string filePath)
        {
            var adapter = _inputModelAdapters.FirstOrDefault(a => a.CanLoadType(modelType));
            if (adapter != null)
            {
                return await adapter.LoadFromContentAsync(File.ReadAllText(filePath), filePath);
            }
            if (IsAssignableToType(modelType, typeof(IJsonInputModel)))
                return JsonConvert.DeserializeObject(File.ReadAllText(filePath), modelType);
            if (IsAssignableToType(modelType, typeof(IInputModel))) // for now (no yaml yet) let's assume that all IInputModel are JSON
                return JsonConvert.DeserializeObject(File.ReadAllText(filePath), modelType);
            throw new NotImplementedException();
        }

        public object LoadModelFromFile(Type modelType, string filePath)
        {
            return LoadModelFromFileAsync(modelType, filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
