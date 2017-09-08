using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Scorer
{
    /// <summary>
    /// Normalizes a set of Tokens against the best scoring Token of a background model.
    /// </summary>
    public class BackgroundModelNormalizer : IScoreNormalizer
    {

        /// <summary>
        /// The active list provider used to determined the best token for normalization. If this reference is not defined no normalization will be applied.
        /// </summary>
        [S4Component(type = typeof(SimpleBreadthFirstSearchManager), mandatory = false)]
        public const string ACTIVE_LIST_PROVIDER = "activeListProvider";
        private SimpleBreadthFirstSearchManager activeListProvider;

        public BackgroundModelNormalizer()
        {
        }

        public void newProperties(PropertySheet ps)
        {
            this.activeListProvider = (SimpleBreadthFirstSearchManager)ps.getComponent(ACTIVE_LIST_PROVIDER);
            Trace.WriteLine("no active list set.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundModelNormalizer"/> class.
        /// </summary>
        /// <param name="activeListProvider">The active list provider used to determined the best token for normalization. 
        /// If this reference is not defined no normalization will be applied.</param>
        public BackgroundModelNormalizer(SimpleBreadthFirstSearchManager activeListProvider)
        {
            this.activeListProvider = activeListProvider;
            Trace.WriteLine("no active list set.");
        }

        public IScoreable normalize(List<IScoreable> scoreableList, IScoreable bestToken)
        {
            if (activeListProvider == null)
            {
                return bestToken;
            }

            var normToken = (Token)activeListProvider.getActiveList().getBestToken();

            Trace.Assert(bestToken.getFrameNumber() == normToken.getFrameNumber() - 1, "frame numbers should be equal for a meaningful normalization");
            //TODO: CHECK SYNTAX
            //assert bestToken.getFrameNumber() == normToken.getFrameNumber() - 1 : "frame numbers should be equal for a meaningful normalization";

            float normScore = normToken.getScore();

            foreach (IScoreable scoreable in scoreableList)
            {
                if (scoreable is Token)
                {
                    scoreable.normalizeScore(normScore);
                }
            }

            return bestToken;
        }
    }
}
