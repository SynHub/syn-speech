//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{

    /// <summary>
    /// Interface defining very basic matrix operations.
    /// </summary>
    public interface AnyMatrix
    {
        /// <summary>
        /// Is this a square matrix?
        /// </summary>
        /// <returns> true if the matrix is square (rowDimension = columnDimension)</returns>
        bool isSquare();

        /// <summary>
        ///  Returns the number of rows in the matrix.
        /// </summary>
        int getRowDimension();

        /// <summary>
        /// Returns the number of columns in the matrix.
        /// </summary>
        int getColumnDimension();

    }
}
