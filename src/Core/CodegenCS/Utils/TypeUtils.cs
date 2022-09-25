using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodegenCS.Utils
{
    public class TypeUtils
    {
        /// <summary>
        /// Determines whether the current type can be assigned to a variable of the specified Generic type targetType.
        /// </summary>
        public static bool IsAssignableToGenericType(Type currentType, Type targetType)
        {
            var interfaceTypes = currentType.GetInterfaces();

            foreach (var it in interfaceTypes)
            {
                if (it.IsGenericType && it.GetGenericTypeDefinition() == targetType)
                    return true;
            }

            if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == targetType)
                return true;

            Type baseType = currentType.BaseType;
            if (baseType == null) return false;

            return IsAssignableToGenericType(baseType, targetType);
        }

        public static bool IsInstanceOfGenericType(object instance, Type genericType)
        {
            Type type = instance.GetType();
            return IsAssignableToGenericType(type, genericType);
        }

        /// <summary>
        /// Determines whether the current type can be assigned to a variable of the specified targetType (even if targetType is generic).
        /// </summary>
        public static bool IsAssignableToType(Type currentType, Type targetType)
        {
            if (targetType.IsGenericType)
                return IsAssignableToGenericType(currentType, targetType);

            return targetType.IsAssignableFrom(currentType);
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


    }
}
