using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// A class that represents the initial word in the search space. It is treated specially because we need to keep track
    /// of the context as well. The context is embodied in the parent node
    /// </summary>
    public class InitialWordNode: WordNode
    {
        /// <summary>
        /// the parent node
        /// </summary>
        readonly HMMNode _parent;

        public HMMNode Parent
        {
            get { return _parent; }
        }

        /// <summary>
        /// Creates an InitialWordNode
        /// </summary>
        /// <param name="pronunciation">the pronunciation</param>
        /// <param name="parent">the parent node</param>
        public InitialWordNode(Pronunciation pronunciation, HMMNode parent)
            : base(pronunciation, LogMath.LogOne)
        {
            _parent = parent;
        }
    }
}
