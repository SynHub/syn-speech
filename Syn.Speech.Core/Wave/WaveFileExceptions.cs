using System;

namespace Syn.Speech.Wave
{
    /// <summary>
    /// Class for exception objects thrown by the WAVFile class when an error occurs
    /// </summary>
   public class WaveFileException : Exception
    {
        public WaveFileException(String pErrorMessage, String pThrowingMethodName)
            : base(pErrorMessage)
        {
            _mThrowingMethodName = pThrowingMethodName;
        }

        public String ThrowingMethodName
        {
            get { return _mThrowingMethodName; }
        }

        private readonly String _mThrowingMethodName; // The method that threw the exception
    }

    /// <summary>
    /// This exception is thrown by the WAVFile class during audio file merging.
    /// </summary>
    public class WaveFileAudioMergeException : WaveFileException
    {
        public WaveFileAudioMergeException(String pErrorMessage, String pThrowingMethodName)
            : base(pErrorMessage, pThrowingMethodName)
        {
        }
    }

    /// <summary>
    /// This exception is thrown by the WAVFile class for read errors.
    /// </summary>
    public class WaveFileReadException : WaveFileException
    {
        public WaveFileReadException(String pErrorMessage, String pThrowingMethodName)
            : base(pErrorMessage, pThrowingMethodName)
        {
        }
    }

    /// <summary>
    /// This exception is thrown by the WAVFile class for write errors.
    /// </summary>
    public class WaveFileWriteException : WaveFileException
    {
        public WaveFileWriteException(String pErrorMessage, String pThrowingMethodName)
            : base(pErrorMessage, pThrowingMethodName)
        {
        }
    }

    /// <summary>
    /// Represents an exception for general WAV file I/O
    /// </summary>
    public class WaveFileIoException : WaveFileException
    {
        public WaveFileIoException(String pErrorMessage, String pThrowingMethodName)
            : base(pErrorMessage, pThrowingMethodName)
        {
        }
    }

    /// <summary>
    /// This exception is thrown by the WAVFile class for an unsupported number of bits per sample.
    /// </summary>
    public class WaveFileBitsPerSampleException : WaveFileException
    {
        public WaveFileBitsPerSampleException(String pErrorMessage, String pThrowingMethodName, short pBitsPerSample)
            : base(pErrorMessage, pThrowingMethodName)
        {
            mBitsPerSample = pBitsPerSample;
        }

        public short BitsPerSample
        {
            get { return mBitsPerSample; }
        }

        private short mBitsPerSample; // The invalid value
    }

    /// <summary>
    /// This exception is thrown by the WAVFile class for an unsupported sample rate.
    /// </summary>
    public class WaveFileSampleRateException : WaveFileException
    {
        public WaveFileSampleRateException(String pErrorMessage, String pThrowingMethodName, int pSampleRate)
            : base(pErrorMessage, pThrowingMethodName)
        {
            mSampleRate = pSampleRate;
        }

        public int SampleRate
        {
            get { return mSampleRate; }
        }

        private int mSampleRate; // The invalid value
    }
}
