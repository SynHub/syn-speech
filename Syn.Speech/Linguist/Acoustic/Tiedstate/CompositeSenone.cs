using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Logging;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    /// <summary>
    /// Represents a composite senone. A composite senone consists of a set of all possible {@link Senone senones} for a
    /// given state. CompositeSenones are used when the exact context of a senone is not known. The CompositeSenone
    /// represents all the possible senones.
    ///
    /// This class currently only needs to be public for testing purposes.
    /// 
    /// Note that all scores are maintained in LogMath log base
    /// </summary>
    public class CompositeSenone : ScoreCachingSenone
    {
        private const int MaxSenones = 20000;
        private const Boolean WantMaxScore = true;
        private readonly float _weight;

        /// <summary>
        /// A factory method that creates a CompositeSenone from a list of senones.
        /// </summary>
        /// <param name="senoneCollection">The Collection of senones.</param>
        /// <param name="weight">The weight.</param>
        /// <returns>a composite senone</returns>
        public static CompositeSenone Create(ICollection<ISenone> senoneCollection, float weight)
        {
            return new CompositeSenone(senoneCollection.ToArray(), weight);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeSenone"/> class given the set of constituent senones
        /// </summary>
        /// <param name="senones">The set of constituent senones.</param>
        /// <param name="weight">The weight.</param>
        public CompositeSenone(ISenone[] senones, float weight)
        {
            Senones = senones;
            _weight = weight;
            Trace.Write(" " + senones.Length);
        }

        /// <summary>
        /// Dumps this senone
        /// </summary>
        /// <param name="msg">Annotation for the dump.</param>
        public override void Dump(String msg)
        {
            this.LogInfo("   CompositeSenone " + msg + ": ");
            foreach (var senone in Senones)
            {
                senone.Dump("   ");
            }
        }


        /// <summary>
        /// Calculates the composite senone score. Typically this is the best score for all of the constituent senones
        /// </summary>
        public override float CalculateScore(IData feature)
        {
            float logScore;
            if (WantMaxScore)
            {
                logScore = -Float.MAX_VALUE;
                foreach (var senone in Senones)
                {
                    logScore = Math.Max(logScore, senone.GetScore(feature));
                }
            }
            else
            { // average score
                logScore = 0.0f;
                foreach (var senone in Senones)
                {
                    logScore += senone.GetScore(feature);
                }
                logScore = logScore / Senones.Length;
            }
            return logScore + _weight;
        }

        /// <summary>
        /// Calculate scores for each component in the senone's distribution. Not yet implemented.
        /// </summary>
        /// <param name="feature">The current feature.</param>
        /// <returns>The score for the feature in LogMath.</returns>
        public override float[] CalculateComponentScore(IData feature)
        {
            Debug.Assert(false, "Not implemented!");
            return null;
        }


        /// <summary>
        /// Returns the set of senones that compose this composite senone. This method is only needed for unit testing.
        /// </summary>
        /// <value>
        /// The array of senones.
        /// </value>
        public ISenone[] Senones { get; private set; }

        /// <summary>
        /// Determines if two objects are equal
        /// </summary>
        /// <param name="o">The object to compare to this.</param>
        /// <returns>true if the objects are equal</returns>
        public override bool Equals(Object o)
        {
            if (!(o is ISenone))
            {
                return false;
            }
            var other = (ISenone)o;
            return ID == other.ID;
        }

        /// <summary>
        /// Returns the hashcode for this object
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var id = ID;
            var high = (int)((id >> 32));
            var low = (int)(id);
            return high + low;
        }


        /// <summary>
        /// Gets the ID for this senone.
        /// </summary>
        public override long ID
        {
            get
            {
                var factor = 1L;
                var id = 0L;
                foreach (var senone in Senones)
                {
                    id += senone.ID * factor;
                    factor = factor * MaxSenones;
                }
                return id;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "senone id: " + ID;
        }

        public override MixtureComponent[] MixtureComponents
        {
            get { return null; }
        }


        public override float[] GetLogMixtureWeights()
        {
            return null;
        }
    }
}
