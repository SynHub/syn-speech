using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public class EndNode: UnitNode
    {
        readonly Unit _baseUnit;
        readonly int _key;


        /**
        /// Creates the node, wrapping the given hmm
         *
        /// @param baseUnit    the base unit for this node
        /// @param lc          the left context
        /// @param probablilty the probability for the transition to this node
         */
        public EndNode(Unit baseUnit, Unit lc, float probablilty) 
            :base(probablilty)
        {
            _baseUnit = baseUnit;
            LeftContext = lc;
            _key = baseUnit.BaseID* 121 + LeftContext.BaseID;
        }


        /**
        /// Returns the base unit for this hmm node
         *
        /// @return the base unit
         */

        public override Unit BaseUnit
        {
            get { return _baseUnit; }
        }


        /**
        /// Returns the base unit for this hmm node
         *
        /// @return the base unit
         */

        public Unit LeftContext { get; private set; }


        public override object Key
        {
            get { return _key; }
        }


        public override HMMPosition Position
        {
            get { return HMMPosition.End; }
        }


        /**
        /// Returns a string representation for this object
         *
        /// @return a string representation
         */

        public override string ToString() 
        {
            return "EndNode base:" + _baseUnit + " lc " + LeftContext + ' ' + _key;
        }


        /** Freeze this node. Convert the set into an array to reduce memory overhead */
        public override void Freeze() 
        {
            base.Freeze();
        }
    }
}
