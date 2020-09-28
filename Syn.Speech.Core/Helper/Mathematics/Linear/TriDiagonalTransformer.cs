using System;
using Syn.Speech.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    
class TriDiagonalTransformer {
    /** Householder vectors. */
    private double[][] householderVectors;
    /** Main diagonal. */
    private  double[] main;
    /** Secondary diagonal. */
    private  double[] secondary;
    /** Cached value of Q. */
    private RealMatrix cachedQ;
    /** Cached value of Qt. */
    private RealMatrix cachedQt;
    /** Cached value of T. */
    private RealMatrix cachedT;

    /**
     * Build the transformation to tridiagonal shape of a symmetrical matrix.
     * <p>The specified matrix is assumed to be symmetrical without any check.
     * Only the upper triangular part of the matrix is used.</p>
     *
     * @param matrix Symmetrical matrix to transform.
     * @throws NonSquareMatrixException if the matrix is not square.
     */
    public TriDiagonalTransformer(RealMatrix matrix) {
        if (!matrix.isSquare()) {
            throw new Exception("NonSquareMatrixException");
        }

        int m = matrix.getRowDimension();
        householderVectors = matrix.getData();
        main      = new double[m];
        secondary = new double[m - 1];
        cachedQ   = null;
        cachedQt  = null;
        cachedT   = null;

        // transform matrix
        transform();
    }

    /**
     * Returns the matrix Q of the transform.
     * <p>Q is an orthogonal matrix, i.e. its transpose is also its inverse.</p>
     * @return the Q matrix
     */
    public RealMatrix getQ() {
        if (cachedQ == null) {
            cachedQ = getQT().transpose();
        }
        return cachedQ;
    }

    /**
     * Returns the transpose of the matrix Q of the transform.
     * <p>Q is an orthogonal matrix, i.e. its transpose is also its inverse.</p>
     * @return the Q matrix
     */
    public RealMatrix getQT() {
        if (cachedQt == null) {
             int m = householderVectors.Length;
            var qta = Java.CreateArray<double[][]>(m, m);

            // build up first part of the matrix by applying Householder transforms
            for (int k = m - 1; k >= 1; --k) {
                 double[] hK = householderVectors[k - 1];
                qta[k][k] = 1;
                if (hK[k] != 0.0) {
                     double inv = 1.0 / (secondary[k - 1] * hK[k]);
                    double beta = 1.0 / secondary[k - 1];
                    qta[k][k] = 1 + beta * hK[k];
                    for (int i = k + 1; i < m; ++i) {
                        qta[k][i] = beta * hK[i];
                    }
                    for (int j = k + 1; j < m; ++j) {
                        beta = 0;
                        for (int i = k + 1; i < m; ++i) {
                            beta += qta[j][i] * hK[i];
                        }
                        beta *= inv;
                        qta[j][k] = beta * hK[k];
                        for (int i = k + 1; i < m; ++i) {
                            qta[j][i] += beta * hK[i];
                        }
                    }
                }
            }
            qta[0][0] = 1;
            cachedQt = MatrixUtils.CreateRealMatrix(qta);
        }

        // return the cached matrix
        return cachedQt;
    }

    /**
     * Returns the tridiagonal matrix T of the transform.
     * @return the T matrix
     */
    public RealMatrix getT() {
        if (cachedT == null) {
            int m = main.Length;
            double[][] ta = Java.CreateArray<double[][]>(m, m);
            for (int i = 0; i < m; ++i) {
                ta[i][i] = main[i];
                if (i > 0) {
                    ta[i][i - 1] = secondary[i - 1];
                }
                if (i < main.Length - 1) {
                    ta[i][i + 1] = secondary[i];
                }
            }
            cachedT = MatrixUtils.CreateRealMatrix(ta);
        }

        // return the cached matrix
        return cachedT;
    }

    /**
     * Get the Householder vectors of the transform.
     * <p>Note that since this class is only intended for internal use,
     * it returns directly a reference to its internal arrays, not a copy.</p>
     * @return the main diagonal elements of the B matrix
     */
    double[][] getHouseholderVectorsRef() {
        return householderVectors;
    }

    /**
     * Get the main diagonal elements of the matrix T of the transform.
     * <p>Note that since this class is only intended for internal use,
     * it returns directly a reference to its internal arrays, not a copy.</p>
     * @return the main diagonal elements of the T matrix
     */

    public double[] getMainDiagonalRef() {
        return main;
    }

    /**
     * Get the secondary diagonal elements of the matrix T of the transform.
     * <p>Note that since this class is only intended for internal use,
     * it returns directly a reference to its internal arrays, not a copy.</p>
     * @return the secondary diagonal elements of the T matrix
     */

    public double[] getSecondaryDiagonalRef() {
        return secondary;
    }

    /**
     * Transform original matrix to tridiagonal form.
     * <p>Transformation is done using Householder transforms.</p>
     */
    private void transform() {
         int m = householderVectors.Length;
         double[] z = new double[m];
        for (int k = 0; k < m - 1; k++) {

            //zero-out a row and a column simultaneously
             double[] hK = householderVectors[k];
            main[k] = hK[k];
            double xNormSqr = 0;
            for (int j = k + 1; j < m; ++j) {
                 double c = hK[j];
                xNormSqr += c * c;
            }
             double a = (hK[k + 1] > 0) ? -Math.Sqrt(xNormSqr) : Math.Sqrt(xNormSqr);
            secondary[k] = a;
            if (a != 0.0) {
                // apply Householder transform from left and right simultaneously

                hK[k + 1] -= a;
                 double beta = -1 / (a * hK[k + 1]);

                // compute a = beta A v, where v is the Householder vector
                // this loop is written in such a way
                //   1) only the upper triangular part of the matrix is accessed
                //   2) access is cache-friendly for a matrix stored in rows
                Arrays.Fill(z, k + 1, m, 0);
                for (int i = k + 1; i < m; ++i) {
                     double[] hI = householderVectors[i];
                     double hKI = hK[i];
                    double zI = hI[i] * hKI;
                    for (int j = i + 1; j < m; ++j) {
                         double hIJ = hI[j];
                        zI   += hIJ * hK[j];
                        z[j] += hIJ * hKI;
                    }
                    z[i] = beta * (z[i] + zI);
                }

                // compute gamma = beta vT z / 2
                double gamma = 0;
                for (int i = k + 1; i < m; ++i) {
                    gamma += z[i] * hK[i];
                }
                gamma *= beta / 2;

                // compute z = z - gamma v
                for (int i = k + 1; i < m; ++i) {
                    z[i] -= gamma * hK[i];
                }

                // update matrix: A = A - v zT - z vT
                // only the upper triangular part of the matrix is updated
                for (int i = k + 1; i < m; ++i) {
                     double[] hI = householderVectors[i];
                    for (int j = i; j < m; ++j) {
                        hI[j] -= hK[i] * z[j] + z[i] * hK[j];
                    }
                }
            }
        }
        main[m - 1] = householderVectors[m - 1][m - 1];
    }
}

}
