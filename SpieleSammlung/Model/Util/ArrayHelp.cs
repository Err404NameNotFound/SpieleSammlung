#region

using System;
using System.Collections.Generic;

#endregion

namespace SpieleSammlung.Model.Util;

public static class ArrayHelp
{
    public static T[] CreateArray<T>(int length, T value)
    {
        T[] array = new T[length];
        for (int i = 0; i < length; ++i)
            array[i] = value;
        return array;
    }

    public static T[] CreateArray<T>(int length, Func<T> generator)
    {
        T[] array = new T[length];
        for (int i = 0; i < length; ++i)
            array[i] = generator();
        return array;
    }

    public static T[] CreateArray<TS, T>(IReadOnlyCollection<TS> original, Func<TS, T> generator)
    {
        T[] array = new T[original.Count];
        int i = 0;
        foreach (TS item in original)
            array[i++] = generator(item);
        return array;
    }
}