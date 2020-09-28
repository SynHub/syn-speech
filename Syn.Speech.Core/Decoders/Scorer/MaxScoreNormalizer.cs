using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// Performs a simple normalization of all token-scores by
    /// </summary>
    public class MaxScoreNormalizer : IScoreNormalizer
    {

        public void NewProperties(PropertySheet ps)
        {
        }

        public MaxScoreNormalizer()
        {
        }

        public IScoreable Normalize<T>(List<T> scoreableList, IScoreable bestToken) where T :IScoreable
        {
            foreach (var scoreable in scoreableList)
            {
                scoreable.NormalizeScore(bestToken.Score);
            }

            return bestToken;
        }
    }
}
