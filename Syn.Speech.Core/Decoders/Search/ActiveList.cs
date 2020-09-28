using System.Collections;
using System.Collections.Generic;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// An active list is maintained as a sorted list
    /// <p/>
    /// Note that all scores are represented in LogMath logbase
    /// </summary>
    /// 
    public abstract class ActiveList : IEnumerable<Token>//,IActiveList
    {
        /// <summary>
        /// property that sets the desired (or target) size for this active list.  This is sometimes referred to as the beam size
        /// </summary>
        [S4Integer(DefaultValue = 2000)]
        public static string  PropAbsoluteBeamWidth = "absoluteBeamWidth";

        /// <summary>
        ///  Property that sets the minimum score relative to the maximum score in the list for pruning.  Tokens with a score
        /// less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// </summary>
        [S4Double(DefaultValue = 0.0)]
        public static string PropRelativeBeamWidth  = "relativeBeamWidth";

        /// <summary>
        /// Property that indicates whether or not the active list will implement 'strict pruning'.  When strict pruning is
        /// enabled, the active list will not remove tokens from the active list until they have been completely scored.  If
        /// strict pruning is not enabled, tokens can be removed from the active list based upon their entry scores. The
        /// default setting is false (disabled).
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static  string PropStrictPruning  = "strictPruning";

        /// <summary>
        /// Adds the given token to the list, keeping track of the lowest scoring token
        /// </summary>
        /// <param name="token">token the token to add</param>
        abstract public void Add(Token token);

        // abstract public void replace(Token oldToken, Token newToken);
        /// <summary>
        /// Replaces an old token with a new token
        /// </summary>
        /// <summary>
        /// Purges the active list of excess members returning a (potentially new) active list
        /// </summary>
        /// <returns>a purged active list</returns>
        abstract public ActiveList Purge();

        /// <summary>
        /// Returns the size of this list
        /// </summary>
        /// <value>the size</value>
        public abstract int Size { get; }

        /// <summary>
        /// Gets the list of all tokens 
        /// </summary>
        /// <returns>set of tokens</returns>
        abstract public List<Token> GetTokens();

        /// <summary>
        /// gets the beam threshold best upon the best scoring token
        /// </summary>
        /// <returns>the beam threshold</returns>
        abstract public float GetBeamThreshold();
        /// <summary>
        /// gets the best score in the list
        /// </summary>
        /// <returns>the best score</returns>
        abstract public float GetBestScore();

        /// <summary>
        /// Sets the best scoring token for this active list
        /// </summary>
        /// <param name="token">token the best scoring token</param>
        abstract public void SetBestToken(Token token);

        /// <summary>
        /// Gets the best scoring token for this active list
        /// </summary>
        /// <returns>the best scoring token</returns>
        abstract public Token GetBestToken();

        /// <summary>
        /// Creates a new empty version of this active list with the same general properties.
        /// </summary>
        /// <returns>a new active list.</returns>
        abstract public ActiveList NewInstance();

        public IEnumerator<Token> GetEnumerator()
        {
            return GetTokens().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
