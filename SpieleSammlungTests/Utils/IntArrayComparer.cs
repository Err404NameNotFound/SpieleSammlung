#region

using System.Collections.Generic;
using SpieleSammlung.Model.Kniffel;

#endregion

namespace SpieleSammlungTests.Utils;

public class IntArrayComparer : IEqualityComparer<int[]>
{
    public bool Equals(int[] a, int[] b)
    {
        if (a == null)
        {
            return b == null;
        }

        if (b == null || a.Length != b.Length)
        {
            return false;
        }

        int i = 0;
        while (i < a.Length && a[i] == b[i])
        {
            ++i;
        }

        return i == a.Length;
    }

    public int GetHashCode(int[] obj)
    {
        int sum = 0;
        const int factor = Dice.HIGHEST_VALUE - Dice.LOWEST_VALUE + 1;
        for (int i = obj.Length - 1; i >= 0; --i)
        {
            sum = (sum + obj[i] - Dice.LOWEST_VALUE) * factor;
        }

        return sum;
    }
}