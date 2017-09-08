using System;
using System.Runtime.Serialization;
using Syn.Speech.Helper.Mathematics.Linear;

namespace Syn.Speech.Helper.Mathematics.Stat.Descriptive.Moment
{

    public class FirstMoment : AbstractStorelessUnivariateStatistic, ISerializable
    {

        /** Count of values that have been added */
        public long n;

        /** First moment of values that have been added */
        public double m1;

        /**
         * Deviation of most recently added value from previous first moment.
         * Retained to prevent repeated computation in higher order moments.
         */
        protected double dev;

        /**
         * Deviation of most recently added value from previous first moment,
         * normalized by previous sample size.  Retained to prevent repeated
         * computation in higher order moments
         */
        protected double nDev;

        /**
         * Create a FirstMoment instance
         */
        public FirstMoment()
        {
            n = 0;
            m1 = Double.NaN;
            dev = Double.NaN;
            nDev = Double.NaN;
        }

        /**
         * Copy constructor, creates a new {@code FirstMoment} identical
         * to the {@code original}
         *
         * @param original the {@code FirstMoment} instance to copy
         * @throws NullArgumentException if original is null
         */
        public FirstMoment(FirstMoment original)
        {
            copy(original, this);
        }

        /**
         * {@inheritDoc}
         */
        public override void increment(double d)
        {
            if (n == 0)
            {
                m1 = 0.0;
            }
            n++;
            double n0 = n;
            dev = d - m1;
            nDev = dev / n0;
            m1 += nDev;
        }

        /**
         * {@inheritDoc}
         */

        public override void clear()
        {
            m1 = Double.NaN;
            n = 0;
            dev = Double.NaN;
            nDev = Double.NaN;
        }

        /**
         * {@inheritDoc}
         */
        public override double getResult()
        {
            return m1;
        }

        /**
         * {@inheritDoc}
         */
        public override long getN()
        {
            return n;
        }


        public override UnivariateStatistic copy()//TODO: Supposed to be public override FirstMoment copy()
        {
            FirstMoment result = new FirstMoment();
            // No try-catch or advertised exception because args are guaranteed non-null
            copy(this, result);
            return result;
        }

        /**
         * Copies source to dest.
         * <p>Neither source nor dest can be null.</p>
         *
         * @param source FirstMoment to copy
         * @param dest FirstMoment to copy to
         * @throws NullArgumentException if either source or dest is null
         */
        public static void copy(FirstMoment source, FirstMoment dest)
        {
            MathUtils.checkNotNull(source);
            MathUtils.checkNotNull(dest);
            dest.setData(source.getDataRef());
            dest.n = source.n;
            dest.m1 = source.m1;
            dest.dev = source.dev;
            dest.nDev = source.nDev;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

}
