using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    /// <summary>
    /// An active list is maintained as a sorted list
    /// </summary>
    public abstract class ActiveListManager:IConfigurable
    {
        /// <summary>
        /// The property that specifies the absolute word beam width
        /// </summary>
        [S4Integer(DefaultValue = 2000)]
        public static string PropAbsoluteWordBeamWidth =
                "absoluteWordBeamWidth";

        /// <summary>
        /// The property that specifies the relative word beam width
        /// </summary>
        [S4Double(DefaultValue = 0.0)]
        public static string PropRelativeWordBeamWidth = "relativeWordBeamWidth";

        /// <summary>
        /// Adds the given token to the list
        /// </summary>
        /// <param name="token">The token to add.</param>
        public abstract void Add(Token token);


        /**
        /// Replaces an old token with a new token
         *
        /// @param oldToken the token to replace (or null in which case, replace works like add).
        /// @param newToken the new token to be placed in the list.
         */
        //public abstract void replace(Token oldToken, Token newToken);


        /// <summary>
        /// Gets an Iterator of all the non-emitting ActiveLists. The iteration order is the same as the search state order.
        /// </summary>
        /// <returns>An Iterator of non-emitting ActiveLists.</returns>
        public abstract Iterator<ActiveList> GetNonEmittingListIterator();

        /// <summary>
        /// Gets the emitting ActiveList from the manager
        /// </summary>
        /// <returns>The emitting ActiveList</returns>
        public abstract ActiveList GetEmittingList();


        /// <summary>
        /// Clears emitting list in manager.
        /// </summary>
        public abstract void ClearEmittingList();


        /// <summary>
        /// Dumps out debug info for the active list manager.
        /// </summary>
        public abstract void Dump();


        /// <summary>
        /// Sets the total number of state types to be managed
        /// </summary>
        /// <param name="numStateOrder">The total number of state types.</param>
        public abstract void SetNumStateOrder(int numStateOrder);


        public abstract void NewProperties(PropertySheet ps);
    }
}
