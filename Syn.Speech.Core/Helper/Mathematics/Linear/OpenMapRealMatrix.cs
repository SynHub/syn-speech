using System;
using System.Runtime.Serialization;
using Syn.Speech.Helper.Mathematics.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    
public class OpenMapRealMatrix : AbstractRealMatrix, SparseRealMatrix, ISerializable {

    /** Number of rows of the matrix. */
    private readonly int rows;
    /** Number of columns of the matrix. */
    private readonly int columns;
    /** Storage for (sparse) matrix elements. */
    private readonly OpenIntToDoubleHashMap entries;

    /**
     * Build a sparse matrix with the supplied row and column dimensions.
     *
     * @param rowDimension Number of rows of the matrix.
     * @param columnDimension Number of columns of the matrix.
     * @throws NotStrictlyPositiveException if row or column dimension is not
     * positive.
     * @throws NumberIsTooLargeException if the total number of entries of the
     * matrix is larger than {@code Integer.MAX_VALUE}.
     */
    public OpenMapRealMatrix(int rowDimension, int columnDimension) :base(rowDimension, columnDimension){
        long lRow = rowDimension;
        long lCol = columnDimension;
        if (lRow * lCol >= Integer.MAX_VALUE) {
            throw new Exception("NumberIsTooLargeException");
        }
        rows = rowDimension;
        columns = columnDimension;
        entries = new OpenIntToDoubleHashMap(0.0);
    }

    /**
     * Build a matrix by copying another one.
     *
     * @param matrix matrix to copy.
     */
    public OpenMapRealMatrix(OpenMapRealMatrix matrix) {
        rows = matrix.rows;
        columns = matrix.columns;
        entries = new OpenIntToDoubleHashMap(matrix.entries);
    }

    /** {@inheritDoc} */

    public override RealMatrix copy() {
        return new OpenMapRealMatrix(this);
    }

    /**
     * {@inheritDoc}
     *
     * @throws NumberIsTooLargeException if the total number of entries of the
     * matrix is larger than {@code Integer.MAX_VALUE}.
     */

    public override RealMatrix createMatrix(int rowDimension, int columnDimension) {
        return new OpenMapRealMatrix(rowDimension, columnDimension);
    }

    /** {@inheritDoc} */

    public override int getColumnDimension() {
        return columns;
    }

    /**
     * Compute the sum of this matrix and {@code m}.
     *
     * @param m Matrix to be added.
     * @return {@code this} + {@code m}.
     * @throws MatrixDimensionMismatchException if {@code m} is not the same
     * size as {@code this}.
     */
    public OpenMapRealMatrix add(OpenMapRealMatrix m){
        //MatrixUtils.checkAdditionCompatible(this, m);

         OpenMapRealMatrix @out = new OpenMapRealMatrix(this);
        for (OpenIntToDoubleHashMap.Iterator iterator = m.entries.iterator(); iterator.hasNext();) {
            iterator.advance();
             int row = iterator.key() / columns;
             int col = iterator.key() - row * columns;
            @out.setEntry(row, col, getEntry(row, col) + iterator.value());
        }

        return @out;

    }

    /** {@inheritDoc} */

    public override RealMatrix subtract(RealMatrix m) /*Todo: Supposed to return OpenMapRealMatrix */ {
        try {
            return subtract((OpenMapRealMatrix) m);
        } catch (InvalidCastException cce) {
            return (OpenMapRealMatrix) base.subtract(m);
        }
    }

    /**
     * Subtract {@code m} from this matrix.
     *
     * @param m Matrix to be subtracted.
     * @return {@code this} - {@code m}.
     * @throws MatrixDimensionMismatchException if {@code m} is not the same
     * size as {@code this}.
     */
    public OpenMapRealMatrix subtract(OpenMapRealMatrix m) {
        //MatrixUtils.checkAdditionCompatible(this, m);

         OpenMapRealMatrix @out = new OpenMapRealMatrix(this);
        for (OpenIntToDoubleHashMap.Iterator iterator = m.entries.iterator(); iterator.hasNext();) {
            iterator.advance();
             int row = iterator.key() / columns;
             int col = iterator.key() - row * columns;
            @out.setEntry(row, col, getEntry(row, col) - iterator.value());
        }

        return @out;
    }

    /**
     * {@inheritDoc}
     *
     * @throws NumberIsTooLargeException if {@code m} is an
     * {@code OpenMapRealMatrix}, and the total number of entries of the product
     * is larger than {@code Integer.MAX_VALUE}.
     */
    public override RealMatrix multiply(RealMatrix m){
        try {
            return multiply((OpenMapRealMatrix) m);
        } catch (InvalidCastException cce) {

            //MatrixUtils.checkMultiplicationCompatible(this, m);

             int outCols = m.getColumnDimension();
             BlockRealMatrix @out = new BlockRealMatrix(rows, outCols);
            for (OpenIntToDoubleHashMap.Iterator iterator = entries.iterator(); iterator.hasNext();) {
                iterator.advance();
                 double value = iterator.value();
                 int key      = iterator.key();
                 int i        = key / columns;
                 int k        = key % columns;
                for (int j = 0; j < outCols; ++j) {
                    @out.addToEntry(i, j, value * m.getEntry(k, j));
                }
            }

            return @out;
        }
    }

    /**
     * Postmultiply this matrix by {@code m}.
     *
     * @param m Matrix to postmultiply by.
     * @return {@code this} * {@code m}.
     * @throws DimensionMismatchException if the number of rows of {@code m}
     * differ from the number of columns of {@code this} matrix.
     * @throws NumberIsTooLargeException if the total number of entries of the
     * product is larger than {@code Integer.MAX_VALUE}.
     */
    public OpenMapRealMatrix multiply(OpenMapRealMatrix m){
        // Safety check.
        //MatrixUtils.checkMultiplicationCompatible(this, m);

         int outCols = m.getColumnDimension();
        OpenMapRealMatrix @out = new OpenMapRealMatrix(rows, outCols);
        for (OpenIntToDoubleHashMap.Iterator iterator = entries.iterator(); iterator.hasNext();) {
            iterator.advance();
             double value = iterator.value();
             int key      = iterator.key();
             int i        = key / columns;
             int k        = key % columns;
            for (int j = 0; j < outCols; ++j) {
                 int rightKey = m.computeKey(k, j);
                if (m.entries.containsKey(rightKey)) {
                     int outKey = @out.computeKey(i, j);
                     double outValue =
                        @out.entries.get(outKey) + value * m.entries.get(rightKey);
                    if (outValue == 0.0) {
                        @out.entries.remove(outKey);
                    } else {
                        @out.entries.put(outKey, outValue);
                    }
                }
            }
        }

        return @out;
    }

    /** {@inheritDoc} */

    public override double getEntry(int row, int column) {
        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
        return entries.get(computeKey(row, column));
    }

    /** {@inheritDoc} */

    public override int getRowDimension() {
        return rows;
    }

    /** {@inheritDoc} */

    public override void setEntry(int row, int column, double value){
        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
        if (value == 0.0) {
            entries.remove(computeKey(row, column));
        } else {
            entries.put(computeKey(row, column), value);
        }
    }

    /** {@inheritDoc} */

    public override void addToEntry(int row, int column, double increment) {
        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
         int key = computeKey(row, column);
         double value = entries.get(key) + increment;
        if (value == 0.0) {
            entries.remove(key);
        } else {
            entries.put(key, value);
        }
    }

    /** {@inheritDoc} */

    public override void multiplyEntry(int row, int column, double factor) {
        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
         int key = computeKey(row, column);
         double value = entries.get(key) * factor;
        if (value == 0.0) {
            entries.remove(key);
        } else {
            entries.put(key, value);
        }
    }

    /**
     * Compute the key to access a matrix element
     * @param row row index of the matrix element
     * @param column column index of the matrix element
     * @return key within the map to access the matrix element
     */
    private int computeKey(int row, int column) {
        return row * columns + column;
    }


    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }
}

}
