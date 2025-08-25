using System;
using System.Reflection;

namespace Androids2
{
    public static class ReflectionUtility
    {
        public static object CloneObjectShallowly(this object sourceObject)
        {
            if (sourceObject == null)
            {
                return null;
            }

            Type type = sourceObject.GetType();
            if (type.IsAbstract)
            {
                return null;
            }

            if (type.IsPrimitive || type.IsValueType || type.IsArray || type == typeof(string))
            {
                return sourceObject;
            }

            object obj = Activator.CreateInstance(type);
            if (obj == null)
            {
                return null;
            }

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (!fieldInfo.IsLiteral)
                {
                    object value = fieldInfo.GetValue(sourceObject);
                    fieldInfo.SetValue(obj, value);
                }
            }

            return obj;
        }
    }
}