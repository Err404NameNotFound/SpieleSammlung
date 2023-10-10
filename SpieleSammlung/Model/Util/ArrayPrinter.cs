using System;

namespace SpieleSammlung.Model.Util
{
    public static class ArrayPrinter
    {
        public static void PrintFirstAndLastElements(int[] arr) => PrintFirstAndLastElements(arr, 4);

        public static void PrintFirstAndLastElements(long[] arr) => PrintFirstAndLastElements(arr, 4);

        public static void PrintFirstAndLastElements(int[] arr, int count)
        {
            ModelLog.Append("Input: [");
            for (int i = 0; i < count; ++i)
            {
                ModelLog.Append("{0}, ", arr[i]);
            }

            ModelLog.Append("...");
            for (int i = arr.Length - count; i < arr.Length; ++i)
            {
                ModelLog.Append(", {0}", arr[i]);
            }

            ModelLog.AppendLine("]");
        }

        public static void PrintFirstAndLastElements(long[] arr, int count)
        {
            ModelLog.Append("Input: [");
            for (int i = 0; i < count; ++i)
            {
                ModelLog.Append("{0}, ", arr[i]);
            }

            ModelLog.Append("...");
            for (int i = arr.Length - count; i < arr.Length; ++i)
            {
                ModelLog.Append(", {0}", arr[i]);
            }

            ModelLog.AppendLine("]");
        }

        public static void PrintArray(long[] input) => PrintArray(i => input[i].ToString(), input.Length);

        public static void PrintArray(int[] input) => PrintArray(i => input[i].ToString(), input.Length);

        public static void PrintArray(long[] input, int[] sorting) =>
            PrintArray(i => input[sorting[i]].ToString(), sorting.Length);


        public static void PrintArray(double[] input, int[] sorting) =>
            PrintArray(i => $"{input[sorting[i]]:0.000}", sorting.Length);


        public static void PrintArray(string before, long[] result, int digits)
        {
            ModelLog.Append(before);
            PrintArray(result, digits);
        }

        public static void PrintArrayWithDecimalPoint(string before, long[] result, int digits)
        {
            ModelLog.Append(before);
            PrintArrayWithDecimalPoint(result, digits);
        }

        public static void PrintArray(string before, long[] result, long[] count, int digits)
        {
            ModelLog.Append(before);
            PrintArray(result, count, digits);
        }

        public static void PrintArray(long[] result, int digits)
            => PrintArray(i => result[i].ToString(), result.Length, digits);


        public static void PrintArrayWithDecimalPoint(long[] result, int digits)
            => PrintArray(i => $"{result[i]:0,0}", result.Length, digits);


        public static void PrintArray(long[] result, long[] count, int digits)
            => PrintArray(i => $"{(double)result[i] / count[i]:0.000}", result.Length, digits);


        public static void PrintArray(string before, string[] strings, int digits)
            => PrintArray(before, i => strings[i], strings.Length, digits);


        public static void PrintArray(string before, Func<int, string> array, int length, int digits)
        {
            ModelLog.Append(before);
            PrintArray(array, length, digits);
        }

        public static void PrintArray(Func<int, string> array, int length, int digits)
        {
            ModelLog.Append("{0," + digits + "}", array(0));
            for (int i = 1; i < length; ++i)
            {
                ModelLog.Append(";{0," + digits + "}", array(i));
            }

            ModelLog.AppendLine();
        }

        public static void PrintArray(string before, Func<int, string> array, int length)
        {
            ModelLog.Append(before);
            PrintArray(array, length);
        }

        public static void PrintArray(Func<int, string> array, int length)
        {
            ModelLog.Append("{0}", array(0));
            for (int i = 1; i < length; ++i)
            {
                ModelLog.Append(";{0}", array(i));
            }

            ModelLog.AppendLine();
        }

        public static long GetNeededDigits(long val)
        {
            if (val == 0) return 1;
            if (val < 0) return 1 + GetNeededDigits(-val);
            val = (long)Math.Ceiling(Math.Log10(val)); // without decimal point
            return val + val / 3; // + decimal points
        }

        public static long GetNeededDigitsSpaced(long val) => 1 + GetNeededDigits(val); // + 1 for spacing

        public static long GetNeededDigits(params long[][] values)
        {
            long max = MathHelp.Max(values);
            return GetNeededDigits(max);
        }

        public static long GetNeededDigits(long min, params long[][] values)
        {
            long max = MathHelp.Max(values);
            return GetNeededDigits(Math.Max(max, min));
        }
    }
}