using System;

namespace SpieleSammlung.Model.Kniffel
{
    public static class Probabilities
    {
        private static readonly long[] Faculties;
        private static readonly long[][] Binomials;

        static Probabilities()
        {
            Faculties = new long[10];
            Faculties[0] = 1;
            for (int i = 1; i < Faculties.Length; ++i)
            {
                Faculties[i] = Faculties[i - 1] * i;
            }

            Binomials = new long[10][];
            Binomials[0] = [1];
            Binomials[1] = [1, 1];
            for (int n = 2; n < Binomials.Length; ++n)
            {
                Binomials[n] = new long[n + 1];
                Binomials[n][0] = 1;
                for (int k = 1; k <= n / 2; ++k)
                {
                    Binomials[n][k] = Binomials[n - 1][k - 1] + Binomials[n - 1][k];
                }

                for (int k = n / 2 + 1; k < Binomials[n].Length; ++k)
                {
                    Binomials[n][k] = Binomials[n][n - k];
                }
            }
        }

        public static long Faculty(int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value must not be negative.");
            }

            if (value < Faculties.Length)
            {
                return Faculties[value];
            }

            long ret = Faculties[Faculties.Length - 1];
            for (int i = Faculties.Length; i <= value; ++i)
            {
                ret *= i;
            }

            return ret;
        }

        public static long Binomial(int n, int k)
        {
            if (k == 0 || k == n)
            {
                return 1;
            }

            if (k == 1 || k == n - 1)
            {
                return n;
            }

            if (k < 0 || n < k)
            {
                throw new ArgumentException(
                    $"k must not be negative and not greater than n. (k was {k} and n was {n})");
            }

            return n < Binomials.Length ? Binomials[n][k] : Faculty(n) / (Faculty(k) * Faculty(n - k));
        }
    }
}