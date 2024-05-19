using CodegenCS.Models;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;

namespace CodegenCS
{
    public static class DependencyContainerExtensions
    {
        public static DependencyContainer AddModelFactory(this DependencyContainer dependencyContainer, string[] searchPaths)
        {
            dependencyContainer.RegisterSingleton<IModelFactory>(ModelFactoryBuilder.CreateModelFactory(searchPaths));
            return dependencyContainer;
        }
    }
}
