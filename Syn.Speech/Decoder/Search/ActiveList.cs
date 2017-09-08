using System;
using System.Collections.Generic;
using Syn.Speech.Common;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// An active list is maintained as a sorted list
    /// <p/>
    /// Note that all scores are represented in LogMath logbase
    /// </summary>
    /// 
    public abstract class ActiveList : SortedSet<Token>//,IActiveList
    {
        /// <summary>
        /// property that sets the desired (or target) size for this active list.  This is sometimes referred to as the beam size
        /// </summary>
        [S4Integer(defaultValue = 2000)]
        public static String  PROP_ABSOLUTE_BEAM_WIDTH = "absoluteBeamWidth";

        /// <summary>
        ///  Property that sets the minimum score relative to the maximum score in the list for pruning.  Tokens with a score
        /// less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// <summary>
        [S4Double(defaultValue = 0.0)]
        public static String PROP_RELATIVE_BEAM_WIDTH  = "relativeBeamWidth";

        /// <summary>
        /// Property that indicates whether or not the active list will implement 'strict pruning'.  When strict pruning is
        /// enabled, the active list will not remove tokens from the active list until they have been completely scored.  If
        /// strict pruning is not enabled, tokens can be removed from the active list based upon their entry scores. The
        /// default setting is false (disabled).
        /// <summary>
        [S4Boolean(defaultValue = true)]
        public static  String PROP_STRICT_PRUNING  = "strictPruning";

        /// <summary>
        /// Adds the given token to the list, keeping track of the lowest scoring token
        /// </summary>
        /// <param name="token">token the token to add</param>
        abstract public void add(Token token);
        /// <summary>
        /// Replaces an old token with a new token
        /// </summary>
        /// <param name="oldToken">the token to replace (or null in which case, replace works like add).</param>
        /// <param name="newToken">the new token to be placed in the list.</param>
       // abstract public void replace(Token oldToken, Token newToken);
        /// <summary>
        /// Purges the active list of excess members returning a (potentially new) active list
        /// </summary>
        /// <returns>a purged active list</returns>
        abstract public ActiveList purge();
        /// <summary>
        /// Returns the size of this list
        /// </summary>
        /// <returns>the size</returns>
        abstract public int size();

        /// <summary>
        /// Gets the list of all tokens 
        /// </summary>
        /// <returns>set of tokens</returns>
        abstract public List<Token> getTokens();

        /// <summary>
        /// gets the beam threshold best upon the best scoring token
        /// </summary>
        /// <returns>the beam threshold</returns>
        abstract public float getBeamThreshold();
        /// <summary>
        /// gets the best score in the list
        /// </summary>
        /// <returns>the best score</returns>
        abstract public float getBestScore();

        /// <summary>
        /// Sets the best scoring token for this active list
        /// </summary>
        /// <param name="token">token the best scoring token</param>
        abstract public void setBestToken(Token token);

        /// <summary>
        /// Gets the best scoring token for this active list
        /// </summary>
        /// <returns>the best scoring token</returns>
        abstract public Token getBestToken();

        /// <summary>
        /// Creates a new empty version of this active list with the same general properties.
        /// </summary>
        /// <returns>a new active list.</returns>
        abstract public ActiveList newInstance();

    }
}
