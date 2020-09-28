using System;
using Syn.Speech.Linguist.Acoustic;

namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// Represents a unit in an SentenceHMMS
    /// </summary>
    public class UnitState:SentenceHMMState,IUnitSearchState
    {
        private readonly HMMPosition _position = HMMPosition.Internal;


        /**
        /// Creates a UnitState. Gets the left and right contexts from the unit itself.
         *
        /// @param parent the parent state
        /// @param which  the index of the given state
        /// @param unit   the unit associated with this state
         */
        public UnitState(PronunciationState parent, int which, Unit unit) 
            :base("U", parent, which)
        {
            
            this.Unit = unit;
            Unit[] units = parent.Pronunciation.Units;
            int length = units.Length;

            // If the last phone is SIL, then we should be using
            // a word-ending phone for the last phone. Decrementing
            // length will make the phone before SIL the last phone.

            if (units[length - 1] == UnitManager.Silence && length > 1) {
                length--;
            }

            if (length == 1) {
                _position = HMMPosition.Single;
            } else if (which == 0) {
                _position = HMMPosition.Begin;
            } else if (which == length - 1) {
                _position = HMMPosition.End;
            }
        }


        /**
        /// Creates a UnitState with the given unit and HMM position.
         *
        /// @param unit     the unit associated with this state
        /// @param position the HMM position of this unit
         */
        public UnitState(Unit unit, HMMPosition position) 
        {
            this.Unit = unit;
            this._position = position;
        }


        /**
        /// Gets the unit associated with this state
         *
        /// @return the unit
         */

        public Unit Unit { get; private set; }


        /**
        /// Returns true if this unit is the last unit of the pronunciation
         *
        /// @return <code>true</code> if the unit is the last unit
         */
        public Boolean IsLast() 
        {
            return _position == HMMPosition.Single || _position == HMMPosition.End;
        }


        /**
        /// Gets the name for this state
         *
        /// @return the name for this state
         */

        public override string Name
        {
            get { return base.Name + '<' + Unit + '>'; }
        }


        /**
        /// Returns the value signature of this unit
         *
        /// @return the value signature
         */
        public override string GetValueSignature()
        {
            return Unit.ToString();
        }


        /**
        /// Gets the pretty name for this unit sate
         *
        /// @return the pretty name
         */

        public override string PrettyName
        {
            get { return Unit.ToString(); }
        }


        /**
        /// Retrieves a short label describing the type of this state. Typically, subclasses of SentenceHMMState will
        /// implement this method and return a short (5 chars or less) label
         *
        /// @return the short label.
         */

        public override string TypeLabel
        {
            get { return "Unit"; }
        }


        /**
        /// Gets the position for this unit
         *
        /// @return the position for this unit
         */
        public HMMPosition GetPosition() 
        {
            return _position;
        }


        public override Boolean IsUnit() 
        {
            return true;
        }

        /// <summary>
        /// Returns the state order for this state type
        /// </summary>
        /// <value></value>
        public override int Order
        {
            get { return 5; }
        }

    }
}
