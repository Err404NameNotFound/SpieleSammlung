namespace SpieleSammlungTests.Utils
{
    public abstract class ArrayHelp
    {
        public static int[] CreateIntArray(int length, int constValue)
        {
            int[] ret = new int[length];
            for (int i = 0; i < length; ++i)
            {
                ret[i] = constValue;
            }

            return ret;
        }
    }
}