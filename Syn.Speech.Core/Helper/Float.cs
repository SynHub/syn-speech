using System;
using System.Globalization;

namespace Syn.Speech.Helper
{
    public class Float
    {

        public const float MAX_VALUE = 3.4028235E38F;
        public const float MIN_VALUE = 1.4E-45F;

        private readonly float _value;

        public Float(float value)
        {
            _value = value;
        }

        public static bool isNaN(Float value)
        {
            return Double.IsNaN(value);
        }

        public static bool isInfinite(Float value)
        {
            return Double.IsInfinity(value);
        }

        public static implicit operator Float(float value)
        {
            return new Float(value);
        }

        public static implicit operator float(Float value)
        {
            return value._value;
        }

        public static float operator +(Float one, Float two)
        {
            return one._value + two._value;
        }


        public static Float operator +(float one, Float two)
        {
            return new Float(one + two._value);
        }


        public static float operator -(Float one, Float two)
        {
            return one._value - two._value;
        }

        public static Float operator -(float one, Float two)
        {
            return new Float(one-two._value);
        }

        public static float IntBitsToFloat(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            return BitConverter.ToSingle(bytes, 0);
        }

        public static int FloatToIntBits(float value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static float ParseFloat(string value)
        {
            return float.Parse(value,CultureInfo.InvariantCulture.NumberFormat);
        }

        public float FloatValue()
        {
            return _value;
        }

        public static string ToString(float value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
