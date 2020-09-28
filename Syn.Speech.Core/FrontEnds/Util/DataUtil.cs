using System;
using System.Diagnostics;
using Syn.Speech.Helper;
//REFACTORED + REFACTORED

namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    /// Defines utility methods for manipulating data values.
    /// </summary>
    public class DataUtil
    {
        private static int HEXADECIMAL = 1;
        private static int SCIENTIFIC = 2;
        private static int DECIMAL = 3;

        /// <summary>
        /// DecimalFormat object to be used by all the methods.
        /// </summary>
        private static string format = "";///new DecimalFormat();

        private static int decimalIntegerDigits = 10;
        private static int decimalFractionDigits = 5;
        private static int floatScientificFractionDigits = 8;
        private static int doubleScientificFractionDigits = 8;

        /// <summary>
        /// The number format to be used by *ArrayToString() methods. The default is scientific.
        /// </summary>
        private static int _dumpFormat = SCIENTIFIC;

        /// <summary>
        /// Prevents a default instance of the <see cref="DataUtil"/> class from being created.
        /// </summary>
        private DataUtil()
        {
        }

        static DataUtil()
        {
            String formatProperty = Java.GetSystemProperty("frontend.util.dumpformat", "SCIENTIFIC");// Properties.Resources.ResourceManager.GetString(,);
            if (String.Compare(formatProperty, "DECIMAL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _dumpFormat = DECIMAL;
            }
            else if (String.Compare(formatProperty, "HEXADECIMAL", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _dumpFormat = HEXADECIMAL;
            }
            else if (String.Compare(formatProperty, "SCIENTIFIC", StringComparison.OrdinalIgnoreCase) == 0)
            {
                _dumpFormat = SCIENTIFIC;
            }
        }

        /**
        /// Converts a big-endian byte array into an array of doubles. Each consecutive bytes in the byte array are converted
        /// into a double, and becomes the next element in the double array. The size of the returned array is
        /// (length/bytesPerValue). Currently, only 1 byte (8-bit) or 2 bytes (16-bit) samples are supported.
         *
        /// @param byteArray     a byte array
        /// @param offset        which byte to start from
        /// @param length        how many bytes to convert
        /// @param bytesPerValue the number of bytes per value
        /// @param signedData    whether the data is signed
        /// @return a double array, or <code>null</code> if byteArray is of zero length
        /// @throws java.lang.ArrayIndexOutOfBoundsException
         *
         */
        public static double[] BytesToValues(byte[] byteArray, int offset, int length, int bytesPerValue, Boolean signedData)
        {

            if (0 < length && (offset + length) <= byteArray.Length)
            {
                Debug.Assert(length % bytesPerValue == 0);
                var doubleArray = new double[length / bytesPerValue];

                var i = offset;

                for (var j = 0; j < doubleArray.Length; j++)
                {
                    int val = byteArray[i++];
                    if (!signedData)
                    {
                        val &= 0xff; // remove the sign extension
                    }
                    for (var c = 1; c < bytesPerValue; c++)
                    {
                        var temp = byteArray[i++] & 0xff;
                        val = (val << 8) + temp;
                    }

                    doubleArray[j] = val;
                }

                return doubleArray;
            }
            else
            {
                throw new IndexOutOfRangeException("offset: " + offset + ", length: " + length
                                + ", array length: " + byteArray.Length);
            }
        }

        /**
        /// Converts a little-endian byte array into an array of doubles. Each consecutive bytes of a float are converted
        /// into a double, and becomes the next element in the double array. The number of bytes in the double is specified
        /// as an argument. The size of the returned array is (data.length/bytesPerValue).
         *
        /// @param data          a byte array
        /// @param offset        which byte to start from
        /// @param length        how many bytes to convert
        /// @param bytesPerValue the number of bytes per value
        /// @param signedData    whether the data is signed
        /// @return a double array, or <code>null</code> if byteArray is of zero length
        /// @throws java.lang.ArrayIndexOutOfBoundsException
         *
         */
        public static double[] LittleEndianBytesToValues(byte[] mainData, int offset, int length, int bytesPerValue, Boolean signedData)
        {
            var data = mainData.ToSignedBytes();

            if (0 < length && (offset + length) <= data.Length)
            {
                Debug.Assert(length % bytesPerValue == 0);
                var doubleArray = new double[length / bytesPerValue];

                var i = offset + bytesPerValue - 1;

                for (var j = 0; j < doubleArray.Length; j++)
                {
                    int val = data[i--];
                    if (!signedData)
                    {
                        val &= 0xff; // remove the sign extension
                    }
                    for (var c = 1; c < bytesPerValue; c++)
                    {
                        var temp = data[i--] & 0xff;
                        val = (val << 8) + temp;
                    }

                    // advance 'i' to the last byte of the next value
                    i += (bytesPerValue * 2);

                    doubleArray[j] = val;
                }

                return doubleArray;

            }
            else
            {
                throw new IndexOutOfRangeException("offset: " + offset + ", length: " + length
                                + ", array length: " + data.Length);
            }
        }

        /// <summary>
        /// Gets the number of samples per window given the sample rate (in Hertz) and window size (in milliseconds).
        /// </summary>
        /// <param name="sampleRate">The sample rate in Hertz (i.e., frequency per seconds).</param>
        /// <param name="windowSizeInMs">The window size in milliseconds.</param>
        /// <returns>The number of samples per window.</returns>
        public static int GetSamplesPerWindow(int sampleRate, float windowSizeInMs)
        {
            return (int)(sampleRate * windowSizeInMs / 1000);
        }

        /// <summary>
        /// Returns the number of samples in a window shift given the sample rate (in Hertz) and the window shift (in milliseconds).
        /// </summary>
        /// <param name="sampleRate">The sample rate in Hertz (i.e., frequency per seconds).</param>
        /// <param name="windowShiftInMs">The window shift in milliseconds.</param>
        /// <returns>The number of samples in a window shift.</returns>
        public static int GetSamplesPerShift(int sampleRate, float windowShiftInMs)
        {
            return (int)(sampleRate * windowShiftInMs / 1000);
        }

        /// <summary>
        /// Converts DoubleData object to FloatDatas.
        /// </summary>
        public static DoubleData FloatData2DoubleData(FloatData data)
        {
            var numSamples = data.Values.Length;

            var doubleData = new double[numSamples];
            var values = data.Values;
            for (var i = 0; i < values.Length; i++)
            {
                doubleData[i] = values[i];
            }
            //      System.arraycopy(data.getValues(), 0, doubleData, 0, numSamples); 

            return new DoubleData(doubleData, data.SampleRate, data.FirstSampleNumber);
        }

        /// <summary>
        /// Converts FloatData object to DoubleData.
        /// </summary>
        public static FloatData DoubleData2FloatData(DoubleData data)
        {
            var numSamples = data.Values.Length;

            var floatData = new float[numSamples];
            var values = data.Values;
            for (var i = 0; i < values.Length; i++)
            {
                floatData[i] = (float)values[i];
            }

            return new FloatData(floatData, data.SampleRate, data.FirstSampleNumber);
        }
    }
}
