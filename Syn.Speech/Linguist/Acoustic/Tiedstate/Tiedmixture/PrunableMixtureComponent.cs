using Syn.Logging;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture
{

    public class PrunableMixtureComponent : MixtureComponent
    {
        public PrunableMixtureComponent(
                float[] mean,
                float[][] meanTransformationMatrix,
                float[] meanTransformationVector,
                float[] variance,
                float[][] varianceTransformationMatrix,
                float[] varianceTransformationVector,
                float distFloor,
                float varianceFloor,
                int id)
            : base(mean, meanTransformationMatrix, meanTransformationVector, variance, varianceTransformationMatrix, varianceTransformationVector, distFloor, varianceFloor)
        {
            PartialScore = LogMath.LogZero;
            StoredScore = LogMath.LogZero;

            Id = id;
        }

        private float ConvertScore(float val)
        {
            // Convert to the appropriate base.
            val = LogMath.LnToLog(val);

            // TODO: Need to use mean and variance transforms here

            if (float.IsNaN(val))
            {
                this.LogInfo("gs is Nan, converting to 0");
                val = LogMath.LogZero;
            }

            if (val < DistFloor)
            {
                val = DistFloor;
            }

            return val;
        }

        public bool IsTopComponent(float[] feature, float threshold)
        {

            var logDval = LogPreComputedGaussianFactor;

            // First, compute the argument of the exponential function in
            // the definition of the Gaussian, then convert it to the
            // appropriate base. If the log base is <code>Math.E</code>,
            // then no operation is necessary.
            for (var i = 0; i < feature.Length; i++)
            {
                var logDiff = feature[i] - MeanTransformed[i];
                logDval += logDiff * logDiff * PrecisionTransformed[i];
                if (logDval < threshold)
                    return false;
            }

            PartialScore = logDval;
            StoredScore = ConvertScore(logDval);
            return true;
        }

        public void UpdateScore(float[] feature)
        {

            var logDval = LogPreComputedGaussianFactor;

            // First, compute the argument of the exponential function in
            // the definition of the Gaussian, then convert it to the
            // appropriate base. If the log base is <code>Math.E</code>,
            // then no operation is necessary.
            for (var i = 0; i < feature.Length; i++)
            {
                var logDiff = feature[i] - MeanTransformed[i];
                logDval += logDiff * logDiff * PrecisionTransformed[i];
            }

            PartialScore = logDval;
            StoredScore = ConvertScore(logDval);
        }

        public float StoredScore { get; private set; }

        public float PartialScore { get; private set; }

        public int Id { get; private set; }
    }

}
