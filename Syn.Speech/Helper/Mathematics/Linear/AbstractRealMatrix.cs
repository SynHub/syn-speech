using System;
using System.Collections.Generic;
using System.Text;
using Syn.Speech.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    /// <summary>
    /// Basic implementation of RealMatrix methods regardless of the underlying storage.
    /// </summary>
public abstract class AbstractRealMatrix : RealMatrix
{

    /** Default format. */
    //private const RealMatrixFormat DEFAULT_FORMAT = RealMatrixFormat.getInstance(Locale.US);
    static AbstractRealMatrix() {
        // set the minimum fraction digits to 1 to keep compatibility
        //DEFAULT_FORMAT.getFormat().setMinimumFractionDigits(1);
    }

    /**
     * Creates a matrix with no data
     */
    protected AbstractRealMatrix() {}

    /**
     * Create a new RealMatrix with the supplied row and column dimensions.
     *
     * @param rowDimension  the number of rows in the new matrix
     * @param columnDimension  the number of columns in the new matrix
     * @throws NotStrictlyPositiveException if row or column dimension is not positive
     */
    protected AbstractRealMatrix(int rowDimension, int columnDimension) {
        if (rowDimension < 1) {
            throw new Exception("NotStrictlyPositiveException");
        }
        if (columnDimension < 1) {
           throw new Exception("NotStrictlyPositiveException");
        }
    }

    /** {@inheritDoc} */
    public virtual RealMatrix add(RealMatrix m) {
        //MatrixUtils.checkAdditionCompatible(this, m);

        int rowCount    = getRowDimension();
        int columnCount = getColumnDimension();
        RealMatrix @out = createMatrix(rowCount, columnCount);
        for (int row = 0; row < rowCount; ++row) {
            for (int col = 0; col < columnCount; ++col) {
                @out.setEntry(row, col, getEntry(row, col) + m.getEntry(row, col));
            }
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual RealMatrix subtract(RealMatrix m){
        //MatrixUtils.checkSubtractionCompatible(this, m);

        int rowCount    = getRowDimension();
        int columnCount = getColumnDimension();
        RealMatrix @out = createMatrix(rowCount, columnCount);
        for (int row = 0; row < rowCount; ++row) {
            for (int col = 0; col < columnCount; ++col) {
                @out.setEntry(row, col, getEntry(row, col) - m.getEntry(row, col));
            }
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual RealMatrix scalarAdd(double d) {
        int rowCount    = getRowDimension();
        int columnCount = getColumnDimension();
        RealMatrix @out = createMatrix(rowCount, columnCount);
        for (int row = 0; row < rowCount; ++row) {
            for (int col = 0; col < columnCount; ++col) {
                @out.setEntry(row, col, getEntry(row, col) + d);
            }
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual RealMatrix scalarMultiply(double d) {
        int rowCount    = getRowDimension();
        int columnCount = getColumnDimension();
        RealMatrix @out = createMatrix(rowCount, columnCount);
        for (int row = 0; row < rowCount; ++row) {
            for (int col = 0; col < columnCount; ++col) {
                @out.setEntry(row, col, getEntry(row, col) * d);
            }
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual RealMatrix multiply(RealMatrix m){
        //MatrixUtils.checkMultiplicationCompatible(this, m);

        int nRows = getRowDimension();
         int nCols = m.getColumnDimension();
         int nSum  = getColumnDimension();
         RealMatrix @out = createMatrix(nRows, nCols);
        for (int row = 0; row < nRows; ++row) {
            for (int col = 0; col < nCols; ++col) {
                double sum = 0;
                for (int i = 0; i < nSum; ++i) {
                    sum += getEntry(row, i) * m.getEntry(i, col);
                }
                @out.setEntry(row, col, sum);
            }
        }

        return @out;
    }

    /** {@inheritDoc} */
    public RealMatrix preMultiply(RealMatrix m) {
        return m.multiply(this);
    }

    /** {@inheritDoc} */
    public RealMatrix power(int p){
        if (p < 0) {
            throw new Exception("NotPositiveException");
        }

        if (!isSquare()) {
            throw new Exception("NonSquareMatrixException");
        }

        if (p == 0) {
            return MatrixUtils.CreateRealIdentityMatrix(getRowDimension());
        }

        if (p == 1) {
            return copy();
        }

        int power = p - 1;

        /*
         * Only log_2(p) operations is used by doing as follows:
         * 5^214 = 5^128 * 5^64 * 5^16 * 5^4 * 5^2
         *
         * In general, the same approach is used for A^p.
         */


        char[] binaryRepresentation = Integer.ToBinaryString(power).ToCharArray();
        List<Integer> nonZeroPositions = new List<Integer>();
        int maxI = -1;

        for (int i = 0; i < binaryRepresentation.Length; ++i) {
            if (binaryRepresentation[i] == '1') {
                int pos = binaryRepresentation.Length - i - 1;
                nonZeroPositions.Add(pos);

                // The positions are taken in turn, so maxI is only changed once
                if (maxI == -1) {
                    maxI = pos;
                }
            }
        }

        RealMatrix[] results = new RealMatrix[maxI + 1];
        results[0] = copy();

        for (int i = 1; i <= maxI; ++i) {
            results[i] = results[i-1].multiply(results[i-1]);
        }

        RealMatrix result = copy();

        foreach (Integer i in nonZeroPositions) {
            result = result.multiply(results[i]);
        }

        return result;
    }

    /** {@inheritDoc} */
    public virtual double[][] getData()
    {
        var data = Java.CreateArray<double[][]>(getRowDimension(), getColumnDimension());

        for (int i = 0; i < data.Length; ++i) {
            double[] dataI = data[i];
            for (int j = 0; j < dataI.Length; ++j) {
                dataI[j] = getEntry(i, j);
            }
        }

        return data;
    }

    /** {@inheritDoc} */
    public virtual double getNorm()
    {
        return walkInColumnOrder(new FirstRealMatrixPreservingVisitor());
    }


    /** {@inheritDoc} */
    public virtual double getFrobeniusNorm()
    {
        return walkInOptimizedOrder(new SecondRealMatrixPreservingVisitor());
    }

    /** {@inheritDoc} */
    public virtual RealMatrix getSubMatrix( int startRow,  int endRow,
                                    int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);

         RealMatrix subMatrix =
            createMatrix(endRow - startRow + 1, endColumn - startColumn + 1);
        for (int i = startRow; i <= endRow; ++i) {
            for (int j = startColumn; j <= endColumn; ++j) {
                subMatrix.setEntry(i - startRow, j - startColumn, getEntry(i, j));
            }
        }

        return subMatrix;
    }

    /** {@inheritDoc} */
    public RealMatrix getSubMatrix( int[] selectedRows, int[] selectedColumns){

        //MatrixUtils.checkSubMatrixIndex(this, selectedRows, selectedColumns);

         RealMatrix subMatrix = createMatrix(selectedRows.Length, selectedColumns.Length);
        subMatrix.walkInOptimizedOrder(new FirstDefaultRealMatrixChangingVisitor(selectedRows, selectedColumns, this));

        return subMatrix;
    }

    /** {@inheritDoc} */
    public void copySubMatrix( int startRow,  int endRow,
                               int startColumn,  int endColumn,
                               double[][] destination) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
         int rowsCount    = endRow + 1 - startRow;
         int columnsCount = endColumn + 1 - startColumn;
        if ((destination.Length < rowsCount) || (destination[0].Length < columnsCount)) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        for (int i = 1; i < rowsCount; i++) {
            if (destination[i].Length < columnsCount) {
                throw new Exception("MatrixDimensionMismatchException");
            }
        }

        walkInOptimizedOrder(new FirstDefaultRealMatrixPreservingVisitor(destination), startRow, endRow, startColumn, endColumn);
    }

    /** {@inheritDoc} */
    public void copySubMatrix(int[] selectedRows, int[] selectedColumns,double[][] destination){
        //MatrixUtils.checkSubMatrixIndex(this, selectedRows, selectedColumns);
         int nCols = selectedColumns.Length;
        if ((destination.Length < selectedRows.Length) ||
            (destination[0].Length < nCols)) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        for (int i = 0; i < selectedRows.Length; i++) {
             double[] destinationI = destination[i];
            if (destinationI.Length < nCols) {
                throw new Exception("MatrixDimensionMismatchException");
            }
            for (int j = 0; j < selectedColumns.Length; j++) {
                destinationI[j] = getEntry(selectedRows[i], selectedColumns[j]);
            }
        }
    }

    /** {@inheritDoc} */
    public virtual void setSubMatrix( double[][] subMatrix,  int row,  int column){
        MathUtils.checkNotNull(subMatrix);
        int nRows = subMatrix.Length;
        if (nRows == 0) {
            throw new Exception("NoDataException");
        }

        int nCols = subMatrix[0].Length;
        if (nCols == 0) {
             throw new Exception("NoDataException");
        }

        for (int r = 1; r < nRows; ++r) {
            if (subMatrix[r].Length != nCols) {
                throw new Exception("DimensionMismatchException");
            }
        }

        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
        //MatrixUtils.checkRowIndex(this, nRows + row - 1);
        //MatrixUtils.checkColumnIndex(this, nCols + column - 1);

        for (int i = 0; i < nRows; ++i) {
            for (int j = 0; j < nCols; ++j) {
                setEntry(row + i, column + j, subMatrix[i][j]);
            }
        }
    }

    /** {@inheritDoc} */
    public virtual RealMatrix getRowMatrix(int row)  {
        //MatrixUtils.checkRowIndex(this, row);
         int nCols = getColumnDimension();
         RealMatrix @out = createMatrix(1, nCols);
        for (int i = 0; i < nCols; ++i) {
            @out.setEntry(0, i, getEntry(row, i));
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual void setRowMatrix(int row, RealMatrix matrix){
        //MatrixUtils.checkRowIndex(this, row);
        int nCols = getColumnDimension();
        if ((matrix.getRowDimension() != 1) ||
            (matrix.getColumnDimension() != nCols)) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nCols; ++i) {
            setEntry(row, i, matrix.getEntry(0, i));
        }
    }

    /** {@inheritDoc} */
    public virtual RealMatrix getColumnMatrix(int column) {
        //MatrixUtils.checkColumnIndex(this, column);
         int nRows = getRowDimension();
         RealMatrix @out = createMatrix(nRows, 1);
        for (int i = 0; i < nRows; ++i) {
            @out.setEntry(i, 0, getEntry(i, column));
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual void setColumnMatrix( int column,  RealMatrix matrix){
        //MatrixUtils.checkColumnIndex(this, column);
        int nRows = getRowDimension();
        if ((matrix.getRowDimension() != nRows) ||
            (matrix.getColumnDimension() != 1)) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nRows; ++i) {
            setEntry(i, column, matrix.getEntry(i, 0));
        }
    }

    /** {@inheritDoc} */
    public virtual RealVector getRowVector(int row) {
        return new ArrayRealVector(getRow(row), false);
    }

    /** {@inheritDoc} */
    public virtual void setRowVector (int row, RealVector vector){
        //MatrixUtils.checkRowIndex(this, row);
        int nCols = getColumnDimension();
        if (vector.getDimension() != nCols) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nCols; ++i) {
            setEntry(row, i, vector.getEntry(i));
        }
    }

    /** {@inheritDoc} */
    public virtual RealVector getColumnVector(int column) {
        return new ArrayRealVector(getColumn(column), false);
    }

    /** {@inheritDoc} */
    public virtual void setColumnVector( int column,  RealVector vector){
        //MatrixUtils.checkColumnIndex(this, column);
        int nRows = getRowDimension();
        if (vector.getDimension() != nRows) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nRows; ++i) {
            setEntry(i, column, vector.getEntry(i));
        }
    }

    /** {@inheritDoc} */
    public virtual double[] getRow(int row) {
        //MatrixUtils.checkRowIndex(this, row);
         int nCols = getColumnDimension();
         double[] @out = new double[nCols];
        for (int i = 0; i < nCols; ++i) {
            @out[i] = getEntry(row, i);
        }

        return @out;
    }



    /** {@inheritDoc} */
    public virtual void setRow( int row,  double[] array){
        //MatrixUtils.checkRowIndex(this, row);
        int nCols = getColumnDimension();
        if (array.Length != nCols) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nCols; ++i) {
            setEntry(row, i, array[i]);
        }
    }

    /** {@inheritDoc} */
    public virtual double[] getColumn( int column)  {
        //MatrixUtils.checkColumnIndex(this, column);
         int nRows = getRowDimension();
         double[] @out = new double[nRows];
        for (int i = 0; i < nRows; ++i) {
            @out[i] = getEntry(i, column);
        }

        return @out;
    }

    /** {@inheritDoc} */
    public virtual void setColumn( int column,  double[] array){
        //MatrixUtils.checkColumnIndex(this, column);
         int nRows = getRowDimension();
        if (array.Length != nRows) {
            throw new Exception("MatrixDimensionMismatchException");
        }
        for (int i = 0; i < nRows; ++i) {
            setEntry(i, column, array[i]);
        }
    }

    /** {@inheritDoc} */
    public virtual void addToEntry(int row, int column, double increment){
       // MatrixUtils.checkMatrixIndex(this, row, column);
        setEntry(row, column, getEntry(row, column) + increment);
    }

    /** {@inheritDoc} */
    public virtual void multiplyEntry(int row, int column, double factor) {
        //MatrixUtils.checkMatrixIndex(this, row, column);
        setEntry(row, column, getEntry(row, column) * factor);
    }

    /** {@inheritDoc} */
    public virtual RealMatrix transpose() {
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
         RealMatrix @out = createMatrix(nCols, nRows);
        walkInOptimizedOrder(new SecondDefaultRealMatrixPreservingVisitor(@out));

        return @out;
    }

    /** {@inheritDoc} */
    public bool isSquare() {
        return getColumnDimension() == getRowDimension();
    }

    /**
     * Returns the number of rows of this matrix.
     *
     * @return the number of rows.
     */

    public abstract int getRowDimension();

    /**
     * Returns the number of columns of this matrix.
     *
     * @return the number of columns.
     */

    public abstract int getColumnDimension();

    /** {@inheritDoc} */
    public double getTrace() {
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
        if (nRows != nCols) {
            throw new Exception("NonSquareMatrixException");
       }
        double trace = 0;
        for (int i = 0; i < nRows; ++i) {
            trace += getEntry(i, i);
        }
        return trace;
    }

    /** {@inheritDoc} */
    public virtual double[] operate( double[] v) {
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
        if (v.Length != nCols) {
            throw new Exception("DimensionMismatchException");
        }

        double[] @out = new double[nRows];
        for (int row = 0; row < nRows; ++row) {
            double sum = 0;
            for (int i = 0; i < nCols; ++i) {
                sum += getEntry(row, i) * v[i];
            }
            @out[row] = sum;
        }

        return @out;
    }


    public RealVector operate( RealVector v) {
        try {
            return new ArrayRealVector(operate(((ArrayRealVector) v).getDataRef()), false);
        } catch (Exception cce) {
             int nRows = getRowDimension();
             int nCols = getColumnDimension();
            if (v.getDimension() != nCols) {
                throw new Exception("DimensionMismatchException");
            }

             double[] @out = new double[nRows];
            for (int row = 0; row < nRows; ++row) {
                double sum = 0;
                for (int i = 0; i < nCols; ++i) {
                    sum += getEntry(row, i) * v.getEntry(i);
                }
                @out[row] = sum;
            }

            return new ArrayRealVector(@out, false);
        }
    }

    /** {@inheritDoc} */
    public virtual double[] preMultiply(double[] v) {

         int nRows = getRowDimension();
         int nCols = getColumnDimension();
        if (v.Length != nRows) {
            throw new Exception("DimensionMismatchException");
        }

         double[] @out = new double[nCols];
        for (int col = 0; col < nCols; ++col) {
            double sum = 0;
            for (int i = 0; i < nRows; ++i) {
                sum += getEntry(i, col) * v[i];
            }
            @out[col] = sum;
        }

        return @out;
    }

    /** {@inheritDoc} */
    public RealVector preMultiply( RealVector v)  {
        try {
            return new ArrayRealVector(preMultiply(((ArrayRealVector) v).getDataRef()), false);
        } catch (Exception cce) {

             int nRows = getRowDimension();
             int nCols = getColumnDimension();
            if (v.getDimension() != nRows) {
                throw new Exception("DimensionMismatchException");
            }

             double[] @out = new double[nCols];
            for (int col = 0; col < nCols; ++col) {
                double sum = 0;
                for (int i = 0; i < nRows; ++i) {
                    sum += getEntry(i, col) * v.getEntry(i);
                }
                @out[col] = sum;
            }

            return new ArrayRealVector(@out, false);
        }
    }

    /** {@inheritDoc} */
    public virtual double walkInRowOrder( RealMatrixChangingVisitor visitor) {
         int rows    = getRowDimension();
         int columns = getColumnDimension();
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int row = 0; row < rows; ++row) {
            for (int column = 0; column < columns; ++column) {
                 double oldValue = getEntry(row, column);
                 double newValue = visitor.visit(row, column, oldValue);
                setEntry(row, column, newValue);
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInRowOrder( RealMatrixPreservingVisitor visitor) {
         int rows    = getRowDimension();
         int columns = getColumnDimension();
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int row = 0; row < rows; ++row) {
            for (int column = 0; column < columns; ++column) {
                visitor.visit(row, column, getEntry(row, column));
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInRowOrder( RealMatrixChangingVisitor visitor,
                                  int startRow,  int endRow,
                                  int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(getRowDimension(), getColumnDimension(),
                      startRow, endRow, startColumn, endColumn);
        for (int row = startRow; row <= endRow; ++row) {
            for (int column = startColumn; column <= endColumn; ++column) {
                 double oldValue = getEntry(row, column);
                 double newValue = visitor.visit(row, column, oldValue);
                setEntry(row, column, newValue);
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInRowOrder( RealMatrixPreservingVisitor visitor,
                                  int startRow,  int endRow,
                                  int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(getRowDimension(), getColumnDimension(),
                      startRow, endRow, startColumn, endColumn);
        for (int row = startRow; row <= endRow; ++row) {
            for (int column = startColumn; column <= endColumn; ++column) {
                visitor.visit(row, column, getEntry(row, column));
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInColumnOrder( RealMatrixChangingVisitor visitor) {
         int rows    = getRowDimension();
         int columns = getColumnDimension();
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int column = 0; column < columns; ++column) {
            for (int row = 0; row < rows; ++row) {
                 double oldValue = getEntry(row, column);
                 double newValue = visitor.visit(row, column, oldValue);
                setEntry(row, column, newValue);
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInColumnOrder( RealMatrixPreservingVisitor visitor) {
         int rows    = getRowDimension();
         int columns = getColumnDimension();
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int column = 0; column < columns; ++column) {
            for (int row = 0; row < rows; ++row) {
                visitor.visit(row, column, getEntry(row, column));
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInColumnOrder( RealMatrixChangingVisitor visitor,
                                     int startRow,  int endRow,
                                     int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(getRowDimension(), getColumnDimension(),
                      startRow, endRow, startColumn, endColumn);
        for (int column = startColumn; column <= endColumn; ++column) {
            for (int row = startRow; row <= endRow; ++row) {
                 double oldValue = getEntry(row, column);
                 double newValue = visitor.visit(row, column, oldValue);
                setEntry(row, column, newValue);
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInColumnOrder( RealMatrixPreservingVisitor visitor,
                                     int startRow,  int endRow,
                                     int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(getRowDimension(), getColumnDimension(),
                      startRow, endRow, startColumn, endColumn);
        for (int column = startColumn; column <= endColumn; ++column) {
            for (int row = startRow; row <= endRow; ++row) {
                visitor.visit(row, column, getEntry(row, column));
            }
        }
        return visitor.end();
    }

    /** {@inheritDoc} */
    public virtual double walkInOptimizedOrder( RealMatrixChangingVisitor visitor) {
        return walkInRowOrder(visitor);
    }

    /** {@inheritDoc} */
    public virtual double walkInOptimizedOrder( RealMatrixPreservingVisitor visitor) {
        return walkInRowOrder(visitor);
    }

    /** {@inheritDoc} */
    public virtual double walkInOptimizedOrder( RealMatrixChangingVisitor visitor,
                                        int startRow,  int endRow,
                                        int startColumn,
                                        int endColumn){
        return walkInRowOrder(visitor, startRow, endRow, startColumn, endColumn);
    }

    /** {@inheritDoc} */
    public virtual double walkInOptimizedOrder( RealMatrixPreservingVisitor visitor,
                                        int startRow,  int endRow,
                                        int startColumn,
                                        int endColumn){
        return walkInRowOrder(visitor, startRow, endRow, startColumn, endColumn);
    }

    /**
     * Get a string representation for this matrix.
     * @return a string representation for this matrix
     */

    public override String ToString() {
        var res = new StringBuilder();
        String fullClassName = GetType().Name; //getClass().getName();
        String shortClassName = fullClassName.Substring(fullClassName.LastIndexOf('.') + 1);
        res.Append(shortClassName);
       // res.Append(DEFAULT_FORMAT.format(this));//TODO: Can use CultureInfo here
        return res.ToString();
    }

    /**
     * Returns true iff <code>object</code> is a
     * <code>RealMatrix</code> instance with the same dimensions as this
     * and all corresponding matrix entries are equal.
     *
     * @param object the object to test equality against.
     * @return true if object equals this
     */
    public override bool Equals( Object value) {
        if (value == this ) {
            return true;
        }
        if (value is RealMatrix == false) {
            return false;
        }
        RealMatrix m = (RealMatrix) value;
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
        if (m.getColumnDimension() != nCols || m.getRowDimension() != nRows) {
            return false;
        }
        for (int row = 0; row < nRows; ++row) {
            for (int col = 0; col < nCols; ++col) {
                if (getEntry(row, col) != m.getEntry(row, col)) {
                    return false;
                }
            }
        }
        return true;
    }

    /**
     * Computes a hashcode for the matrix.
     *
     * @return hashcode for matrix
     */

    public override int GetHashCode() {
        int ret = 7;
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
        ret = ret * 31 + nRows;
        ret = ret * 31 + nCols;
        for (int row = 0; row < nRows; ++row) {
            for (int col = 0; col < nCols; ++col) {
               ret = ret * 31 + (11 * (row+1) + 17 * (col+1)) *
                   MathUtils.hash(getEntry(row, col));
           }
        }
        return ret;
    }


    /*
     * Empty implementations of these methods are provided in order to allow for
     * the use of the @Override tag with Java 1.5.
     */

    /** {@inheritDoc} */
    public abstract RealMatrix createMatrix(int rowDimension, int columnDimension);

    /** {@inheritDoc} */
    public abstract RealMatrix copy();

    /** {@inheritDoc} */
    public abstract double getEntry(int row, int column);

    /** {@inheritDoc} */
    public abstract void setEntry(int row, int column, double value);

    public void superSetSubMatrix(double[][] subMatrix, int row, int column)
    {
        MathUtils.checkNotNull(subMatrix);
        int nRows = subMatrix.Length;
        if (nRows == 0)
        {
            throw new Exception("NoDataException");
        }

        int nCols = subMatrix[0].Length;
        if (nCols == 0)
        {
            throw new Exception("NoDataException");
        }

        for (int r = 1; r < nRows; ++r)
        {
            if (subMatrix[r].Length != nCols)
            {
                throw new Exception("DimensionMismatchException");
            }
        }

        //MatrixUtils.checkRowIndex(this, row);
        //MatrixUtils.checkColumnIndex(this, column);
        //MatrixUtils.checkRowIndex(this, nRows + row - 1);
        //MatrixUtils.checkColumnIndex(this, nCols + column - 1);

        for (int i = 0; i < nRows; ++i)
        {
            for (int j = 0; j < nCols; ++j)
            {
                setEntry(row + i, column + j, subMatrix[i][j]);
            }
        }
    }

}

#region Custom

    public class SecondDefaultRealMatrixPreservingVisitor : DefaultRealMatrixPreservingVisitor{

            /** {@inheritDoc} */
        private readonly RealMatrix _realMatrix;
        public SecondDefaultRealMatrixPreservingVisitor(RealMatrix matrix)
        {
            _realMatrix = matrix;
        }

            public override void visit( int row,  int column,  double value) {
                _realMatrix.setEntry(column, row, value);
            }

        }
public class FirstRealMatrixPreservingVisitor: RealMatrixPreservingVisitor {

            /** Last row index. */
            private double endRow;

            /** Sum of absolute values on one column. */
            private double columnSum;

            /** Maximal sum across all columns. */
            private double maxColSum;

            /** {@inheritDoc} */
            public void start( int rows,  int columns,
                               int startRow,  int endRow,
                               int startColumn,  int endColumn) {
                this.endRow = endRow;
                columnSum   = 0;
                maxColSum   = 0;
            }

            /** {@inheritDoc} */
            public void visit( int row,  int column,  double value) {
                columnSum += Math.Abs(value);
                if (row == endRow) {
                    maxColSum = Math.Max(maxColSum, columnSum);
                    columnSum = 0;
                }
            }

            /** {@inheritDoc} */
            public double end() {
                return maxColSum;
            }
        }

public class FirstDefaultRealMatrixPreservingVisitor: DefaultRealMatrixPreservingVisitor
{

    private readonly double[][] _parentDestination;

            /** Initial row index. */
            private int startRow;

            /** Initial column index. */
            private int startColumn;

            /** {@inheritDoc} */

    public FirstDefaultRealMatrixPreservingVisitor(double[][] destination)
    {
        _parentDestination = destination;
    }

            public override void start( int rows,  int columns,
                               int startRow,  int endRow,
                               int startColumn,  int endColumn) {
                this.startRow    = startRow;
                this.startColumn = startColumn;
            }

            /** {@inheritDoc} */

            public override void visit( int row,  int column,  double value) {
                _parentDestination[row - startRow][column - startColumn] = value;
            }

        }

public class SecondRealMatrixPreservingVisitor :RealMatrixPreservingVisitor  {

            /** Sum of squared entries. */
            private double sum;

            /** {@inheritDoc} */
            public void start( int rows,  int columns,
                               int startRow,  int endRow,
                               int startColumn,  int endColumn) {
                sum = 0;
            }

            /** {@inheritDoc} */
            public void visit( int row,  int column,  double value) {
                sum += value * value;
            }

            /** {@inheritDoc} */
            public double end() {
                return Math.Sqrt(sum);
            }
        }

public class FirstDefaultRealMatrixChangingVisitor: DefaultRealMatrixChangingVisitor {

            /** {@inheritDoc} */

    private readonly int[] _selectedRows;
    private readonly int[] _selectedColumns;
    private readonly AbstractRealMatrix _parent;
    public FirstDefaultRealMatrixChangingVisitor(int[] selectedRows, int[] selectedColumns, AbstractRealMatrix parent)
    {
        _parent = parent;
        _selectedRows = selectedRows;
        _selectedColumns = selectedColumns;
    }

            public override double visit( int row,  int column,  double value) {
                return _parent.getEntry(_selectedRows[row], _selectedColumns[column]);
            }

        }
#endregion

}
