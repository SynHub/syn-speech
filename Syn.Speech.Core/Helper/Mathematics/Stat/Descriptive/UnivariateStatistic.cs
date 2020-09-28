//INCOMPLETE
namespace Syn.Speech.Helper.Mathematics.Stat.Descriptive
{
    
/**
 * Base interface implemented by all statistics.
 *
 */
public interface UnivariateStatistic /* extends MathArrays.Function */ {
    /**
     * Returns the result of evaluating the statistic over the input array.
     *
     * @param values input array
     * @return the value of the statistic applied to the input array
     * @throws MathIllegalArgumentException  if values is null
     */
    double evaluate(double[] values);

    /**
     * Returns the result of evaluating the statistic over the specified entries
     * in the input array.
     *
     * @param values the input array
     * @param begin the index of the first element to include
     * @param length the number of elements to include
     * @return the value of the statistic applied to the included array entries
     * @throws MathIllegalArgumentException if values is null or the indices are invalid
     */
    double evaluate(double[] values, int begin, int length);

    /**
     * Returns a copy of the statistic with the same internal state.
     *
     * @return a copy of the statistic
     */
    UnivariateStatistic copy();
}

}
