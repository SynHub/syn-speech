//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    /// <summary>
    /// a visitor interface
    /// </summary>
    public interface ISentenceHMMStateVisitor
    {
        /**
        /// Method called when a state is visited by the vistor
         *
        /// @param state the state that is being visited
        /// @return true if the visiting should be terminated
         */
        bool Visit(SentenceHMMState state);
    }
}
