namespace Syn.Speech.Helper.Mathematics.Linear
{
    public class Solver
    {
        /** Entries of LU decomposition. */
        private readonly double[][] lu;

        /** Pivot permutation associated with LU decomposition. */
        private readonly int[] pivot;

        /** Singularity indicator. */
        private bool singular;

        internal Solver(double[][] lu, int[] pivot, bool singular)
        {
            this.lu = lu;
            this.pivot = pivot;
            this.singular = singular;
        }

        public ArrayRealVector solve(ArrayRealVector b)
        {
             int m = pivot.Length;
             //if (b.getDimension() != m)
             //{
             //    throw new DimensionMismatchException(b.getDimension(), m);
             //}
             //if (singular)
             //{
             //    throw new SingularMatrixException();
             //}

            double[] bp = new double[m];

            // Apply permutations to b
            for (int row = 0; row < m; row++) {
                bp[row] = b.getEntry(pivot[row]);
            }

            // Solve LY = b
            for (int col = 0; col < m; col++) {
                double bpCol = bp[col];
                for (int i = col + 1; i < m; i++) {
                    bp[i] -= bpCol * lu[i][col];
                }
            }

            // Solve UX = Y
            for (int col = m - 1; col >= 0; col--) {
                bp[col] /= lu[col][col];
                 double bpCol = bp[col];
                for (int i = 0; i < col; i++) {
                    bp[i] -= bpCol * lu[i][col];
                }
            }

            return new ArrayRealVector(bp, false);
        }
    }
}
