//REFACTORED
namespace Syn.Speech.Linguist
{
    /** Represents a single state in a language search space */
    public interface ISearchStateArc
    {

        /**
         * Gets a successor to this search state
         *
         * @return the successor state
         */
        ISearchState State { get; }


        /**
         * Gets the composite probability of entering this state
         *
         * @return the log probability
         */
        float GetProbability();


        /**
         * Gets the language probability of entering this state
         *
         * @return the log probability
         */
        float LanguageProbability { get; }

        /**
         * Gets the insertion probability of entering this state
         *
         * @return the log probability
         */
        float InsertionProbability { get; }
    }

}
