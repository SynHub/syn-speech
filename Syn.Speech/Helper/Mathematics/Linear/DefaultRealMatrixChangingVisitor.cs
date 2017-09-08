//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{

    /// <summary>
    /// Default implementation of the {@link RealMatrixChangingVisitor} interface.
    /// </summary>
    public class DefaultRealMatrixChangingVisitor : RealMatrixChangingVisitor
    {
        public void start(int rows, int columns,int startRow, int endRow, int startColumn, int endColumn)
        {
        }

        public virtual double visit(int row, int column, double value)
        {
            return value;
        }

        public double end()
        {
            return 0;
        }
    }
}
