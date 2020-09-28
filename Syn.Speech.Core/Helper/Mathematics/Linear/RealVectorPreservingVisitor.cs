//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{

    /**
     * This interface defines a visitor for the entries of a vector. Visitors
     * implementing this interface do not alter the entries of the vector being
     * visited.
     *
     * @since 3.1
     */
    public interface RealVectorPreservingVisitor
    {
        /**
         * Start visiting a vector. This method is called once, before any entry
         * of the vector is visited.
         *
         * @param dimension the size of the vector
         * @param start the index of the first entry to be visited
         * @param end the index of the last entry to be visited (inclusive)
         */
        void start(int dimension, int start, int end);

        /**
         * Visit one entry of the vector.
         *
         * @param index the index of the entry being visited
         * @param value the value of the entry being visited
         */
        void visit(int index, double value);

        /**
         * End visiting a vector. This method is called once, after all entries of
         * the vector have been visited.
         *
         * @return the value returned by
         * {@link RealVector#walkInDefaultOrder(RealVectorPreservingVisitor)},
         * {@link RealVector#walkInDefaultOrder(RealVectorPreservingVisitor, int, int)},
         * {@link RealVector#walkInOptimizedOrder(RealVectorPreservingVisitor)}
         * or
         * {@link RealVector#walkInOptimizedOrder(RealVectorPreservingVisitor, int, int)}
         */
        double end();
    }

}
