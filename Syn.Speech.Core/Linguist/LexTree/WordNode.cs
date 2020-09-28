using System;
using System.Linq;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    /// A node representing a word in the HMM tree
    /// </summary>
    public class WordNode: Node
    {
        private readonly Boolean _isFinal;
        /// <summary>
        /// true if the node is the final one
        /// </summary>
        public Boolean IsFinal
        {
            get { return _isFinal; }
        }

        /// <summary>
        /// Creates a word node
        /// </summary>
        /// <param name="pronunciation">the pronunciation to wrap in this node</param>
        /// <param name="probability">the word unigram probability</param>
        public WordNode(Pronunciation pronunciation, float probability) 
            :base(probability)
        {
            this.Pronunciation = pronunciation;
            _isFinal = pronunciation.Word.IsSentenceEndWord;
        }

        /// <summary>
        /// Gets the word associated with this node
        /// </summary>
        /// <returns>the word</returns>
        public Word GetWord() 
        {
            return Pronunciation.Word;
        }

        /// <summary>
        /// Gets the pronunciation associated with this node
        /// </summary>
        /// <value>the pronunciation</value>
        public Pronunciation Pronunciation { get; private set; }

        /// <summary>
        /// Gets the last unit for this word
        /// </summary>
        /// <value></value>
        public Unit LastUnit
        {
            get { return Pronunciation.Units.Last(); }
        }

        /// <summary>
        /// Returns the successors for this node 
        /// throw NotSupportedException
        /// </summary>
        /// <returns>set of successor nodes</returns>
        public override Node[] GetSuccessors() 
        {
            throw new NotSupportedException("Not supported");
        }

        /// <summary>
        /// Returns a string representation for this object 
        /// </summary>
        /// <returns>a string representation</returns>
        public override string ToString() 
        {
            return "WordNode " + Pronunciation + " p " + UnigramProbability;
        }


    }
}
