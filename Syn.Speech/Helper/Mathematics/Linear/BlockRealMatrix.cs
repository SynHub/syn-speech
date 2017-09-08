using System;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
    public class BlockRealMatrix : AbstractRealMatrix
    {
            /** Block size. */
            public const int BLOCK_SIZE = 52;

            /** Blocks of matrix entries. */
            private readonly double[][] blocks;
            /** Number of rows of the matrix. */
            private readonly int rows;
            /** Number of columns of the matrix. */
            private readonly int columns;
            /** Number of block rows of the matrix. */
            private readonly int blockRows;
            /** Number of block columns of the matrix. */
            private readonly int blockColumns;

    /**
     * Create a new matrix with the supplied row and column dimensions.
     *
     * @param rows  the number of rows in the new matrix
     * @param columns  the number of columns in the new matrix
     * @throws NotStrictlyPositiveException if row or column dimension is not
     * positive.
     */
    public BlockRealMatrix( int rows,  int columns) :base(rows, columns){
        this.rows = rows;
        this.columns = columns;

        // number of blocks
        blockRows = (rows + BLOCK_SIZE - 1) / BLOCK_SIZE;
        blockColumns = (columns + BLOCK_SIZE - 1) / BLOCK_SIZE;

        // allocate storage blocks, taking care of smaller ones at right and bottom
        blocks = createBlocksLayout(rows, columns);
    }

    /**
     * Create a new dense matrix copying entries from raw layout data.
     * <p>The input array <em>must</em> already be in raw layout.</p>
     * <p>Calling this constructor is equivalent to call:
     * <pre>matrix = new BlockRealMatrix(rawData.length, rawData[0].length,
     *                                   toBlocksLayout(rawData), false);</pre>
     * </p>
     *
     * @param rawData data for new matrix, in raw layout
     * @throws DimensionMismatchException if the shape of {@code blockData} is
     * inconsistent with block layout.
     * @throws NotStrictlyPositiveException if row or column dimension is not
     * positive.
     * @see #BlockRealMatrix(int, int, double[][], boolean)
     */
    public BlockRealMatrix( double[][] rawData) : this(rawData.Length, rawData[0].Length, toBlocksLayout(rawData), false){
        ;
    }

    /**
     * Create a new dense matrix copying entries from block layout data.
     * <p>The input array <em>must</em> already be in blocks layout.</p>
     *
     * @param rows Number of rows in the new matrix.
     * @param columns Number of columns in the new matrix.
     * @param blockData data for new matrix
     * @param copyArray Whether the input array will be copied or referenced.
     * @throws DimensionMismatchException if the shape of {@code blockData} is
     * inconsistent with block layout.
     * @throws NotStrictlyPositiveException if row or column dimension is not
     * positive.
     * @see #createBlocksLayout(int, int)
     * @see #toBlocksLayout(double[][])
     * @see #BlockRealMatrix(double[][])
     */
    public BlockRealMatrix( int rows,  int columns,  double[][] blockData,  bool copyArray) :base(rows, columns) {
        
        this.rows = rows;
        this.columns = columns;

        // number of blocks
        blockRows = (rows + BLOCK_SIZE - 1) / BLOCK_SIZE;
        blockColumns = (columns + BLOCK_SIZE - 1) / BLOCK_SIZE;

        if (copyArray) {
            // allocate storage blocks, taking care of smaller ones at right and bottom
            blocks = new double[blockRows * blockColumns][];
        } else {
            // reference existing array
            blocks = blockData;
        }

        int index = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock, ++index) {
                if (blockData[index].Length != iHeight * blockWidth(jBlock)) {
                    throw new Exception("DimensionMismatchException");
                }
                if (copyArray) {
                    blocks[index] = blockData[index].Clone() as double[];
                }
            }
        }
    }

    /**
     * Convert a data array from raw layout to blocks layout.
     * <p>
     * Raw layout is the straightforward layout where element at row i and
     * column j is in array element <code>rawData[i][j]</code>. Blocks layout
     * is the layout used in {@link BlockRealMatrix} instances, where the matrix
     * is split in square blocks (except at right and bottom side where blocks may
     * be rectangular to fit matrix size) and each block is stored in a flattened
     * one-dimensional array.
     * </p>
     * <p>
     * This method creates an array in blocks layout from an input array in raw layout.
     * It can be used to provide the array argument of the {@link
     * #BlockRealMatrix(int, int, double[][], boolean)} constructor.
     * </p>
     * @param rawData Data array in raw layout.
     * @return a new data array containing the same entries but in blocks layout.
     * @throws DimensionMismatchException if {@code rawData} is not rectangular.
     * @see #createBlocksLayout(int, int)
     * @see #BlockRealMatrix(int, int, double[][], boolean)
     */
    public static double[][] toBlocksLayout( double[][] rawData){
         int rows = rawData.Length;
         int columns = rawData[0].Length;
         int blockRows = (rows    + BLOCK_SIZE - 1) / BLOCK_SIZE;
         int blockColumns = (columns + BLOCK_SIZE - 1) / BLOCK_SIZE;

        // safety checks
        for (int i = 0; i < rawData.Length; ++i) {
             int length = rawData[i].Length;
            if (length != columns) {
                throw new Exception("DimensionMismatchException");
            }
        }

        // convert array
         double[][] blocks = new double[blockRows * blockColumns][];
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
             int iHeight = pEnd - pStart;
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                 int jWidth = qEnd - qStart;

                // allocate new block
                 double[] block = new double[iHeight * jWidth];
                blocks[blockIndex] = block;

                // copy data
                int index = 0;
                for (int p = pStart; p < pEnd; ++p) {
                    Array.Copy(rawData[p], qStart, block, index, jWidth);
                    index += jWidth;
                }
                ++blockIndex;
            }
        }

        return blocks;
    }

    /**
     * Create a data array in blocks layout.
     * <p>
     * This method can be used to create the array argument of the {@link
     * #BlockRealMatrix(int, int, double[][], boolean)} constructor.
     * </p>
     * @param rows Number of rows in the new matrix.
     * @param columns Number of columns in the new matrix.
     * @return a new data array in blocks layout.
     * @see #toBlocksLayout(double[][])
     * @see #BlockRealMatrix(int, int, double[][], boolean)
     */
    public static double[][] createBlocksLayout( int rows,  int columns) {
         int blockRows = (rows    + BLOCK_SIZE - 1) / BLOCK_SIZE;
         int blockColumns = (columns + BLOCK_SIZE - 1) / BLOCK_SIZE;

         double[][] blocks = new double[blockRows * blockColumns][];
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
             int iHeight = pEnd - pStart;
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                 int jWidth = qEnd - qStart;
                blocks[blockIndex] = new double[iHeight * jWidth];
                ++blockIndex;
            }
        }

        return blocks;
    }


    public override RealMatrix createMatrix(int rowDimension, int columnDimension)/*TODO: Supposed to be BlockRealMatrix*/{
        return new BlockRealMatrix(rowDimension, columnDimension);
    }


    public override RealMatrix copy() /*TODO: Supposed to be BlockRealMatrix*/{
        // create an empty matrix
        BlockRealMatrix copied = new BlockRealMatrix(rows, columns);

        // copy the blocks
        for (int i = 0; i < blocks.Length; ++i) {
            Array.Copy(blocks[i], 0, copied.blocks[i], 0, blocks[i].Length);
        }

        return copied;
    }

    public override RealMatrix add(RealMatrix m)/*TODO: Supposed to be BlockRealMatrix*/{
        try {
            return add((BlockRealMatrix) m);
        } catch (InvalidCastException cce) {
            // safety check
            //MatrixUtils.checkAdditionCompatible(this, m);

            BlockRealMatrix blockRealMatrix = new BlockRealMatrix(rows, columns);

            // perform addition block-wise, to ensure good cache behavior
            int blockIndex = 0;
            for (int iBlock = 0; iBlock < blockRealMatrix.blockRows; ++iBlock) {
                for (int jBlock = 0; jBlock < blockRealMatrix.blockColumns; ++jBlock) {

                    // perform addition on the current block
                     double[] outBlock = blockRealMatrix.blocks[blockIndex];
                     double[] tBlock   = blocks[blockIndex];
                     int pStart = iBlock * BLOCK_SIZE;
                     int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
                     int qStart = jBlock * BLOCK_SIZE;
                     int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                    int k = 0;
                    for (int p = pStart; p < pEnd; ++p) {
                        for (int q = qStart; q < qEnd; ++q) {
                            outBlock[k] = tBlock[k] + m.getEntry(p, q);
                            ++k;
                        }
                    }
                    // go to next block
                    ++blockIndex;
                }
            }

            return blockRealMatrix;
        }
    }

    /**
     * Compute the sum of this matrix and {@code m}.
     *
     * @param m Matrix to be added.
     * @return {@code this} + m.
     * @throws MatrixDimensionMismatchException if {@code m} is not the same
     * size as this matrix.
     */
    public BlockRealMatrix add( BlockRealMatrix m){
        // safety check
        //MatrixUtils.checkAdditionCompatible(this, m);

         BlockRealMatrix @out = new BlockRealMatrix(rows, columns);

        // perform addition block-wise, to ensure good cache behavior
        for (int blockIndex = 0; blockIndex < @out.blocks.Length; ++blockIndex) {
             double[] outBlock = @out.blocks[blockIndex];
             double[] tBlock = blocks[blockIndex];
             double[] mBlock = m.blocks[blockIndex];
            for (int k = 0; k < outBlock.Length; ++k) {
                outBlock[k] = tBlock[k] + mBlock[k];
            }
        }

        return @out;
    }



    public override RealMatrix subtract(RealMatrix m)/*TODO: Supposed to be BlockRealMatrix*/{
        try {
            return subtract((BlockRealMatrix) m);
        } catch (InvalidCastException cce) {
            // safety check
            //MatrixUtils.checkSubtractionCompatible(this, m);

             BlockRealMatrix blockRealMatrix = new BlockRealMatrix(rows, columns);

            // perform subtraction block-wise, to ensure good cache behavior
            int blockIndex = 0;
            for (int iBlock = 0; iBlock < blockRealMatrix.blockRows; ++iBlock) {
                for (int jBlock = 0; jBlock < blockRealMatrix.blockColumns; ++jBlock) {

                    // perform subtraction on the current block
                     double[] outBlock = blockRealMatrix.blocks[blockIndex];
                     double[] tBlock = blocks[blockIndex];
                     int pStart = iBlock * BLOCK_SIZE;
                     int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
                     int qStart = jBlock * BLOCK_SIZE;
                     int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                    int k = 0;
                    for (int p = pStart; p < pEnd; ++p) {
                        for (int q = qStart; q < qEnd; ++q) {
                            outBlock[k] = tBlock[k] - m.getEntry(p, q);
                            ++k;
                        }
                    }
                    // go to next block
                    ++blockIndex;
                }
            }

            return blockRealMatrix;
        }
    }

    /**
     * Subtract {@code m} from this matrix.
     *
     * @param m Matrix to be subtracted.
     * @return {@code this} - m.
     * @throws MatrixDimensionMismatchException if {@code m} is not the
     * same size as this matrix.
     */
    public BlockRealMatrix subtract( BlockRealMatrix m){
        // safety check
        //MatrixUtils.checkSubtractionCompatible(this, m);

         BlockRealMatrix @out = new BlockRealMatrix(rows, columns);

        // perform subtraction block-wise, to ensure good cache behavior
        for (int blockIndex = 0; blockIndex < @out.blocks.Length; ++blockIndex) {
             double[] outBlock = @out.blocks[blockIndex];
             double[] tBlock = blocks[blockIndex];
             double[] mBlock = m.blocks[blockIndex];
            for (int k = 0; k < outBlock.Length; ++k) {
                outBlock[k] = tBlock[k] - mBlock[k];
            }
        }

        return @out;
    }

    public override RealMatrix scalarAdd(double d) /*TODO: Supposed to be BlockRealMatrix*/{

        BlockRealMatrix blockRealMatrix = new BlockRealMatrix(rows, columns);

        // perform subtraction block-wise, to ensure good cache behavior
        for (int blockIndex = 0; blockIndex < blockRealMatrix.blocks.Length; ++blockIndex) {
             double[] outBlock = blockRealMatrix.blocks[blockIndex];
             double[] tBlock = blocks[blockIndex];
            for (int k = 0; k < outBlock.Length; ++k) {
                outBlock[k] = tBlock[k] + d;
            }
        }

        return blockRealMatrix;
    }


    public override RealMatrix scalarMultiply(double d) {
         BlockRealMatrix @out = new BlockRealMatrix(rows, columns);

        // perform subtraction block-wise, to ensure good cache behavior
        for (int blockIndex = 0; blockIndex < @out.blocks.Length; ++blockIndex) {
             double[] outBlock = @out.blocks[blockIndex];
             double[] tBlock = blocks[blockIndex];
            for (int k = 0; k < outBlock.Length; ++k) {
                outBlock[k] = tBlock[k] * d;
            }
        }

        return @out;
    }


    public override RealMatrix multiply( RealMatrix m) /*TODO: Supposed to be BlockRealMatrix*/ {
        try {
            return multiply((BlockRealMatrix) m);
        } catch (InvalidCastException cce) {
            // safety check
            //MatrixUtils.checkMultiplicationCompatible(this, m);

             BlockRealMatrix @out = new BlockRealMatrix(rows, m.getColumnDimension());

            // perform multiplication block-wise, to ensure good cache behavior
            int blockIndex = 0;
            for (int iBlock = 0; iBlock < @out.blockRows; ++iBlock) {
                 int pStart = iBlock * BLOCK_SIZE;
                 int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);

                for (int jBlock = 0; jBlock < @out.blockColumns; ++jBlock) {
                     int qStart = jBlock * BLOCK_SIZE;
                     int qEnd = Math.Min(qStart + BLOCK_SIZE, m.getColumnDimension());

                    // select current block
                     double[] outBlock = @out.blocks[blockIndex];

                    // perform multiplication on current block
                    for (int kBlock = 0; kBlock < blockColumns; ++kBlock) {
                         int kWidth = blockWidth(kBlock);
                         double[] tBlock = blocks[iBlock * blockColumns + kBlock];
                         int rStart = kBlock * BLOCK_SIZE;
                        int k = 0;
                        for (int p = pStart; p < pEnd; ++p) {
                             int lStart = (p - pStart) * kWidth;
                             int lEnd = lStart + kWidth;
                            for (int q = qStart; q < qEnd; ++q) {
                                double sum = 0;
                                int r = rStart;
                                for (int l = lStart; l < lEnd; ++l) {
                                    sum += tBlock[l] * m.getEntry(r, q);
                                    ++r;
                                }
                                outBlock[k] += sum;
                                ++k;
                            }
                        }
                    }
                    // go to next block
                    ++blockIndex;
                }
            }

            return @out;
        }
    }

    /**
     * Returns the result of postmultiplying this by {@code m}.
     *
     * @param m Matrix to postmultiply by.
     * @return {@code this} * m.
     * @throws DimensionMismatchException if the matrices are not compatible.
     */
    public BlockRealMatrix multiply(BlockRealMatrix m){
        // safety check
        //MatrixUtils.checkMultiplicationCompatible(this, m);

         BlockRealMatrix blockRealMatrix = new BlockRealMatrix(rows, m.columns);

        // perform multiplication block-wise, to ensure good cache behavior
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockRealMatrix.blockRows; ++iBlock) {

             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);

            for (int jBlock = 0; jBlock < blockRealMatrix.blockColumns; ++jBlock) {
                 int jWidth = blockRealMatrix.blockWidth(jBlock);
                 int jWidth2 = jWidth  + jWidth;
                 int jWidth3 = jWidth2 + jWidth;
                 int jWidth4 = jWidth3 + jWidth;

                // select current block
                 double[] outBlock = blockRealMatrix.blocks[blockIndex];

                // perform multiplication on current block
                for (int kBlock = 0; kBlock < blockColumns; ++kBlock) {
                     int kWidth = blockWidth(kBlock);
                     double[] tBlock = blocks[iBlock * blockColumns + kBlock];
                     double[] mBlock = m.blocks[kBlock * m.blockColumns + jBlock];
                    int k = 0;
                    for (int p = pStart; p < pEnd; ++p) {
                         int lStart = (p - pStart) * kWidth;
                         int lEnd = lStart + kWidth;
                        for (int nStart = 0; nStart < jWidth; ++nStart) {
                            double sum = 0;
                            int l = lStart;
                            int n = nStart;
                            while (l < lEnd - 3) {
                                sum += tBlock[l] * mBlock[n] +
                                       tBlock[l + 1] * mBlock[n + jWidth] +
                                       tBlock[l + 2] * mBlock[n + jWidth2] +
                                       tBlock[l + 3] * mBlock[n + jWidth3];
                                l += 4;
                                n += jWidth4;
                            }
                            while (l < lEnd) {
                                sum += tBlock[l++] * mBlock[n];
                                n += jWidth;
                            }
                            outBlock[k] += sum;
                            ++k;
                        }
                    }
                }
                // go to next block
                ++blockIndex;
            }
        }

        return blockRealMatrix;
    }

 
    public override double[][] getData()
    {
        double[][] data = Java.CreateArray<double[][]>(getRowDimension(), getColumnDimension());//  
         int lastColumns = columns - (blockColumns - 1) * BLOCK_SIZE;

        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            int regularPos = 0;
            int lastPos = 0;
            for (int p = pStart; p < pEnd; ++p) {
                 double[] dataP = data[p];
                int blockIndex = iBlock * blockColumns;
                int dataPos = 0;
                for (int jBlock = 0; jBlock < blockColumns - 1; ++jBlock) {
                    Array.Copy(blocks[blockIndex++], regularPos, dataP, dataPos, BLOCK_SIZE);
                    dataPos += BLOCK_SIZE;
                }
               Array.Copy(blocks[blockIndex], lastPos, dataP, dataPos, lastColumns);
                regularPos += BLOCK_SIZE;
                lastPos    += lastColumns;
            }
        }

        return data;
    }

  
    public override double getNorm() {
         double[] colSums = new double[BLOCK_SIZE];
        double maxColSum = 0;
        for (int jBlock = 0; jBlock < blockColumns; jBlock++) {
             int jWidth = blockWidth(jBlock);
            Arrays.Fill(colSums, 0, jWidth, 0.0);
            for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
                 int iHeight = blockHeight(iBlock);
                 double[] block = blocks[iBlock * blockColumns + jBlock];
                for (int j = 0; j < jWidth; ++j) {
                    double sum = 0;
                    for (int i = 0; i < iHeight; ++i) {
                        sum += Math.Abs(block[i * jWidth + j]);
                    }
                    colSums[j] += sum;
                }
            }
            for (int j = 0; j < jWidth; ++j) {
                maxColSum = Math.Max(maxColSum, colSums[j]);
            }
        }
        return maxColSum;
    }


    public override double getFrobeniusNorm() {
        double sum2 = 0;
        for (int blockIndex = 0; blockIndex < blocks.Length; ++blockIndex) {
            foreach (double entry in blocks[blockIndex]) {
                sum2 += entry * entry;
            }
        }
        return Math.Sqrt(sum2);
    }


    public override RealMatrix getSubMatrix(int startRow, int endRow, int startColumn, int endColumn) /*TODO: Supposed to be BlockRealMatrix */{
        // safety checks
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);

        // create the output matrix
         BlockRealMatrix blockRealMatrix =new BlockRealMatrix(endRow - startRow + 1, endColumn - startColumn + 1);

        // compute blocks shifts
         int blockStartRow = startRow / BLOCK_SIZE;
         int rowsShift = startRow % BLOCK_SIZE;
         int blockStartColumn = startColumn / BLOCK_SIZE;
         int columnsShift = startColumn % BLOCK_SIZE;

        // perform extraction block-wise, to ensure good cache behavior
        int pBlock = blockStartRow;
        for (int iBlock = 0; iBlock < blockRealMatrix.blockRows; ++iBlock) {
             int iHeight = blockRealMatrix.blockHeight(iBlock);
            int qBlock = blockStartColumn;
            for (int jBlock = 0; jBlock < blockRealMatrix.blockColumns; ++jBlock) {
                 int jWidth = blockRealMatrix.blockWidth(jBlock);

                // handle one block of the output matrix
                 int outIndex = iBlock * blockRealMatrix.blockColumns + jBlock;
                 double[] outBlock = blockRealMatrix.blocks[outIndex];
                 int index = pBlock * blockColumns + qBlock;
                 int width = blockWidth(qBlock);

                 int heightExcess = iHeight + rowsShift - BLOCK_SIZE;
                 int widthExcess = jWidth + columnsShift - BLOCK_SIZE;
                if (heightExcess > 0) {
                    // the submatrix block spans on two blocks rows from the original matrix
                    if (widthExcess > 0) {
                        // the submatrix block spans on two blocks columns from the original matrix
                         int width2 = blockWidth(qBlock + 1);
                        copyBlockPart(blocks[index], width,
                                      rowsShift, BLOCK_SIZE,
                                      columnsShift, BLOCK_SIZE,
                                      outBlock, jWidth, 0, 0);
                        copyBlockPart(blocks[index + 1], width2,
                                      rowsShift, BLOCK_SIZE,
                                      0, widthExcess,
                                      outBlock, jWidth, 0, jWidth - widthExcess);
                        copyBlockPart(blocks[index + blockColumns], width,
                                      0, heightExcess,
                                      columnsShift, BLOCK_SIZE,
                                      outBlock, jWidth, iHeight - heightExcess, 0);
                        copyBlockPart(blocks[index + blockColumns + 1], width2,
                                      0, heightExcess,
                                      0, widthExcess,
                                      outBlock, jWidth, iHeight - heightExcess, jWidth - widthExcess);
                    } else {
                        // the submatrix block spans on one block column from the original matrix
                        copyBlockPart(blocks[index], width,
                                      rowsShift, BLOCK_SIZE,
                                      columnsShift, jWidth + columnsShift,
                                      outBlock, jWidth, 0, 0);
                        copyBlockPart(blocks[index + blockColumns], width,
                                      0, heightExcess,
                                      columnsShift, jWidth + columnsShift,
                                      outBlock, jWidth, iHeight - heightExcess, 0);
                    }
                } else {
                    // the submatrix block spans on one block row from the original matrix
                    if (widthExcess > 0) {
                        // the submatrix block spans on two blocks columns from the original matrix
                         int width2 = blockWidth(qBlock + 1);
                        copyBlockPart(blocks[index], width,
                                      rowsShift, iHeight + rowsShift,
                                      columnsShift, BLOCK_SIZE,
                                      outBlock, jWidth, 0, 0);
                        copyBlockPart(blocks[index + 1], width2,
                                      rowsShift, iHeight + rowsShift,
                                      0, widthExcess,
                                      outBlock, jWidth, 0, jWidth - widthExcess);
                    } else {
                        // the submatrix block spans on one block column from the original matrix
                        copyBlockPart(blocks[index], width,
                                      rowsShift, iHeight + rowsShift,
                                      columnsShift, jWidth + columnsShift,
                                      outBlock, jWidth, 0, 0);
                    }
               }
                ++qBlock;
            }
            ++pBlock;
        }

        return blockRealMatrix;
    }

    /**
     * Copy a part of a block into another one
     * <p>This method can be called only when the specified part fits in both
     * blocks, no verification is done here.</p>
     * @param srcBlock source block
     * @param srcWidth source block width ({@link #BLOCK_SIZE} or smaller)
     * @param srcStartRow start row in the source block
     * @param srcEndRow end row (exclusive) in the source block
     * @param srcStartColumn start column in the source block
     * @param srcEndColumn end column (exclusive) in the source block
     * @param dstBlock destination block
     * @param dstWidth destination block width ({@link #BLOCK_SIZE} or smaller)
     * @param dstStartRow start row in the destination block
     * @param dstStartColumn start column in the destination block
     */
    private void copyBlockPart( double[] srcBlock,  int srcWidth,
                                int srcStartRow,  int srcEndRow,
                                int srcStartColumn,  int srcEndColumn,
                                double[] dstBlock,  int dstWidth,
                                int dstStartRow,  int dstStartColumn) {
         int length = srcEndColumn - srcStartColumn;
        int srcPos = srcStartRow * srcWidth + srcStartColumn;
        int dstPos = dstStartRow * dstWidth + dstStartColumn;
        for (int srcRow = srcStartRow; srcRow < srcEndRow; ++srcRow) {
            Array.Copy(srcBlock, srcPos, dstBlock, dstPos, length);
            srcPos += srcWidth;
            dstPos += dstWidth;
        }
    }


    public override void  setSubMatrix( double[][] subMatrix,  int row, int column){
        // safety checks
        MathUtils.checkNotNull(subMatrix);
         int refLength = subMatrix[0].Length;
        if (refLength == 0) {
            throw new Exception("NoDataException");
        }
         int endRow = row + subMatrix.Length - 1;
         int endColumn = column + refLength - 1;
       // MatrixUtils.checkSubMatrixIndex(this, row, endRow, column, endColumn);
        foreach ( double[] subRow in subMatrix) {
            if (subRow.Length != refLength) {
                throw new Exception("DimensionMismatchException");
            }
        }

        // compute blocks bounds
         int blockStartRow = row / BLOCK_SIZE;
         int blockEndRow = (endRow + BLOCK_SIZE) / BLOCK_SIZE;
         int blockStartColumn = column / BLOCK_SIZE;
         int blockEndColumn = (endColumn + BLOCK_SIZE) / BLOCK_SIZE;

        // perform copy block-wise, to ensure good cache behavior
        for (int iBlock = blockStartRow; iBlock < blockEndRow; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             int firstRow = iBlock * BLOCK_SIZE;
             int iStart = Math.Max(row,    firstRow);
             int iEnd = Math.Min(endRow + 1, firstRow + iHeight);

            for (int jBlock = blockStartColumn; jBlock < blockEndColumn; ++jBlock) {
                 int jWidth = blockWidth(jBlock);
                 int firstColumn = jBlock * BLOCK_SIZE;
                 int jStart = Math.Max(column,    firstColumn);
                 int jEnd = Math.Min(endColumn + 1, firstColumn + jWidth);
                 int jLength = jEnd - jStart;

                // handle one block, row by row
                 double[] block = blocks[iBlock * blockColumns + jBlock];
                for (int i = iStart; i < iEnd; ++i) {
                    Array.Copy(subMatrix[i - row], jStart - column,
                                     block, (i - firstRow) * jWidth + (jStart - firstColumn),
                                     jLength);
                }

            }
        }
    }

    public override RealMatrix getRowMatrix(int row)/*TODO: Supposed to be BlockRealMatrix */{
        //MatrixUtils.checkRowIndex(this, row);
         BlockRealMatrix @out = new BlockRealMatrix(1, columns);

        // perform copy block-wise, to ensure good cache behavior
         int iBlock = row / BLOCK_SIZE;
         int iRow = row - iBlock * BLOCK_SIZE;
        int outBlockIndex = 0;
        int outIndex = 0;
        double[] outBlock = @out.blocks[outBlockIndex];
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth = blockWidth(jBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
             int available = outBlock.Length - outIndex;
            if (jWidth > available) {
                Array.Copy(block, iRow * jWidth, outBlock, outIndex, available);
                outBlock = @out.blocks[++outBlockIndex];
                Array.Copy(block, iRow * jWidth, outBlock, 0, jWidth - available);
                outIndex = jWidth - available;
            } else {
                Array.Copy(block, iRow * jWidth, outBlock, outIndex, jWidth);
                outIndex += jWidth;
            }
        }

        return @out;
    }


    public override void setRowMatrix( int row,  RealMatrix matrix){
        try {
            setRowMatrix(row, (BlockRealMatrix) matrix);
        } catch (InvalidCastException cce) {
            base.setRowMatrix(row, matrix);
        }
    }

    /**
     * Sets the entries in row number <code>row</code>
     * as a row matrix.  Row indices start at 0.
     *
     * @param row the row to be set
     * @param matrix row matrix (must have one row and the same number of columns
     * as the instance)
     * @throws OutOfRangeException if the specified row index is invalid.
     * @throws MatrixDimensionMismatchException if the matrix dimensions do
     * not match one instance row.
     */
    public void setRowMatrix( int row,  BlockRealMatrix matrix) {
        //MatrixUtils.checkRowIndex(this, row);
         int nCols = getColumnDimension();
        if ((matrix.getRowDimension() != 1) ||
            (matrix.getColumnDimension() != nCols)) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        // perform copy block-wise, to ensure good cache behavior
         int iBlock = row / BLOCK_SIZE;
         int iRow = row - iBlock * BLOCK_SIZE;
        int mBlockIndex = 0;
        int mIndex = 0;
        double[] mBlock = matrix.blocks[mBlockIndex];
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth = blockWidth(jBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
             int available  = mBlock.Length - mIndex;
            if (jWidth > available) {
                Array.Copy(mBlock, mIndex, block, iRow * jWidth, available);
                mBlock = matrix.blocks[++mBlockIndex];
                 Array.Copy(mBlock, 0, block, iRow * jWidth, jWidth - available);
                mIndex = jWidth - available;
            } else {
                 Array.Copy(mBlock, mIndex, block, iRow * jWidth, jWidth);
                mIndex += jWidth;
           }
        }
    }


    public override RealMatrix getColumnMatrix(int column) /*TODO: Supposed to be BlockRealMatrix */{
        //MatrixUtils.checkColumnIndex(this, column);
         BlockRealMatrix @out = new BlockRealMatrix(rows, 1);

        // perform copy block-wise, to ensure good cache behavior
         int jBlock = column / BLOCK_SIZE;
         int jColumn = column - jBlock * BLOCK_SIZE;
         int jWidth = blockWidth(jBlock);
        int outBlockIndex = 0;
        int outIndex = 0;
        double[] outBlock = @out.blocks[outBlockIndex];
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            for (int i = 0; i < iHeight; ++i) {
                if (outIndex >= outBlock.Length) {
                    outBlock = @out.blocks[++outBlockIndex];
                    outIndex = 0;
                }
                outBlock[outIndex++] = block[i * jWidth + jColumn];
            }
        }

        return @out;
    }


    public override void setColumnMatrix( int column,  RealMatrix matrix) {
        try {
            setColumnMatrix(column, (BlockRealMatrix) matrix);
        } catch (InvalidCastException cce) {
            base.setColumnMatrix(column, matrix);
        }
    }

    /**
     * Sets the entries in column number <code>column</code>
     * as a column matrix.  Column indices start at 0.
     *
     * @param column the column to be set
     * @param matrix column matrix (must have one column and the same number of rows
     * as the instance)
     * @throws OutOfRangeException if the specified column index is invalid.
     * @throws MatrixDimensionMismatchException if the matrix dimensions do
     * not match one instance column.
     */
    void setColumnMatrix( int column,  BlockRealMatrix matrix) {
        //MatrixUtils.checkColumnIndex(this, column);
         int nRows = getRowDimension();
        if ((matrix.getRowDimension() != nRows) ||
            (matrix.getColumnDimension() != 1)) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        // perform copy block-wise, to ensure good cache behavior
         int jBlock = column / BLOCK_SIZE;
         int jColumn = column - jBlock * BLOCK_SIZE;
         int jWidth = blockWidth(jBlock);
        int mBlockIndex = 0;
        int mIndex = 0;
        double[] mBlock = matrix.blocks[mBlockIndex];
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            for (int i = 0; i < iHeight; ++i) {
                if (mIndex >= mBlock.Length) {
                    mBlock = matrix.blocks[++mBlockIndex];
                    mIndex = 0;
                }
                block[i * jWidth + jColumn] = mBlock[mIndex++];
            }
        }
    }


    public override RealVector getRowVector(int row) {
        //MatrixUtils.checkRowIndex(this, row);
         double[] outData = new double[columns];

        // perform copy block-wise, to ensure good cache behavior
         int iBlock = row / BLOCK_SIZE;
         int iRow = row - iBlock * BLOCK_SIZE;
        int outIndex = 0;
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth = blockWidth(jBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            Array.Copy(block, iRow * jWidth, outData, outIndex, jWidth);
            outIndex += jWidth;
        }

        return new ArrayRealVector(outData, false);
    }


    public override void setRowVector( int row,  RealVector vector) {
        try {
            setRow(row, ((ArrayRealVector) vector).getDataRef());
        } catch (InvalidCastException cce) {
            base.setRowVector(row, vector);
        }
    }

    public override RealVector getColumnVector( int column) {
        //MatrixUtils.checkColumnIndex(this, column);
         double[] outData = new double[rows];

        // perform copy block-wise, to ensure good cache behavior
         int jBlock = column / BLOCK_SIZE;
         int jColumn = column - jBlock * BLOCK_SIZE;
         int jWidth = blockWidth(jBlock);
        int outIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            for (int i = 0; i < iHeight; ++i) {
                outData[outIndex++] = block[i * jWidth + jColumn];
            }
        }

        return new ArrayRealVector(outData, false);
    }

    public override void setColumnVector( int column,  RealVector vector){
        try {
            setColumn(column, ((ArrayRealVector) vector).getDataRef());
        } catch (InvalidCastException cce) {
            base.setColumnVector(column, vector);
        }
    }


    public override double[] getRow( int row) {
       // MatrixUtils.checkRowIndex(this, row);
         double[] @out = new double[columns];

        // perform copy block-wise, to ensure good cache behavior
         int iBlock = row / BLOCK_SIZE;
         int iRow = row - iBlock * BLOCK_SIZE;
        int outIndex = 0;
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth     = blockWidth(jBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            Array.Copy(block, iRow * jWidth, @out, outIndex, jWidth);
            outIndex += jWidth;
        }

        return @out;
    }


    public override void setRow( int row,  double[] array) {
        //MatrixUtils.checkRowIndex(this, row);
         int nCols = getColumnDimension();
        if (array.Length != nCols) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        // perform copy block-wise, to ensure good cache behavior
         int iBlock = row / BLOCK_SIZE;
         int iRow = row - iBlock * BLOCK_SIZE;
        int outIndex = 0;
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth     = blockWidth(jBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            Array.Copy(array, outIndex, block, iRow * jWidth, jWidth);
            outIndex += jWidth;
        }
    }


    public override double[] getColumn( int column) {
        //MatrixUtils.checkColumnIndex(this, column);
         double[] @out = new double[rows];

        // perform copy block-wise, to ensure good cache behavior
         int jBlock  = column / BLOCK_SIZE;
         int jColumn = column - jBlock * BLOCK_SIZE;
         int jWidth  = blockWidth(jBlock);
        int outIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            for (int i = 0; i < iHeight; ++i) {
                @out[outIndex++] = block[i * jWidth + jColumn];
            }
        }

        return @out;
    }


    public override void setColumn( int column,  double[] array) {
        //MatrixUtils.checkColumnIndex(this, column);
         int nRows = getRowDimension();
        if (array.Length != nRows) {
            throw new Exception("MatrixDimensionMismatchException");
        }

        // perform copy block-wise, to ensure good cache behavior
         int jBlock  = column / BLOCK_SIZE;
         int jColumn = column - jBlock * BLOCK_SIZE;
         int jWidth = blockWidth(jBlock);
        int outIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int iHeight = blockHeight(iBlock);
             double[] block = blocks[iBlock * blockColumns + jBlock];
            for (int i = 0; i < iHeight; ++i) {
                block[i * jWidth + jColumn] = array[outIndex++];
            }
        }
    }


    public override double getEntry( int row,  int column) {
        //MatrixUtils.checkMatrixIndex(this, row, column);
         int iBlock = row / BLOCK_SIZE;
         int jBlock = column / BLOCK_SIZE;
         int k = (row - iBlock * BLOCK_SIZE) * blockWidth(jBlock) +
            (column - jBlock * BLOCK_SIZE);
        return blocks[iBlock * blockColumns + jBlock][k];
    }


    public override void setEntry( int row,  int column,  double value) {
        //MatrixUtils.checkMatrixIndex(this, row, column);
         int iBlock = row / BLOCK_SIZE;
         int jBlock = column / BLOCK_SIZE;
         int k = (row - iBlock * BLOCK_SIZE) * blockWidth(jBlock) +
            (column - jBlock * BLOCK_SIZE);
        blocks[iBlock * blockColumns + jBlock][k] = value;
    }


    public override void addToEntry( int row,  int column, double increment) {
        //MatrixUtils.checkMatrixIndex(this, row, column);
         int iBlock = row    / BLOCK_SIZE;
         int jBlock = column / BLOCK_SIZE;
         int k = (row    - iBlock * BLOCK_SIZE) * blockWidth(jBlock) +
            (column - jBlock * BLOCK_SIZE);
        blocks[iBlock * blockColumns + jBlock][k] += increment;
    }


    public override void multiplyEntry( int row,  int column, double factor) {
        //MatrixUtils.checkMatrixIndex(this, row, column);
         int iBlock = row / BLOCK_SIZE;
         int jBlock = column / BLOCK_SIZE;
         int k = (row - iBlock * BLOCK_SIZE) * blockWidth(jBlock) +
            (column - jBlock * BLOCK_SIZE);
        blocks[iBlock * blockColumns + jBlock][k] *= factor;
    }


    public override RealMatrix transpose() /*TODO: Supposed to be BlockRealMatrix */{
         int nRows = getRowDimension();
         int nCols = getColumnDimension();
         BlockRealMatrix @out = new BlockRealMatrix(nCols, nRows);

        // perform transpose block-wise, to ensure good cache behavior
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockColumns; ++iBlock) {
            for (int jBlock = 0; jBlock < blockRows; ++jBlock) {
                // transpose current block
                 double[] outBlock = @out.blocks[blockIndex];
                 double[] tBlock = blocks[jBlock * blockColumns + iBlock];
                 int pStart = iBlock * BLOCK_SIZE;
                 int pEnd = Math.Min(pStart + BLOCK_SIZE, columns);
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, rows);
                int k = 0;
                for (int p = pStart; p < pEnd; ++p) {
                     int lInc = pEnd - pStart;
                    int l = p - pStart;
                    for (int q = qStart; q < qEnd; ++q) {
                        outBlock[k] = tBlock[l];
                        ++k;
                        l+= lInc;
                    }
                }
                // go to next block
                ++blockIndex;
            }
        }

        return @out;
    }

 
    public override int getRowDimension() {
        return rows;
    }

    public override int getColumnDimension() {
        return columns;
    }

    public override double[] operate( double[] v){
        if (v.Length != columns) {
            throw new Exception("DimensionMismatchException");
        }
         double[] @out = new double[rows];

        // perform multiplication block-wise, to ensure good cache behavior
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                 double[] block  = blocks[iBlock * blockColumns + jBlock];
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                int k = 0;
                for (int p = pStart; p < pEnd; ++p) {
                    double sum = 0;
                    int q = qStart;
                    while (q < qEnd - 3) {
                        sum += block[k]     * v[q]     +
                               block[k + 1] * v[q + 1] +
                               block[k + 2] * v[q + 2] +
                               block[k + 3] * v[q + 3];
                        k += 4;
                        q += 4;
                    }
                    while (q < qEnd) {
                        sum += block[k++] * v[q++];
                    }
                    @out[p] += sum;
                }
            }
        }

        return @out;
    }


    public override double[] preMultiply( double[] v){
        if (v.Length != rows) {
            throw new Exception("DimensionMismatchException");
        }
         double[] @out = new double[columns];

        // perform multiplication block-wise, to ensure good cache behavior
        for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
             int jWidth  = blockWidth(jBlock);
             int jWidth2 = jWidth  + jWidth;
             int jWidth3 = jWidth2 + jWidth;
             int jWidth4 = jWidth3 + jWidth;
             int qStart = jBlock * BLOCK_SIZE;
             int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
            for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
                 double[] block  = blocks[iBlock * blockColumns + jBlock];
                 int pStart = iBlock * BLOCK_SIZE;
                 int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
                for (int q = qStart; q < qEnd; ++q) {
                    int k = q - qStart;
                    double sum = 0;
                    int p = pStart;
                    while (p < pEnd - 3) {
                        sum += block[k]           * v[p]     +
                               block[k + jWidth]  * v[p + 1] +
                               block[k + jWidth2] * v[p + 2] +
                               block[k + jWidth3] * v[p + 3];
                        k += jWidth4;
                        p += 4;
                    }
                    while (p < pEnd) {
                        sum += block[k] * v[p++];
                        k += jWidth;
                    }
                    @out[q] += sum;
                }
            }
        }

        return @out;
    }


    public override double walkInRowOrder( RealMatrixChangingVisitor visitor) {
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            for (int p = pStart; p < pEnd; ++p) {
                for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                     int jWidth = blockWidth(jBlock);
                     int qStart = jBlock * BLOCK_SIZE;
                     int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                     double[] block = blocks[iBlock * blockColumns + jBlock];
                    int k = (p - pStart) * jWidth;
                    for (int q = qStart; q < qEnd; ++q) {
                        block[k] = visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
             }
        }
        return visitor.end();
    }

    public override double walkInRowOrder(RealMatrixPreservingVisitor visitor) {
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            for (int p = pStart; p < pEnd; ++p) {
                for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                     int jWidth = blockWidth(jBlock);
                     int qStart = jBlock * BLOCK_SIZE;
                     int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                     double[] block = blocks[iBlock * blockColumns + jBlock];
                    int k = (p - pStart) * jWidth;
                    for (int q = qStart; q < qEnd; ++q) {
                        visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
             }
        }
        return visitor.end();
    }


    public override double walkInRowOrder( RealMatrixChangingVisitor visitor,
                                  int startRow,  int endRow,
                                  int startColumn,  int endColumn){
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(rows, columns, startRow, endRow, startColumn, endColumn);
        for (int iBlock = startRow / BLOCK_SIZE; iBlock < 1 + endRow / BLOCK_SIZE; ++iBlock) {
             int p0 = iBlock * BLOCK_SIZE;
             int pStart = Math.Max(startRow, p0);
             int pEnd = Math.Min((iBlock + 1) * BLOCK_SIZE, 1 + endRow);
            for (int p = pStart; p < pEnd; ++p) {
                for (int jBlock = startColumn / BLOCK_SIZE; jBlock < 1 + endColumn / BLOCK_SIZE; ++jBlock) {
                     int jWidth = blockWidth(jBlock);
                     int q0 = jBlock * BLOCK_SIZE;
                     int qStart = Math.Max(startColumn, q0);
                     int qEnd = Math.Min((jBlock + 1) * BLOCK_SIZE, 1 + endColumn);
                     double[] block = blocks[iBlock * blockColumns + jBlock];
                    int k = (p - p0) * jWidth + qStart - q0;
                    for (int q = qStart; q < qEnd; ++q) {
                        block[k] = visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
             }
        }
        return visitor.end();
    }


    public override double walkInRowOrder( RealMatrixPreservingVisitor visitor,
                                  int startRow,  int endRow,
                                  int startColumn,  int endColumn) {
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(rows, columns, startRow, endRow, startColumn, endColumn);
        for (int iBlock = startRow / BLOCK_SIZE; iBlock < 1 + endRow / BLOCK_SIZE; ++iBlock) {
             int p0 = iBlock * BLOCK_SIZE;
             int pStart = Math.Max(startRow, p0);
             int pEnd = Math.Min((iBlock + 1) * BLOCK_SIZE, 1 + endRow);
            for (int p = pStart; p < pEnd; ++p) {
                for (int jBlock = startColumn / BLOCK_SIZE; jBlock < 1 + endColumn / BLOCK_SIZE; ++jBlock) {
                     int jWidth = blockWidth(jBlock);
                     int q0 = jBlock * BLOCK_SIZE;
                     int qStart = Math.Max(startColumn, q0);
                     int qEnd = Math.Min((jBlock + 1) * BLOCK_SIZE, 1 + endColumn);
                     double[] block = blocks[iBlock * blockColumns + jBlock];
                    int k = (p - p0) * jWidth + qStart - q0;
                    for (int q = qStart; q < qEnd; ++q) {
                        visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
             }
        }
        return visitor.end();
    }


    public override double walkInOptimizedOrder( RealMatrixChangingVisitor visitor) {
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                 double[] block = blocks[blockIndex];
                int k = 0;
                for (int p = pStart; p < pEnd; ++p) {
                    for (int q = qStart; q < qEnd; ++q) {
                        block[k] = visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
                ++blockIndex;
            }
        }
        return visitor.end();
    }

    public override double walkInOptimizedOrder( RealMatrixPreservingVisitor visitor) {
        visitor.start(rows, columns, 0, rows - 1, 0, columns - 1);
        int blockIndex = 0;
        for (int iBlock = 0; iBlock < blockRows; ++iBlock) {
             int pStart = iBlock * BLOCK_SIZE;
             int pEnd = Math.Min(pStart + BLOCK_SIZE, rows);
            for (int jBlock = 0; jBlock < blockColumns; ++jBlock) {
                 int qStart = jBlock * BLOCK_SIZE;
                 int qEnd = Math.Min(qStart + BLOCK_SIZE, columns);
                 double[] block = blocks[blockIndex];
                int k = 0;
                for (int p = pStart; p < pEnd; ++p) {
                    for (int q = qStart; q < qEnd; ++q) {
                        visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
                ++blockIndex;
            }
        }
        return visitor.end();
    }

    public override double walkInOptimizedOrder( RealMatrixChangingVisitor visitor,
                                        int startRow,  int endRow,
                                        int startColumn,
                                        int endColumn){
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(rows, columns, startRow, endRow, startColumn, endColumn);
        for (int iBlock = startRow / BLOCK_SIZE; iBlock < 1 + endRow / BLOCK_SIZE; ++iBlock) {
             int p0 = iBlock * BLOCK_SIZE;
             int pStart = Math.Max(startRow, p0);
             int pEnd = Math.Min((iBlock + 1) * BLOCK_SIZE, 1 + endRow);
            for (int jBlock = startColumn / BLOCK_SIZE; jBlock < 1 + endColumn / BLOCK_SIZE; ++jBlock) {
                 int jWidth = blockWidth(jBlock);
                 int q0 = jBlock * BLOCK_SIZE;
                 int qStart = Math.Max(startColumn, q0);
                 int qEnd = Math.Min((jBlock + 1) * BLOCK_SIZE, 1 + endColumn);
                 double[] block = blocks[iBlock * blockColumns + jBlock];
                for (int p = pStart; p < pEnd; ++p) {
                    int k = (p - p0) * jWidth + qStart - q0;
                    for (int q = qStart; q < qEnd; ++q) {
                        block[k] = visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
            }
        }
        return visitor.end();
    }


    public override double walkInOptimizedOrder( RealMatrixPreservingVisitor visitor,
                                        int startRow,  int endRow,
                                        int startColumn,
                                        int endColumn){
        //MatrixUtils.checkSubMatrixIndex(this, startRow, endRow, startColumn, endColumn);
        visitor.start(rows, columns, startRow, endRow, startColumn, endColumn);
        for (int iBlock = startRow / BLOCK_SIZE; iBlock < 1 + endRow / BLOCK_SIZE; ++iBlock) {
             int p0 = iBlock * BLOCK_SIZE;
             int pStart = Math.Max(startRow, p0);
             int pEnd = Math.Min((iBlock + 1) * BLOCK_SIZE, 1 + endRow);
            for (int jBlock = startColumn / BLOCK_SIZE; jBlock < 1 + endColumn / BLOCK_SIZE; ++jBlock) {
                 int jWidth = blockWidth(jBlock);
                 int q0 = jBlock * BLOCK_SIZE;
                 int qStart = Math.Max(startColumn, q0);
                 int qEnd = Math.Min((jBlock + 1) * BLOCK_SIZE, 1 + endColumn);
                 double[] block = blocks[iBlock * blockColumns + jBlock];
                for (int p = pStart; p < pEnd; ++p) {
                    int k = (p - p0) * jWidth + qStart - q0;
                    for (int q = qStart; q < qEnd; ++q) {
                        visitor.visit(p, q, block[k]);
                        ++k;
                    }
                }
            }
        }
        return visitor.end();
    }

    /**
     * Get the height of a block.
     * @param blockRow row index (in block sense) of the block
     * @return height (number of rows) of the block
     */
    private int blockHeight(int blockRow) {
        return (blockRow == blockRows - 1) ? rows - blockRow * BLOCK_SIZE : BLOCK_SIZE;
    }

    /**
     * Get the width of a block.
     * @param blockColumn column index (in block sense) of the block
     * @return width (number of columns) of the block
     */
    private int blockWidth(int blockColumn) {
        return (blockColumn == blockColumns - 1) ? columns - blockColumn * BLOCK_SIZE : BLOCK_SIZE;
    }
    }
}
