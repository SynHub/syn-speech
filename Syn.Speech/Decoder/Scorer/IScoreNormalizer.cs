using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Scorer
{
    /// <summary>
    /// Describes all API-elements that are necessary  to normalize token-scores after these have been computed by an
    /// AcousticScorer.
    ///
    /// @author Holger Brandl
    /// @see Decoder.Scorer.AcousticScorer
    /// @see Decoder.Search.Token
    /// </summary>
    public interface IScoreNormalizer:IConfigurable
    {
         /**
        /// Normalizes the scores of a set of Tokens.
         *
        /// @param scoreableList The set of scores to be normalized
        /// @param bestToken     The best scoring Token of the above mentioned list. Although not strictly necessary it's
        ///                      included because of convenience reasons and to reduce computational overhead.
        /// @return The best token after the all <code>Token</code>s have been normalized. In most cases normalization won't
        ///         change the order but to keep the API open for any kind of approach it seemed reasonable to include this.
         */
        IScoreable normalize(List<IScoreable> scoreableList, IScoreable bestToken);
    }
}
