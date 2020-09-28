using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{

    public class JSGFRuleAlternatives : JSGFRule
    {
        protected List<JSGFRule> Rules;
        protected List<Float> Weights;

        public JSGFRuleAlternatives()
        {
        }

        public JSGFRuleAlternatives(List<JSGFRule> rules)
        {
            SetRules(rules);
            Weights = null;
        }

        public JSGFRuleAlternatives(List<JSGFRule> rules, List<Float> weights)
        {
            Debug.Assert(rules.Count == weights.Count);
            SetRules(rules);
            SetWeights(weights);
        }

        public void Append(JSGFRule rule)
        {
            Debug.Assert(rule != null);
            Rules.Add(rule);
            if (Weights != null)
                Weights.Add(1.0f);
        }

        public List<JSGFRule> GetRules()
        {
            return Rules;
        }

        public List<Float> GetWeights()
        {
            return Weights;
        }

        public void SetRules(List<JSGFRule> rules)
        {
            if ((Weights != null) && (rules.Count != Weights.Count))
            {
                Weights = null;
            }
            Rules = rules;
        }

        public void SetWeights(List<Float> newWeights)
        {
            if ((newWeights == null) || (newWeights.Count == 0))
            {
                Weights = null;
                return;
            }

            if (newWeights.Count != Rules.Count)
            {
                throw new ArgumentException(
                        "weights/rules array length mismatch");
            }
            float f = 0.0F;

            foreach (Float w in newWeights)
            {
                if (Float.isNaN(w))
                    throw new ArgumentException("illegal weight value: NaN");
                if (Float.isInfinite(w))
                    throw new ArgumentException(
                            "illegal weight value: infinite");
                if (w < 0.0D)
                {
                    throw new ArgumentException(
                            "illegal weight value: negative");
                }
                f += w;
            }

            if (f <= 0.0D)
            {
                throw new ArgumentException(
                        "illegal weight values: all zero");
            }
            Weights = newWeights;
        }


        public override String ToString()
        {
            if (Rules == null || Rules.Count == 0)
            {
                return "<VOID>";
            }
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Rules.Count; ++i)
            {
                if (i > 0)
                    sb.Append(" | ");

                if (Weights != null)
                    sb.Append("/" + Weights[i] + "/ ");

                JSGFRule r = Rules[i];
                if (Rules[i] is JSGFRuleAlternatives)
                    sb.Append("( ").Append(r).Append(" )");
                else
                {
                    sb.Append(r);
                }
            }
            return sb.ToString();
        }
    }
}
