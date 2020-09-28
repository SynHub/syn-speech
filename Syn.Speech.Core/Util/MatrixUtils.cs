using System;
using System.Text;
using Syn.Speech.Helper.Mathematics.Linear;
//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Some simple matrix and vector manipulation methods.
    /// </summary>
    public class MatrixUtils
    {
        public static string DecimalFormat = "0.00";

        public static RealMatrix CreateRealIdentityMatrix(int dimension) {
         var m = CreateRealMatrix(dimension, dimension);
        for (var i = 0; i < dimension; ++i) {
            m.setEntry(i, i, 1.0);
        }
        return m;
    }

        public static RealMatrix CreateRealDiagonalMatrix( double[] diagonal) {
            var m = CreateRealMatrix(diagonal.Length, diagonal.Length);
            for (var i = 0; i < diagonal.Length; ++i)
            {
            m.setEntry(i, i, diagonal[i]);
        }
        return m;
    }

        public static bool IsSymmetric(RealMatrix matrix,
                                     double eps)
        {
            return IsSymmetricInternal(matrix, eps, false);
        }

        public static bool IsSymmetricInternal(RealMatrix matrix,
                                               double relativeTolerance,
                                               bool raiseException) {
         var rows = matrix.getRowDimension();
        if (rows != matrix.getColumnDimension()) {
            if (raiseException) {
                throw new Exception("NonSquareMatrixException");
            } else {
                return false;
            }
        }
        for (var i = 0; i < rows; i++) {
            for (var j = i + 1; j < rows; j++) {
                 var mij = matrix.getEntry(i, j);
                 var mji = matrix.getEntry(j, i);
                if (Math.Abs(mij - mji) >
                    Math.Max(Math.Abs(mij), Math.Abs(mji)) * relativeTolerance)
                {
                    if (raiseException) {
                        throw new Exception("NonSymmetricMatrixException");
                    } else {
                        return false;
                    }
                }
            }
        }
        return true;
    }

        public static RealMatrix CreateRealMatrix( int rows,  int columns) {
        return (rows * columns <= 4096) ? (RealMatrix) new Array2DRowRealMatrix(rows, columns) : new BlockRealMatrix(rows, columns);
    }

        public static RealMatrix CreateRealMatrix(double[][] data) {
        if (data == null ||
            data[0] == null) {
            throw new NullReferenceException();
        }
        return (data.Length * data[0].Length <= 4096) ? (RealMatrix) new Array2DRowRealMatrix(data) : new BlockRealMatrix(data);
    }

        public static string ToString(double[][] m) 
        {
            var s = new StringBuilder("[");

            foreach (var row  in m) 
            {
                s.Append(ToString(row));
                s.Append('\n');
            }

            return s.Append(" ]").ToString();
        }


        public static string ToString(double[] m) 
        {
            var s = new StringBuilder("[");

            foreach (var val in m) 
            {
                s.Append(' ').Append(String.Format(DecimalFormat,val));
            }

            return s.Append(" ]").ToString();
        }


        public static int NumCols(double[][] m) 
        {
            return m[0].Length;
        }


        public static string ToString(float[][] matrix) 
        {
            return ToString(Float2Double(matrix));
        }


        public static float[] Double2Float(double[] values) 
        { // what a mess !!! -> fixme: how to convert number arrays ?
            var newVals = new float[values.Length];
            for (var i = 0; i < newVals.Length; i++) 
            {
                newVals[i] = (float) values[i];
            }

            return newVals;
        }


        public static float[][] Double2Float(double[][] array) 
        {
            var floatArr = new float[array.Length][];
            for (var i = 0; i < array.Length; i++)
                floatArr[i] = Double2Float(array[i]);

            return floatArr;
        }


        public static double[] Float2Double(float[] values) 
        {
            var doubArr = new double[values.Length];
            for (var i = 0; i < doubArr.Length; i++)
                doubArr[i] = values[i];

            return doubArr;
        }


        public static double[][] Float2Double(float[][] array) 
        {
            var doubArr = new double[array.Length][];
            for (var i = 0; i < array.Length; i++)
                doubArr[i] = Float2Double(array[i]);

            return doubArr;
        }
    }
}
