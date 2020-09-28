//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Rule
{
    public class JSGFRule
    {
        public string RuleName;
        public JSGFRule Parent;

        public override string ToString()
        {
            return RuleName;
        }
    }
}
