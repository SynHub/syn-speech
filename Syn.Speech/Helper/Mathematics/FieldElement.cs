//PATROLLED
namespace Syn.Speech.Helper.Mathematics
{

    public interface FieldElement<T>
    {

        /** Compute this + a.
     * @param a element to add
     * @return a new element representing this + a
     * @throws NullArgumentException if {@code addend} is {@code null}.
     */
        T add(T a);

        /** Compute this - a.
     * @param a element to subtract
     * @return a new element representing this - a
     * @throws NullArgumentException if {@code a} is {@code null}.
     */
        T subtract(T a);

        /**
     * Returns the additive inverse of {@code this} element.
     * @return the opposite of {@code this}.
     */
        T negate();

        /** Compute n &times; this. Multiplication by an integer number is defined
     * as the following sum
     * <center>
     * n &times; this = &sum;<sub>i=1</sub><sup>n</sup> this.
     * </center>
     * @param n Number of times {@code this} must be added to itself.
     * @return A new element representing n &times; this.
     */
        T multiply(int n);

        /** Compute this &times; a.
     * @param a element to multiply
     * @return a new element representing this &times; a
     * @throws NullArgumentException if {@code a} is {@code null}.
     */
        T multiply(T a);

        /** Compute this &divide; a.
     * @param a element to add
     * @return a new element representing this &divide; a
     * @throws NullArgumentException if {@code a} is {@code null}.
     * @throws MathArithmeticException if {@code a} is zero
     */
        T divide(T a);

        /**
     * Returns the multiplicative inverse of {@code this} element.
     * @return the inverse of {@code this}.
     * @throws MathArithmeticException if {@code this} is zero
     */
        T reciprocal();

        /** Get the {@link Field} to which the instance belongs.
     * @return {@link Field} to which the instance belongs
     */
        Field<T> getField();
    }
}

