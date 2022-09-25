using System;
using System.Threading.Tasks;

namespace CodegenCS.Models
{
    /// <summary>
    /// Input Model Adapters are used to load third-party models
    /// </summary>
    public interface IInputModelAdapter
    {
        bool CanLoadType(Type targetType);
        Task<object> LoadFromContentAsync(string content, string filePath);
    }
}
