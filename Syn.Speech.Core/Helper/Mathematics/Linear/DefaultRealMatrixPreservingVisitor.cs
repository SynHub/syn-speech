//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{

    public class DefaultRealMatrixPreservingVisitor : RealMatrixPreservingVisitor
    {
        /** {@inheritDoc} */
        public virtual void start(int rows, int columns,
                          int startRow, int endRow, int startColumn, int endColumn)
        {
        }

        /** {@inheritDoc} */
        public virtual void visit(int row, int column, double value) { }

        /** {@inheritDoc} */
        public double end()
        {
            return 0;
        }
    }
}
