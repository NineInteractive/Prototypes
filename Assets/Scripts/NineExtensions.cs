using System;
using System.Collections.Generic;

public static class Extensions
{
    private static Random random = new Random();

    public static T GetRandomElement<T>(this HashSet<T> set)
    {
        // If there are no elements in the collection, return the default value of T
        T[] array = new T[set.Count];
        set.CopyTo(array);

        if (array.Length == 0)
            return default(T);

        return array[random.Next(array.Length)];
    }
}
