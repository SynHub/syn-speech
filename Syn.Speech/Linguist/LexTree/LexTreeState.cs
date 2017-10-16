using System;
using System.Diagnostics;
using Syn.Speech.Logging;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public abstract class LexTreeState : ISearchState, ISearchStateArc 
    {

        public LexTreeLinguist Parent;

        private readonly Node _node;
        private readonly WordSequence _wordSequence;

        /**
           /// Creates a LexTreeState.
            *
           /// @param node         the node associated with this state
           /// @param wordSequence the history of words up until this point


            */
        public LexTreeState(Node node, WordSequence wordSequence, float smearTerm,
            float smearProb,LexTreeLinguist parent) 
        {
            Parent = parent;
            _node = node;
            _wordSequence = wordSequence;
            SmearTerm = smearTerm;
            SmearProb = smearProb;
        }


        /**
           /// Gets the unique signature for this state. The signature building code is slow and should only be used for
           /// non-time-critical tasks such as plotting states.
            *
           /// @return the signature
            */

        public virtual string Signature
        {
            get { return "lts-" + _node.GetHashCode() + "-ws-" + _wordSequence; }
        }


        public float SmearTerm { get; private set; }


        public float SmearProb { get; private set; }


        /**
           /// Generate a hashcode for an object
            *
           /// @return the hashcode
            */
        public override int GetHashCode() 
        {
            //TODO: PERFORMANCE CRTICAL - COMPUTED ONLY ONCE PER SESSION
            var hashCode = _wordSequence.GetHashCode() * 37;
            hashCode += _node.GetHashCode();
            return hashCode;
        }


        /**
           /// Determines if the given object is equal to this object
            *
           /// @param o the object to test
           /// @return <code>true</code> if the object is equal to this
            */
        
        public override bool Equals(Object o)
        {
            if (o == this) {
                return true;
            }
            if (o is LexTreeState) {
                var other = (LexTreeState) o;
                if (_node != other._node)
                    return false;
                return _wordSequence.Equals(other._wordSequence);
            }
            return false;
        }


        /**
           /// Gets a successor to this search state
            *
           /// @return the successor state
            */

        public ISearchState State
        {
            get { return this; }
        }


        /**
           /// Gets the composite probability of entering this state
            *
           /// @return the log probability
            */
        public float GetProbability() {
            return LanguageProbability + InsertionProbability;
        }


        /**
           /// Gets the language probability of entering this state
            *
           /// @return the log probability
            */

        public virtual float LanguageProbability
        {
            get { return Parent.LogOne; }
        }


        /**
           /// Gets the insertion probability of entering this state
            *
           /// @return the log probability
            */

        public virtual float InsertionProbability
        {
            get { return Parent.LogOne; }
        }


        /**
           /// Determines if this is an emitting state
            *
           /// @return <code>true</code> if this is an emitting state.
            */

        public virtual bool IsEmitting
        {
            get { return false; }
        }


        /**
           /// Determines if this is a final state
            *
           /// @return <code>true</code> if this is an final state.
            */

        public virtual bool IsFinal
        {
            get { return false; }
        }


        /**
           /// Gets the hmm tree node representing the unit
            *
           /// @return the unit lex node
            */

        protected Node GetNode()
        {
            return _node;
        }


        /**
           /// Returns the word sequence for this state
            *
           /// @return the word sequence
            */

        public WordSequence WordHistory
        {
            get { return _wordSequence; }
        }


        public object LexState
        {
            get { return _node; }
        }


        /**
           /// Returns the list of successors to this state
            *
           /// @return a list of SearchState objects
            */
        public virtual ISearchStateArc[] GetSuccessors() 
        {
            ISearchStateArc[] arcs = GetCachedArcs();
            if (arcs == null) {
                arcs = GetSuccessors(_node);
                PutCachedArcs(arcs);
            }
            return arcs;
        }


        /**
           /// Returns the list of successors to this state
            *

           /// @return a list of SearchState objects
            */
        protected ISearchStateArc[] GetSuccessors(Node theNode) 
        {
            Node[] nodes = theNode.GetSuccessors();
            //this.LogDebug("GetSuccessors(Node theNode): {0}", nodes.Length);
            ISearchStateArc[] arcs = new ISearchStateArc[nodes.Length];
            //this.LogInfo("Arc: "+ this);
            int i = 0;
            foreach (Node nextNode in nodes) 
            {
                //this.LogDebug("Next Node is of type: {0}",nextNode.GetType());
                if (nextNode is WordNode) 
                {
                    arcs[i] = CreateWordStateArc((WordNode) nextNode,
                        (HMMNode) GetNode(), this);
                } 
                else if (nextNode is EndNode) 
                {
                    arcs[i] = CreateEndUnitArc((EndNode) nextNode, this);
                } 
                else {
                    arcs[i] = CreateUnitStateArc((HMMNode) nextNode, this);
                }
                i++;
            }
            return arcs;
        }


        /**
           /// Creates a word search state for the given word node
            *
           /// @param wordNode the wordNode


           /// @return the search state for the wordNode
            */
        protected ISearchStateArc CreateWordStateArc(WordNode wordNode,
            HMMNode lastUnit, LexTreeState previous) 
        {
            //TODO: UNCOMMENT DURING RELEASE
            //this.LogInfo("CWSA " + wordNode + " fup " /*+ fixupProb*/);
            float languageProbability = Parent.LogOne;
            Word nextWord = wordNode.GetWord();
            float smearTerm = previous.SmearTerm;

            if (nextWord.IsFiller && !Equals(nextWord, Parent.SentenceEndWord)) {
                return new LexTreeWordState(wordNode, lastUnit,
                    _wordSequence,
                    smearTerm, Parent.LogOne, languageProbability ,Parent);
            }

            WordSequence nextWordSequence = _wordSequence.AddWord(nextWord, Parent.MaxDepth);
            float probability = Parent.LanguageModel.GetProbability(nextWordSequence) * Parent.LanguageWeight;
            smearTerm = Parent.GetSmearTermFromLanguageModel(nextWordSequence);

            //this.LogInfo("LP " + nextWordSequence + " " /*+ logProbability*/);
            //    subtract off the previously applied smear probability
            languageProbability = probability - previous.SmearProb;

            //Boolean collapse = (probability.depth < parent.maxDepth - 1) || !parent.fullWordHistories;

            if (Equals(nextWord, Parent.SentenceEndWord)) 
            {
                return new LexTreeEndWordState(wordNode, lastUnit,
                    nextWordSequence.Trim(Parent.MaxDepth - 1),
                    smearTerm, Parent.LogOne, languageProbability,Parent);
            }

            return new LexTreeWordState(wordNode, lastUnit,
                nextWordSequence.Trim(Parent.MaxDepth - 1),
                smearTerm, Parent.LogOne, languageProbability ,Parent);
        }


        /**
           /// Creates a unit search state for the given unit node
            *
           /// @param hmmNode the unit node

           /// @return the search state
            */
        public ISearchStateArc CreateUnitStateArc(HMMNode hmmNode, LexTreeState previous) 
        {
            ISearchStateArc arc;

            float insertionProbability = Parent.CalculateInsertionProbability(hmmNode);
            float smearProbability = Parent.GetUnigramSmear(hmmNode) + previous.SmearTerm;
            float languageProbability = smearProbability - previous.SmearProb;

            //if we want a unit state create it, otherwise
            //get the first hmm state of the unit

            if (Parent.GenerateUnitStates) {
                arc = new LexTreeUnitState(hmmNode, WordHistory, previous.SmearTerm, smearProbability, languageProbability,
                    insertionProbability,Parent);
            } else {
                IHMM hmm = hmmNode.HMM;
                arc = new LexTreeHmmState(hmmNode, WordHistory, previous.SmearTerm, smearProbability, hmm.GetInitialState(),
                    languageProbability, insertionProbability, null,Parent);
            }
            return arc;
        }


        /**
           /// Creates a unit search state for the given unit node
            *
           /// @param endNode  the unit node
           /// @param previous the previous state
           /// @return the search state
            */
        ISearchStateArc CreateEndUnitArc(EndNode endNode, LexTreeState previous) 
        {
            float smearProbability = Parent.GetUnigramSmear(endNode) + previous.SmearTerm;
            float languageProbability = smearProbability - previous.SmearProb;
            float insertionProbability = Parent.CalculateInsertionProbability(endNode);
            return new LexTreeEndUnitState(endNode, WordHistory, previous.SmearTerm, smearProbability, languageProbability,
                insertionProbability, Parent);
        }


        /**
           /// Returns the string representation of this object
            *
           /// @return the string representation
            */

        public override string ToString()
        {
            return "lt-" + _node + ' ' + GetProbability() + '{' + _wordSequence
                   + '}';
            }


        /**
           /// Returns a pretty version of the string representation for this object
            *
           /// @return a pretty string
            */
        public string ToPrettyString() {
            return ToString();
        }


        /**
           /// Gets the successor arcs for this state from the cache
            *
           /// @return the next set of arcs for this state, or null if none can be found or if caching is disabled.
            */
        public ISearchStateArc[] GetCachedArcs()
        {
            if (Parent.CacheEnabled) 
            {
                ISearchStateArc[] arcs = Parent.ArcCache.Get(this);
                if (arcs != null) {
                    Parent.CacheHits++;
                }
                if (++Parent.CacheTrys % 1000000 == 0) {
                    this.LogInfo("Hits: " + Parent.CacheHits
                                    + " of " + Parent.CacheTrys + ' ' +
                                    ((float) Parent.CacheHits) / Parent.CacheTrys * 100f);
                }
                return arcs;
            }
            return null;
        }


        /**
           /// Puts the set of arcs into the cache
            *
           /// @param arcs the arcs to cache.
            */
        public void PutCachedArcs(ISearchStateArc[] arcs) 
        {
            if (Parent.CacheEnabled) {
                Parent.ArcCache.Put(this, arcs);
            }
        }


        public abstract int Order { get; }
    }
}