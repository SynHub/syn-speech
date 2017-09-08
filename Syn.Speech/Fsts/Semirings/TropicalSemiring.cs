using System;
//REFACTORED
namespace Syn.Speech.Fsts.Semirings
{
    /// <summary>
    /// Tropical semiring implementation.
    /// 
    /// @author "John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class TropicalSemiring: Semiring
    {
        //zero value
        public static float _zero = float.PositiveInfinity;

        //one value
        public static float _one = 0.0f;

        public override float Plus(float w1, float w2) 
        {
            if (!IsMember(w1) || !IsMember(w2)) {
                return float.NegativeInfinity;
            }

            return w1 < w2 ? w1 : w2;
        }

        public override float Times(float w1, float w2) 
        {
            if (!IsMember(w1) || !IsMember(w2)) {
                return float.NegativeInfinity;
            }

            return w1 + w2;
        }


        public override float Divide(float w1, float w2) 
        {
            if (!IsMember(w1) || !IsMember(w2)) {
                return float.NegativeInfinity;
            }

            if (w2 == _zero) 
            {
                return float.NegativeInfinity;
            } else if (w1 == _zero) {
                return _zero;
            }

            return w1 - w2;
        }

        public override float Zero
        {
            get { return _zero; }
        }


        public override float One
        {
            get { return _one; }
        }

        public override Boolean IsMember(float w) {
            return (!float.IsNaN(w)) // not a NaN
                    && (w != float.NegativeInfinity); // and different from -inf
        }

        public override float Reverse(float w1) {
            return w1;
        }
    }
}
