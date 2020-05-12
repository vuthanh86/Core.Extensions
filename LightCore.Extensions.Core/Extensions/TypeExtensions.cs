﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NetCore.Extensions.Core.Extensions
{
    public static class TypeExtensions
    {
        public static bool IsCollection(this Type type)
        {
            // string implements IEnumerable, but for our purposes we don't consider it a collection.
            if (type == typeof(string)) return false;

            var interfaces = from inf in type.GetTypeInfo().GetInterfaces()
                             where inf == typeof(IEnumerable) ||
                                 (inf.GetTypeInfo().IsGenericType && inf.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                             select inf;
            return interfaces.Count() != 0;
        }

        public static bool IsNullable(this Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type.GetTypeInfo().IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }
    }
}
