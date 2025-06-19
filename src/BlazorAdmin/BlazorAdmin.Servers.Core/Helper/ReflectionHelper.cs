using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorAdmin.Core.Helper
{
    public static class ReflectionHelper
    {
        public static int GetNonNullPropertyCount(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            int count = 0;
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties(
                BindingFlags.DeclaredOnly |
                BindingFlags.Public | 
                BindingFlags.Instance);


            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                if (value != null && !IsEmpty(value))
                {
                    count++;
                }
            }

            return count;
        }

        private static bool IsEmpty(object value)
        {
            if (value == null)
                return true;

            var type = value.GetType();

            if (type.IsArray)
            {
                var asArray = (Array)value;
                return asArray.Length == 0;
            }

            if (typeof(System.Collections.ICollection).IsAssignableFrom(type))
            {
                var asICollection = (System.Collections.ICollection)value;
                return asICollection.Count == 0;
            }

            if (type == typeof(string))
            {
                return string.IsNullOrEmpty((string)value);
            }

            return false;
        }
    }
}
