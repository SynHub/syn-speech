using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.EndPoint;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Test.FrontEnds
{
    [TestClass]
    public class FrontEndElementTest
    {
        [TestMethod]
        public void FrontEndElement_ValueTest()
        {
            var dictionary = new Dictionary<string, string> {
            {
                "preempTest",
                "after-preemp.dump"},
            {
                "windowTest",
                "after-window.dump"},
            {
                "fftTest",
                "after-fft.dump"},
            {
                "melTest",
                "after-mel.dump"},
            {
                "dctTest",
                "after-dct.dump"},
            {
                "cmnTest",
                "after-cmn.dump"},
            {
                "feTest",
                "after-feature.dump"}
        };

            foreach (var item in dictionary)
            {
                TestElement(item.Key, Helper.FilesDirectory + "/frontend/" + item.Value);
            }
        }


        public void TestElement(String frontendName, String name)
        {
            var url = new URL(Helper.FilesDirectory + "/frontend/frontend.xml");
            var cm = new ConfigurationManager(url);

            var ds = (AudioFileDataSource)cm.Lookup("audioFileDataSource");
            ds.SetAudioFile(new URL(Helper.FilesDirectory + "/frontend/test-feat.wav"), null);

            var frontend = (Speech.FrontEnds.FrontEnd)cm.Lookup(frontendName);
            CompareDump(frontend, name);
        }

        private void CompareDump(Speech.FrontEnds.FrontEnd frontend, String name)
        {

            var stream = new URL(name).OpenStream();
            var st = new BufferedStream(stream, 8192);
            var br = new StreamReader(st);
            
            
            String line;

            // To dump data next time
            //        while (true) {
            //            Data data = frontend.getData();
            //            if (data == null)
            //                    break;
            //        }  
            //        if (false)

            int counter=0;
            while (null != (line = br.ReadLine()))
            {
                counter++;

                if (counter == 4 && name.Contains("after-cmn"))
                {
                    Trace.WriteLine("we are there");
                }

                var data = frontend.GetData();

                if (line.StartsWith("DataStartSignal"))
                    Assert.IsTrue(data is DataStartSignal);
                if (line.StartsWith("DataEndSignal"))
                    Assert.IsTrue(data is DataEndSignal);
                if (line.StartsWith("SpeechStartSignal"))
                    Assert.IsTrue(data is SpeechStartSignal);
                if (line.StartsWith("SpeechEndSignal"))
                    Assert.IsTrue(data is SpeechEndSignal);

                if (line.StartsWith("Frame"))
                {
                    Assert.IsTrue(data is DoubleData);

                    var values = ((DoubleData)data).Values;
                    var tokens = line.Split(" ");

                    Assert.AreEqual(values.Length, Convert.ToInt32(tokens[1]));


                    for (var i = 0; i < values.Length; i++)
                    {
                        var theValue = values[i];
                        var doubleValue = Convert.ToDouble(tokens[2 + i], CultureInfo.InvariantCulture.NumberFormat);
                        var closeValue = Math.Abs(0.01*values[i]);
                        Assert.IsTrue(Helper.CloseTo(theValue, doubleValue, closeValue));
                    }
                }

                if (line.StartsWith("FloatFrame"))
                {
                    var tokens = line.Split(" ");
                    Assert.IsTrue(data is FloatData);
                    var values = ((FloatData)data).Values;
                    Assert.AreEqual(values.Length, Integer.ValueOf(tokens[1]));
                    for (var i = 0; i < values.Length; i++)
                        Assert.IsTrue(Helper.CloseTo(values[i], Convert.ToSingle(tokens[2 + i], CultureInfo.InvariantCulture.NumberFormat), Math.Abs(0.01 * values[i])));
                }
            }
        }
    }
}
