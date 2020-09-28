using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a single state in an SentenceHMM
    /// </summary>
    public abstract class SentenceHMMState : ISerializable, ISearchState
    {
        private const int MaskIsFinal = 0x1;
        private const int MaskColorRed = 0x2;
        private const int MaskProcessed = 0x4;
        private const int MaskFanIn = 0x8;
        private const int MaskIsWordStart = 0x10;
        private const int MaskIsSharedState = 0x20;
        private const int MaskWhich = 0xffff;
        private const int ShiftWhich = 0x8;

        /** A Color is used to tag SentenceHMM nodes */
        public enum Color { Red, Green }

        private static int _globalStateNumber = -1000;

        // a number of separate variables are maintained in 'fields'
        // inorder to reduce the size of the SentenceHMM

        private int _fields;

        public LinkedHashMap<string, SentenceHMMStateArc> arcs;
        private readonly SentenceHMMState _parent;
        private string _cachedName;
        private string _fullName;
        public SentenceHMMStateArc[] successorArray;


        /**
        /// Creates a SentenceHMMState
         *
        /// @param name   the name of the current SentenceHMMState
        /// @param parent the parent of the current instance
        /// @param which  the index of the current instance
         */
        protected SentenceHMMState(String name, SentenceHMMState parent, int which) 
            :this()
        {
            this.Name = name + which;
            this._parent = parent;
            SetWhich(which);
            SetProcessed(false);
            SetColor(Color.Red);
        }


        /** Empty contructor */
        protected SentenceHMMState() 
        {
            StateNumber = _globalStateNumber--;
            arcs = new LinkedHashMap<String, SentenceHMMStateArc>();
        }


        /**
        /// Determines if this state marks the beginning of a word
         *
        /// @return true if the state marks the beginning of a word
         */
        public virtual Boolean IsWordStart() 
        {
            return (_fields & MaskIsWordStart) == MaskIsWordStart;
        }


        /**
        /// Sets the 'wordStart' flag
         *
        /// @param wordStart <code>true</code> if this state marks the beginning of a word.
         */
        public void SetWordStart(Boolean wordStart) 
        {
            if (wordStart) 
            {
                _fields |= MaskIsWordStart;
            } 
            else 
            {
                _fields &= ~MaskIsWordStart;
            }
        }


        /**
        /// Determines if this state is a shard state
         *
        /// @return true if the state marks the beginning of a word
         */
        public Boolean IsSharedState() 
        {
            return (_fields & MaskIsSharedState) == MaskIsSharedState;
        }


        /**
        /// Sets the shared state flag
         *
        /// @param shared <code>true</code> if this state is shared
         */
        public void SetSharedState(Boolean shared) 
        {
            if (shared) {
                _fields |= MaskIsSharedState;
            } else {
                _fields &= ~MaskIsSharedState;
            }
        }


        /**
        /// Returns the word associated with the particular unit
         *
        /// @return the word associated with this state, or null if there is no word associated with this state.
         */
        public Word GetAssociatedWord() 
        {
            Word word = null;
            var state = this;

            while (state != null && !(state is WordState)) 
            {
                state = state.GetParent();
            }

            if (state != null) 
            {
                var wordState = (WordState) state;
                word = wordState.GetWord();
            }
            return word;
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public virtual string TypeLabel
        {
            get { return "state"; }
        }


        /**
        /// Determines if this state is a fan-in state. The search may need to adjust the pruning for states that fan in
        /// since they are shared by multiple paths
         *
        /// @return <code>true</code> if the state is a fan in state
         */
        public Boolean IsFanIn() 
        {
            return (_fields & MaskFanIn) == MaskFanIn;
        }


        /**
        /// Sets the fan in state
         *
        /// @param fanIn if true its a fan in state
         */
        public void SetFanIn(bool fanIn) 
        {
            if (fanIn) {
                _fields |= MaskFanIn;
            } else {
                _fields &= ~MaskFanIn;
            }
        }


        /**
        /// Sets the processed flag for this state
         *
        /// @param processed the new setting for the processed flag
         */
        public void SetProcessed(bool processed) 
        {
            if (processed) {
                _fields |= MaskProcessed;
            } else {
                _fields &= ~MaskProcessed;
            }
        }


        /**
        /// Determines if this state has been 'processed'. The meaning of 'processed' is not defined here, but is up to the
        /// higher levels
         *
        /// @return true if the state has been processed.
         */
        public Boolean IsProcessed() 
        {
            return (_fields & MaskProcessed) == MaskProcessed;
        }


        /** Reset process flags for this state and all successor states */
        public void ResetAllProcessed() 
        {
            VisitStates(new FirstSentenceHMMStateVisitor() ,this, false);
        }


        /**
        /// Gets the word history for this state.
         *
        /// @return the word history.
         */
        //TODO Not implemented

        public WordSequence WordHistory
        {
            get { return WordSequence.Empty; }
        }

        /**
        /// Gets the number of successors
         *
        /// @return the number of successors
         */

        public int NumSuccessors
        {
            get { return arcs.Count; }
        }

        /// <summary>
        /// Gets a successor to this search state
        /// </summary>
        /// <returns>the set of successors</returns>
        public ISearchStateArc[] GetSuccessors()
        {
            if (successorArray == null)
            {
                successorArray = arcs.Values.ToArray();
            }
            //this.LogDebug("{0} SuccessorArray: {1}", this.ToString(), successorArray.Length);
            return successorArray;
        }

        


        

        /**
        /// Returns the lextree state
         *
        /// @return the lex tree state
         */
        public SentenceHMMState GetLexState() 
        {
            return this;
        }


        /**
        /// remove the given arc from the set of succors
         *
        /// @param arc the arc to remove
         */

        void DeleteSuccessor(SentenceHMMStateArc arc)
        {
            arcs.Remove(arc);
        }


        /**
        /// Connects the arc to this sentence hmm.  If the node at the end of the arc is already pointing to some other node
        /// as its predecessor, don't change that relationship, since its probably a result of the nodes being reused'
         *
        /// @param arc the path to the next state
         */
        public void Connect(SentenceHMMStateArc arc) 
        {
            if (successorArray != null) {
                successorArray = null;
            }
            RawConnect(arc);
        }


        /**
        /// Connects the arc to this sentence hmm, but don't affect the predecessor relation ship
         *
        /// @param arc the arc to the next state
         */
        private void RawConnect(SentenceHMMStateArc arc) 
        {
            var state = (SentenceHMMState) arc.State;
            
            // attach the state-number because the state-signature is not necessarily unique
            arcs.Put(state.GetValueSignature() + state.StateNumber, arc);
        }


        /**
        /// Determines if this state is an emitting state
         *
        /// @return true if the state is an emitting state
         */

        public virtual bool IsEmitting
        {
            get { return false; }
        }


        /**
        /// Determines if this is a final state
         *
        /// @return true if this is a final state
         */

        public bool IsFinal
        {
            get { return (_fields & MaskIsFinal) == MaskIsFinal; }
        }


        /**
        /// Sets this is to be final state
         *
        /// @param state true if this is a final state
         */
        public void SetFinalState(Boolean state) 
        {
            if (state) {
                _fields |= MaskIsFinal;
            } else {
                _fields &= ~MaskIsFinal;
            }
        }


        /**
        /// Determines if this state is a unit state
         *
        /// @return <code>true</code> if the state is a unit state.
         */
        public virtual Boolean IsUnit() 
        {
            return false;
        }


        /** Dumps this SentenceHMMState and all its successors. Just for debugging. */
        public void DumpAll() 
        {
            VisitStates(new SecondSentenceHMMStateVisitor(), this, true);
        }


        /**
        /// Returns any annotation for this state
         *
        /// @return the annotation
         */

        protected string Annotation
        {
            get { return ""; }
        }


        /** Dumps this state */

        public void Dump() 
        {
            Console.WriteLine(" ----- " + GetTitle() + " ---- ");
            for (var i = 0; i < GetSuccessors().Length; i++) {
                var arc = (SentenceHMMStateArc) GetSuccessors()[i];
                Console.WriteLine("   -> " +
                        arc.State.ToPrettyString());
            }
        }


        /** Validates this SentenceHMMState and all successors */
        public void ValidateAll() 
        {
            // TODO fix me
        }


        /**
        /// Gets the name for this state
         *
        /// @return the name
         */

        public virtual string Name { get; private set; }


        /**
        /// Returns a pretty name for this HMM
         *
        /// @return a pretty name
         */

        public virtual string PrettyName
        {
            get { return Name; }
        }


        /** Returns the string representation of this object */

        public override string ToString() 
        {
            if (_cachedName == null) {
                var sb = new StringBuilder();
                if (IsEmitting) {
                    sb.Append('*');
                }
                sb.Append(Name);

                // string base = (isEmitting() ? "*" : "") + getName()
                //       + getWhich() + (isFinal() ? "!" : "");

                if (_parent != null) {
                    sb.Append('_');
                    sb.Append(_parent);
                }

                if (IsFinal) {
                    sb.Append('!');
                }
                _cachedName = sb.ToString();
            }
            return _cachedName;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }


        /*
       /// Returns a pretty version of the string representation
       /// for this object
        *
       /// @return a pretty string
        */
        public string ToPrettyString() 
        {
            return ToString();
        }


        /**
        /// Gets the fullName for this state
         *
        /// @return the full name for this state
         */
        public virtual string GetFullName() 
        {
            if (_fullName == null) {
                if (_parent == null) {
                    _fullName = Name;
                } else {
                    _fullName = Name + '.' + _parent.GetFullName();
                }
            }

            return _fullName;
        }


        /**
        /// Gets the signature for this state
         *
        /// @return the signature
         */

        public string Signature
        {
            get { return GetFullName(); }
        }


        /**
        /// gets the title (fullname + stateNumber) for this state
         *
        /// @return the title
         */
        public string GetTitle() 
        {
            return GetFullName() + ':' + StateNumber;
            // return getSignature() + ":" + stateNumber;
        }


        /**
        /// Retrieves the index for this state words
         *
        /// @return the index
         */
        public int GetWhich() 
        {
            return (_fields >> ShiftWhich) & MaskWhich;
        }


        /**
        /// Sets the index for this state
         *
        /// @param which the index for this state
         */
        public void SetWhich(int which) 
        {
            Debug.Assert(which >= 0 && which <= MaskWhich);
            _fields |= (which & MaskWhich) << ShiftWhich;
        }


        /**
        /// Retrieves the parent sate
         *
        /// @return the parent state (or null if this state does not have a parent state).
         */
        public SentenceHMMState GetParent() 
        {
            return _parent;
        }
    

        /**
        /// Searches the set of arcs for an arc that points to a state with an identical value
         *
        /// @param state the state to search for
        /// @return the arc or null if none could be found.
         */
        public SentenceHMMStateArc FindArc(SentenceHMMState state) 
        {
            return arcs.Get(state.GetValueSignature());
        }


        /**
        /// Returns the value signature of this unit
         *
        /// @return the value signature
         */
        public virtual string GetValueSignature() 
        {
            return GetFullName();
        }


        /**
        /// Visit all of the states starting at start with the given vistor
         *
        /// @param visitor the state visitor
        /// @param start   the place to start the search
        /// @param sorted  if true, states are sorted before visited
        /// @return true if the visiting was terminated before all nodes were visited
         */
        public static Boolean VisitStates(ISentenceHMMStateVisitor visitor, SentenceHMMState start, Boolean sorted) 
        {
            IEnumerable<SentenceHMMState> states = CollectStates(start);

            if (sorted) {
                // sort the states by stateNumber

                var sortedStates = new SortedSet<SentenceHMMState>(new FirstComparer());
                sortedStates.AddAll(states);
                states = sortedStates;
            }

            foreach (var state in states) 
            {
                if (visitor.Visit(state)) {
                    return true;
                }
            }
            return false;
        }


        /**
        /// Sets the color for this node
         *
        /// @param color the color of this node
         */
        public void SetColor(Color color) 
        {
            if (color == Color.Red) {
                _fields |= MaskColorRed;
            } else {
                _fields &= ~MaskColorRed;
            }
        }


        /**
        /// Gets the color for this node
         *
        /// @return the color of this node
         */
        public Color GetColor() {
            if ((_fields & MaskColorRed) == MaskColorRed) {
                return Color.Red;
            } else {
                return Color.Green;
            }
        }


        /**
        /// Gets the state number for this state
         *
        /// @return the state number
         */

        public int StateNumber { get; set; }


        /// <summary>
        /// Collect all states starting from the given start state 
        /// </summary>
        /// <param name="start">the state to start the search from</param>
        /// <returns>set of collected state</returns>
        public static HashSet<SentenceHMMState> CollectStates(SentenceHMMState start) 
        {
            var visitedStates = new HashSet<SentenceHMMState>();
            var queue = new List<SentenceHMMState> {start};

            while (queue.Count!=0) 
            {
                var state = queue[0];
                queue.RemoveAt(0);
                visitedStates.Add(state);
                var successors = ((ISearchState)state).GetSuccessors();
                foreach (var arc in successors) 
                {
                    var nextState = (SentenceHMMState)arc.State;
                    if (!visitedStates.Contains(nextState) && !queue.Contains(nextState)) {
                        queue.Add(nextState);
                    }
                }
            }
            return visitedStates;
        }

        /**
        /// Returns the order of this particular state
         *
        /// @return the state order for this state
         */
        public abstract int Order { get; }


        object ISearchState.LexState
        {
            get { return GetLexState(); }
        }
    }

    #region Extra
    public class FirstSentenceHMMStateVisitor : ISentenceHMMStateVisitor
    {
        public bool Visit(SentenceHMMState state)
        {
            state.SetProcessed(false);
            return false;
        }
    }

    public class SecondSentenceHMMStateVisitor : ISentenceHMMStateVisitor
    {
        public bool Visit(SentenceHMMState state)
        {
            state.Dump();
            return false;
        }
    }

    public class FirstComparer : IComparer<SentenceHMMState>
    {
        public int Compare(SentenceHMMState x, SentenceHMMState y)
        {
            var so1 = x;
            var so2 = y;
            return so1.StateNumber - so2.StateNumber;
        }
    }
#endregion
}
