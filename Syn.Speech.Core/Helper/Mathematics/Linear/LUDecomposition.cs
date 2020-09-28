using System;

namespace Syn.Speech.Helper.Mathematics.Linear
{
    public class LUDecomposition
    {

        /** Default bound to determine effective singularity in LU decomposition. */
        private static double DEFAULT_TOO_SMALL = 1e-11;
        /** Entries of LU decomposition. */
        private readonly double[][] lu;
        /** Pivot permutation associated with LU decomposition. */
        private readonly int[] pivot;
        /** Parity of the permutation associated with the LU decomposition. */
        private bool even;
        /** Singularity indicator. */
        private readonly bool singular;
        /** Cached value of L. */
        private Array2DRowRealMatrix cachedL;
        /** Cached value of U. */
        private Array2DRowRealMatrix cachedU;
        /** Cached value of P. */
        private Array2DRowRealMatrix cachedP;

        public LUDecomposition(Array2DRowRealMatrix matrix) : this(matrix, DEFAULT_TOO_SMALL) { }

        public LUDecomposition(Array2DRowRealMatrix matrix, double singularityThreshold)
        {
            //     if (!matrix.isSquare()) {
            //    throw new NonSquareMatrixException(matrix.getRowDimension(),
            //                                       matrix.getColumnDimension());
            //}

            int m = matrix.getColumnDimension();
            lu = matrix.getData();
            pivot = new int[m];
            cachedL = null;
            cachedU = null;
            cachedP = null;

            // Initialize permutation array and parity
            for (int row = 0; row < m; row++)
            {
                pivot[row] = row;
            }
            even = true;
            singular = false;

            // Loop over columns
            for (int col = 0; col < m; col++)
            {

                // upper
                for (int row = 0; row < col; row++)
                {
                    double[] luRow = lu[row];
                    double sum = luRow[col];
                    for (int i = 0; i < row; i++)
                    {
                        sum -= luRow[i] * lu[i][col];
                    }
                    luRow[col] = sum;
                }

                // lower
                int max = col; // permutation row
                double largest = double.NegativeInfinity;
                for (int row = col; row < m; row++)
                {
                    double[] luRow = lu[row];
                    double sum = luRow[col];
                    for (int i = 0; i < col; i++)
                    {
                        sum -= luRow[i] * lu[i][col];
                    }
                    luRow[col] = sum;

                    // maintain best permutation choice
                    if (Math.Abs(sum) > largest)
                    {
                        largest = Math.Abs(sum);
                        max = row;
                    }
                }

                // Singularity check
                if (Math.Abs(lu[max][col]) < singularityThreshold)
                {
                    singular = true;
                    return;
                }

                // Pivot if necessary
                if (max != col)
                {
                    double tmp = 0;
                    double[] luMax = lu[max];
                    double[] luCol = lu[col];
                    for (int i = 0; i < m; i++)
                    {
                        tmp = luMax[i];
                        luMax[i] = luCol[i];
                        luCol[i] = tmp;
                    }
                    int temp = pivot[max];
                    pivot[max] = pivot[col];
                    pivot[col] = temp;
                    even = !even;
                }

                // Divide the lower elements by the "winning" diagonal elt.
                double luDiag = lu[col][col];
                for (int row = col + 1; row < m; row++)
                {
                    lu[row][col] /= luDiag;
                }
            }
        }

        public Solver getSolver()
        {
            return new Solver(lu, pivot, singular);
        }
    }
}
