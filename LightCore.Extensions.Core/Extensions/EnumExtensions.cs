using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NetCore.Extensions.Core.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType().GetMember(enumValue.ToString())
                    .First()
                    .GetCustomAttribute<DisplayAttribute>()
                    .Name;
        }

        public static T ToEnum<T>(this string str) where T : struct
        {
            return (T) Enum.Parse(typeof (T), str);
        }

        public static T ToEnum<T>(this string str, bool ignoreCase) where T : struct
        {
            return (T) Enum.Parse(typeof (T), str, ignoreCase);
        }

        public static T[] GetValues<T>()
        {
            return (T[]) Enum.GetValues(typeof (T));
        }

        public static T Parse<T>(string value, bool ignoreCase = true)
        {
            return (T) Enum.Parse(typeof (T), value, ignoreCase);
        }

        public static IEnumerable<T> GetFlags<T>(this Enum value)
        {
            if (value == null)
            {
                return Enumerable.Empty<T>();
            }

            return GetFlags(value, Enum.GetValues(value.GetType()).Cast<T>().ToArray());
        }

        public static IEnumerable<T> GetIndividualFlags<T>(this Enum value) where T : struct
        {
            if (value == null)
            {
                return Enumerable.Empty<T>();
            }

            return GetFlags(value, GetFlagValues<T>(value.GetType()).ToArray());
        }

        private static IEnumerable<T> GetFlags<T>(Enum value, T[] values)
        {
            ulong bits = Convert.ToUInt64(value);
            var results = new List<T>();
            for (int i = values.Length - 1; i >= 0; i--)
            {
                ulong mask = Convert.ToUInt64(values[i]);
                if (i == 0 && mask == 0L)
                    break;
                if ((bits & mask) == mask)
                {
                    results.Add(values[i]);
                    bits -= mask;
                }
            }
            if (bits != 0L)
                return Enumerable.Empty<T>();
            if (Convert.ToUInt64(value) != 0L)
                return results.Reverse<T>();
            if (bits == Convert.ToUInt64(value) && values.Length > 0 && Convert.ToUInt64(values[0]) == 0L)
                return values.Take(1);
            return Enumerable.Empty<T>();
        }

        public static IEnumerable<T> GetFlagValues<T>(this Type enumType) where T : struct
        {
            ulong flag = 0x1;
            foreach (var value in Enum.GetValues(enumType).Cast<T>())
            {
                ulong bits = Convert.ToUInt64(value);
                if (bits == 0L)
                    //yield return value;
                    continue; // skip the zero value
                while (flag < bits) flag <<= 1;
                if (flag == bits)
                    yield return value;
            }
        }

        public static bool TryMerge<T>(this T[] enums, out T @enum) where T : struct
        {
            if (typeof (T).GetTypeInfo().IsEnum && enums.Length > 0)
            {
                @enum = (T) MergeEnum(enums.Cast<int>().ToArray());
                return true;
            }

            @enum = default(T);
            return false;
        }

        public static T Merge<T>(this IEnumerable<T> enums) where T : struct
        {
            if (typeof (T).GetTypeInfo().IsEnum && enums.Any())
            {
                return MergeEnum<T>(enums.Cast<Enum>().ToArray());
            }
            return default(T);
        }

        private static T MergeEnum<T>(Enum[] enums)
        {
            if (enums == null || enums.Length == 0)
                throw new InvalidDataException("Enums is null for empty");

            int v = Convert.ToInt32(enums[0]);
            for (var i = 0; i < enums.Length; i++)
            {
                v = v | Convert.ToInt32(enums[i]);
            }
            return (T) Enum.ToObject(enums[0].GetType(), v);
        }

        private static object MergeEnum(int[] enums)
        {
            var v = enums[0];
            for (var i = 0; i < enums.Length; i++)
            {
                v = v | enums[i];
            }
            return v;
        }

        public static bool Is<T>(this T a, T b) where T : struct
        {
            if (typeof (T).GetTypeInfo().IsEnum)
            {
                return Is(a as Enum, b as Enum);
            }
            return false;
        }

        private static bool Is(Enum a, Enum b)
        {
            return BitContains(Convert.ToInt32(a), Convert.ToInt32(b));
        }

        private static bool BitContains(int a, int b)
        {
            return (a & b) == b;
        }

        public static T Find<T>(string name, T @default)
        {
            if (string.IsNullOrEmpty(name)) return @default;

            var remarks = Enum.GetValues(typeof (T)).Cast<T>();
            foreach (var r in remarks)
            {
                var n = Enum.GetName(typeof (T), r);
                if (n == name)
                    return r;
            }
            return @default;
        }

        public static IEnumerable<IGrouping<int, T>> Batches<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            var g = 0;
            var group = new SimpleGrouping<int, T>(g);
            foreach (var item in enumerable)
            {
                if (group.Count >= batchSize)
                {
                    yield return group;

                    g++;
                    group = new SimpleGrouping<int, T>(g);
                }

                group.Add(item);
            }
            yield return group;
        }
    }

    public class SimpleGrouping<TGroup, TValue> : List<TValue>, IGrouping<TGroup, TValue>
    {
        public SimpleGrouping(TGroup group)
        {
            Key = group;
        }

        public SimpleGrouping(TGroup group, IEnumerable<TValue> collection) : base(collection)
        {
            Key = group;
        }

        public SimpleGrouping(TGroup group, int capacity) : base(capacity)
        {
            Key = group;
        }

        public TGroup Key { get; }
    }
}
