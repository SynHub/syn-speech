using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Syn.Speech.Helper
{
    /// <summary>
    /// Java Extensions
    /// </summary>
    public static class Java
    {
        private static readonly DateTime Jan1St1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private const int InitialHash = 17; // Prime number
        private const int Multiplier = 23; // Different prime number
        private static int _hashIndex = int.MinValue;

        public static double Random()
        {
            return new Random().Next();
        }

        public static int GetHashCode<T>(params T[] values)
        {
            //unchecked // overflow is fine
            {
                var hash = InitialHash;

                if (values != null)
                    for (var i = 0; i < values.Length; i++)
                    {
                        var currentValue = values[i];
                        hash = hash*Multiplier + currentValue.GetHashCode();
                    }

                return hash;
            }
        }

        public static bool IsEmpty(this string source)
        {
            return string.IsNullOrEmpty(source);
        }

        public static string JSubString(this string s, int start, int end)
        {
            return s.Substring(start, end - start);
        }

        public static int GetUniqueNumber()
        {
            var toReturn = _hashIndex;
            _hashIndex++;
            return toReturn;
        }

        public static string ReplaceAll(this string value, string pattern, string replacement)
        {
            string toReturn = value;
            var regex = new Regex(pattern);
            return regex.Replace(toReturn, replacement);
        }

        public static bool Offer<T>(this LinkedList<T> source, T item)
        {
            source.AddLast(item);
            return true;
        }

        public static T Poll<T>(this LinkedList<T> source)
        {
            if (source.Count == 0)
            {
                return default(T);
            }
            var toReturn = source.First.Value;
            source.RemoveFirst();
            return toReturn;
        }

        public static T PeekLast<T>(this LinkedList<T> source)
        {
            return source.Last.Value;
        }

        public static TV GetProperty<T, TV>(this Dictionary<T, TV> source, T expectedValue, TV defaultValue)
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

        public static string GetSystemProperty(string name, string defaultValue)
        {
            var resourceValue = Properties.Resources.ResourceManager.GetString(name);
            if (string.IsNullOrEmpty(resourceValue))
            {
                return defaultValue;
            }
            return resourceValue;
        }

        public static double[] CopyOfRange(double[] src, int start, int end)
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

        public static double[] CopyOf(double[] src, int length)
        {
            return CopyOfRange(src, 0, length);
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

        public static IEnumerable<T> SubList<T>(this IEnumerable<T> source, int fromRange, int toRange)
        {
            return source.Skip(fromRange).Take(toRange - fromRange);
        }

        public static T Remove<T>(this LinkedList<T> source, int index)
        {
            var element = source.ElementAt(0); //Note: This is an O(n) operation
            source.Remove(element);
            return element;
        }


        /// <summary>
        /// Extension for Java's System.currentTimeMillis
        /// </summary>
        /// <returns></returns>
        public static long CurrentTimeMillis()
        {
            return (long)((DateTime.UtcNow - Jan1St1970).TotalMilliseconds);
        }

        public static T Remove<T>(this List<T> source, int index)
        {
            var toReturn = source[index];
            source.RemoveAt(index);
            return toReturn;
        }

        public static void Put<T, TV>(this Dictionary<T, TV> source, T key, TV value)
        {
            if (key == null || value == null)
            {
                return;
            }
            //TODO: EXTEND NULL KEY TO IMITATE JAVA HASHMAPs
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else { source.Add(key,value);}
        }

        public static void Put<T, V>(SortedDictionary<T, V> source, T key, V value)
        {
            if (key == null || value == null)
            {
                return;
            }
            //TODO: EXTEND NULL KEY TO IMITATE JAVA HASHMAPs
            if (source.ContainsKey(key))
            {
                source[key] = value;
            }
            else { source.Add(key, value); }
        }

        public static TV Get<T,TV>(this Dictionary<T, TV> source, T key)
        {
            if (source.ContainsKey(key))
            {
                return source[key];
            }
            return default(TV);
        }

        public static V Get<T, V>(this SortedDictionary<T, V> source, T key)
        {
            if (source.ContainsKey(key))
            {
                return source[key];
            }
            return default(V);
        }

        public static void Add<T>(this LinkedList<T> source, T value)
        {
            source.AddLast(value);
        }

        public static T CreateArray<T>(params int[] lengths)
        {
            return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths);
        }

        static object InitializeJaggedArray(Type type, int index, int[] lengths)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(
                        InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

        public static long Skip(this Stream source, long toSkip)
        {
            var currentPosition = source.Position;
            source.Seek(toSkip, SeekOrigin.Current);
            var toReturn = source.Position - currentPosition;
            return toReturn;
        }

        public static IEnumerator<T> ListIterator<T>(this IEnumerator<T> source, int index)
        {
            var cloneList = new List<T>();
            while (source.MoveNext())
            {
                cloneList.Add(source.Current);
            }

            var clone = cloneList.GetEnumerator();
            var toReach = 0;
            while (toReach != index)
            {
                clone.MoveNext();
                toReach++;
            }
            return clone;
        }

        public static void AddAll<T>(this HashSet<T> source, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                source.Add(item);
            }
        }

        public static void AddAll<T>(this SortedSet<T> source, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                source.Add(item);
            }
        }

        public static void AddAll<T>(this LinkedList<T> source, IEnumerable<T> values)
        {
            foreach (var item in values)
            {
                source.Add(item);
            }
        }

        public static int Read(this FileStream source, byte[] bytes)
        {
            return source.Read(bytes, 0, bytes.Length);
        }

        public static int ReadInt(this Stream source)
        {
            int val = source.ReadByte() << 24 | source.ReadByte() << 16 | source.ReadByte() << 8 | source.ReadByte();
            return val;
        }

        public static float ReadFloat(this Stream source)
        {
            int val = ReadInt(source);
            return Float.IntBitsToFloat(val);
        }

        public static sbyte[] ToSignedBytes(this byte[] maindata)
        {
           //return Array.ConvertAll(maindata, b => unchecked((sbyte)b));
            return (sbyte[])(Array)maindata;
        }

        public static bool IsEmpty<T>(this List<T> source)
        {
            return source.Count == 0;
        }

        public static bool IsEmpty<T>(this LinkedList<T> source)
        {
            return source.Count == 0;
        }

        public static T Set<T>(this List<T> source, int index, T element)
        {
            var toReturn = source[index];
            source[index] = element;
            return toReturn;
        }

        public static int TripleShift(int n, int s)
        {
            if (n >= 0)
                return n >> s;
            return (n >> s) + (2 << ~s);
        }

        /// <summary>
        /// Splits the specified string using regex pattern.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public static string[] Split(this string source, string pattern)
        {
            return Regex.Split(source, pattern);
        }

        public static void PrintStackTrace(this Exception source)
        {
            Console.WriteLine(source.ToString());
        }

        public static T Min<T>(IEnumerable<T> values, IComparer<T> comparer)
        {
            bool first = true;
            T result = default(T);
            foreach (T value in values)
            {
                if (first)
                {
                    result = value;
                    first = false;
                }
                else
                {
                    if (comparer.Compare(result, value) > 0)
                    {
                        result = value;
                    }
                }
            }
            return result;
        }

        public static bool IsValidIdentifier(char value)
        {
            // using System.CodeDom.Compiler;
            CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
            return provider.IsValidIdentifier(value.ToString(CultureInfo.InvariantCulture));
        }

        public static char ToChar(int value)
        {
            return Convert.ToChar(value);
        }

        public static int ToInt(char chr)
        {
            return Convert.ToInt32(chr, CultureInfo.InvariantCulture.NumberFormat);
        }

        public static void Reset(this Stream source, int position)
        {
            source.Seek(position, SeekOrigin.Begin);
        }
    }
}
