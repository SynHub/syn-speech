using System;
//PATROLLED + REFACTORED
using System.IO;
using Syn.Speech.Wave;

namespace Syn.Speech.Api
{
    public class Microphone
    {
        public Microphone(float sampleRate, int sampleSize, bool signed, bool bigEndian)
        {
            //var format = new WaveFormat(1, (int) sampleRate, (short) sampleSize);
            try
            {
              
            }
            catch (Exception e)
            {
                throw new SystemException(e.Message);
            }
        }

        public void StartRecording()
        {
            
        }

        public void StopRecording()
        {
            
        }

        public Stream Stream { get; set; }
    }
}
