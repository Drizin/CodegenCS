using System;
using System.Threading.Tasks;

namespace CodegenCS.Models
{
    /// <summary>
    /// Can be injected into any template and can be used to load models from file.
    /// Loading can be just a deserialization or can use an <see cref="IInputModelAdapter"/> that describes a custom factory for loading third-party models
    /// </summary>
    public interface IModelFactory
    {
        Task<TModel> LoadModelFromFileAsync<TModel>(string filePath);
        TModel LoadModelFromFile<TModel>(string filePath);
        Task<object> LoadModelFromFileAsync(Type modelType, string filePath);
        object LoadModelFromFile(Type modelType, string filePath);
        bool CanCreateModel(Type modelType);
    }
}
