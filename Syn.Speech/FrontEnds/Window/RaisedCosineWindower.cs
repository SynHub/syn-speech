using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Window
{
    /// <summary>
    /// Slices up a Data object into a number of overlapping windows (usually referred to as "frames" in the speech world). In
    /// order to minimize the signal discontinuities at the boundaries of each frame, we multiply each frame with a raised
    /// cosine windowing function. Moreover, the system uses overlapping windows to capture information that may occur at the
    /// window boundaries. These events would not be well represented if the windows were juxtaposed. <p> The number of
    /// resulting windows depends on the {@link #PROP_WINDOW_SIZE_MS window size} and the {@link #PROP_WINDOW_SHIFT_MS window
    /// shift} (commonly known as frame shift in speech world). Figure 1 shows the relationship between the original data
    /// stream, the window size, the window shift, and the windows returned. <p> <img src="doc-files/framing.jpg">
    /// <br><b>Figure 1: Relationship between original data, window size, window shift, and the windows returned.</b> <p> The
    /// raised cosine windowing function will be applied to each such window. Since the {@link #getData()} method returns a
    /// window, and multiple windows are created for each Data object, this is a 1-to-many processor. Also note that the
    /// returned windows should have the same number of data points as the windowing function. <p> The applied windowing
    /// function, <i>W(n)</i>, of length <i>N</i> (the window size), is given by the following formula:
    /// <pre>
    /// W(n) = (1-a) - (a/// cos((2/// Math.PI/// n)/(N - 1)))
    /// </pre>
    /// where <b>a</b> is commonly known as the "alpha" value. This variable can be set by the user using the property
    /// defined by {@link #PROP_ALPHA}. Please follow the links to the see the constant field values. Some values of alpha
    /// receive special names, since they are used so often. A value of 0.46 for the alpha results in a window named Hamming
    /// window. A value of 0.5 results in the Hanning window. And a value of 0 results in the Rectangular window. The default
    /// for this system is the Hamming window, with alpha 0.46 !). Figure 2 below shows the Hamming window function (a =
    /// 0.46), using our default window size of 25.625 ms and assuming a sample rate of 16kHz, thus yielding 410 samples per
    /// window. <p> <img src="doc-files/hamming-window.gif"> <br><b>Figure 2: The Hamming window function.</b>
    ///
    /// @see Data
    /// </summary>
    public class RaisedCosineWindower : BaseDataProcessor
    {
        /// <summary>
        /// The property for window size in milliseconds.
        /// </summary>
        [S4Double(DefaultValue = 25.625)]
        public static string PropWindowSizeMs = "windowSizeInMs";
        private float _windowSizeInMs;

        /// <summary>
        /// The property for window shift in milliseconds, which has a default value of 10F.
        /// </summary>
        [S4Double(DefaultValue = 10.0)]
        public static string PropWindowShiftMs = "windowShiftInMs";
        private float _windowShiftInMs;

        /// <summary>
        /// The property for the alpha value of the Window, which is the value for the RaisedCosineWindow.
        /// </summary>
        [S4Double(DefaultValue = 0.46)]
        public static string PropAlpha = "alpha";
        private double _alpha;


        //required to access the DataStartSignal-properties
        public static string WindowShiftSamples = "windowSize";
        public static string WindowSizeSamples = "windowShift";

        private double[] _cosineWindow; // the raised consine window
        private int _windowShift; // the window size

        private List<IData> _outputQueue; // cache for output windows
        private DoubleBuffer _overflowBuffer; // cache for overlapped audio regions
        private long _currentFirstSampleNumber;

        public RaisedCosineWindower(double alpha, float windowSizeInMs, float windowShiftInMs)
        {
            _alpha = alpha;
            _windowSizeInMs = windowSizeInMs;
            _windowShiftInMs = windowShiftInMs;
        }

        public RaisedCosineWindower()
        {

        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);

            _alpha = ps.GetDouble(PropAlpha);
            _windowSizeInMs = ps.GetFloat(PropWindowSizeMs);
            _windowShiftInMs = ps.GetFloat(PropWindowShiftMs);
        }

        /// <summary>
        /// @see Sphincs.frontend.DataProcessor#initialize(Sphincs.frontend.CommonConfig)
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // createWindow();
            _outputQueue = new List<IData>();
        }

        /// <summary>
        /// Creates the Hamming Window.
        /// </summary>
        /// <param name="sampleRate"></param>
        private void CreateWindow(int sampleRate)
        {
            if (_cosineWindow != null && sampleRate == SampleRate)
            {
                return;
            }

            SampleRate = sampleRate;

            var windowSize = DataUtil.GetSamplesPerWindow(sampleRate, _windowSizeInMs);
            _cosineWindow = new double[windowSize];

            _windowShift = DataUtil.GetSamplesPerShift(sampleRate, _windowShiftInMs);

            if (_cosineWindow.Length > 1)
            {
                var oneMinusAlpha = (1 - _alpha);
                for (var i = 0; i < _cosineWindow.Length; i++)
                {
                    _cosineWindow[i] = oneMinusAlpha -
                            _alpha * Math.Cos(2 * Math.PI * i / (_cosineWindow.Length - 1.0));
                }
            }

            _overflowBuffer = new DoubleBuffer(windowSize);
        }

        /// <summary>
        /// Returns the next Data object, which is usually a window of the input Data, with the windowing function applied to
        /// it.
        /// </summary>
        /// <returns>the next available Data object, returns null if no Data object is available</returns>
        public override IData GetData()
        {
            if (_outputQueue.Count == 0)
            {
                var input = Predecessor.GetData();

                if (input != null)
                {
                    if (input is DoubleData)
                    {
                        var data = (DoubleData)input;
                        // System.err.print("to windower: ");
                        // System.err.println(Arrays.toString(data.getValues()));
                        if (_currentFirstSampleNumber == -1)
                        {
                            _currentFirstSampleNumber = data.FirstSampleNumber;
                        }

                        // should not be necessary if all DataProcessor would forward Signals. Unfortunately this
                        // is currently not the case.
                        CreateWindow(data.SampleRate);

                        // process the Data, and output the windows
                        Process(data);
                    }
                    else
                    {
                        if (input is DataStartSignal)
                        {
                            var startSignal = (DataStartSignal)input;

                            CreateWindow(startSignal.SampleRate);

                            // attach the frame-length and the shift-length to the start-signal to allow
                            // detection of incorrect frontend settings
                            var props = startSignal.GetProps();
                            Java.Put(props, WindowShiftSamples, _windowShift);
                            Java.Put(props, WindowSizeSamples, _cosineWindow.Length);

                            // reset the current first sample number
                            _currentFirstSampleNumber = -1;
                        }
                        else if (input is SpeechStartSignal)
                        {
                            // reset the current first sample number
                            _currentFirstSampleNumber = -1;
                        }
                        else if (input is DataEndSignal || input is SpeechEndSignal)
                        {
                            // end of utterance handling
                            ProcessUtteranceEnd();
                        }

                        _outputQueue.Add(input);
                    }
                }

            }

            if (_outputQueue.Count != 0)
            {
                var output = _outputQueue[0];
                _outputQueue.RemoveAt(0);
                if (output is DoubleData)
                {
                    Debug.Assert(((DoubleData)output).Values.Length == _cosineWindow.Length);
                }
                return output;
            }
            return null;
        }


        /// <summary>
        /// Applies the Windowing to the given Data. The resulting windows are cached in the outputQueue.
        /// </summary>
        /// <param name="input">the input Data object</param>
        private void Process(DoubleData input)
        {
            var values = input.Values;
            var length = _overflowBuffer.Occupancy + values.Length;

            var dataList = new List<DoubleData>();
            dataList.Add(input);

            IData utteranceEnd = null;

            // read in more Data if we have under one window's length of data
            while (length < _cosineWindow.Length)
            {
                var next = Predecessor.GetData();
                if (next is DoubleData)
                {
                    dataList.Add((DoubleData)next);
                    length += ((DoubleData)next).Values.Length;
                }
                else
                {
                    if (next is DataEndSignal || next is SpeechEndSignal)
                    {
                        utteranceEnd = next;
                        break;
                    }

                    _outputQueue.Add(next);
                }
            }

            var allSamples = values;

            // prepend overflow samples
            if (length != values.Length)
            {

                allSamples = new double[length];

                // copy overflow samples to allSamples buffer
                Array.Copy(_overflowBuffer.Buffer, 0, allSamples, 0, _overflowBuffer.Occupancy);
                var start = _overflowBuffer.Occupancy;

                // copy input samples to allSamples buffer
                foreach (var aDataList in dataList)
                {
                    var samples = aDataList.Values;
                    Array.Copy(samples, 0, allSamples, start, samples.Length);
                    start += samples.Length;
                }
            }

            // apply Hamming window
            var residual = ApplyRaisedCosineWindow(allSamples, length);

            // save elements that also belong to the next window
            _overflowBuffer.Reset();
            if (length - residual > 0)
            {
                _overflowBuffer.Append(allSamples, residual, length - residual);
            }
            if (utteranceEnd != null)
            {
                // end of utterance handling
                ProcessUtteranceEnd();
                _outputQueue.Add(utteranceEnd);
            }
        }

        /// <summary>
        /// What happens when an DataEndSignal is received. Basically pads up to a window of the overflow buffer with zeros,
        /// and then apply the Hamming window to it. Checks if buffer has data.
        /// </summary>
        private void ProcessUtteranceEnd()
        {
            if (_overflowBuffer.Occupancy > 0)
            {
                _overflowBuffer.PadWindow(_cosineWindow.Length);
                ApplyRaisedCosineWindow(_overflowBuffer.Buffer, _cosineWindow.Length);
                _overflowBuffer.Reset();
            }
        }


        /// <summary>
        /// Applies the Hamming window to the given double array. The windows are added to the output queue. Returns the
        /// index of the first array element of next window that is not produced because of insufficient data.
        /// </summary>
        /// <param name="_in">the audio data to apply window and the Hamming window</param>
        /// <param name="length">the number of elements in the array to apply the RaisedCosineWindow</param>
        /// <returns>the index of the first array element of the next window</returns>
        private int ApplyRaisedCosineWindow(double[] _in, int length)
        {
            int windowCount;

            // if no windows can be created but there is some data,
            // pad it with zeros
            if (length < _cosineWindow.Length)
            {
                var padded = new double[_cosineWindow.Length];
                Array.Copy(_in, 0, padded, 0, length);
                _in = padded;
                windowCount = 1;
            }
            else
            {
                windowCount = GetWindowCount(length, _cosineWindow.Length, _windowShift);
            }

            // create all the windows at once, not individually, saves time
            var windows = new double[windowCount][/**/];
            for (var element = 0; element < windowCount; element++)
                windows[element] = new double[_cosineWindow.Length];

            var windowStart = 0;

            for (var i = 0; i < windowCount; windowStart += _windowShift, i++)
            {

                var myWindow = windows[i];

                // apply the Hamming Window function to the window of data
                for (int w = 0, s = windowStart; w < myWindow.Length; s++, w++)
                {
                    myWindow[w] = _in[s] * _cosineWindow[w];
                }

                // add the frame to the output queue
                _outputQueue.Add(new DoubleData
                        (myWindow, SampleRate,
                                _currentFirstSampleNumber));
                _currentFirstSampleNumber += _windowShift;
            }

            return windowStart;
        }


        /// <summary>
        /// Gets the number of windows in the given array, given the windowSize and windowShift.
        /// </summary>
        /// <param name="arraySize">The size of the array.</param>
        /// <param name="windowSize">The window size.</param>
        /// <param name="windowShift">The window shift.</param>
        /// <returns>the number of windows</returns>
        private static int GetWindowCount(int arraySize, int windowSize, int windowShift)
        {
            if (arraySize < windowSize)
            {
                return 0;
            }
            var windowCount = 1;
            for (var windowEnd = windowSize;
                    windowEnd + windowShift <= arraySize;
                    windowEnd += windowShift)
            {
                windowCount++;
            }
            return windowCount;

        }

        /// <summary>
        /// Returns the shift size used to window the incoming speech signal. This value might be used by other components to
        /// determine the time resolution of feature vectors.
        /// </summary>
        /// <returns>The shift of the window.</returns>
        /// <exception cref="System.SystemException"></exception>
        public float GetWindowShiftInMs()
        {
            if (_windowShiftInMs == 0)
                throw new SystemException(this + " was not initialized yet!");

            return _windowShiftInMs;
        }


        public int SampleRate { get; private set; }

        /// <summary>
        ///Rounds a given sample-number to the number of samples will be processed by this instance including the padding samples at the end..
        /// </summary>
        /// <param name="samples">The samples.</param>
        /// <returns></returns>
        public long RoundToFrames(long samples)
        {
            var windowSize = DataUtil.GetSamplesPerWindow(SampleRate, _windowSizeInMs);
            var windowShift = DataUtil.GetSamplesPerShift(SampleRate, _windowShiftInMs);

            var mxNumShifts = samples / windowShift;

            for (var i = (int)mxNumShifts; ; i--)
            {
                var remainingSamples = samples - windowShift * i;

                if (remainingSamples > windowSize)
                    return windowShift * (i + 1) + windowSize;
            }
        }
    }
}
