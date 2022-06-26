using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenCS
{
    /// <summary>
    /// Dependency Injection Container which can create types (in our case mostly for Templates) 
    /// and will automatically resolve (inject) ICodegenContext, CodegenOutputFile, and any other registered dependency.
    /// </summary>
    /// 
    public class DependencyContainer
    {
        private readonly Dictionary<Type, Func<object>> regs = new Dictionary<Type, Func<object>>();

        public object Resolve(Type type, params object[] otherDependencies)
        {
            // If type is registered (either as singleton or per instance)
            if (regs.TryGetValue(type, out Func<object> fac)) return fac();

            // Else try to create..
            var obj = CreateInstance(type, otherDependencies);
            if (obj == null)
                throw new InvalidOperationException($"{type} is not registered and couldn't be created");
            return obj;
        }

        protected object CreateInstance(Type type, params object[] otherDependencies)
        {
            if (type.IsValueType && Nullable.GetUnderlyingType(type) == null) // primitive non-nullable types  - use the default value
                return Activator.CreateInstance(type);

            // Check if any dependencies (like models required by a template) were explicitly provided as otherDependencies.
            foreach (var arg in otherDependencies)
                if (type.IsAssignableFrom(arg.GetType()))
                    return arg;

            if (Nullable.GetUnderlyingType(type) != null)
            {
                var underlyingTypeInstance = CreateInstance(Nullable.GetUnderlyingType(type), otherDependencies);
                if (underlyingTypeInstance != null)
                    return underlyingTypeInstance;
            }
            if (type.IsAbstract)
                throw new InvalidOperationException("Can't create abstract type " + type);

            
            // Prioritize the constructor with more parameters
            var ctors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).ToList();

            for (int ci = 0; ci < ctors.Count(); ci++)
            {
                var ctor = ctors[ci];
                var parmInfos = ctor.GetParameters();
                var obj = new object[parmInfos.Length];
                for (int i = 0; i < parmInfos.Length; i++)
                {
                    var parmInfo = parmInfos[i];

                    if (regs.TryGetValue(parmInfo.ParameterType, out Func<object> fac)) // type is registered
                        obj[i] = fac();
                    else if (parmInfo.HasDefaultValue) // not registered but has a default value (might be null)
                        obj[i] = parmInfo.DefaultValue;
                    else
                        obj[i] = CreateInstance(parmInfo.ParameterType, otherDependencies); // try to create (result might also be null)
                }
                try
                {
                    return Activator.CreateInstance(type, obj);
                }
                catch (Exception ex)
                {
                    // if this constructor failed, try next one
                    if (ci < ctors.Count - 1)
                        continue;
                }
            }
            return null;
        }

        /// <summary>
        /// Transient means that a new instance is always created
        /// </summary>
        public void RegisterTransient<TService, TImpl>() where TImpl : TService => regs.Add(typeof(TService), () => this.Resolve(typeof(TImpl)));

        /// <summary>
        /// Transient means that a new instance is always created
        /// </summary>
        public void RegisterTransient<TService>() => regs.Add(typeof(TService), () => this.Resolve(typeof(TService)));

        /// <summary>
        /// Transient objects are always different (new instance is always created)
        /// </summary>
        public void RegisterTransient<TService>(Func<TService> factory) => regs.Add(typeof(TService), () => factory());

        /// <summary>
        /// Singleton registration for cases when the instance is already created.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="instance"></param>
        public void RegisterSingleton<TService>(TService instance) => regs.Add(typeof(TService), () => instance);

        /// <summary>
        /// Singleton registration for cases when the instance is not yet created or might need other dependencies.
        /// The factory is wrapped under a Lazy wrapper so it's possible that the service might not even be created if not required.
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="factory"></param>
        public void RegisterSingleton<TService>(Func<TService> factory)
        {
            var lazy = new Lazy<TService>(factory);
            regs.Add(typeof(TService), () => lazy.Value); // exactly like RegisterTransient, but the lambda will always return the same lazy instance.
        }

    }
}
