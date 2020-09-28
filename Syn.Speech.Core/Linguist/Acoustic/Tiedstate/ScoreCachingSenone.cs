using System;
using System.Runtime.Serialization;
using Syn.Speech.FrontEnds;
//PATROLLED REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Implements a Senone that contains a cache of the last scored data.
    ///
    /// Subclasses should implement the abstract {@link #calculateScore} method,
    /// which is called by the {@link #getScore} method to calculate the score
    /// for each cache miss.
    ///
    /// Note: this implementation is thread-safe and can be safely used
    /// across different threads without external synchronization.
    ///
    /// @author Yaniv Kunda
    /// </summary>
    public abstract class ScoreCachingSenone : ISenone
    {
        private class ScoreCache {
            internal readonly IData Feature;
            internal readonly float Score;

            public ScoreCache(IData feature, float score) 
            {
                Feature = feature;
                Score = score;
            }
        }

        private volatile ScoreCache _scoreCache = new ScoreCache(null, 0.0f);

        /// <summary>
        /// Gets the cached score for this senone based upon the given feature.
        /// If the score was not cached, it is calculated using {@link #calculateScore},
        /// cached, and then returned.  
        /// </summary>
        public virtual float GetScore(IData feature) {
            var cached = _scoreCache;
            if (feature != cached.Feature) {
                cached = new ScoreCache(feature, CalculateScore(feature));
                _scoreCache = cached;
            }
            return cached.Score;
        }

        /// <summary>
        /// Calculates the score for this senone based upon the given feature.
        /// </summary>
        /// <param name="feature">The feature vector to score this senone against.</param>
        /// <returns>The score for this senone in LogMath log base.</returns>
        public abstract float CalculateScore(IData feature);

        public abstract float[] CalculateComponentScore(IData feature);

        public abstract long ID { get; }

        public abstract void Dump(string msg);

        public abstract float[] GetLogMixtureWeights();

        /// <summary>
        /// Returns the mixture components associated with this Gaussian
        /// </summary>
        /// <value>The array of mixture components.</value>
        public abstract MixtureComponent[] MixtureComponents { get; }


        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
