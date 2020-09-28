using System;

namespace Syn.Speech.Helper.Mathematics.Linear
{
    /// <summary>
    /// Miscellaneous utility functions.
    /// </summary>
    public sealed class MathUtils
    {
        /**
         * \(2\pi\)
         * @since 2.1
         */
        public static double TWO_PI = 2 * Math.PI;

        /**
         * \(\pi^2\)
         * @since 3.4
         */
        public static double PI_SQUARED = Math.PI * Math.PI;


        /**
         * Class contains only static methods.
         */
        private MathUtils() { }


        /**
         * Returns an integer hash code representing the given double value.
         *
         * @param value the value to be hashed
         * @return the hash code
         */
        public static int GetHashCode(double value)
        {
            return value.GetHashCode();
        }

        public static int hash(double[] value)
        {
            return Arrays.HashCode(value);
        }

        /**
         * Returns {@code true} if the values are equal according to semantics of
         * {@link Double#equals(Object)}.
         *
         * @param x Value
         * @param y Value
         * @return {@code new Double(x).equals(new Double(y))}
         */
        public static bool Equals(double x, double y)
        {
            return x.Equals(y);
        }

        /**
         * Returns an integer hash code representing the given double array.
         *
         * @param value the value to be hashed (may be null)
         * @return the hash code
         * @since 1.2
         */
        public static int GetHashCode(double[] value)
        {
            return value.GetHashCode();
        }

        /**
         * Normalize an angle in a 2&pi; wide interval around a center value.
         * <p>This method has three main uses:</p>
         * <ul>
         *   <li>normalize an angle between 0 and 2&pi;:<br/>
         *       {@code a = MathUtils.normalizeAngle(a, FastMath.PI);}</li>
         *   <li>normalize an angle between -&pi; and +&pi;<br/>
         *       {@code a = MathUtils.normalizeAngle(a, 0.0);}</li>
         *   <li>compute the angle between two defining angular positions:<br>
         *       {@code angle = MathUtils.normalizeAngle(end, start) - start;}</li>
         * </ul>
         * <p>Note that due to numerical accuracy and since &pi; cannot be represented
         * exactly, the result interval is <em>closed</em>, it cannot be half-closed
         * as would be more satisfactory in a purely mathematical view.</p>
         * @param a angle to normalize
         * @param center center of the desired 2&pi; interval for the result
         * @return a-2k&pi; with integer k and center-&pi; &lt;= a-2k&pi; &lt;= center+&pi;
         * @since 1.2
         */
        public static double normalizeAngle(double a, double center)
        {
            return a - TWO_PI * Math.Floor((a + Math.PI - center) / TWO_PI);
        }

        /**
         * <p>Reduce {@code |a - offset|} to the primary interval
         * {@code [0, |period|)}.</p>
         *
         * <p>Specifically, the value returned is <br/>
         * {@code a - |period| * floor((a - offset) / |period|) - offset}.</p>
         *
         * <p>If any of the parameters are {@code NaN} or infinite, the result is
         * {@code NaN}.</p>
         *
         * @param a Value to reduce.
         * @param period Period.
         * @param offset Value that will be mapped to {@code 0}.
         * @return the value, within the interval {@code [0 |period|)},
         * that corresponds to {@code a}.
         */
        public static double reduce(double a,
                                    double period,
                                    double offset)
        {
            double p = Math.Abs(period);
            return a - p * Math.Floor((a - offset) / p) - offset;
        }

        /**
         * Returns the first argument with the sign of the second argument.
         *
         * @param magnitude Magnitude of the returned value.
         * @param sign Sign of the returned value.
         * @return a value with magnitude equal to {@code magnitude} and with the
         * same sign as the {@code sign} argument.
         * @throws MathArithmeticException if {@code magnitude == Byte.MIN_VALUE}
         * and {@code sign >= 0}.
         */
        public static byte copySign(byte magnitude, byte sign)
        {
            if ((magnitude >= 0 && sign >= 0) ||
                (magnitude < 0 && sign < 0))
            { // Sign is OK.
                return magnitude;
            }
            else if (sign >= 0 &&
                     magnitude == Byte.MinValue)
            {
                throw new OverflowException();
            }
            else
            {
                return (byte)-magnitude; // Flip sign.
            }
        }

        /**
         * Returns the first argument with the sign of the second argument.
         *
         * @param magnitude Magnitude of the returned value.
         * @param sign Sign of the returned value.
         * @return a value with magnitude equal to {@code magnitude} and with the
         * same sign as the {@code sign} argument.
         * @throws MathArithmeticException if {@code magnitude == Short.MIN_VALUE}
         * and {@code sign >= 0}.
         */
        public static short copySign(short magnitude, short sign)
        {
            if ((magnitude >= 0 && sign >= 0) ||
                (magnitude < 0 && sign < 0))
            { // Sign is OK.
                return magnitude;
            }
            else if (sign >= 0 &&
                     magnitude == short.MinValue)
            {
                throw new ArithmeticException();
            }
            else
            {
                return (short)-magnitude; // Flip sign.
            }
        }

        /**
         * Returns the first argument with the sign of the second argument.
         *
         * @param magnitude Magnitude of the returned value.
         * @param sign Sign of the returned value.
         * @return a value with magnitude equal to {@code magnitude} and with the
         * same sign as the {@code sign} argument.
         * @throws MathArithmeticException if {@code magnitude == Integer.MIN_VALUE}
         * and {@code sign >= 0}.
         */
        public static int copySign(int magnitude, int sign)
        {
            if ((magnitude >= 0 && sign >= 0) ||
                (magnitude < 0 && sign < 0))
            { // Sign is OK.
                return magnitude;
            }
            else if (sign >= 0 &&
                     magnitude == Integer.MIN_VALUE)
            {
                throw new ArgumentException();
            }
            else
            {
                return -magnitude; // Flip sign.
            }
        }

        /**
         * Returns the first argument with the sign of the second argument.
         *
         * @param magnitude Magnitude of the returned value.
         * @param sign Sign of the returned value.
         * @return a value with magnitude equal to {@code magnitude} and with the
         * same sign as the {@code sign} argument.
         * @throws MathArithmeticException if {@code magnitude == Long.MIN_VALUE}
         * and {@code sign >= 0}.
         */
        public static long copySign(long magnitude, long sign)
        {
            if ((magnitude >= 0 && sign >= 0) ||
                (magnitude < 0 && sign < 0))
            { // Sign is OK.
                return magnitude;
            }
            else if (sign >= 0 &&
                     magnitude == JLong.MIN_VALUE)
            {
                throw new ArithmeticException();
            }
            else
            {
                return -magnitude; // Flip sign.
            }
        }
        /**
         * Check that the argument is a real number.
         *
         * @param x Argument.
         * @throws NotFiniteNumberException if {@code x} is not a
         * finite real number.
         */
        public static void checkFinite(double x)
        {
            if (Double.IsInfinity(x) || Double.IsNaN(x))
            {
                throw new NotFiniteNumberException(x);
            }
        }

        /**
         * Check that all the elements are real numbers.
         *
         * @param val Arguments.
         * @throws NotFiniteNumberException if any values of the array is not a
         * finite real number.
         */
        public static void checkFinite(double[] val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                double x = val[i];
                if (Double.IsInfinity(x) || Double.IsNaN(x))
                {
                    throw new Exception();
                }
            }
        }

        ///**
        // * Checks that an object is not null.
        // *
        // * @param o Object to be checked.
        // * @param pattern Message pattern.
        // * @param args Arguments to replace the placeholders in {@code pattern}.
        // * @throws NullArgumentException if {@code o} is {@code null}.
        // */
        //public static void checkNotNull(Object o, Localizable pattern, params  object[] args)
        //{
        //    if (o == null)
        //    {
        //        throw new ArgumentNullException();
        //    }
        //}

        /**
         * Checks that an object is not null.
         *
         * @param o Object to be checked.
         * @throws NullArgumentException if {@code o} is {@code null}.
         */
        public static void checkNotNull(Object o)
        {
            if (o == null)
            {
                throw new ArgumentNullException();
            }
        }

        public static int hash(double value)
        {
            Double toReturn = value;
            return toReturn.GetHashCode();
        }
    }

}
