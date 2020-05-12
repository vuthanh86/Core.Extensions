using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

//using Wire.Extensions;

namespace NetCore.Extensions.Core.Extensions
{
    public static class ReflectionExtensions
    {
        public static string FullName(this MethodInfo method)
        {
            var arg = string.Join(".", method.GetParameters().Select(o => o.ParameterType.Name).ToArray());
            return string.IsNullOrEmpty(arg) ? method.Name : $"{method.Name}({arg})";
        }

        public static object GetValue<T>(this T obj, string parameter)
        {
            var p = typeof (T).GetProperty(parameter);

            return p?.GetMethod == null ? null : p.GetValue(obj);
        }

        public static Func<T, bool> And<T>(this Func<T, bool> func1, Func<T, bool> func2)
        {
            return h => func1(h) && func2(h);
        }

        public static IEnumerable<FieldInfo> GetFieldInfos(this Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(fi => (fi.Attributes & FieldAttributes.NotSerialized) == 0)
                .OrderBy(f => f.Name, StringComparer.Ordinal);

            if (type.GetTypeInfo().BaseType == null)
            {
                return fields;
            }
            else
            {
                var baseFields = GetFieldInfos(type.GetTypeInfo().BaseType);
                return baseFields.Concat(fields);
            }
        }

        //public static IEnumerable<Type> GetAllSubTypes(this Type type)
        //{
        //    var all = new HashSet<Type>();
        //    var stack = new Stack<Type>(new []{type});

        //    while (stack.Count>0)
        //    {
        //        var t = stack.Pop();
        //        if (all.Contains(t))
        //            continue;

        //        var fields = t.GetFieldInfosForType();

        //        foreach (var f in fields)
        //        {
        //            stack.Push(f.FieldType);
        //        }

        //        all.Add(t);
        //    }

        //    return all;
        //}

        public static IEnumerable<PropertyInfo> GetPropertyInfos(this Type type)
        {
            var fields = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(f => f.Name, StringComparer.Ordinal);

            if (type.GetTypeInfo().BaseType == null)
            {
                return fields;
            }
            else
            {
                var baseFields = GetPropertyInfos(type.GetTypeInfo().BaseType);
                return baseFields.Concat(fields);
            }
        }

        public static string GetPropertyName<T, TP>(this Expression<Func<T, TP>> action)
        {
            var expression = action.Body as MemberExpression;
            return expression?.Member.Name;
        }

        public static Expression<Action<T, TP>> Setter<T, TP>(this Expression<Func<T, TP>> getter)
        {
            var pName = getter.GetPropertyName();

            var xpe = Expression.Parameter(typeof(T), "x");
            var ppe = Expression.Parameter(typeof(TP), "p");

            var left = Expression.Property(xpe, pName);
            var lambda = Expression.Assign(left, ppe);
            var expression = Expression.Lambda<Action<T, TP>>(lambda, xpe, ppe);
            return expression;
        }
    }
}
