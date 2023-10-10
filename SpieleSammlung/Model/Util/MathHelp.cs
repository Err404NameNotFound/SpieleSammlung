using System;
using System.Collections.Generic;
using System.Linq;

namespace SpieleSammlung.Model.Util
{
    public static class MathHelp
    {
        public static double Log(double value, double baseOfLog)
        {
            return Math.Log(value) / Math.Log(baseOfLog);
        }

        public static long NlogN(long x)
        {
            return (long)Math.Round(x * Math.Log(x));
        }

        public static long NChooseK(int n, int k)
        {
            long sum = 1;
            for (int i = 1; i <= k; ++i)
            {
                sum *= n - i + 1;
                sum /= i;
            }

            return sum;
        }

        public static double nChooseK_double(int n, int k)
        {
            double sum = 1;
            for (int i = 1; i <= k; ++i)
            {
                sum *= n - i + 1;
                sum /= i;
            }

            return sum;
        }

        public static long Max(IEnumerable<long> array) => array.Max();

        public static long Max(Func<int, long> array, int length)
        {
            long max = array(0);
            for (int i = 1; i < length; ++i)
            {
                long next = array(i);
                if (next > max)
                {
                    max = next;
                }
            }

            return max;
        }

        public static long Max(params long[][] arrays)
        {
            long max = arrays[0].Max();
            for (int i = 1; i < arrays.Length; ++i)
            {
                long current = arrays[i].Max();
                if (current > max)
                {
                    max = current;
                }
            }

            return max;
        }
    }
}