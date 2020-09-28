using System;
using Syn.Speech.Helper.Mathematics.Util;
using Syn.Speech.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{

    class HessenbergTransformer
    {
        /** Householder vectors. */
        private readonly double[][] householderVectors;
        /** Temporary storage vector. */
        private readonly double[] ort;
        /** Cached value of P. */
        private RealMatrix cachedP;
        /** Cached value of Pt. */
        private RealMatrix cachedPt;
        /** Cached value of H. */
        private RealMatrix cachedH;

        /**
         * Build the transformation to Hessenberg form of a general matrix.
         *
         * @param matrix matrix to transform
         * @throws NonSquareMatrixException if the matrix is not square
         */
        public HessenbergTransformer(RealMatrix matrix)
        {
            if (!matrix.isSquare())
            {
                throw new Exception("NonSquareMatrixException");
            }

            int m = matrix.getRowDimension();
            householderVectors = matrix.getData();
            ort = new double[m];
            cachedP = null;
            cachedPt = null;
            cachedH = null;

            // transform matrix
            transform();
        }

        /**
         * Returns the matrix P of the transform.
         * <p>P is an orthogonal matrix, i.e. its inverse is also its transpose.</p>
         *
         * @return the P matrix
         */
        public RealMatrix getP()
        {
            if (cachedP == null)
            {
                int n = householderVectors.Length;
                int high = n - 1;
                double[][] pa = Java.CreateArray<double[][]>(n, n);// new double[n][n];

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        pa[i][j] = (i == j) ? 1 : 0;
                    }
                }

                for (int m = high - 1; m >= 1; m--)
                {
                    if (householderVectors[m][m - 1] != 0.0)
                    {
                        for (int i = m + 1; i <= high; i++)
                        {
                            ort[i] = householderVectors[i][m - 1];
                        }

                        for (int j = m; j <= high; j++)
                        {
                            double g = 0.0;

                            for (int i = m; i <= high; i++)
                            {
                                g += ort[i] * pa[i][j];
                            }

                            // Double division avoids possible underflow
                            g = (g / ort[m]) / householderVectors[m][m - 1];

                            for (int i = m; i <= high; i++)
                            {
                                pa[i][j] += g * ort[i];
                            }
                        }
                    }
                }

                cachedP = MatrixUtils.CreateRealMatrix(pa);
            }
            return cachedP;
        }

        /**
         * Returns the transpose of the matrix P of the transform.
         * <p>P is an orthogonal matrix, i.e. its inverse is also its transpose.</p>
         *
         * @return the transpose of the P matrix
         */
        public RealMatrix getPT()
        {
            if (cachedPt == null)
            {
                cachedPt = getP().transpose();
            }

            // return the cached matrix
            return cachedPt;
        }

        /**
         * Returns the Hessenberg matrix H of the transform.
         *
         * @return the H matrix
         */
        public RealMatrix getH()
        {
            if (cachedH == null)
            {
                int m = householderVectors.Length;
                double[][] h = Java.CreateArray<double[][]>(m, m);// new double[m][m];
                for (int i = 0; i < m; ++i)
                {
                    if (i > 0)
                    {
                        // copy the entry of the lower sub-diagonal
                        h[i][i - 1] = householderVectors[i][i - 1];
                    }

                    // copy upper triangular part of the matrix
                    for (int j = i; j < m; ++j)
                    {
                        h[i][j] = householderVectors[i][j];
                    }
                }
                cachedH = MatrixUtils.CreateRealMatrix(h);
            }

            // return the cached matrix
            return cachedH;
        }

        /**
         * Get the Householder vectors of the transform.
         * <p>Note that since this class is only intended for internal use, it returns
         * directly a reference to its internal arrays, not a copy.</p>
         *
         * @return the main diagonal elements of the B matrix
         */
        double[][] getHouseholderVectorsRef()
        {
            return householderVectors;
        }

        /**
         * Transform original matrix to Hessenberg form.
         * <p>Transformation is done using Householder transforms.</p>
         */
        private void transform()
        {
            int n = householderVectors.Length;
            int high = n - 1;

            for (int m = 1; m <= high - 1; m++)
            {
                // Scale column.
                double scale = 0;
                for (int i = m; i <= high; i++)
                {
                    scale += Math.Abs(householderVectors[i][m - 1]);
                }

                if (!Precision.equals(scale, 0))
                {
                    // Compute Householder transformation.
                    double h = 0;
                    for (int i = high; i >= m; i--)
                    {
                        ort[i] = householderVectors[i][m - 1] / scale;
                        h += ort[i] * ort[i];
                    }
                    double g = (ort[m] > 0) ? -Math.Sqrt(h) : Math.Sqrt(h);

                    h -= ort[m] * g;
                    ort[m] -= g;

                    // Apply Householder similarity transformation
                    // H = (I - u*u' / h) * H * (I - u*u' / h)

                    for (int j = m; j < n; j++)
                    {
                        double f = 0;
                        for (int i = high; i >= m; i--)
                        {
                            f += ort[i] * householderVectors[i][j];
                        }
                        f /= h;
                        for (int i = m; i <= high; i++)
                        {
                            householderVectors[i][j] -= f * ort[i];
                        }
                    }

                    for (int i = 0; i <= high; i++)
                    {
                        double f = 0;
                        for (int j = high; j >= m; j--)
                        {
                            f += ort[j] * householderVectors[i][j];
                        }
                        f /= h;
                        for (int j = m; j <= high; j++)
                        {
                            householderVectors[i][j] -= f * ort[j];
                        }
                    }

                    ort[m] = scale * ort[m];
                    householderVectors[m][m - 1] = scale * g;
                }
            }
        }
    }
}
