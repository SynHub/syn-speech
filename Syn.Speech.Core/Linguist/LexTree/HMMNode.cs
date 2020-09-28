using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    /// <summary>
    ///  A node that represents an HMM in the hmm tree
    /// </summary>
    public class HMMNode: UnitNode
    {
        // There can potentially be a large number of nodes (millions),
        // therefore it is important to conserve space as much as
        // possible.  While building the HMMNodes, we keep right contexts
        // in a set to allow easy pruning of duplicates.  Once the tree is
        // entirely built, we no longer need to manage the right contexts
        // as a set, a simple array will do. The freeze method converts
        // the set to the array of units.  This rcSet object holds the set
        // during construction and the array after the freeze.

        private Object _rcSet;


        /**
        /// Creates the node, wrapping the given hmm
         *
        /// @param hmm the hmm to hold
         */
        public HMMNode(IHMM hmm, float probablilty) 
            :base(probablilty)
        {
            
            HMM = hmm;

            Unit _base = BaseUnit;

            int type = SimpleUnit;
            if (_base.IsSilence) 
            {
                type = SilenceUnit;
            } 
            else if (_base.IsFiller) 
            {
                type = FillerUnit;
            } 
            else if (hmm.Position.IsWordBeginning()) 
            {
                type = WordBeginningUnit;
            }

            Type = type;

        }


        /**
        /// Returns the base unit for this hmm node
         *
        /// @return the base unit
         */

        public override Unit BaseUnit
        {
            get
            {
                // return hmm.getUnit().getBaseUnit();
                return HMM.BaseUnit;
            }
        }


        /**
        /// Returns the hmm for this node
         *
        /// @return the hmm
         */

        public IHMM HMM { get; private set; }


        public override HMMPosition Position
        {
            get { return HMM.Position; }
        }


        public override object Key
        {
            get { return HMM; }
        }


        /**
        /// Returns a string representation for this object
         *
        /// @return a string representation
         */
        
        public override string ToString() 
        {
            return "HMMNode " + HMM + " p " +  UnigramProbability.ToString("R");
        }


        /**
        /// Adds a right context to the set of possible right contexts for this node. This is typically only needed for hmms
        /// at the ends of words.
         *
        /// @param rc the right context.
         */
        public void AddRC(Unit rc) 
        {
            GetRCSet().Add(rc);
        }


        /** Freeze this node. Convert the set into an array to reduce memory overhead */
        public override void Freeze() 
        {
            base.Freeze();
            if (_rcSet is HashSet<Unit>) 
            {
                HashSet<Unit> set = (HashSet<Unit>)_rcSet;
                _rcSet = set.ToArray();
            }
        }


        /**
        /// Gets the rc as a set. If we've already been frozen it is an error
         *
        /// @return the set of right contexts
         */
        private HashSet<Unit> GetRCSet() 
        {
            if (_rcSet == null) {
                _rcSet = new HashSet<Unit>();
            }

            Debug.Assert(_rcSet is HashSet<Unit>);
            return (HashSet<Unit>)_rcSet;
        }


        /**
        /// returns the set of right contexts for this node
         *
        /// @return the set of right contexts
         */
        public Unit[] GetRC() 
        {
            if (_rcSet is HashSet<Unit>) 
            {
                Freeze();
            }
            return (Unit[]) _rcSet;
        }

    }
}
