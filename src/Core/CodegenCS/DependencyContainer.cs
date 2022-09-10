using System;
using System.Collections.Generic;
using System.CommandLine.Binding;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Reflection;
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

        public T Resolve<T>(params object[] otherDependencies)
        {
            return (T)Resolve(typeof(T), otherDependencies);
        }

        public object Resolve(Type type, params object[] otherDependencies)
        {
            // If type is registered (either as singleton or per instance)
            if (regs.TryGetValue(type, out Func<object> fac)) 
                return fac();

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

            if (IsSimpleType(type))
                throw new InvalidOperationException("Can't create simple type " + type);
            if (type.IsArray && IsSimpleType(type.GetElementType()))
                throw new InvalidOperationException("Can't create simple type " + type);


            if (typeof(IAutoBindCommandLineArgs).IsAssignableFrom(type))
            {
                var binder = new ModelBinder(type);
                try
                {
                    BindingContext bindingContext = null;
                    if (regs.TryGetValue(typeof(BindingContext), out Func<object> fac))
                        bindingContext = (BindingContext)fac();
                    if (bindingContext != null)
                    {
                        // create an instance of MyTemplateArgs based on the parsed command line
                        var instance = binder.CreateInstance(bindingContext);
                        return instance;
                    }
                }
                catch { }
            }


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

                    if (regs.TryGetValue(parmInfo.ParameterType, out Func<object> fac)) // type is registered
                        objects[i] = fac();
                    else if (parmInfo.HasDefaultValue) // not registered but has a default value (might be null)
                        objects[i] = parmInfo.DefaultValue;
                    else
                        try
                        {
                            objects[i] = CreateInstance(parmInfo.ParameterType, otherDependencies); // try to create (result might also be null)
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

        public static bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                new Type[] {
            typeof(string),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(Guid)
                }.Contains(type) ||
                type.IsEnum ||
                Convert.GetTypeCode(type) != TypeCode.Object ||
                (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && IsSimpleType(type.GetGenericArguments()[0]));
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
