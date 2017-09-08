using System;

namespace Syn.Speech.Helper
{
    public class JDouble
    {
        public const double MIN_VALUE = 4.9E-324;
        public const double MAX_VALUE =  1.7976931348623157E308;

        public static long doubleToRawLongBits(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }
    }
}
