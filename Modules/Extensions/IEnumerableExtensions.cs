using System;
using System.Collections.Generic;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> predicate)
    {
        foreach (var item in source)
        {
            predicate(item);
        }

        return source;
    }
}
