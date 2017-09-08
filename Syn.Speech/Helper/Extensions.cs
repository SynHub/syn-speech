using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Syn.Speech.Helper
{
    public static class Extensions
    {
        private static DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ReplaceAll(this string value, string pattern, string replacement)
        {
            string toReturn = value;
            var regex = new Regex(pattern);
            return regex.Replace(toReturn, replacement);
        }

        public static void Fill<T>(this T[] originalArray, T with)
        {
            for (var i = 0; i < originalArray.Length; i++)
            {
                originalArray[i] = with;
            }
        }

        public static bool offer<T>(this LinkedList<T> source, T item)
        {
            source.AddLast(item);
            return true;
        }


        public static T poll<T>(this LinkedList<T> source)
        {
            var toReturn = source.First.Value;
            source.RemoveFirst();
            return toReturn;
        }

        public static V getProperty<T, V>(this Dictionary<T, V> source, T expectedValue, V defaultValue)
        {
            if (source.ContainsKey(expectedValue))
            {
                return source[expectedValue];
            }
            else
            {
                return defaultValue;
            }
        }

        public static double[] copyOfRange(double[] src, int start, int end)
        {
            int len = end - start;
            double[] dest = new double[len];
            // note i is always from 0
            for (int i = 0; i < len; i++)
            {
                dest[i] = src[start + i]; // so 0..n = 0+x..n+x
            }
            return dest;
        }

        public static double[] copyOf(double[] src, int length)
        {
            return copyOfRange(src, 0, length);
        }

        public static T GetField<T>(this Type source, string memberName)
        {
            foreach (var property in source.GetMembers())
            {
                var attribute = property.GetCustomAttributes(typeof(T), false);
                if (property.Name == memberName) return (T) attribute[0];
            }
            return default (T);
        }

        /// <summary>
        /// Extension for Java's System.currentTimeMillis
        /// </summary>
        /// <returns></returns>
        public static long currentTimeMillis()
        {
            return (long)((DateTime.UtcNow - Jan1st1970).TotalMilliseconds);
        }

        public static T Remove<T>(this List<T> source, int index)
        {
            var toReturn = source[index];
            source.RemoveAt(index);
            return toReturn;
        }

        public static void Put<V, T>(Dictionary<T, V> source, T key, V value)
        {
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else { source.Add(key,value);}
        }

    }
}
