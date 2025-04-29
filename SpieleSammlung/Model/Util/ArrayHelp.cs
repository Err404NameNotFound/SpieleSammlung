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
}