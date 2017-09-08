//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    public class SpeechSourceProvider
    {
        public Microphone GetMicrophone()
        {
            return new Microphone(16000, 16, true, false);
        }
    }
}
