using System;
using Syn.Speech.Decoders.Scorer;
using Syn.Speech.FrontEnds;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Allphone
{

    public class PhoneHmmSearchState : ISearchState, ISearchStateArc, IScoreProvider
    {

        private readonly IHMMState _state;
        private readonly AllphoneLinguist _linguist;

        public PhoneHmmSearchState(IHMMState hmmState, AllphoneLinguist linguist, float insertionProb, float languageProb)
        {
            _state = hmmState;
            _linguist = linguist;
            InsertionProbability = insertionProb;
            LanguageProbability = languageProb;
        }

        public ISearchState State
        {
            get { return this; }
        }

        public int GetBaseId()
        {
            return ((SenoneHMM)_state.HMM).BaseUnit.BaseID;
        }

        public float GetProbability()
        {
            return LanguageProbability + InsertionProbability;
        }

        public float LanguageProbability { get; private set; }

        public float InsertionProbability { get; private set; }

        /// <summary>
        /// If we are final, transfer to all possible phones, otherwise return all successors of this hmm state.
        /// </summary>
        public ISearchStateArc[] GetSuccessors()
        {
            if (_state.IsExitState())
            {
                var units = _linguist.GetUnits(((SenoneHMM)_state.HMM).SenoneSequence);
                var result = new ISearchStateArc[units.Count];
                for (var i = 0; i < result.Length; i++)
                    result[i] = new PhoneNonEmittingSearchState(units[i], _linguist, InsertionProbability, LanguageProbability);
                return result;
            }
            var successors = _state.GetSuccessors();
            var results = new ISearchStateArc[successors.Length];
            for (var i = 0; i < successors.Length; i++)
            {
                results[i] = new PhoneHmmSearchState(successors[i].HmmState, _linguist, InsertionProbability, LanguageProbability);
            }
            return results;
        }

        public bool IsEmitting
        {
            get { return _state.IsEmitting; }
        }

        public bool IsFinal
        {
            get { return false; }
        }

        public string ToPrettyString()
        {
            return "HMM " + _state;
        }

        public string Signature
        {
            get { return null; }
        }

        public WordSequence WordHistory
        {
            get { return null; }
        }

        public object LexState
        {
            get { return null; }
        }

        public int Order
        {
            get { return 2; }
        }


        public float GetScore(IData data)
        {
            return _state.GetScore(data);
        }


        public float[] GetComponentScore(IData feature)
        {
            return _state.CalculateComponentScore(feature);
        }


        public override bool Equals(Object obj)
        {
            if (!(obj is PhoneHmmSearchState))
                return false;
            var otherSenoneSeq = ((SenoneHMM)((PhoneHmmSearchState)obj)._state.HMM).SenoneSequence;
            var thisSenoneSeq = ((SenoneHMM)_state.HMM).SenoneSequence;
            return thisSenoneSeq.Equals(otherSenoneSeq);
        }


        public override int GetHashCode()
        {
            return ((SenoneHMM)_state.HMM).SenoneSequence.GetHashCode() + _state.State * 37;
        }
    }
}
