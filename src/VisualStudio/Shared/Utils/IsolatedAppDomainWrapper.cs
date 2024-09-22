using CodegenCS.Runtime.Reflection;
using System;
using System.IO;
using System.Reflection;

namespace CodegenCS.VisualStudio.Shared.Utils
{
    /// <summary>
    /// Wraps an isolated AppDomain, and allows the creation of objects inside the isolated domain, 
    /// while still communicating with external (parent) domain.
    /// This is important in old versions of Visual Studio because newer packages conflict with old IDE versions.
    /// Packages: Microsoft.CodeAnalysis.CSharp compiler and dependencies (System.Reflection.Metadata, System.Memory, System.Collections.Immutable, etc).
    /// </summary>
    [Serializable]
    internal class IsolatedAppDomainWrapper : IDisposable
    {
        protected AppDomain _domain;

        public IsolatedAppDomainWrapper()
        {
            var setup = new AppDomainSetup
            {
                //ApplicationBase = AppDomain.CurrentDomain.BaseDirectory, // BaseDirectory would be devenv.exe, we don't want that
                ApplicationBase = new FileInfo(typeof(IsolatedAppDomainWrapper).Assembly.Location).Directory.FullName,
                PrivateBinPath = "lib",
                TargetFrameworkName = ".NETFramework, Version=v4.7.2",
                DisallowBindingRedirects = true, // ignore redirects from host process (devenv.exe) 
            };

            var adevidence = AppDomain.CurrentDomain.Evidence;
            _domain = AppDomain.CreateDomain("IsolatedAppDomainWrapper" + Guid.NewGuid().ToString(), adevidence, setup);
        }

        protected AssembliesLoader _loader;
        public AssembliesLoader Loader
        {
            get { return _loader; }
            set
            {
                if (_loader != null && value != null) throw new ArgumentException("Loader already defined");
                _loader = value;
                _domain.AssemblyResolve += _loader.AssemblyResolve;
            }
        }

        /// <summary>
        /// Creates a new type in the isolated AppDomain and returns a Transparent Proxy so that this type can be accessed from the parent AppDomain.
        /// </summary>
        /// <typeparam name="T">Should implement MarshalByRefObject.</typeparam>
        /// <param name="ctorArgs"></param>
        /// <returns>Transparent Proxy that can forward commands across AppDomains.
        /// (and can also forward the responses as long as they are serializable or MarshalByRefObject)
        /// </returns>
        public T Create<T>(params object[] ctorArgs) where T : MarshalByRefObject
        {
            Type type = typeof(T);
            // CreateInstanceAndUnwrap might fail if the assembly resolution fails in the child AppDomain _domain (AssemblyResolve handler)
            // But if CreateInstanceAndUnwrap works and the cast fails (System.InvalidCastException: 'Unable to cast transparent proxy to type 'T')
            // then it's beacuse assembly resolution failed in the parent host AppDomain (Visual Studio) - that's why parent process should also
            // bind AssemblyResolve event to dynamically load the libraries embedded in the extension
            T _value = (T) _domain.CreateInstanceAndUnwrap(
                type.Assembly.FullName,
                     type.FullName,
                     true,
                     BindingFlags.Public | BindingFlags.Instance,
                     null,
                     ctorArgs,
                     null,
                     null);
            return _value;
            // Any [Serializable] class can also be created using CreateInstanceAndUnwrap,
            // but in this case the caller process would get a CLONE (deserialized) of the real object,
            // so this wrapper requires MarshalByRefObject to make it clear that the type is not a copy, but a Transparent Proxy to the real object
        }

        public void Dispose()
        {
            if (_domain != null)
            {
                AppDomain.Unload(_domain);
                _domain = null;
            }
        }
    }
}
