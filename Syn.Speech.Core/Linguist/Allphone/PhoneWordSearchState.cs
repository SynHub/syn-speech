using System;
using System.Collections.Generic;
using Syn.Speech.Linguist.Acoustic;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Allphone
{
   
public class PhoneWordSearchState : PhoneNonEmittingSearchState , IWordSearchState {
    
    public PhoneWordSearchState(Unit unit, AllphoneLinguist linguist, float insertionProb, float languageProb) :base(unit, linguist, insertionProb, languageProb) {

    }

    public override ISearchStateArc[] GetSuccessors()
    {
        var result = new List<ISearchStateArc>();
        var rc = UnitManager.Silence;
        var lc = unit.BaseUnit as Unit;
        if (unit.IsContextDependent())
            rc = ((LeftRightContext)unit.Context).RightContext[0] as Unit;
        var successors = linguist.UseContextDependentPhones ? linguist.GetCDSuccessors(lc, rc) : linguist.CISuccessors;
        foreach (var successor in successors)
            result.Add(new PhoneHmmSearchState(successor.GetInitialState(), linguist, linguist.PhoneInsertionProb, LogMath.LogOne));
        return result.ToArray();
    }

    public override bool IsFinal
    {
        get { return true; }
    }

    public Pronunciation Pronunciation
    {
        get
        {
            var pronUnits = new Unit[1];
            pronUnits[0] = unit;
            var p = new Pronunciation(pronUnits, "", null, 1.0f);
            p.SetWord(new Word(unit.Name, null, false));
            return p;
        }
    }

    public bool IsWordStart() {
        return false;
    }

    public override int Order
    {
        get { return 1; }
    }

    public override bool Equals(Object obj) {
        if (!(obj is PhoneWordSearchState))
            return false;
        var haveSameBaseId = ((PhoneWordSearchState)obj).unit.BaseID == unit.BaseID;
        var haveSameContex = ((PhoneWordSearchState)obj).unit.Context.Equals(unit.Context);
        return haveSameBaseId && haveSameContex;
    }
    
    public override int GetHashCode() {
    	return unit.Context.GetHashCode() * 91 + unit.BaseID;
    }
}
}
