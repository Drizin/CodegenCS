using CodegenCS.Models;
using DependencyContainer = CodegenCS.Utils.DependencyContainer;

namespace CodegenCS
{
    public static class DependencyContainerExtensions
    {
        public static DependencyContainer AddModelFactory(this DependencyContainer dependencyContainer, string[] args = null)
        {
            dependencyContainer.RegisterSingleton<IModelFactory>(ModelFactoryBuilder.GetModelFactory());
            return dependencyContainer;
        }
    }
}
