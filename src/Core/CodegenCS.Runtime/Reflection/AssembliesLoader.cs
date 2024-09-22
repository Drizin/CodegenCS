using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CodegenCS.Runtime.Reflection
{
    [Serializable]
    public class AssembliesLoader
    {
        protected List<LoadableAssemblyInfo> _loadableAssemblies = null;
        protected List<LoadedAssemblyInfo> _loadedAssemblies = null;
        protected List<string> _searchPaths = null;

        public AssembliesLoader(List<LoadedAssemblyInfo> loadedAssemblies, List<LoadableAssemblyInfo> loadableAssemblies, List<string> searchPaths)
        {
            _loadedAssemblies = loadedAssemblies?.OrderBy(asm => asm.Name).ThenByDescending(asm => asm.Version).ToList();
            _loadableAssemblies = loadableAssemblies?.OrderBy(asm => asm.Name).ThenByDescending(asm => asm.Version).ToList();
            _searchPaths = searchPaths;
        }

        public Assembly AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Code running under new child AppDomain (IsolatedAppDomainWrapper) doesn't hit here (it's not attached to Debugger), requires this manual attaching?
            //if (sender is AppDomain && ((AppDomain)sender).FriendlyName.StartsWith("IsolatedAppDomainWrapper") && !System.Diagnostics.Debugger.IsAttached)
            //    System.Diagnostics.Debugger.Launch();
            // Looks like Newtonsoft dependency never hits here but yet the whole process crashes if it's not embedded

            var name = new AssemblyName(args.Name);

            if (_loadedAssemblies != null)
            {
                var match =
                    _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                    _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                    _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                    _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                    _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name);

                if (match == null && (name.Name.EndsWith(".15.0") || name.Name.EndsWith(".16.0") || name.Name.EndsWith(".17.0")))
                {
                    name.Name = name.Name.Substring(0, name.Name.Length - 5);
                    match =
                        _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                        _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                        _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                        _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                        _loadedAssemblies.FirstOrDefault(asm => asm.Name == name.Name);
                }

                if (match != null)
                    return match.Assembly;
            }

            if (_loadableAssemblies != null)
            {
                var match =
                    _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                    _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                    _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                    _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                    _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name);

                if (match == null && (name.Name.EndsWith(".15.0") || name.Name.EndsWith(".16.0") || name.Name.EndsWith(".17.0")))
                {
                    name.Name = name.Name.Substring(0, name.Name.Length - 5);
                    match =
                        _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version == name.Version) ??
                        _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor == name.Version.Minor) ??
                        _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major == name.Version.Major && asm.Version.Minor >= name.Version.Minor) ??
                        _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name && asm.Version.Major >= name.Version.Major) ??
                        _loadableAssemblies.FirstOrDefault(asm => asm.Name == name.Name);
                }

                if (match != null)
                    return Assembly.LoadFrom(match.Location);
            }

            // For the full .net framework (including VS Extension) this is mscorlib 4.0.0.0
            // For .net core this is System.Private.CoreLib (under C:\Program Files\dotnet\shared\Microsoft.NETCore.App\)
            if (name.Name.Contains("CoreLib"))
                return typeof(object).GetTypeInfo().Assembly;

            if (_searchPaths != null)
            {
                foreach (var path in _searchPaths)
                {
                    string dll = Path.Combine(path, name.Name + ".dll");
                    if (File.Exists(dll))
                        return Assembly.LoadFrom(dll);
                }
            }
            // Then we check in the same folder of Extension (assemblies packed together)
            // Libraries like System.Reflection.Metadata.dll and System.Memory.dll are required (in specific versions) by Microsoft.CodeAnalysis 4.2.0 (Roslyn) 

            return null;

            // Enable Managed Debugging Assistant 'LoadFromContext'
            // 'The assembly named '...' was loaded from '...' using the LoadFrom context.
            // The use of this context can result in unexpected behavior for serialization, casting and dependency resolution.
            // In almost all cases, it is recommended that the LoadFrom context be avoided.
            // This can be done by installing assemblies in the Global Assembly Cache or in the ApplicationBase directory and using Assembly.Load when explicitly loading assemblies.'
            // TODO: Install assemblies in GAC and use Assembly.Load(name)
        }
    }

    /// <summary>
    /// Assemblies that are already loaded and should just be redirected
    /// </summary>
    [DebuggerDisplay("{Name,nq}")]
    public class LoadedAssemblyInfo
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public Assembly Assembly { get; set; }
    }

    /// <summary>
    /// Assemblies that should be loaded by their paths
    /// </summary>
    [Serializable]
    [DebuggerDisplay("{Name,nq}")]
    public class LoadableAssemblyInfo
    {
        public string Name { get; set; }
        public Version Version { get; set; }
        public string Location { get; set; }
    }

}
