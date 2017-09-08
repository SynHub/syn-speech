using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Window
{
    public class DoubleBuffer 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DoubleBuffer"/> class.
        /// </summary>
        /// <param name="size">The size.</param>
        public DoubleBuffer(int size) {
            Buffer = new double[size];
            Occupancy = 0;
        }

        /// <summary>
        /// Gets the number of elements in this DoubleBuffer.
        /// </summary>
        /// <value></value>
        public int Occupancy { get; private set; }

        /// <summary>
        /// Returns the underlying double array used to store the data.
        /// </summary>
        /// <value>The underlying double array.</value>
        public double[] Buffer { get; private set; }

        /// <summary>
        /// Appends all the elements in the given array to this DoubleBuffer.
        /// </summary>
        /// <param name="src">The array to copy from.</param>
        /// <returns>The resulting number of elements in this DoubleBuffer.</returns>
        public int AppendAll(double[] src) {
            return Append(src, 0, src.Length);
        }


        /// <summary>
        /// Appends the specified elements in the given array to this DoubleBuffer.
        /// </summary>
        /// <param name="src">The array to copy from.</param>
        /// <param name="srcPos">Where in the source array to start from.</param>
        /// <param name="length">The number of elements to copy.</param>
        /// <returns>The resulting number of elements in this DoubleBuffer.</returns>
        /// <exception cref="System.Exception">RaisedCosineWindower:  +
        ///                                     overflow-buffer: attempting to fill  +
        ///                                     buffer beyond its capacity.</exception>
        public int Append(double[] src, int srcPos, int length) {
            if (Occupancy + length > Buffer.Length) {
                throw new Exception("RaisedCosineWindower: " +
                                    "overflow-buffer: attempting to fill " +
                                    "buffer beyond its capacity.");
            }
            Array.Copy(src, srcPos, Buffer, Occupancy, length);
            Occupancy += length;
            return Occupancy;
        }

        /// <summary>
        /// If there are less than windowSize elements in this DoubleBuffer, pad the up to windowSize elements with zero.
        /// </summary>
        /// <param name="windowSize">The window size.</param>
        public void PadWindow(int windowSize) 
        {
            if (Occupancy < windowSize) 
            {
                for(var i= Occupancy;i<windowSize;i++)
                    Buffer[i]=0;
            }
        }


        /// <summary>
        /// Sets the number of elements in this DoubleBuffer to zero, without actually remove the elements.
        /// </summary>
        public void Reset() {
            Occupancy = 0;
        }
    }
}