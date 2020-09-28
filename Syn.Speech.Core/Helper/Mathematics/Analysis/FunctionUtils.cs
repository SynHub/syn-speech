namespace Syn.Speech.Helper.Mathematics.Analysis
{
    public class FunctionUtils
    {
        private FunctionUtils() { }

        /// <summary>
        /// Creates a unary function by fixing the second argument of a binary function.
        /// </summary>
        /// <param name="f">Binary function..</param>
        /// <param name="fixed">Value to which the second argument of {@code f} is set..</param>
        /// <returns>the unary function h(x) = f(x, fixed)</returns>
        public static UnivariateFunction fix2ndArgument(BivariateFunction f, double @fixed)
        {
            return new FirstUnivariateFunction(f, @fixed);
        }
    }

    #region Custom
    public class FirstUnivariateFunction : UnivariateFunction
    {

        private readonly BivariateFunction _bivariateFunction;
        private readonly double _fixedValue;

        public FirstUnivariateFunction(BivariateFunction bivariate, double fixedValue)
        {
            _bivariateFunction = bivariate;
            _fixedValue = fixedValue;
        }

        public double value(double x)
        {
            return _bivariateFunction.value(x, _fixedValue);
        }
    }

#endregion
}
