using System;
using System.Collections.Generic;

namespace Blox.Utility
{
    public static class CommonUtilities
    {
        public static void Iterate<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action.Invoke(item);
        }

        public static T Find<T>(this IEnumerable<T> collection, Func<T, bool> function)
        {
            foreach (var item in collection)
            {
                if (function.Invoke(item))
                    return item;
            }
            return default;
        }
    }
}