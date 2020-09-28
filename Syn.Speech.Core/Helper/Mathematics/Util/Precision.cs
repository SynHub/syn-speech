using System;

namespace Syn.Speech.Helper.Mathematics.Util
{
    public class Precision
    {

        public static readonly double EPSILON = BitConverter.Int64BitsToDouble((EXPONENT_OFFSET - 53L) << 52);
        private const long EXPONENT_OFFSET = 1023l;
        /**
   * Returns true if both arguments are NaN or neither is NaN and they are
   * equal as defined by {@link #equals(float,float) equals(x, y, 1)}.
   *
   * @param x first value
   * @param y second value
   * @return {@code true} if the values are equal or both are NaN.
   * @since 2.2
   */

        public static bool equalsWithRelativeTolerance(double x, double y, double eps) {
        if (equals(x, y, 1)) {
            return true;
        }

         double absoluteMax = Math.Max(Math.Abs(x), Math.Abs(y));
         double relativeDifference = Math.Abs((x - y) / absoluteMax);

        return relativeDifference <= eps;
    }

        public static bool equals(double x, double y)
        {
            return equals(x, y, 1);
        }

        public static bool equalsIncludingNaN(float x, float y)
        {
            return (x != x || y != y) ? !(x != x ^ y != y) : equals(x, y, 1);
        }

        public static int compareTo(double x, double y, double eps)
        {
            if (equals(x, y, eps))
            {
                return 0;
            }
            else if (x < y)
            {
                return -1;
            }
            return 1;
        }


        /**
    * Returns true if both arguments are equal or within the range of allowed
    * error (inclusive).
    *
    * @param x first value
    * @param y second value
    * @param eps the amount of absolute error to allow.
    * @return {@code true} if the values are equal or within range of each other.
    * @since 2.2
    */
        public static bool equals(float x, float y, float eps)
        {
            return equals(x, y, 1) || Math.Abs(y - x) <= eps;
        }

        public static bool equalsIncludingNaN(double x, double y)
        {
            return (x != x || y != y) ? !(x != x ^ y != y) : equals(x, y, 1);
        }

        public static bool equals(double x, double y, double eps)
        {
            return equals(x, y, 1) || Math.Abs(y - x) <= eps;
        }
    }
}
