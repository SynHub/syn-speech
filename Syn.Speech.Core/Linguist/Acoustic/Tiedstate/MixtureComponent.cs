using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using Syn.Speech.Logging;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Defines the set of shared elements for a GaussianMixture. Since these elements are potentially
    /// shared by a number of {@link GaussianMixture GaussianMixtures}, these elements should not be
    /// written to. The GaussianMixture defines a single probability density function along with a set of
    /// adaptation parameters.
    /// 
    /// Note that all scores and weights are in LogMath log base
    /// </summary>
    public class MixtureComponent : ICloneable, ISerializable
    {
        /// <summary>
        /// Mean after transformed by the adaptation parameters.
        /// </summary>
        protected float[] MeanTransformed;
        private float[][] _meanTransformationMatrix;
        private float[] _meanTransformationVector;
        /// <summary>
        /// Precision is the inverse of the variance. This includes adaptation.
        /// </summary>
        protected float[] PrecisionTransformed;
        private float[][] _varianceTransformationMatrix;
        private float[] _varianceTransformationVector;

        protected float DistFloor;
        private float _varianceFloor;

        public static float DefaultVarFloor = 0.0001f; // this also seems to be the default of SphinxTrain
        public static float DefaultDistFloor = 0.0f;

        protected float LogPreComputedGaussianFactor;
        protected LogMath LogMath;


        /**
        /// Create a MixtureComponent with the given sub components.
         *
        /// @param mean     the mean vector for this PDF
        /// @param variance the variance for this PDF
         */
        public MixtureComponent(float[] mean, float[] variance) 
            :this(mean, null, null, variance, null, null, DefaultDistFloor, DefaultVarFloor)
        {
   
        }


        /**
        /// Create a MixtureComponent with the given sub components.
         *
        /// @param mean                         the mean vector for this PDF
        /// @param meanTransformationMatrix     transformation matrix for this pdf
        /// @param meanTransformationVector     transform vector for this PDF
        /// @param variance                     the variance for this PDF
        /// @param varianceTransformationMatrix var. transform matrix for this PDF
        /// @param varianceTransformationVector var. transform vector for this PDF
         */
        public MixtureComponent(
                float[] mean,
                float[][] meanTransformationMatrix,
                float[] meanTransformationVector,
                float[] variance,
                float[][] varianceTransformationMatrix,
                float[] varianceTransformationVector) 
            :this(mean, meanTransformationMatrix, meanTransformationVector, variance,
                    varianceTransformationMatrix, varianceTransformationVector, DefaultDistFloor, DefaultVarFloor)
        {
            
        }


        /**
        /// Create a MixtureComponent with the given sub components.
         *
        /// @param mean                         the mean vector for this PDF
        /// @param meanTransformationMatrix     transformation matrix for this pdf
        /// @param meanTransformationVector     transform vector for this PDF
        /// @param variance                     the variance for this PDF
        /// @param varianceTransformationMatrix var. transform matrix for this PDF
        /// @param varianceTransformationVector var. transform vector for this PDF
        /// @param distFloor                    the lowest score value (in linear domain)
        /// @param varianceFloor                the lowest value for the variance
         */
        public MixtureComponent(
                float[] mean,
                float[][] meanTransformationMatrix,
                float[] meanTransformationVector,
                float[] variance,
                float[][] varianceTransformationMatrix,
                float[] varianceTransformationVector,
                float distFloor,
                float varianceFloor) {

            Debug.Assert(variance.Length == mean.Length);

            LogMath = LogMath.GetLogMath();
            Mean = mean;
            _meanTransformationMatrix = meanTransformationMatrix;
            _meanTransformationVector = meanTransformationVector;
            Variance = variance;
            _varianceTransformationMatrix = varianceTransformationMatrix;
            _varianceTransformationVector = varianceTransformationVector;

            Debug.Assert( distFloor >= 0.0 , "distFloot seems to be already in log-domain");
            this.DistFloor = LogMath.LinearToLog(distFloor);
            _varianceFloor = varianceFloor;

            TransformStats();

            LogPreComputedGaussianFactor = PrecomputeDistance();
        }


        /**
        /// Returns the mean for this component.
         *
        /// @return the mean
         */

        public float[] Mean { get; private set; }


        /**
        /// Returns the variance for this component.
         *
        /// @return the variance
         */

        public float[] Variance { get; private set; }


        /**
        /// Calculate the score for this mixture against the given feature.
        /// <p/>
        /// Note: The support of <code>DoubleData</code>-features would require an array conversion to
        /// float[]. Because getScore might be invoked with very high frequency, features are restricted
        /// to be <code>FloatData</code>s.
         *
        /// @param feature the feature to score
        /// @return the score, in log, for the given feature
         */
        public float GetScore(FloatData feature) {
            return GetScore(feature.Values);
        }


        /**
        /// Calculate the score for this mixture against the given feature. We model the output
        /// distributions using a mixture of Gaussians, therefore the current implementation is simply
        /// the computation of a multi-dimensional Gaussian. <p/> <p><b>Normal(x) = exp{-0.5/// (x-m)' *
        /// inv(Var)/// (x-m)} / {sqrt((2/// PI) ^ N)/// det(Var))}</b></p>
        /// <p/>
        /// where <b>x</b> and <b>m</b> are the incoming cepstra and mean vector respectively,
        /// <b>Var</b> is the Covariance matrix, <b>det()</b> is the determinant of a matrix,
        /// <b>inv()</b> is its inverse, <b>exp</b> is the exponential operator, <b>x'</b> is the
        /// transposed vector of <b>x</b> and <b>N</b> is the dimension of the vectors <b>x</b> and
        /// <b>m</b>.
         *
        /// @param feature the feature to score
        /// @return the score, in log, for the given feature
         */
        public float GetScore(float[] feature) {
            // float logVal = 0.0f;
            var logDval = LogPreComputedGaussianFactor;

            // First, compute the argument of the exponential function in
            // the definition of the Gaussian, then convert it to the
            // appropriate base. If the log base is <code>Math.E</code>,
            // then no operation is necessary.

            for (var i = 0; i < feature.Length; i++) {
                var logDiff = feature[i] - MeanTransformed[i];
                logDval += logDiff* logDiff* PrecisionTransformed[i];
            }
            // logDval = -logVal / 2;

            // At this point, we have the ln() of what we need, that is,
            // the argument of the exponential in the javadoc comment.

            // Convert to the appropriate base.
            logDval = LogMath.LnToLog(logDval);


            // System.out.println("MC: getscore " + logDval);

            // TODO: Need to use mean and variance transforms here

            if (float.IsNaN(logDval)) {
                this.LogInfo("gs is Nan, converting to 0");
                logDval = LogMath.LogZero;
            }

            if (logDval < DistFloor) {
                logDval = DistFloor;
            }

            return logDval;
        }


        /**
        /// Pre-compute factors for the Mahalanobis distance. Some of the Mahalanobis distance
        /// computation can be carried out in advance. Specifically, the factor containing only variance
        /// in the Gaussian can be computed in advance, keeping in mind that the the determinant of the
        /// covariance matrix, for the degenerate case of a mixture with independent components - only
        /// the diagonal elements are non-zero - is simply the product of the diagonal elements. <p/>
        /// We're computing the expression: <p/> <p><b>{sqrt((2/// PI) ^ N)/// det(Var))}</b></p>
         *
        /// @return the precomputed distance
         */
        public float PrecomputeDistance() {
            double logPreComputedGaussianFactor = 0.0f; // = log(1.0)
            // Compute the product of the elements in the Covariance
            // matrix's main diagonal. Covariance matrix is assumed
            // diagonal - independent dimensions. In log, the product
            // becomes a summation.
            for (var i = 0; i < Variance.Length; i++) {
                logPreComputedGaussianFactor +=
                        Math.Log(PrecisionTransformed[i]* -2);
                //	     variance[i] = 1.0f / (variance[i]/// 2.0f);
            }

            // We need the minus sign since we computed
            // logPreComputedGaussianFactor based on precision, which is
            // the inverse of the variance. Therefore, in the log domain,
            // the two quantities have opposite signs.

            // The covariance matrix's dimension becomes a multiplicative
            // factor in log scale.
            logPreComputedGaussianFactor =
                    Math.Log(2.0* Math.PI)*Variance.Length
                            - logPreComputedGaussianFactor;

            // The sqrt above is a 0.5 multiplicative factor in log scale.
            return -(float)logPreComputedGaussianFactor * 0.5f;
        }


        /** Applies transformations to means and variances. */
        public void TransformStats() {
            var featDim = Mean.Length;
            /*
           /// The transformed mean vector is given by:
            *
           /// <p><b>M = A/// m + B</b></p>
            *
           /// where <b>M</b> and <b>m</b> are the mean vector after and
           /// before transformation, respectively, and <b>A</b> and
           /// <b>B</b> are the transformation matrix and vector,
           /// respectively.
            *
           /// if A or B are <code>null</code> the according substeps are skipped
            */
            if (_meanTransformationMatrix != null) {
                MeanTransformed = new float[featDim];
                for (var i = 0; i < featDim; i++)
                    for (var j = 0; j < featDim; j++)
                        MeanTransformed[i] += Mean[j]* _meanTransformationMatrix[i][j];
            } else {
                MeanTransformed = Mean;
            }

            if (_meanTransformationVector != null)
                for (var k = 0; k < featDim; k++)
                    MeanTransformed[k] += _meanTransformationVector[k];

            /**
            /// We do analogously with the variance. In this case, we also
            /// invert the variance, and work with precision instead of
            /// variance.
             */
            if (_varianceTransformationMatrix != null) {
                PrecisionTransformed = new float[Variance.Length];
                for (var i = 0; i < featDim; i++)
                    for (var j = 0; j < featDim; j++)
                        PrecisionTransformed[i] += Variance[j]* _varianceTransformationMatrix[i][j];
            } else
                PrecisionTransformed = Variance.ToArray();

            if (_varianceTransformationVector != null)
                for (var k = 0; k < featDim; k++)
                    PrecisionTransformed[k] += _varianceTransformationVector[k];

            for (var k = 0; k < featDim; k++) {
                var flooredPrecision = (PrecisionTransformed[k] < _varianceFloor ? _varianceFloor : PrecisionTransformed[k]);
                PrecisionTransformed[k] = 1.0f / (-2.0f* flooredPrecision);
            }
        }

        public MixtureComponent Clone()
        {
            var mixComp = (MixtureComponent)base.MemberwiseClone();

            mixComp.DistFloor = DistFloor;
            mixComp._varianceFloor = _varianceFloor;
            mixComp.LogPreComputedGaussianFactor = LogPreComputedGaussianFactor;

            mixComp.Mean = (float[]) (Mean != null ? Mean.Clone() : null);
            if (_meanTransformationMatrix != null)
            {
                mixComp._meanTransformationMatrix = (float[][]) _meanTransformationMatrix.Clone();
                for (int i = 0; i < _meanTransformationMatrix.Length; i++)
                    mixComp._meanTransformationMatrix[i] = (float[]) _meanTransformationMatrix[i].Clone();
            }
            mixComp._meanTransformationVector = (float[]) (_meanTransformationVector != null ?
                _meanTransformationVector.Clone() : null);
            mixComp.MeanTransformed = (float[]) (MeanTransformed != null ? MeanTransformed.Clone() : null);

            mixComp.Variance = (float[]) (Variance != null ? Variance.Clone() : null);
            if (_varianceTransformationMatrix != null)
            {
                mixComp._varianceTransformationMatrix = (float[][]) _varianceTransformationMatrix.Clone();
                for (int i = 0; i < _varianceTransformationMatrix.Length; i++)
                    mixComp._varianceTransformationMatrix[i] = (float[]) _varianceTransformationMatrix[i].Clone();
            }
            mixComp._varianceTransformationVector = (float[]) (_varianceTransformationVector != null ?
                _varianceTransformationVector.Clone() : null);
            mixComp.PrecisionTransformed = (float[]) (PrecisionTransformed != null ?
                PrecisionTransformed.Clone() : null);

            return mixComp;
        }



        public override string ToString()
        {
            return "mu=" + Arrays.ToString(Mean) + " cov=" + Arrays.ToString(Variance);
        }

        object ICloneable.Clone()
        {
            return Clone();
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
