//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Analysis
{
    /// <summary>
    /// An interface representing a bivariate real function.
    /// </summary>
    public interface BivariateFunction
    {
        /// <summary>
        /// Compute the value for the function.
        /// </summary>
        /// <param name="x">Abscissa for which the function value should be computed..</param>
        /// <param name="y">Ordinate for which the function value should be computed..</param>
        /// <returns>the value.</returns>
        double value(double x, double y);
    }
}
