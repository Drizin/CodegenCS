using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static CodegenCS.Utils.TypeUtils;

namespace CodegenCS.Utils
{
    /// <summary>
    /// Dependency Injection Container which can create types (in our case mostly for Templates) 
    /// and will automatically resolve (inject) ICodegenContext, CodegenOutputFile, and any other registered dependency.
    /// </summary>
    public class DependencyContainer
    {
        private readonly Dictionary<Type, Func<object>> regs = new Dictionary<Type, Func<object>>();
        private readonly List<ITypeResolver> _customTypeResolvers = new List<ITypeResolver>();
        public DependencyContainer ParentContainer { get; set; } = null; // TODO: replace this by Autofac scopes
        public DependencyContainer() 
        {
        }
        public DependencyContainer(DependencyContainer parentContainer)
        {
            ParentContainer = parentContainer;
        }

        public T Resolve<T>(params object[] otherDependencies)
        {
            return (T)Resolve(typeof(T), otherDependencies);
        }

        public object Resolve(Type type, params object[] otherDependencies)
        {
            // If type is registered (either as singleton or per instance)
            if (TryGetValue(type, out object value)) 
                return value;

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
                var underlyingTypeInstance = Resolve(Nullable.GetUnderlyingType(type), otherDependencies);
                if (underlyingTypeInstance != null)
                    return underlyingTypeInstance;
            }
            if (type.IsAbstract)
                throw new InvalidOperationException("Can't create abstract type " + type);

            if (IsSimpleType(type))
                throw new InvalidOperationException("Can't create simple type " + type);
            if (type.IsArray && IsSimpleType(type.GetElementType()))
                throw new InvalidOperationException("Can't create simple type " + type);

            // Prioritize the constructor with more parameters
            var ctors = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).ToList();
            Dictionary<ConstructorInfo, object[]> ctorToArgs = new Dictionary<ConstructorInfo, object[]>();
            Dictionary<ConstructorInfo, decimal> ctorToWeight = new Dictionary<ConstructorInfo, decimal>();

            foreach(var ctor in ctors)
            {
                var parmInfos = ctor.GetParameters();
                var objects = new object[parmInfos.Length];
                ctorToArgs.Add(ctor, objects);
                decimal weight = 0;
                for (int i = 0; i < parmInfos.Length; i++)
                {
                    var parmInfo = parmInfos[i];

                    if (TryGetValue(parmInfo.ParameterType, out object value))
                        objects[i] = value;
                    else if (parmInfo.HasDefaultValue) // not registered but has a default value (might be null)
                        objects[i] = parmInfo.DefaultValue;
                    else
                        try
                        {
                            objects[i] = Resolve(parmInfo.ParameterType, otherDependencies); // try to create (result might also be null)
                        }
                        catch (Exception)
                        {
                            // primitive types won't be created
                        }
                    if (objects[i] != null)
                        weight++;
                    // Complex/custom types are slightly more preferrable than simple types.
                    if (!IsSimpleType(parmInfo.ParameterType) && !(parmInfo.ParameterType.IsArray && IsSimpleType(parmInfo.ParameterType.GetElementType())))
                        weight += 0.2m;
                }
                ctorToWeight.Add(ctor, weight);
            }
            foreach (var ctor in ctorToWeight.OrderByDescending(c => c.Value).Select(c => c.Key))
            {
                var args = ctorToArgs[ctor];
                try
                {
                    return ctor.Invoke(args);
                }
                catch (Exception)
                {
                    // if this constructor failed, try next one
                }
            }
            

            return null;
        }

        /// <summary>
        /// Tries to resolve the given type in this dependency container and in parent scopes.
        /// </summary>
        protected bool TryGetValue(Type type, out object value)
        {
            var scope = this;
            while (scope != null)
            {
                if (scope.regs.TryGetValue(type, out Func<object> fac))
                {
                    value = fac();
                    return true;
                }
                var customResolver = scope._customTypeResolvers.FirstOrDefault(r => r.CanResolveType(type));
                if (customResolver != null && customResolver.TryResolveType(type, this, out value))
                {
                    return true;
                }
                scope = scope.ParentContainer;
            }
            value = null;
            return false;
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
        /// Singleton registration for cases when the instance is already created.
        /// </summary>
        public void RegisterSingleton(Type serviceType, object instance) => regs.Add(serviceType, () => instance);

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

        public void RegisterCustomTypeResolver(ITypeResolver typeResolver)
        {
            _customTypeResolvers.Add(typeResolver);
        }


        /// <summary>
        /// Type resolvers are more powerful because they can be used to TEST if a given type should be resolved by this resolver,
        /// so as an example it can be used to build multiple different types that implement a given interface
        /// </summary>
        public interface ITypeResolver
        {
            bool CanResolveType(Type targetType);
            bool TryResolveType(Type targetType, DependencyContainer dependencyContainer, out object value);
        }

    }
}
