using CodegenCS.Runtime.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodegenCS.VisualStudio.Shared.Utils
{
    public class AssemblyLoaderInitialization
    {
        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized) return;

            // Host AppDomain (VisualStudio) may need to resolve libraries embedded in our extension

            var loadableAssemblies = GetCurrentAssemblies();
            List<string> searchPaths = new List<string>()
                        {
                            Path.GetDirectoryName(Assembly.GetAssembly(typeof(TemplateLauncher.TemplateLauncher)).Location),
                            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                            //TODO: Add to searchPaths locations of all available dlls in loadableAssemblies folders? Or maybe only VS folders?
                        }.Distinct().ToList();

            // Latest VS2022 can run TemplateBuilder (Roslyn Microsoft.CodeAnalysis.CSharp 4.2) in same AppDomain without conflict
#if VS2022_OR_NEWER
            // VS2022 version will just run in same process
            var loader = new AssembliesLoader(null, null, searchPaths);
#else // VS2019_OR_OLDER
            // Compatibility edition runs in an isolated process, and uses old VS references
            // The list of loadableAssemblies is used to redirect assemblies to newer versions
            // e.g. "Microsoft.VisualStudio.TextTemplating.VSHost.15.0" 16.x should be redirected to "whatever is available and loaded" (in VS2022 that would be 17.x)
            // All assemblies loaded by Visual Studio should be available to child AppDomain
            var loader = new AssembliesLoader(null, loadableAssemblies, searchPaths);
#endif
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(loader.AssemblyResolve);

            _initialized = true;

        }
        public static List<LoadableAssemblyInfo> GetCurrentAssemblies()
        {
            var currentAssemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .Select(asm => new LoadableAssemblyInfo()
                {
                    Location = asm.Location,
                    Name = asm.GetName().Name,
                    Version = asm.GetName().Version
                })
                .ToList();
            return currentAssemblies;
        }
    }
}