using System;
using System.Collections.Generic;
using System.IO;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.FrequencyWarp;
using Syn.Speech.FrontEnds.Transform;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.FrontEnds.Window;
//PATROLLED + REFACTORED
namespace Syn.Speech.Tools.Bandwidth
{
    /**
     * A simple energy-based detector for upsampled audio. Could be used to detect
     * bandwidth issues leading to the accuracy issues.
     * 
     * The detector simply looks for energies in different mel bands and using the
     * threshold it decides if we have cut of the frequencies signal. On every frame
     * we find the maximum energy band, then we just control that energy doesn't
     * fall too fast in upper bands.
     * 
     * A paper on the subject is "DETECTING BANDLIMITED AUDIO IN BROADCAST TELEVISION SHOWS"
     * by by Mark C. Fuhs, Qin Jin and Tanja Schultz where spline approximation is proposed
     * for detection. However, the paper seems to contain a fundamental flaw. The
     * decision is made on average spectrum, not per-frame. This probably leads
     * to omission of the events in high frequency which might signal about wide band.
     */
    public class BandDetector
    {
        const int Bands = 40;

        //From 4750 to 6800 Hz
        const int HighRangeStart = 35;
        const int HighRangeEnd = 39;

        //From 2156 to 3687 Hz
        const int LowRangeStart = 23;
        const int LowRangeEnd = 29;

        //Thresholds, selected during the experiments, about -30dB
        const double NoSignalLevel = 0.02;
        const double SignalLevel = 0.5;

        //Don't care if intensity is very low
        const double LowIntensity = 1e+5;

        private readonly FrontEnds.FrontEnd _frontend;
        private readonly AudioFileDataSource _source;

        public BandDetector()
        {

            // standard frontend
            _source = new AudioFileDataSource(320, null);
            var windower = new RaisedCosineWindower(0.97f,25.625f, 10.0f);
            var fft = new DiscreteFourierTransform(512, false);
            var filterbank = new MelFrequencyFilterBank(130.0, 6800.0, Bands);

            var list = new List<IDataProcessor> {_source, windower, fft, filterbank};

            _frontend = new FrontEnds.FrontEnd(list);
        }

        public static void Main(string[] args)
        {

            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Detector <filename.wav> or Detector <filelist>");
                return;
            }

            if (args[0].EndsWith(".wav"))
            {
                BandDetector detector = new BandDetector();
                Console.WriteLine("Bandwidth for " + args[0] + " is " + detector.Bandwidth(args[0]));
            }
            else
            {
                BandDetector detector = new BandDetector();
                TextReader reader = new StreamReader(args[0]);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (detector.Bandwidth(line)) Console.WriteLine("Bandwidth for " + line + " is low");
                }
                reader.Close();
            }
        }

        public bool Bandwidth(String file)
        {

            _source.SetAudioFile(file, "");

            IData data;
            var energy = new double[Bands];

            while ((data = _frontend.GetData()) != null)
            {
                if (data is DoubleData)
                {

                    double maxIntensity = LowIntensity;
                    double[] frame = ((DoubleData)data).Values;

                    for (int i = 0; i < Bands; i++)
                        maxIntensity = Math.Max(maxIntensity, frame[i]);

                    if (maxIntensity <= LowIntensity)
                    {
                        continue;
                    }

                    for (int i = 0; i < Bands; i++)
                    {
                        energy[i] = Math.Max(frame[i] / maxIntensity, energy[i]);
                    }
                }
            }

            double maxLow = Max(energy, LowRangeStart, LowRangeEnd);
            double maxHi = Max(energy, HighRangeStart, HighRangeEnd);

            // System.out.format("%f %f\n", maxHi, maxLow);
            // for (int i = 0; i < bands; i++)
            // System.out.format("%.4f ", energy[i]);
            // System.out.println();

            if (maxHi < NoSignalLevel && maxLow > SignalLevel)
                return true;

            return false;
        }

        private static double Max(double[] energy, int start, int end)
        {
            double max = 0;
            for (int i = start; i <= end; i++)
                max = Math.Max(max, energy[i]);
            return max;
        }
    }
}
