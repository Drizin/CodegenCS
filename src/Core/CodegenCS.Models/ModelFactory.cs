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
        protected string[] _searchPaths;

        public ModelFactory(string[] searchPaths)
        {
            _searchPaths = searchPaths;
            _inputModelAdapters = new List<IInputModelAdapter>();
        }

        public ModelFactory(string[] searchPaths, List<IInputModelAdapter> inputModelAdapters) 
        {
            _searchPaths = searchPaths;
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

        protected string FindFile(string filePath)
        {
            string path;
            if (!Path.IsPathRooted(filePath) && _searchPaths != null)
            {
                foreach (var searchPath in _searchPaths)
                {
                    if (!string.IsNullOrEmpty(searchPath) && File.Exists(path = Path.Combine(searchPath, filePath)))
                        return path;
                }
            }
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Can't find {filePath}");
            return filePath;
        }

        public async Task<T> LoadModelFromFileAsync<T>(string filePath)
        {
            return (T)await LoadModelFromFileAsync(typeof(T), filePath);
        }

        public async Task<object> LoadModelFromFileAsync(Type modelType, string filePath)
        {
            filePath = FindFile(filePath);
            var adapter = _inputModelAdapters.FirstOrDefault(a => a.CanLoadType(modelType));
            if (adapter != null)
            {
                return await adapter.LoadFromContentAsync(File.ReadAllText(filePath), filePath);
            }
            if (IsAssignableToType(modelType, typeof(IJsonInputModel)))
                return JsonConvert.DeserializeObject(File.ReadAllText(filePath), modelType);
            if (IsAssignableToType(modelType, typeof(IInputModel))) // for now (no yaml yet) let's assume that all IInputModel are JSON
                return JsonConvert.DeserializeObject(File.ReadAllText(filePath), modelType);
            // For any other types (not implementing IInputModel) we assume it's JSON
            var content = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject(content, modelType);
        }

        public object LoadModelFromFile(Type modelType, string filePath)
        {
            return LoadModelFromFileAsync(modelType, filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public T LoadModelFromFile<T>(string filePath)
        {
            return LoadModelFromFileAsync<T>(filePath).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
