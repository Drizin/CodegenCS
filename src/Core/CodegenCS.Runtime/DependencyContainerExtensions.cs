using DependencyContainer = CodegenCS.Utils.DependencyContainer;

namespace CodegenCS.Runtime
{
    public static class DependencyContainerExtensions
    {
        public static DependencyContainer AddConsole(this DependencyContainer dependencyContainer, string[] args = null)
        {
            dependencyContainer.RegisterCustomTypeResolver(new AutoBindCommandLineArgsTypeResolver());
            dependencyContainer.RegisterSingleton<ILogger>(new ColoredConsoleLogger());
            if (args != null)
                dependencyContainer.RegisterSingleton<CommandLineArgs>(new CommandLineArgs(args));
            return dependencyContainer;
        }
        internal static DependencyContainer AddTestsConsole(this DependencyContainer dependencyContainer, string[] args = null)
        {
            dependencyContainer.RegisterCustomTypeResolver(new AutoBindCommandLineArgsTypeResolver());
            dependencyContainer.RegisterSingleton<ILogger>(new DebugOutputLogger());
            if (args != null)
                dependencyContainer.RegisterSingleton<CommandLineArgs>(new CommandLineArgs(args));
            return dependencyContainer;
        }
    }
}
