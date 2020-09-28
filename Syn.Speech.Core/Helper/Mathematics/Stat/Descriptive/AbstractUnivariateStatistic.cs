using System;
using Syn.Speech.Helper.Mathematics.Util;
//PATROLLED
namespace Syn.Speech.Helper.Mathematics.Stat.Descriptive
{

    public abstract class AbstractUnivariateStatistic : UnivariateStatistic
    {
        private double[] storedData;

        public void setData(double[] values)
        {
            storedData = ((values == null) ? null : values.Clone()) as double[];
        }

        public double[] getData()
        {
            return ((storedData == null) ? null : storedData.Clone()) as double[];
        }

        protected double[] getDataRef()
        {
            return storedData;
        }

        public void setData(double[] values, int begin, int length)
        {
            if (values == null)
            {
                throw new Exception("NullArgumentException");
            }

            if (begin < 0)
            {
                throw new Exception("NotPositiveException");
            }

            if (length < 0)
            {
                throw new Exception("NotPositiveException");
            }

            if (begin + length > values.Length)
            {
                throw new Exception("NumberIsTooLargeException");
            }
            storedData = new double[length];
            Array.Copy(values, begin, storedData, 0, length);
        }

        public double evaluate()
        {
            return evaluate(storedData);
        }

        public virtual double evaluate(double[] values)
        {
            test(values, 0, 0);
            return evaluate(values, 0, values.Length);
        }


        public abstract double evaluate(double[] values, int begin, int length);

        public abstract UnivariateStatistic copy();

        protected Boolean test(
             double[] values,
             int begin,
             int length)
        {
            return MathArrays.verifyValues(values, begin, length, false);
        }

        protected Boolean test(double[] values, int begin,
                 int length, bool allowEmpty)
        {
            return MathArrays.verifyValues(values, begin, length, allowEmpty);
        }

        protected Boolean test(
             double[] values,
             double[] weights,
             int begin,
             int length)
        {
            return MathArrays.verifyValues(values, weights, begin, length, false);
        }

        protected Boolean test(double[] values, double[] weights,
                 int begin, int length, bool allowEmpty)
        {
            return MathArrays.verifyValues(values, weights, begin, length, allowEmpty);
        }
    }
}
