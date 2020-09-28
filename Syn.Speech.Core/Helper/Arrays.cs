using System;
using System.Collections.Generic;
using System.Linq;

namespace Syn.Speech.Helper
{
    public class Arrays
    {
        /// <summary>
        /// Java: Method copies the specified array, truncating or padding with zeros (if necessary) so the copy has the specified length. 
        /// For all indices that are valid in both the original array and the copy, the two arrays will contain identical values. 
        /// For any indices that are valid in the copy but not the original, 
        /// the copy will contain 0.Such indices will exist if and only if the specified length is greater than that of the original array.
        /// </summary>
        public static T[] copyOf<T>(T[] source, int newLength)
        {
            var toReturn = new T[newLength];
            for (int i = 0; i < toReturn.Length; i++)
            {
                if (i < source.Length) toReturn[i] = source[i];
            }
            return toReturn;
        }

        public static void Fill<T>(T[] originalArray, T with)
        {
            for (var i = 0; i < originalArray.Length; i++)
            {
                originalArray[i] = with;
            }
        }

        public static int HashCode<T>(IEnumerable<T> enumerable)
        {
            int hash = 0x218A9B2C;
            foreach (var item in enumerable)
            {
                int thisHash = item.GetHashCode();
                //mix up the bits.
                hash = thisHash ^ ((hash << 5) + hash);
            }
            return hash;
        }


        public static void Fill<T>(T[] array, int fromIndex, int toIndex, T value)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (fromIndex < 0 || fromIndex >= toIndex)
            {
                throw new ArgumentOutOfRangeException("fromIndex");
            }
            if (toIndex >= array.Length)
            {
                throw new ArgumentOutOfRangeException("toIndex");
            }
            for (int i = fromIndex; i < toIndex; i++)
            {
                array[i] = value;
            }
        }

        public static T[] copyOfRange<T>(T[] src, int start, int end)
        {
            int len = end - start;
            T[] dest = new T[len];
            for (int i = 0; i < len; i++)
            {
                var range = start + i;
                if (range < src.Length) dest[i] = src[start + i];
            }
            return dest;
        }

        public static string ToString<T>(T[] source)
        {
            return string.Join(",", source.Select(x => x.ToString()).ToArray());
        } 

        public static bool AreEqual<T>(T[] source, T[] destination)
        {
            //return source.Length == destination.Length && new HashSet<T>(source).SetEquals(destination);//Note: this has 1 second disadvantage
            //return source.SequenceEqual(destination);
            return ArraysEqual(source, destination);
        }

        public static bool AreEqual<T>(IList<T> source, IList<T> destination)
        {
            return ArraysEqual(source, destination);
        }


 


        private static bool ArraysEqual<T>(IList<T> a1, IList<T> a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Count != a2.Count)
                return false;

            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Count; i++)
            {
                if (!comparer.Equals(a1[i], a2[i])) return false;
            }
            return true;
        }

        public static List<T> AsList<T>(params T[] source)
        {
            return source.ToList();
        } 
    }
}
