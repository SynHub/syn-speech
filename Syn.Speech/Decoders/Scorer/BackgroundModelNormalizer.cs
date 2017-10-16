using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Logging;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// Normalizes a set of Tokens against the best scoring Token of a background model.
    /// </summary>
    public class BackgroundModelNormalizer : IScoreNormalizer
    {

        /// <summary>
        /// The active list provider used to determined the best token for normalization. If this reference is not defined no normalization will be applied.
        /// </summary>
        [S4Component(Type = typeof(SimpleBreadthFirstSearchManager), Mandatory = false)]
        public const string ActiveListProvider = "activeListProvider";
        private SimpleBreadthFirstSearchManager _activeListProvider;

        public BackgroundModelNormalizer()
        {
        }

        public void NewProperties(PropertySheet ps)
        {
            _activeListProvider = (SimpleBreadthFirstSearchManager)ps.GetComponent(ActiveListProvider);
            this.LogInfo("no active list set.");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundModelNormalizer"/> class.
        /// </summary>
        /// <param name="activeListProvider">The active list provider used to determined the best token for normalization. 
        /// If this reference is not defined no normalization will be applied.</param>
        public BackgroundModelNormalizer(SimpleBreadthFirstSearchManager activeListProvider)
        {
            this._activeListProvider = activeListProvider;
            this.LogInfo("no active list set.");
        }

        public IScoreable Normalize<T>(List<T> scoreableList, IScoreable bestToken) where T : IScoreable
        {
            if (_activeListProvider == null)
            {
                return bestToken;
            }

            var normToken = _activeListProvider.GetActiveList().GetBestToken();

            Debug.Assert(bestToken.FrameNumber == normToken.FrameNumber - 1, "frame numbers should be equal for a meaningful normalization");

            float normScore = normToken.Score;

            foreach (IScoreable scoreable in scoreableList)
            {
                if (scoreable is Token)
                {
                    scoreable.NormalizeScore(normScore);
                }
            }

            return bestToken;
        }
    }
}
