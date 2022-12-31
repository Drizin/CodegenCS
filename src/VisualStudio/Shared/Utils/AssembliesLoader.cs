using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodegenCS.VisualStudio.Shared.Utils
{
    [Serializable]
    public class AssembliesLoader
    {
        protected List<AssemblyDetail> _hostAssemblies = null;

        public AssembliesLoader(List<AssemblyDetail> hostAssemblies)
        {
            _hostAssemblies = hostAssemblies?.OrderBy(asm => asm.Name).ThenByDescending(asm => asm.Version).ToList();
        }

        public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Code running under new child AppDomain (IsolatedAppDomainWrapper) doesn't hit here (it's not attached to Debugger), requires this manual attaching?
            //if (sender is AppDomain && ((AppDomain)sender).FriendlyName.StartsWith("IsolatedAppDomainWrapper") && !System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            // Looks like Newtonsoft dependency never hits here but yet the whole process crashes if it's not embedded

            var name = new AssemblyName(args.Name);

            // First we check in the same folder of Extension (assemblies packed together)
            // Libraries like System.Reflection.Metadata.dll and System.Memory.dll are required (in specific versions) by Microsoft.CodeAnalysis 4.2.0 (Roslyn) 
            string dll = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + name.Name + ".dll";
            if (File.Exists(dll))
                return Assembly.LoadFrom(dll);

            // Then we check in the host process (Visual Studio)
            if (_hostAssemblies != null /*&& name.Name.StartsWith("Microsoft.VisualStudio")*/)
            {
                var match =
                    _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                    _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                    _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                    _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                    _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name);

                if (match == null && (name.Name.EndsWith(".15.0") || name.Name.EndsWith(".16.0") || name.Name.EndsWith(".17.0")))
                {
                    name.Name = name.Name.Substring(0, name.Name.Length - 5);
                    match =
                        _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                        _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                        _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                        _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                        _hostAssemblies.FirstOrDefault(asm => asm.Name == name.Name);
                }
                
                if (match != null)
                    return Assembly.LoadFrom(match.Location);
            }

            // For the full .net framework (including VS Extension) this is mscorlib 4.0.0.0
            // For .net core this is System.Private.CoreLib (under C:\Program Files\dotnet\shared\Microsoft.NETCore.App\)
            if (name.Name.Contains("CoreLib"))
                return typeof(object).GetTypeInfo().Assembly;

            //TODO: Preload the locations of all available dlls in all _hostAssemblies folders? Or maybe only VS folders
            return null;

            // Enable Managed Debugging Assistant 'LoadFromContext'
            // 'The assembly named '...' was loaded from '...' using the LoadFrom context.
            // The use of this context can result in unexpected behavior for serialization, casting and dependency resolution.
            // In almost all cases, it is recommended that the LoadFrom context be avoided.
            // This can be done by installing assemblies in the Global Assembly Cache or in the ApplicationBase directory and using Assembly.Load when explicitly loading assemblies.'
            // TODO: Install assemblies in GAC and use Assembly.Load(name)
        }
    }
    [Serializable]
    [DebuggerDisplay("{Name,nq}")]
    public class AssemblyDetail
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public string Location { get; set; }
    }

    public class AssemblyLoaderInitialization
    {
        private static bool _initialized = false;
        public static void Initialize()
        {
            if (_initialized) return;

            // Host AppDomain (VisualStudio) may need to resolve libraries embedded in our extension

#if VS2019_OR_OLDER
            // And also (since we have compatibility edition) we may need to redirect some loads like "Microsoft.VisualStudio.TextTemplating.VSHost.15.0" 16.x to "whatever is available" (17.x in 2022)
            // All assemblies loaded by Visual Studio should be available to child AppDomain
            var hostAssemblies = GetCurrentAssemblies();
            var loader = new AssembliesLoader(hostAssemblies);
#else
            var loader = new AssembliesLoader(null);
#endif
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(loader.AssemblyResolve);

            _initialized = true;

        }
        public static List<AssemblyDetail> GetCurrentAssemblies()
        {
            var currentAssemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                .Where(asm => !asm.IsDynamic)
                .Select(asm => new AssemblyDetail()
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