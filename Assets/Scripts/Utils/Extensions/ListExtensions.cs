using System;
using System.Collections.Generic;
using System.Linq;

namespace GameJam
{
    public static class ListExtensions
    {
        public static void Push<T>(this IList<T> list, T item)
        {
            list.Insert(0, item);
        }
        public static T Pop<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                T item = list[0];
                list.RemoveAt(0);
                return item;
            }
            else
                return default;
        }
        public static T PopLast<T>(this IList<T> list)
        {
            if (list.Count > 0)
            {
                T item = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                return item;
            }
            else
                return default;
        }

        // find all duplicates in a list
        // note: this is only called once on start, so Linq is fine here!
        public static List<U> FindDuplicates<T, U>(this List<T> list, Func<T, U> keySelector)
        {
            return list.GroupBy(keySelector)
                       .Where(group => group.Count() > 1)
                       .Select(group => group.Key).ToList();
        }

        // check if a list has duplicates
        // new List<int>(){1, 2, 2, 3}.HasDuplicates() => true
        // new List<int>(){1, 2, 3, 4}.HasDuplicates() => false
        // new List<int>().HasDuplicates() => false
        public static bool HasDuplicates<T>(this List<T> list)
        {
            return list.Count != list.Distinct().Count();
        }
    }
}