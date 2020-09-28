using System;
using Syn.Speech.Helper;

//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts.Semirings
{

    /// <summary>
    /// Probability semiring implementation.
    /// @author "John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class ProbabilitySemiring : Semiring
    {
        // zero value
        private const float zero = 0f;

        // one value
        private const float one = 1f;

        public override float Plus(float w1, float w2)
        {
            if (!IsMember(w1) || !IsMember(w2))
            {
                return float.NegativeInfinity;
            }

            return w1 + w2;
        }

        public override float Times(float w1, float w2)
        {
            if (!IsMember(w1) || !IsMember(w2))
            {
                return float.NegativeInfinity;
            }

            return w1 * w2;
        }


        public override float Divide(float w1, float w2)
        {
            // TODO Auto-generated method stub
            return float.NegativeInfinity;
        }


        public override float Zero
        {
            get { return zero; }
        }

        public override float One
        {
            get { return one; }
        }

        public override bool IsMember(float w)
        {
            return !Float.isNaN(w) // not a NaN,
                    && (w >= 0); // and positive
        }

        public override float Reverse(float w1)
        {
            // TODO: ???
            Console.WriteLine("Not Implemented");
            return float.NegativeInfinity;
        }
    }
}
