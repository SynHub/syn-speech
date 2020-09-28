using System;
//INCOMPLETE
namespace Syn.Speech.Helper.Mathematics.Util
{
    public class MathArrays
    {
        public static bool verifyValues( double[] values,  int begin,
             int length,  bool allowEmpty)  {

        if (values == null) {
            throw new Exception("NullArgumentException");
        }

        if (begin < 0)
        {
            throw new Exception("NotPositiveException");
        }

        if (length < 0) {
            throw new Exception("NotPositiveException");
        }

        if (begin + length > values.Length) {
            throw new Exception("NumberIsTooLargeException");
        }

        if (length == 0 && !allowEmpty) {
            return false;
        }

        return true;

    }

          public static bool verifyValues( double[] values,  double[] weights,
             int begin,  int length,  bool allowEmpty)  {

        if (weights == null || values == null) {
            throw new Exception("NullArgumentException");
        }

        if (weights.Length != values.Length) {
            throw new Exception("DimensionMismatchException");
        }

        var containsPositiveWeight = false;
        for (int i = begin; i < begin + length; i++) {
             double weight = weights[i];
            if (Double.IsNaN(weight)) {
                throw new Exception("MathIllegalArgumentException");
            }
            if (Double.IsInfinity(weight)) {
                throw new Exception("MathIllegalArgumentException");
            }
            if (weight < 0) {
                throw new Exception("MathIllegalArgumentException");
            }
            if (!containsPositiveWeight && weight > 0.0) {
                containsPositiveWeight = true;
            }
        }

        if (!containsPositiveWeight) {
            throw new Exception("MathIllegalArgumentException");
        }

        return verifyValues(values, begin, length, allowEmpty);
    }
    }
}
