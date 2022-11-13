using DependencyContainer = CodegenCS.Utils.DependencyContainer;
using System;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using static CodegenCS.Utils.DependencyContainer;

namespace CodegenCS.Runtime
{
    /// <summary>
    /// Any class implementing IModelBinderArgs will be automatically resolved using ModelBinder,
    /// which binds (by matching their names) all class properties to the command-line Arguments and Options.
    /// Example: Templates may expect a TemplateArgs class, and those properties can be passed using command-line arguments.
    /// 
    /// Other options for passing command-line arguments to templates are <see cref="CommandLineArgs"/> 
    /// or specifying a "public static void ConfigureCommand(Command command)" (see TemplateLauncher)
    /// </summary>
    public class AutoBindCommandLineArgsTypeResolver : ITypeResolver
    {
        public bool CanResolveType(Type targetType)
        {
            return typeof(IAutoBindCommandLineArgs).IsAssignableFrom(targetType);
        }

        public bool TryResolveType(Type targetType, DependencyContainer dependencyContainer, out object value)
        {
            var binder = new ModelBinder(targetType);
            try
            {
                BindingContext bindingContext = (BindingContext)dependencyContainer.Resolve(typeof(BindingContext));
                // create an instance of MyTemplateArgs based on the parsed command line
                value = binder.CreateInstance(bindingContext);
                return true;
            }
            catch 
            {
                value = null;
                return false;
            }
        }
    }
}
