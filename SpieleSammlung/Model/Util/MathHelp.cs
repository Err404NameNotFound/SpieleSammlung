#region

using System;
using System.Linq;

#endregion

namespace SpieleSammlung.Model.Util;

public static class MathHelp
{
    public static double Log(double value, double baseOfLog) => Math.Log(value) / Math.Log(baseOfLog);

    public static long Max(Func<int, long> array, int length)
    {
        long max = array(0);
        for (int i = 1; i < length; ++i)
        {
            long next = array(i);
            if (next > max)
                max = next;
        }

        return max;
    }

    public static long Max(params long[][] arrays) => arrays.Max(array => array.Max());
}