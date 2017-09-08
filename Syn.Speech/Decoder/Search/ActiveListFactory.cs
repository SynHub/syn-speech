using System;
using Syn.Speech.Common;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// Creates new active lists. 
    /// </summary>
    public abstract class ActiveListFactory:IConfigurable
    {
        /// <summary>
        /// property that sets the desired (or target) size for this active list.  This is sometimes referred to as the beam
        /// size 
        /// </summary>
        [S4Integer(defaultValue = -1)]
        public static String PROP_ABSOLUTE_BEAM_WIDTH = "absoluteBeamWidth";

        /// <summary>
        /// Property that sets the minimum score relative to the maximum score in the list for pruning.  Tokens with a score
        /// less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// </summary>
        [S4Double(defaultValue = 1E-80)]
        public static String PROP_RELATIVE_BEAM_WIDTH = "relativeBeamWidth";

        /// <summary>
        /// Property that indicates whether or not the active list will implement 'strict pruning'.  When strict pruning is
        /// enabled, the active list will not remove tokens from the active list until they have been completely scored.  If
        /// strict pruning is not enabled, tokens can be removed from the active list based upon their entry scores. The
        /// default setting is false (disabled).
        /// </summary>
        [S4Boolean(defaultValue = true)]
        public static String PROP_STRICT_PRUNING = "strictPruning";

        protected LogMath logMath;
        protected int absoluteBeamWidth;
        protected float logRelativeBeamWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveListFactory"/> class.
        /// </summary>
        /// <param name="absoluteBeamWidth">Width of the absolute beam.</param>
        /// <param name="relativeBeamWidth">Width of the relative beam.</param>
        protected ActiveListFactory(int absoluteBeamWidth,double relativeBeamWidth)
        {
            logMath = LogMath.getLogMath();
            this.absoluteBeamWidth = absoluteBeamWidth;
            this.logRelativeBeamWidth = logMath.linearToLog(relativeBeamWidth);      
        }

        protected ActiveListFactory() 
        {
        }


        public void newProperties(PropertySheet ps)
        {
            logMath = LogMath.getLogMath();
            absoluteBeamWidth = ps.getInt(PROP_ABSOLUTE_BEAM_WIDTH);
            double relativeBeamWidth = ps.getDouble(PROP_RELATIVE_BEAM_WIDTH);

            logRelativeBeamWidth = logMath.linearToLog(relativeBeamWidth);
        }

        /// <summary>
        /// Creates a new active list of a particular type
        /// </summary>
        /// <returns></returns>
        public abstract ActiveList newInstance();

    }
}
