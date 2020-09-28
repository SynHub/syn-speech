using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
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
        [S4Integer(DefaultValue = -1)]
        public static string PropAbsoluteBeamWidth = "absoluteBeamWidth";

        /// <summary>
        /// Property that sets the minimum score relative to the maximum score in the list for pruning.  Tokens with a score
        /// less than relativeBeamWidth/// maximumScore will be pruned from the list
        /// </summary>
        [S4Double(DefaultValue = 1E-80)]
        public static string PropRelativeBeamWidth = "relativeBeamWidth";

        /// <summary>
        /// Property that indicates whether or not the active list will implement 'strict pruning'.  When strict pruning is
        /// enabled, the active list will not remove tokens from the active list until they have been completely scored.  If
        /// strict pruning is not enabled, tokens can be removed from the active list based upon their entry scores. The
        /// default setting is false (disabled).
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropStrictPruning = "strictPruning";

        protected LogMath LogMath;
        protected int AbsoluteBeamWidth;
        protected float LogRelativeBeamWidth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveListFactory"/> class.
        /// </summary>
        /// <param name="absoluteBeamWidth">Width of the absolute beam.</param>
        /// <param name="relativeBeamWidth">Width of the relative beam.</param>
        protected ActiveListFactory(int absoluteBeamWidth,double relativeBeamWidth)
        {
            LogMath = LogMath.GetLogMath();
            AbsoluteBeamWidth = absoluteBeamWidth;
            LogRelativeBeamWidth = LogMath.LinearToLog(relativeBeamWidth);      
        }

        protected ActiveListFactory() 
        {
        }


        public virtual void NewProperties(PropertySheet ps)
        {
            LogMath = LogMath.GetLogMath();
            AbsoluteBeamWidth = ps.GetInt(PropAbsoluteBeamWidth);
            double relativeBeamWidth = ps.GetDouble(PropRelativeBeamWidth);

            LogRelativeBeamWidth = LogMath.LinearToLog(relativeBeamWidth);
        }

        /// <summary>
        /// Creates a new active list of a particular type
        /// </summary>
        /// <returns></returns>
        public abstract ActiveList NewInstance();

    }
}
