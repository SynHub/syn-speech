//PATROLLED + REFACTORED
namespace Syn.Speech.Recognizers
{
    public interface IStateListener
    {
        /// <summary>
        /// Called when the status has changed.
        /// </summary>
        /// <param name="status">status the new status</param>
        void StatusChanged(Recognizer.State status);
    }
}
