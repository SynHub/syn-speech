using System;

//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    public class Array2DRowRealMatrix : AbstractRealMatrix
    {
        private double[][] data;

        public Array2DRowRealMatrix() { }

        public Array2DRowRealMatrix(int rowDimension, int columnDimension)
        {
            data = Java.CreateArray<double[][]>(rowDimension, columnDimension); // new double[rowDimension][columnDimension];
        }

        public Array2DRowRealMatrix(double[][] d)
        {
            copyIn(d);
        }

        public Array2DRowRealMatrix(double[][] d, bool copyArray)
        {
            if (copyArray)
            {
                copyIn(d);
            }
            else
            {
                if (d == null)
                {
                    throw new ArgumentNullException();
                }
                int nRows = d.Length;
                if (nRows == 0)
                {
                    throw new Exception("NoDataException");
                }
                int nCols = d[0].Length;
                if (nCols == 0)
                {
                    throw new Exception("NoDataException");
                }
                for (int r = 1; r < nRows; r++)
                {
                    if (d[r].Length != nCols)
                    {
                        throw new Exception("DimensionMismatchException");
                    }
                }
                data = d;
            }
        }

        public Array2DRowRealMatrix(double[] v)
        {
            int nRows = v.Length;
            data = Java.CreateArray<double[][]>(nRows, 1);//  new double[nRows][1];
            for (int row = 0; row < nRows; row++)
            {
                data[row][0] = v[row];
            }
        }




        public override RealMatrix createMatrix(int rowDimension, int columnDimension)
        {
            return new Array2DRowRealMatrix(rowDimension, columnDimension);
        }

        public override RealMatrix copy()
        {
            return new Array2DRowRealMatrix(copyOut(), false);
        }


        public Array2DRowRealMatrix add(Array2DRowRealMatrix m)
        {
            // Safety check.
            //MatrixUtils.checkAdditionCompatible(this, m);

            int rowCount = getRowDimension();
            int columnCount = getColumnDimension();
            double[][] outData = Java.CreateArray<double[][]>(rowCount, columnCount); // new double[rowCount][columnCount];
            for (int row = 0; row < rowCount; row++)
            {
                double[] dataRow = data[row];
                double[] mRow = m.data[row];
                double[] outDataRow = outData[row];
                for (int col = 0; col < columnCount; col++)
                {
                    outDataRow[col] = dataRow[col] + mRow[col];
                }
            }

            return new Array2DRowRealMatrix(outData, false);
        }



        public Array2DRowRealMatrix subtract(Array2DRowRealMatrix m)
        {
            //MatrixUtils.checkSubtractionCompatible(this, m);

            int rowCount = getRowDimension();
            int columnCount = getColumnDimension();
            double[][] outData = Java.CreateArray<double[][]>(rowCount, columnCount);// new double[rowCount][columnCount];
            for (int row = 0; row < rowCount; row++)
            {
                double[] dataRow = data[row];
                double[] mRow = m.data[row];
                double[] outDataRow = outData[row];
                for (int col = 0; col < columnCount; col++)
                {
                    outDataRow[col] = dataRow[col] - mRow[col];
                }
            }

            return new Array2DRowRealMatrix(outData, false);
        }

        public Array2DRowRealMatrix multiply(Array2DRowRealMatrix m)
        {
            //MatrixUtils.checkMultiplicationCompatible(this, m);

            int nRows = getRowDimension();
            int nCols = m.getColumnDimension();
            int nSum = getColumnDimension();

            double[][] outData = Java.CreateArray<double[][]>(nRows, nCols);// new double[nRows][nCols];
            // Will hold a column of "m".
            double[] mCol = new double[nSum];
            double[][] mData = m.data;

            // Multiply.
            for (int col = 0; col < nCols; col++)
            {
                // Copy all elements of column "col" of "m" so that
                // will be in contiguous memory.
                for (int mRow = 0; mRow < nSum; mRow++)
                {
                    mCol[mRow] = mData[mRow][col];
                }

                for (int row = 0; row < nRows; row++)
                {
                    double[] dataRow = data[row];
                    double sum = 0;
                    for (int i = 0; i < nSum; i++)
                    {
                        sum += dataRow[i] * mCol[i];
                    }
                    outData[row][col] = sum;
                }
            }

            return new Array2DRowRealMatrix(outData, false);
        }




        public override double[][] getData()
        {
            return copyOut();
        }


        public double[][] getDataRef()
        {
            return data;
        }





        public override void setSubMatrix(double[][] subMatrix, int row, int column)
        {
            if (data == null)
            {
                if (row > 0)
                {
                    throw new Exception("MathIllegalStateException");
                }
                if (column > 0)
                {
                    throw new Exception("MathIllegalStateException");
                }
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
                data = new double[subMatrix.Length][];
                // data = new double[subMatrix.Length][nCols];
                for (int i = 0; i < data.Length; ++i)
                {
                    if (subMatrix[i].Length != nCols)
                    {
                        throw new Exception("DimensionMismatchException");
                    }
                    Array.Copy(subMatrix[i], 0, data[i + row], column, nCols);
                }
            }
            else
            {
                superSetSubMatrix(subMatrix, row, column);
            }

        }

        public override double getEntry(int row, int column)
        {
            return data[row][column];
        }

        public override void setEntry(int row, int column, double value)
        {
            // MatrixUtils.checkMatrixIndex(this, row, column);
            data[row][column] = value;
        }

        public override void addToEntry(int row, int column, double increment)
        {
            //MatrixUtils.checkMatrixIndex(this, row, column);
            data[row][column] += increment;
        }

        public override void multiplyEntry(int row, int column, double factor)
        {
            //MatrixUtils.checkMatrixIndex(this, row, column);
            data[row][column] *= factor;
        }

        public override int getColumnDimension()
        {
            return ((data == null) || (data[0] == null)) ? 0 : data[0].Length;
        }

        public override int getRowDimension()
        {
            return (data == null) ? 0 : data.Length;
        }

        public override double[] operate(double[] v)
        {
            int nRows = getRowDimension();
            int nCols = getColumnDimension();
            if (v.Length != nCols)
            {
                throw new Exception("DimensionMismatchException");
            }
            double[] @out = new double[nRows];
            for (int row = 0; row < nRows; row++)
            {
                double[] dataRow = data[row];
                double sum = 0;
                for (int i = 0; i < nCols; i++)
                {
                    sum += dataRow[i] * v[i];
                }
                @out[row] = sum;
            }
            return @out;
        }

        public override double[] preMultiply(double[] v)
        {
            int nRows = getRowDimension();
            int nCols = getColumnDimension();
            if (v.Length != nRows)
            {
                throw new Exception("DimensionMismatchException");
            }

            double[] @out = new double[nCols];
            for (int col = 0; col < nCols; ++col)
            {
                double sum = 0;
                for (int i = 0; i < nRows; ++i)
                {
                    sum += data[i][col] * v[i];
                }
                @out[col] = sum;
            }

            return @out;
        }

        public override double walkInRowOrder(RealMatrixChangingVisitor visitor)
        {
            int rows = getRowDimension();
            int columns = getColumnDimension();
            visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
            for (int i = 0; i < rows; ++i)
            {
                double[] rowI = data[i];
                for (int j = 0; j < columns; ++j)
                {
                    rowI[j] = visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInRowOrder(RealMatrixPreservingVisitor visitor)
        {
            int rows = getRowDimension();
            int columns = getColumnDimension();
            visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
            for (int i = 0; i < rows; ++i)
            {
                double[] rowI = data[i];
                for (int j = 0; j < columns; ++j)
                {
                    visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInRowOrder(RealMatrixChangingVisitor visitor,
                                  int startRow, int endRow,
                                  int startColumn, int endColumn)
        {
            //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
            visitor.start(getRowDimension(), getColumnDimension(),
                          startRow, endRow, startColumn, endColumn);
            for (int i = startRow; i <= endRow; ++i)
            {
                double[] rowI = data[i];
                for (int j = startColumn; j <= endColumn; ++j)
                {
                    rowI[j] = visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInRowOrder(RealMatrixPreservingVisitor visitor,
                                 int startRow, int endRow,
                                 int startColumn, int endColumn)
        {
            //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
            visitor.start(getRowDimension(), getColumnDimension(),
                          startRow, endRow, startColumn, endColumn);
            for (int i = startRow; i <= endRow; ++i)
            {
                double[] rowI = data[i];
                for (int j = startColumn; j <= endColumn; ++j)
                {
                    visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInColumnOrder(RealMatrixChangingVisitor visitor)
        {
            int rows = getRowDimension();
            int columns = getColumnDimension();
            visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
            for (int j = 0; j < columns; ++j)
            {
                for (int i = 0; i < rows; ++i)
                {
                    double[] rowI = data[i];
                    rowI[j] = visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInColumnOrder(RealMatrixPreservingVisitor visitor)
        {
            int rows = getRowDimension();
            int columns = getColumnDimension();
            visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
            for (int j = 0; j < columns; ++j)
            {
                for (int i = 0; i < rows; ++i)
                {
                    visitor.visit(i, j, data[i][j]);
                }
            }
            return visitor.end();
        }

        public override double walkInColumnOrder(RealMatrixChangingVisitor visitor,
                                    int startRow, int endRow,
                                    int startColumn, int endColumn)
        {
            //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
            visitor.start(getRowDimension(), getColumnDimension(),
                          startRow, endRow, startColumn, endColumn);
            for (int j = startColumn; j <= endColumn; ++j)
            {
                for (int i = startRow; i <= endRow; ++i)
                {
                    double[] rowI = data[i];
                    rowI[j] = visitor.visit(i, j, rowI[j]);
                }
            }
            return visitor.end();
        }

        public override double walkInColumnOrder(RealMatrixPreservingVisitor visitor,
                                     int startRow, int endRow,
                                     int startColumn, int endColumn)
        {
            // MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
            visitor.start(getRowDimension(), getColumnDimension(),
                          startRow, endRow, startColumn, endColumn);
            for (int j = startColumn; j <= endColumn; ++j)
            {
                for (int i = startRow; i <= endRow; ++i)
                {
                    visitor.visit(i, j, data[i][j]);
                }
            }
            return visitor.end();
        }

        private double[][] copyOut()
        {
            int nRows = getRowDimension();
            double[][] toReturn = new double[nRows][];
            //double[][] toReturn = new double[nRows][getColumnDimension()];
            // can't copy 2-d array in one shot, otherwise get row references
            for (int i = 0; i < nRows; i++)
            {
                Array.Copy(data[i], 0, toReturn[i], 0, data[i].Length);
            }
            return toReturn;
        }

        private void copyIn(double[][] value)
        {
            setSubMatrix(value, 0, 0);
        }












    }
}