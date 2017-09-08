using System;
using Syn.Logging;
using Syn.Speech.FrontEnds;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    public class GaussianMixture : ScoreCachingSenone
    {

        // these data element in a senone may be shared with other senones
        // and therefore should not be written to.
        protected GaussianWeights MixtureWeights;
        private readonly MixtureComponent[] _mixtureComponents;
        protected int _Id;

        protected LogMath LogMath;

        /// <summary>
        /// Initializes a new instance of the <see cref="GaussianMixture"/> class.
        /// </summary>
        /// <param name="mixtureWeights">The mixture weights for this senone in LogMath log base.</param>
        /// <param name="mixtureComponents">The mixture components for this senone.</param>
        /// <param name="id">The identifier.</param>
        public GaussianMixture(GaussianWeights mixtureWeights, MixtureComponent[] mixtureComponents, int id)
        {

            LogMath = LogMath.GetLogMath();
            _mixtureComponents = mixtureComponents;
            MixtureWeights = mixtureWeights;
            _Id = id;
        }

        /// <summary>
        /// Dumps a senone
        /// </summary>
        /// <param name="msg">Annotation message.</param>
        public override void Dump(String msg)
        {
            this.LogInfo(msg + " GaussianMixture: ID " + ID);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="o">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (!(o is ISenone))
            {
                return false;
            }
            var other = (ISenone)o;
            return ID == other.ID;
        }


        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var _id = ID;
            var high = (int)((_id >> 32));
            var low = (int)(_id);
            return high + low;
        }


        public override long ID
        {
            get { return _Id; }
        }

        /// <summary>
        /// Retrieves a string form of this object.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "senone id: " + ID;
        }

        public override float CalculateScore(IData feature)
        {
            if (feature is DoubleData)
                this.LogInfo("DoubleData conversion required on mixture level!");

            var featureVector = FloatData.ToFloatData(feature).Values;

            var logTotal = LogMath.LogZero;
            for (var i = 0; i < _mixtureComponents.Length; i++)
            {
                // In linear form, this would be:
                //
                // Total += Mixture[i].score * MixtureWeight[i]
                logTotal = LogMath.AddAsLinear(logTotal,
                        _mixtureComponents[i].GetScore(featureVector) + MixtureWeights.Get(_Id, 0, i));
            }

            return logTotal;
        }

        /// <summary>
        /// Calculates the scores for each component in the senone.
        /// </summary>
        /// <param name="feature">The feature to score.</param>
        /// <returns>The LogMath log scores for the feature, one for each component.</returns>
        public override float[] CalculateComponentScore(IData feature)
        {
            if (feature is DoubleData)
                this.LogInfo("DoubleData conversion required on mixture level!");

            var featureVector = FloatData.ToFloatData(feature).Values;

            var logComponentScore = new float[_mixtureComponents.Length];
            for (var i = 0; i < _mixtureComponents.Length; i++)
            {
                // In linear form, this would be:
                //
                // Total += Mixture[i].score * MixtureWeight[i]
                logComponentScore[i] = _mixtureComponents[i].GetScore(featureVector) + MixtureWeights.Get(_Id, 0, i);
            }

            return logComponentScore;
        }

        public override MixtureComponent[] MixtureComponents
        {
            get { return _mixtureComponents; }
        }

        /// <summary>
        /// Gets the dimension of the modeled feature space
        /// </summary>
        public virtual int GetDimension()
        {
            return _mixtureComponents[0].Mean.Length;
        }


        /// <summary>
        /// Numbers the number of component densities of this <code>GaussianMixture</code>.
        /// </summary>
        /// <value></value>
        public virtual int NumComponents
        {
            get { return _mixtureComponents.Length; }
        }

        public override float[] GetLogMixtureWeights()
        {
            var logWeights = new float[MixtureComponents.Length];
            for (var i = 0; i < logWeights.Length; i++)
                logWeights[i] = MixtureWeights.Get(_Id, 0, i);
            return logWeights;
        }

        /// <summary>
        /// Gets the (linearly scaled) mixture weights of the component densities
        /// </summary>
        /// <returns></returns>
        public float[] GetComponentWeights()
        {
            var mixWeights = new float[MixtureComponents.Length];
            for (var i = 0; i < mixWeights.Length; i++)
                mixWeights[i] = (float)LogMath.LogToLinear(MixtureWeights.Get(_Id, 0, i));

            return mixWeights;
        }

        /// <summary>
        /// the (log-scaled) mixture weight of the component density<code>index</code>.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public float GetLogComponentWeight(int index)
        {
            return MixtureWeights.Get(_Id, 0, index);
        }
    }
}
