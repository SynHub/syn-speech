using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Scorer
{
    /// <summary>
    /// Performs a simple normalization of all token-scores by
    /// </summary>
    public class MaxScoreNormalizer : IScoreNormalizer
    {

        public void newProperties(PropertySheet ps)
        {
        }

        public MaxScoreNormalizer()
        {
        }

        public IScoreable normalize(List<IScoreable> scoreableList, IScoreable bestToken)
        {
            foreach (var scoreable in scoreableList)
            {
                scoreable.normalizeScore(bestToken.getScore());
            }

            return bestToken;
        }
    }
}
