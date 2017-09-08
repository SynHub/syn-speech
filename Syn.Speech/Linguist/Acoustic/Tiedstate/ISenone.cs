using System;
using System.Runtime.Serialization;
using Syn.Speech.FrontEnds;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Represents a set of acoustic data that can be scored against a feature
    /// </summary>
    public interface ISenone : ISerializable
    {
        /// <summary>
        /// Calculates the score for this senone based upon the given feature.
        /// </summary>
        /// <param name="feature">The feature vector to score this senone against.</param>
        /// <returns>The score for this senone in LogMath log base.</returns>
        float GetScore(IData feature);

        /// <summary>
        /// Calculates the component scores for the mixture components in this senone based upon the given feature.
        /// </summary>
        /// <param name="feature">The feature vector to score this senone against.</param>
        /// <returns>The scores for this senone in LogMath log base.</returns>
        float[] CalculateComponentScore(IData feature);

        /// <summary>
        /// Gets the ID for this senone
        /// </summary>
        /// <value>
        /// The senone id.
        /// </value>
        long ID { get; }


        /// <summary>
        /// Dumps a senone
        /// </summary>
        /// <param name="msg">The annotation for the dump.</param>
        void Dump(String msg);


        /// <summary>
        /// Gets the mixture components associated with this Gaussian.
        /// </summary>
        /// <value>
        /// The array of mixture components.
        /// </value>
        MixtureComponent[] MixtureComponents { get; }

        /// <summary>
        /// Gets the mixture weights vector.
        /// </summary>
        float[] GetLogMixtureWeights();
    }
}
