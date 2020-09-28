using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeWordState : LexTreeState, IWordSearchState
    {

        private readonly HMMNode _lastNode;
        private readonly float _logLanguageProbability;


        /**
           /// Constructs a LexTreeWordState
            *
           /// @param wordNode       the word node
           /// @param wordSequence   the sequence of words triphone context
           /// @param languageProbability the probability of this word
            */
        public LexTreeWordState(WordNode wordNode, HMMNode lastNode,
            WordSequence wordSequence, float smearTerm, float smearProb,
            float languageProbability, LexTreeLinguist _parent)
            : base(wordNode, wordSequence, smearTerm, smearProb, _parent)
        {

            // Trace.WriteLine(string.Format("LexTreeWordState Created with values wordNode: {0}, lastNode: {1}, wordSequence: {2}, smearTerm: {3}, smearProb: {4}, languageProbability: {5}",
            // wordNode, lastNode, wordSequence, smearTerm, smearProb, languageProbability));
            this._lastNode = lastNode;
            _logLanguageProbability = languageProbability;
            //if (wordNode.ToString().Contains("NSN"))
            //{
            //    this.LogInfo("FOUND NOISE!");
            //}
        }


        /**
           /// Gets the word pronunciation for this state
            *
           /// @return the pronunciation for this word
            */

        public Pronunciation Pronunciation
        {
            get { return ((WordNode)GetNode()).Pronunciation; }
        }


        /**
           /// Determines if this is a final state
            *
           /// @return <code>true</code> if this is an final state.
            */

        public override bool IsFinal
        {
            get { return ((WordNode)GetNode()).IsFinal; }
        }


        /**
           /// Generate a hashcode for an object
            *
           /// @return the hashcode
            */
        public override int GetHashCode()
        {
            //TODO: PERFORMANCE CRITICAL - Called randomly in 1 or 2s sequence.
            return base.GetHashCode() * 41 + _lastNode.GetHashCode();
        }


        /**
           /// Gets the unique signature for this state. The signature building code is slow and should only be used for
           /// non-time-critical tasks such as plotting states.
            *
           /// @return the signature
            */

        public override string Signature
        {
            get { return base.Signature + "-ln-" + _lastNode.GetHashCode(); }
        }


        /**
           /// Determines if the given object is equal to this object
            *
           /// @param o the object to test
           /// @return <code>true</code> if the object is equal to this
            */

        public override bool Equals(Object o)
        {
            if (o == this)
            {
                return true;
            }
            if (o is LexTreeWordState)
            {
                LexTreeWordState other = (LexTreeWordState)o;
                return _lastNode == other._lastNode && base.Equals(o);
            }
            return false;
        }


        /**
           /// Gets the language probability of entering this state
            *
           /// @return the log probability
            */

        public override float LanguageProbability
        {
            get { return _logLanguageProbability; }
        }


        /**
           /// Returns the list of successors to this state
            *
           /// @return a list of SearchState objects
            */

        public override ISearchStateArc[] GetSuccessors()
        {
            ISearchStateArc[] arcs = GetCachedArcs();
            if (arcs == null)
            {
                arcs = LexTreeLinguist.EmptyArc;
                WordNode wordNode = (WordNode)GetNode();

                if (wordNode.GetWord() != Parent.SentenceEndWord)
                {
                    int index = 0;
                    List<Node> list = new List<Node>();
                    Unit[] rc = _lastNode.GetRC();
                    Unit left = wordNode.LastUnit;

                    foreach (Unit unit in rc)
                    {
                        Node[] epList = Parent.HMMTree.GetEntryPoint(left, unit);
                        foreach (Node n in epList)
                        {
                            list.Add(n);
                        }
                    }

                    //this.LogDebug("NodeList: {0}",list.Count);

                    //    add a link to every possible entry point as well
                    //    as link to the </s> node
                    arcs = new ISearchStateArc[list.Count + 1];
                    foreach (Node node in list)
                    {
                        arcs[index++] = CreateUnitStateArc((HMMNode)node, this);
                    }

                    //    now add the link to the end of sentence arc:

                    arcs[index++] = CreateWordStateArc(Parent.HMMTree.SentenceEndWordNode, _lastNode, this);
                }
                PutCachedArcs(arcs);
            }
            return arcs;
        }


        public override int Order
        {
            get { return 1; }
        }


        /**
           /// Returns true if this LexTreeWordState indicates the start of a word. Returns false if this LexTreeWordState
           /// indicates the end of a word.
            *
           /// @return true if this LexTreeWordState indicates the start of a word, false if this LexTreeWordState indicates
           ///         the end of a word
            */
        public Boolean IsWordStart()
        {
            return false;
        }
    }
}