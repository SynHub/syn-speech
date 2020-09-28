using System.Diagnostics;
//PATROLLED + REFACTORED    
namespace Syn.Speech.FrontEnds.Util
{
    /// <summary>
    ///{@code FrontEnd} element that asserts the audio-stream to be continuous. This is often a mandatory property for
    ///frontend setups. The component operates on the acoustic data level and needs to plugged into the frontend
    ///before the actual feature extraction starts.
    /// This component can help to debug new VAD implementations, where it has been shown that data-blocks easily get lost.
    /// @author Holger Brandl
    /// </summary>
    public class AudioContinuityTester : BaseDataProcessor
    {

        long _lastSampleNum = -1;

        public AudioContinuityTester()
        {
            //initLogger();
        }


        public override IData GetData()
        {
            var d = Predecessor.GetData();

            Debug.Assert(IsAudioStreamContinuous(d), "audio stream is not continuous");

            return d;
        }


        private bool IsAudioStreamContinuous(IData input)
        {
            if (input is DoubleData)
            {
                var d = (DoubleData)input;
                if (_lastSampleNum != -1 && _lastSampleNum != d.FirstSampleNumber)
                {
                    return false;
                }

                _lastSampleNum = d.FirstSampleNumber + d.Values.Length;

            }
            else if (input is DataStartSignal)
                _lastSampleNum = -1;

            return true;
        }
    }
}
