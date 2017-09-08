using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Syn.Logging;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Provides a set of generic utilities
    /// </summary>
    public class Utilities
    {
        private const Boolean TrackingObjects = false;
        static long _maxUsed;

        // Unconstructable.
        private Utilities() {
        }


        /**
        /// Returns a string with the given number of spaces.
         *
        /// @param padding the number of spaces in the string
        /// @return a string of length 'padding' containing only the SPACE char.
         */
        public static string Pad(int padding) 
        {
            if (padding > 0) {
                var sb = new StringBuilder(padding);
                for (var i = 0; i < padding; i++) {
                    sb.Append(' ');
                }
                return sb.ToString();
            } else {
                return "";
            }
        }


        /**
        /// Pads with spaces or truncates the given string to guarantee that it is exactly the desired length.
         *
        /// @param string    the string to be padded
        /// @param minLength the desired length of the string
        /// @return a string of length containing string padded with whitespace or truncated
         */
        public static string Pad(String _string, int minLength) 
        {
            var result = _string;
            var pad = minLength - _string.Length;
            if (pad > 0) 
            {
                result = _string + Utilities.Pad(minLength - _string.Length);
            } 
            else if (pad < 0) {
                result = _string.Substring(0, minLength);
            }
            return result;
        }


        /**
        /// Pads with spaces or truncates the given int to guarantee that it is exactly the desired length.
         *
        /// @param val       the value to be padded
        /// @param minLength the desired length of the string
        /// @return a string of length containing string padded with whitespace or truncated
         */
        public static string Pad(int val, int minLength) 
        {
            return Pad(val.ToString(CultureInfo.InvariantCulture), minLength);
        }


        /**
        /// Pads with spaces or truncates the given double to guarantee that it is exactly the desired length.
         *
        /// @param val       the value to be padded
        /// @param minLength the desired length of the string
        /// @return a string of length containing string padded with whitespace or truncated
         */
        public static string Pad(double val, int minLength) 
        {
            return Pad(val.ToString(CultureInfo.InvariantCulture), minLength);
        }

        /**
        /// Dumps padded text. This is a simple tool for helping dump text with padding to a Writer.
         *
        /// @param pw      the stream to send the output
        /// @param padding the number of spaces in the string
        /// @param string  the string to output
         */
        public static void Dump(int padding, string _string) 
        {
            Trace.Write(Pad(padding));
            Logger.LogInfo<Utilities>(_string);
        }


        /**
        /// utility method for tracking object counts
         *
        /// @param name  the name of the object
        /// @param count the count of objects
         */
        public static void ObjectTracker(String name, int count) 
        {
            if (TrackingObjects) {
                if (count % 1000 == 0) {
                    Logger.LogInfo<Utilities>("OT: " + name + ' ' + count);
                }
            }
        }

        /**
        /// Dumps  out memory information
         *
        /// @param msg addditional text for the dump
         */

        public static void DumpMemoryInfo(String msg) 
        {
            var rt = Process.GetCurrentProcess();
            
            long free = rt.PrivateMemorySize;
            
            var reclaimedMemory = (rt.PrivateMemorySize - free)
                    / (1024* 1024);
            long freeMemory = rt.PrivateMemorySize / (1024* 1024);
            long totalMemory = rt.PrivateMemorySize / (1024* 1024);
            long usedMemory = rt.PrivateMemorySize - rt.PrivateMemorySize;

            if (usedMemory > _maxUsed) {
                _maxUsed = usedMemory;
            }

            Logger.LogInfo<Utilities>("Memory (mb) "
                    + " total: " + totalMemory
                    + " reclaimed: " + reclaimedMemory
                    + " free: " + freeMemory
                    + " Max Used: " + (_maxUsed / (1024* 1024))
                    + " -- " + msg);
        }

        /**
        /// Returns the string representation of the given double value in normalized scientific notation. The
        /// <code>fractionDigits</code> argument gives the number of decimal digits in the fraction portion. For example, if
        /// <code>fractionDigits</code> is 4, then the 123450 will be "1.2345e+05". There will always be two digits in the
        /// exponent portion, and a plus or minus sign before the exponent.
         *
        /// @param number         the double to convert
        /// @param fractionDigits the number of digits in the fraction part, e.g., 4 in "1.2345e+05".
        /// @return the string representation of the double in scientific notation
         */
        public static string DoubleToScientificString(double number,int fractionDigits) 
        {
            var formatter = new StringBuilder(5 + fractionDigits).Append("0.");
            for (var i = 0; i < fractionDigits; i++) 
            {
                formatter.Append('0');
            }
            formatter.Append("E00");

      
            var formatted = String.Format("{0:" + formatter + "}",number);

            var index = formatted.IndexOf('E');
            if (formatted[index + 1] != '-') 
            {
                return formatted.Substring(0, index + 1) + '+' +
                        formatted.Substring(index + 1);
            } else {
                return formatted;
            }
        }

        /**
        /// Returns true if the given binary cepstra file is in big-endian format. It assumes that the first 4 bytes of the
        /// file tells you how many 4-byte floating point cepstra values are in the file.
         *
        /// @param filename the cepstra file name
        /// @return true if the given binary cepstra file is big-endian
         */
        //public static Boolean isCepstraFileBigEndian(String filename)
        //{
        //    StringReader cepstraFile = new StringReader(filename);
        //    int fileSize = (int) cepstraFile.Length;
        //    DataInputStream stream =
        //            new DataInputStream(new FileInputStream(filename));
        //    int numberBytes = stream.readInt()/// 4 + 4;
        //    stream.close();
        //    return (fileSize == numberBytes);
        //}


        /**
        /// Reads the next float from the given DataInputStream, where the data is in little endian.
         *
        /// @param dataStream the DataInputStream to read from
        /// @return a float
         */
        public static float ReadLittleEndianFloat(Stream dataStream)
        {
            var toReturn = Float.IntBitsToFloat(ReadLittleEndianInt(dataStream));
            return toReturn;
        }


        /**
        /// Reads the next little-endian integer from the given DataInputStream.
         *
        /// @param dataStream the DataInputStream to read from
        /// @return an integer
         */
        public static int ReadLittleEndianInt(Stream dataStream)
        {
            var toReturn= dataStream.ReadByte() | dataStream.ReadByte() << 8 |
                   dataStream.ReadByte() << 16 | dataStream.ReadByte() << 24;

            return toReturn;
        }


        /**
        /// Byte-swaps the given integer to the other endian. That is, if this integer is big-endian, it becomes
        /// little-endian, and vice-versa.
         *
        /// @param integer the integer to swap
         */
        public static int SwapInteger(int _integer)
        {
            var ret = ((0xff000000 & _integer) >> 24);
            return (((0x000000ff & _integer) << 24) |
                    ((0x0000ff00 & _integer) << 8) |
                    ((0x00ff0000 & _integer) >> 8) | (int)ret
                    );
        }

        ///**
        ///// Byte-swaps the given float to the other endian. That is, if this float is big-endian, it becomes little-endian,
        ///// and vice-versa.
        // *
        ///// @param floatValue the float to swap
        // */
        //public static float swapFloat(float floatValue) 
        //{
        //    return float.intBitsToFloat
        //            (Utilities.swapInteger(float.floatToRawIntBits(floatValue)));
        //}


        /**
        /// If a data point is below 'floor' make it equal to floor.
         *
        /// @param data  the data to floor
        /// @param floor the floored value
         */
        public static void floorData(float[] data, float floor)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] < floor)
                {
                    data[i] = floor;
                }
            }
        }
        /**
        /// If a data point is non-zero and below 'floor' make it equal to floor
        /// (don't floor zero values though).
        /// 
        /// @param data the data to floor
        /// @param floor the floored value
         */
        public static void NonZeroFloor(float[] data, float floor)
        {
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] != 0.0 && data[i] < floor)
                {
                    data[i] = floor;
                }
            }
        }
    

        /**
        /// Normalize the given data.
        /// 
        /// @param data the data to normalize
         */
        public static void Normalize(float[] data) 
        {
            float sum = 0;
            foreach (var val in data) {
                sum += val;
            }
            if (sum != 0.0f) {
                for (var i = 0; i < data.Length; i++) 
                {
                    data[i] = data[i] / sum;
                }
            }
        }


        public static string Join(List<String> tokens) 
        {
            var builder = new StringBuilder();
            foreach (var token in tokens) 
            {
                builder.Append(token);
                builder.Append(' ');
            }
            return builder.ToString().Trim();
        }


        public static List<int> AsList(int[] align)
        {
            return align.ToList();
        }
    }
}
