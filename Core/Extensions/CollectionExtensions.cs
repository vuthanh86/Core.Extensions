using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Extensions
{
    public static class CollectionExtensions
    {
        /// <summary>
        /// Modifies a collection in-place by removing items from the collection that match
        /// a given <see cref="T:Predicate[T]"/>.
        /// </summary>
        /// <remarks>
        /// The type of collection passed in will affect how the method performs. For collections
        /// with a built-in method to remove in-place (such as sets) the existing implementation
        /// will be used. For collections implementing IList[T], the method will perform better
        /// because the collection can be enumerated more efficiently. For all other collections,
        /// the items to remove will be buffered and Remove will be called individually which,
        /// depending on the collection type, can be very slow resulting in an O(n) scan to
        /// determine the items to remove, then a separate O(n) scan for each item that matched.
        /// </remarks>
        /// <typeparam name="T">The type of item in the collection.</typeparam>
        /// <param name="collection">The collection to remove from.</param>
        /// <param name="match">The predicate that determines if the item will be removed.</param>
        /// <returns>The number of items removed.</returns>
        /// <example>
        /// <![CDATA[
        ///     var numbers = new Collection<int>() { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        ///     numbers.RemoveWhere( x => x % 2 == 0 );  // remove even numbers
        /// ]]>
        /// </example>
        public static int RemoveWhere<T>(this ICollection<T> collection, Predicate<T> match)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            if (collection.IsReadOnly) { throw new NotSupportedException("The collection is read-only."); }

            // Defer to existing implementation...
            var hashSetOfT = collection as HashSet<T>;
            if (hashSetOfT != null)
            {
                return hashSetOfT.RemoveWhere(match);
            }

            // Defer to existing implementation...
            var sortedSetOfT = collection as SortedSet<T>;
            if (sortedSetOfT != null)
            {
                return sortedSetOfT.RemoveWhere(match);
            }

            // Defer to existing implementation...
            var listOfT = collection as List<T>;
            if (listOfT != null)
            {
                return listOfT.RemoveAll(match);
            }

            // Have to use our own implementation.

            int removed = 0;

            // IList<T> is pretty efficient because we only have to enumerate
            // the list once and if a match, we remove at that position.
            // Enumerate backwards so that the indexes don't shift out from under us.
            var list = collection as IList<T>;
            if (list != null)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    T item = list[i];
                    if (match(item))
                    {
                        list.RemoveAt(i);
                        removed++;
                    }
                }

                return removed;
            }

            // For ICollection<T> it isn't as efficient because we have to first
            // buffer all the items to remove in a temporary collection.
            // Then we enumerate that temp collection removing each individually
            // from the ICollection<T> which could be potentially O(n).

            var itemsToRemove = new List<T>(from x in collection where match(x) select x);
            foreach (T item in itemsToRemove)
            {
                if (collection.Remove(item))
                {
                    removed++;
                }
            }

            return removed;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            return list == null || !list.Any();
        }

        public static T[] Append<T>(this T[] source, T[] destination)
        {
            var newArray = new T[source.Length + destination.Length];
            Array.Copy(source,newArray,source.Length);
            Array.Copy(destination, 0, newArray, source.Length, destination.Length);
            return newArray;
        }

        public static string GetOrEmpty<T>(this IDictionary<T, string> dictionary, T key)
        {
            string value;
            return dictionary.TryGetValue(key, out value) ? value : string.Empty;
        }

        public static TV GetOrNull<TK, TV>(this IDictionary<TK, TV> dictionary, TK key) where TV : class
        {
            TV value;
            return dictionary.TryGetValue(key, out value) ? value : default(TV);
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }

            return enumerable;
        }

        public static IEnumerable<T> Each<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (T item in enumerable)
            {
                action(item);
            }

            return enumerable;
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> list, Func<T, TKey> lookup)
        {
            return list.Distinct(new StructEqualityComparer<T, TKey>(lookup));
        }

        private class StructEqualityComparer<T, TKey> : IEqualityComparer<T>
        {
            private readonly Func<T, TKey> lookup;

            public StructEqualityComparer(Func<T, TKey> lookup)
            {
                this.lookup = lookup;
            }

            public bool Equals(T x, T y)
            {
                return lookup(x).Equals(lookup(y));
            }

            public int GetHashCode(T obj)
            {
                return lookup(obj).GetHashCode();
            }
        }

        public static void AddRange<T>(this IList<T> list, IEnumerable<T> enumerable)
        {
            foreach (var e in enumerable)
            {
                list.Add(e);
            }
        }

        public static string Join(this IEnumerable<string> enumerable, string seprator)
        {
            return string.Join(seprator, enumerable);
        }

        public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> prediction)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                if (prediction(item))
                    return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether this collection contains any of the specified values
        /// </summary>
        /// <typeparam name="T">The type of the values to compare</typeparam>
        /// <param name="t">This collection</param>
        /// <param name="items">The values to compare</param>
        /// <returns>true if the collection contains any of the specified values, otherwise false</returns>
        public static bool ContainsAny<T>(this IEnumerable<T> t, params T[] items)
        {
            return items.Any(t.Contains);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> t, IEnumerable<T> items)
        {
            return items.Any(t.Contains);
        }

        public static bool ContainsAll<T>(this IEnumerable<T> a, IEnumerable<T> b)
        {
            return !b.Except(a).Any();
        }

        public static IDictionary<TKey, TValue> SafeToDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        {
            var dic = new Dictionary<TKey, TValue>();

            foreach (var element in source)
            {
                var key = keySelector(element);
                dic[key] = valueSelector(element);
            }

            return dic;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                collection.Add(item);
            }
        }

        public static ICollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.ToList();
        }

        public static IList<T> AddIfNotExist<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }

            return list;
        }

        public static T GetOrAdd<T>(this IList<T> list,Func<T,bool> prediction, Func<T> func)
        {
            foreach (var item in list)
            {
                if (prediction(item))
                    return item;
            }

            var newItem = func();
            list.Add(newItem);

            return newItem;
        }

        public static void AddTo<T>(this T item, IList<T> list)
        {
            list.Add(item);
        }

        public static void AddTo<T>(this IEnumerable<T> item, IList<T> list)
        {
            list.AddRange(item);
        }

        public static IEnumerable<T> Join<T>(this IEnumerable<T> e1, IEnumerable<T> e2)
        {
            foreach (var item in e1)
            {
                yield return item;
            }

            foreach (var item in e2)
            {
                yield return item;
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> e)
        {
            foreach (var innerEnumerable in e)
            {
                foreach (var i in innerEnumerable)
                {
                    yield return i;
                }
            }
        }

        public static async Task<IEnumerable<TResult>> SelectAsync<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, Task<TResult>> selector)
        {
            var tasks = new List<Task<TResult>>();

            foreach (var item in enumerable)
            {
                tasks.Add(selector(item));
            }

            return await Task.WhenAll(tasks);
        }

        public static T[] Join<T>(this ICollection<T[]> arrays)
        {
            if(arrays == null)
                throw new ArgumentNullException(nameof(arrays));
            var total = arrays.Sum(arr => arr.Length);

            var @new = new T[total];
            var offset = 0;

            foreach(var arr in arrays)
            {
                Array.Copy(arr,0, @new,offset,arr.Length);
            }

            return @new;
        }
    }
}
