using Syn.Speech.Linguist.Acoustic;
//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public abstract class UnitNode: Node
    {
        public static int SimpleUnit = 1;
        public static int WordBeginningUnit = 2;
        public static int SilenceUnit = 3;
        public static int FillerUnit = 4;


        /**
        /// Creates the UnitNode
         *
        /// @param probablilty the probability for the node
         */

        protected UnitNode(float probablilty) 
            :base(probablilty)
        {
            
        }


        /**
        /// Returns the base unit for this hmm node
         *
        /// @return the base unit
         */
        public abstract Unit BaseUnit { get; }


        public abstract object Key { get; }


        public abstract HMMPosition Position { get; }

        /// <summary>
        /// Gets and set the unit type (one of SIMPLE_UNIT, WORD_BEGINNING_UNIT, SIMPLE_UNIT or FILLER_UNIT
        /// </summary>
        public int Type { get; set; }
    }
}
