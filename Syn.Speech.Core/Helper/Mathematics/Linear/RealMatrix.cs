//PATROLLED

namespace Syn.Speech.Helper.Mathematics.Linear
{
    public interface RealMatrix : AnyMatrix
    {
        RealMatrix createMatrix(int rowDimension, int columnDimension);

        RealMatrix copy();

        RealMatrix add(RealMatrix m);

        RealMatrix subtract(RealMatrix m);

        RealMatrix scalarAdd(double d);

        RealMatrix scalarMultiply(double d);

        RealMatrix multiply(RealMatrix m);

        RealMatrix preMultiply(RealMatrix m);

        RealMatrix power(int p);

        double[][] getData();

        double getNorm();

        double getFrobeniusNorm();

        RealMatrix getSubMatrix(int startRow, int endRow, int startColumn, int endColumn);

        RealMatrix getSubMatrix(int[] selectedRows, int[] selectedColumns);

        void copySubMatrix(int startRow, int endRow, int startColumn, int endColumn, double[][] destination);

        void copySubMatrix(int[] selectedRows, int[] selectedColumns, double[][] destination);

        void setSubMatrix(double[][] subMatrix, int row, int column);

        RealMatrix getRowMatrix(int row);

        void setRowMatrix(int row, RealMatrix matrix);

        RealMatrix getColumnMatrix(int column);

        void setColumnMatrix(int column, RealMatrix matrix);

        RealVector getRowVector(int row);

        void setRowVector(int row, RealVector vector);

        RealVector getColumnVector(int column);

        void setColumnVector(int column, RealVector vector);

        double[] getRow(int row);

        void setRow(int row, double[] array);

        double[] getColumn(int column);

        void setColumn(int column, double[] array);

        double getEntry(int row, int column);

        void setEntry(int row, int column, double value);

        void addToEntry(int row, int column, double increment);

        void multiplyEntry(int row, int column, double factor);

        RealMatrix transpose();

        double getTrace();

        double[] operate(double[] v);

        RealVector operate(RealVector v);

        double[] preMultiply(double[] v);

        RealVector preMultiply(RealVector v);

        double walkInRowOrder(RealMatrixChangingVisitor visitor);

        double walkInRowOrder(RealMatrixPreservingVisitor visitor);

        double walkInRowOrder(RealMatrixChangingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);

        double walkInRowOrder(RealMatrixPreservingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);

        double walkInColumnOrder(RealMatrixChangingVisitor visitor);

        double walkInColumnOrder(RealMatrixPreservingVisitor visitor);

        double walkInColumnOrder(RealMatrixChangingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);

        double walkInColumnOrder(RealMatrixPreservingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);

        double walkInOptimizedOrder(RealMatrixChangingVisitor visitor);

        double walkInOptimizedOrder(RealMatrixPreservingVisitor visitor);

        double walkInOptimizedOrder(RealMatrixChangingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);

        double walkInOptimizedOrder(RealMatrixPreservingVisitor visitor, int startRow, int endRow, int startColumn, int endColumn);
    }

}
