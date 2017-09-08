namespace Syn.Speech.Wave
{
    /// <summary>
    /// This struct contains audio format information and is used by the WAVFile class.
    /// </summary>
    public struct WaveFormat
    {

        // Data members
        private byte _mChannels;      // The # of channels (1 or 2)
        private int _mSampleRate;      // The audio sample rate (Hz)
        private short _mBitsPerSample;   // # of bits per sample

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pChannels">The number of channels</param>
        /// <param name="pSampleRate">The sample rate (Hz)</param>
        /// <param name="pBitsPerSample">The number of bits per sample</param>
        public WaveFormat(byte pChannels, int pSampleRate, short pBitsPerSample)
        {
            _mChannels = pChannels;
            _mSampleRate = pSampleRate;
            _mBitsPerSample = pBitsPerSample;
        }

        public static bool operator ==(WaveFormat pWaveFormat1, WaveFormat pWaveFormat2)
        {
            return ((pWaveFormat1._mChannels == pWaveFormat2._mChannels) &&
                   (pWaveFormat1._mSampleRate == pWaveFormat2._mSampleRate) &&
                   (pWaveFormat1._mBitsPerSample == pWaveFormat2._mBitsPerSample));
        }

        public static bool operator !=(WaveFormat pWaveFormat1, WaveFormat pWaveFormat2)
        {
            return ((pWaveFormat1._mChannels != pWaveFormat2._mChannels) ||
                   (pWaveFormat1._mSampleRate != pWaveFormat2._mSampleRate) ||
                   (pWaveFormat1._mBitsPerSample != pWaveFormat2._mBitsPerSample));
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || (GetType() != obj.GetType())) return false;
            var format = (WaveFormat)obj;
            return (this == format);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        ////////////////
        // Properties //
        ////////////////

        public byte Channels
        {
            get { return _mChannels; }
            set { _mChannels = value; }
        }

        public bool IsStereo
        {
            get { return (_mChannels == 2); }
        }

        public int SampleRate
        {
            get { return _mSampleRate; }
            set { _mSampleRate = value; }
        }

        public short BitsPerSample
        {
            get { return _mBitsPerSample; }
            set { _mBitsPerSample = value; }
        }

        
    }
}
