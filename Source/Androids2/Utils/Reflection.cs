using System;
using System.Linq;
using System.Reflection;

namespace Androids2
{
    public static class ReflectionUtility
    {
        public static object CloneObjectShallowly(this object sourceObject, params string[] excludedFields)
        {
            if (sourceObject == null) return null;

            Type type = sourceObject.GetType();
            if (type.IsAbstract) return null;

            if (type.IsPrimitive || type.IsValueType || type.IsArray || type == typeof(string))
                return sourceObject;

            object obj = Activator.CreateInstance(type);
            if (obj == null) return null;

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsLiteral) continue; // Skip constants

                // Skip excluded fields by name
                if (excludedFields.Contains(fieldInfo.Name)) continue;

                object value = fieldInfo.GetValue(sourceObject);
                fieldInfo.SetValue(obj, value);
            }

            return obj;
        }
    }
}