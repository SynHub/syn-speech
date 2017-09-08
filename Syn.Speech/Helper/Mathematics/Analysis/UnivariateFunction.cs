//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Analysis
{
    public interface UnivariateFunction
    {
        /// <summary>
        ///Compute the value of the function.
        /// </summary>
        /// <param name="x">Point at which the function value should be computed..</param>
        /// <returns></returns>
        double value(double x);
    }
}
