using System;
using Syn.Speech.Linguist.Acoustic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Allphone
{

    public class PhoneNonEmittingSearchState : ISearchState, ISearchStateArc
    {

        protected readonly Unit unit;
        protected AllphoneLinguist linguist;

        public PhoneNonEmittingSearchState(Unit unit, AllphoneLinguist linguist, float insertionProb, float languageProb)
        {
            this.unit = unit;
            this.linguist = linguist;
            InsertionProbability = insertionProb;
            LanguageProbability = languageProb;
        }

        public virtual ISearchStateArc[] GetSuccessors()
        {
            var result = new ISearchStateArc[1];
            result[0] = new PhoneWordSearchState(unit, linguist, InsertionProbability, LanguageProbability);
            return result;
        }

        public bool IsEmitting
        {
            get { return false; }
        }

        public virtual bool IsFinal
        {
            get { return false; }
        }

        public string ToPrettyString()
        {
            return "Unit " + unit;
        }

        public string Signature
        {
            get { return null; }
        }

        public WordSequence WordHistory
        {
            get { return null; }
        }

        public virtual int Order
        {
            get { return 0; }
        }

        public ISearchState State
        {
            get { return this; }
        }

        public float GetProbability()
        {
            return LanguageProbability + InsertionProbability;
        }

        public float LanguageProbability { get; private set; }

        public float InsertionProbability { get; private set; }

        public object LexState
        {
            get { return null; }
        }

        public override bool Equals(Object obj)
        {
            if (!(obj is PhoneNonEmittingSearchState))
                return false;
            bool haveSameBaseId = ((PhoneNonEmittingSearchState)obj).unit.BaseID == unit.BaseID;
            bool haveSameContex = ((PhoneNonEmittingSearchState)obj).unit.Context.Equals(unit.Context);
            return haveSameBaseId && haveSameContex;
        }

        public override int GetHashCode()
        {
            return unit.Context.GetHashCode() * 91 + unit.BaseID;
        }
    }
}
