namespace Syn.Speech.Helper.Mathematics.Util
{
    public class FastMath
    {
        public static double copySign(double magnitude, double sign){
        // The highest order bit is going to be zero if the
        // highest order bit of m and s is the same and one otherwise.
        // So (m^s) will be positive if both m and s have the same sign
        // and negative otherwise.
         long m = JDouble.doubleToRawLongBits(magnitude); // don't care about NaN
         long s = JDouble.doubleToRawLongBits(sign);
        if ((m^s) >= 0) {
            return magnitude;
        }
        return -magnitude; // flip sign
    }
    }
}
