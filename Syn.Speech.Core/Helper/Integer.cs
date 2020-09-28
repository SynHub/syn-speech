using System;
using System.Globalization;

namespace Syn.Speech.Helper
{
    public class Integer : IComparable<Integer>
    {
        public const int MAX_VALUE = 2147483647;
        public const int MIN_VALUE = -2147483648;

        private readonly int _value;

        public Integer(int value)
        {
            _value = value;
        }

        public static int HighestOneBit(int number)
        {
            return (int)Math.Pow(2, Convert.ToString(number, 2).Length - 1);
        }

        public static uint HighestOneBit(uint i)
        {
            i |= (i >> 1);
            i |= (i >> 2);
            i |= (i >> 4);
            i |= (i >> 8);
            i |= (i >> 16);
            return i - (i >> 1);
        }

        public int CompareTo(Integer otherInt)
        {
            return _value.CompareTo(otherInt._value);
        }

        public static string ToBinaryString(int x)
        {
            char[] bits = new char[32];
            int i = 0;

            while (x != 0)
            {
                bits[i++] = (x & 1) == 1 ? '1' : '0';
                x >>= 1;
            }

            Array.Reverse(bits, 0, i);
            return new string(bits);
        }

        public static implicit operator Integer(int value)
        {
            return new Integer(value);
        }

        public static implicit operator int(Integer integer)
        {
            return integer._value;
        }

        public static int operator +(Integer one, Integer two)
        {
            return one._value + two._value;
        }

        public static Integer operator +(int one, Integer two)
        {
            return new Integer(one + two._value);
        }

        public static int operator -(Integer one, Integer two)
        {
            return one._value - two._value;
        }

        public static Integer operator -(int one, Integer two)
        {
            return new Integer(one-two._value);
        }


        public static string ToString(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToString(char character, int radix)
        {
            return Convert.ToInt32(character.ToString(CultureInfo.InvariantCulture), 16).ToString(CultureInfo.InvariantCulture);
        }

        public static Integer ParseInt(string value)
        {
            int outValue;
            if (int.TryParse(value, NumberStyles.Number,CultureInfo.InvariantCulture.NumberFormat, out outValue))
            {
                return outValue;
            }
            return null;
        }

        public override bool Equals(object obj)
        {
            var op = (Integer) obj;
            return _value.Equals(op._value); ;
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static Integer Decode(string flattenProp)
        {
            if (flattenProp == null) return null;
            return new Integer(Convert.ToInt32(flattenProp , CultureInfo.InvariantCulture.NumberFormat));
        }

        public override string ToString()
        {
            return _value.ToString(CultureInfo.InvariantCulture);
        }

        public static int ValueOf(string s)
        {
           return new Integer(ParseInt(s));
        }
    }
}
