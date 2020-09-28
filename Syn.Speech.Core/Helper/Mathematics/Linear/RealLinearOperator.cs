using System;

//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Linear
{
  
public abstract class RealLinearOperator {
    /**
     * Returns the dimension of the codomain of this operator.
     *
     * @return the number of rows of the underlying matrix
     */
    public abstract int getRowDimension();

    /**
     * Returns the dimension of the domain of this operator.
     *
     * @return the number of columns of the underlying matrix
     */
    public abstract int getColumnDimension();

    /**
     * Returns the result of multiplying {@code this} by the vector {@code x}.
     *
     * @param x the vector to operate on
     * @return the product of {@code this} instance with {@code x}
     * @throws DimensionMismatchException if the column dimension does not match
     * the size of {@code x}
     */
    public abstract RealVector operate(RealVector x);

    /**
     * Returns the result of multiplying the transpose of {@code this} operator
     * by the vector {@code x} (optional operation). The default implementation
     * throws an {@link UnsupportedOperationException}. Users overriding this
     * method must also override {@link #isTransposable()}.
     *
     * @param x the vector to operate on
     * @return the product of the transpose of {@code this} instance with
     * {@code x}
     * @throws org.apache.commons.math3.exception.DimensionMismatchException
     * if the row dimension does not match the size of {@code x}
     * @throws UnsupportedOperationException if this operation is not supported
     * by {@code this} operator
     */
    public RealVector operateTranspose(RealVector x){
        throw new InvalidOperationException();
    }

    /**
     * Returns {@code true} if this operator supports
     * {@link #operateTranspose(RealVector)}. If {@code true} is returned,
     * {@link #operateTranspose(RealVector)} should not throw
     * {@code UnsupportedOperationException}. The default implementation returns
     * {@code false}.
     *
     * @return {@code false}
     */
    public bool isTransposable() {
        return false;
    }
}
}
