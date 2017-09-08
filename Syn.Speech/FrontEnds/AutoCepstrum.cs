using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Syn.Logging;
using Syn.Speech.FrontEnds.Denoises;
using Syn.Speech.FrontEnds.FrequencyWarp;
using Syn.Speech.FrontEnds.Transform;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{

    /// <summary>
    /// Cepstrum is an auto-configurable DataProcessor which is used to compute a
    /// specific cepstrum (for a target acoustic model) given the spectrum. The
    /// Cepstrum is computed using a pipeline of front end components which are
    /// selected, customized or ignored depending on the feat.params file which
    /// characterizes the target acoustic model for which this cepstrum is computed.
    /// A typical legacy MFCC Cepstrum will use a MelFrequencyFilterBank, followed
    /// by a DiscreteCosineTransform. A typical denoised MFCC Cepstrum will use a
    /// MelFrequencyFilterBank, followed by a Denoise component, followed by a
    /// DiscreteCosineTransform2, followed by a Lifter component. The
    /// MelFrequencyFilterBank parameters (numberFilters, minimumFrequency and
    /// maximumFrequency) are auto-configured based on the values found in
    /// feat.params.
    /// @author Horia Cucu
    /// </summary>
    public class AutoCepstrum : BaseDataProcessor
    {
        /// <summary>
        /// The property specifying the acoustic model for which this cepstrum will
        /// be configured. For this acoustic model (AM) it is mandatory to specify a
        /// location in the configuration file. The Cepstrum will be configured
        /// based on the feat.params file that will be found in the specified AM
        /// location.
        /// </summary>
        [S4Component(Type = typeof(ILoader))]
        public static string PropLoader = "loader";
        protected ILoader Loader;


        /// <summary>
        /// The filter bank which will be used for creating the cepstrum. The filter
        /// bank is always inserted in the pipeline and its minimum frequency,
        /// maximum frequency and number of filters are configured based on the
        /// "lowerf", "upperf" and "nfilt" values in the feat.params file of the
        /// target acoustic model.
        /// </summary>
        protected BaseDataProcessor FilterBank;

        /// <summary>
        /// The denoise component which could be used for creating the cepstrum. The
        /// denoise component is inserted in the pipeline only if
        /// "-remove_noise yes" is specified in the feat.params file of the target
        /// acoustic model.
        /// </summary>
        protected Denoise Denoise;

        /// <summary>
        /// The property specifying the DCT which will be used for creating the
        /// cepstrum. If "-transform legacy" is specified in the feat.params file of
        /// the target acoustic model or if the "-transform" parameter does not
        /// appear in this file at all, the legacy DCT component is inserted in the
        /// pipeline. If "-transform dct" is specified in the feat.params file of
        /// the target acoustic model, then the current DCT component is inserted in
        /// the pipeline.
        /// </summary>
        protected DiscreteCosineTransform Dct;

        /// <summary>
        /// The lifter component which could be used for creating the cepstrum. The
        /// lifter component is inserted in the pipeline only if
        /// "-lifter <lifterValue>" is specified in the feat.params file of the
        /// target acoustic model.
        /// </summary>
        protected Lifter Lifter;

        /// <summary>
        /// The list of <code>DataProcessor</code>s which were auto-configured for
        /// this Cepstrum component.
        /// </summary>
        protected List<IDataProcessor> SelectedDataProcessors;

        public AutoCepstrum(ILoader loader)
        {
            Loader = loader;
            loader.Load();
            InitDataProcessors();
        }

        public AutoCepstrum()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            Loader = (ILoader)ps.GetComponent(PropLoader);
            try
            {
                Loader.Load();
            }
            catch (IOException e)
            {
                throw new PropertyException(e);
            }
            InitDataProcessors();
        }

        private void InitDataProcessors()
        {
            try
            {
                var featParams = Loader.Properties;
                SelectedDataProcessors = new List<IDataProcessor>();

                double lowFreq = double.Parse(featParams["-lowerf"], CultureInfo.InvariantCulture.NumberFormat);
                double hiFreq = double.Parse(featParams["-upperf"], CultureInfo.InvariantCulture.NumberFormat);
                int numFilter = int.Parse(featParams["-nfilt"], CultureInfo.InvariantCulture.NumberFormat);

                // TODO: should not be there, but for now me must preserve
                // backward compatibility with the legacy code.
                if (Loader is KaldiLoader)
                {
                    FilterBank = new MelFrequencyFilterBank2(lowFreq, hiFreq, numFilter);
                }
                else
                {
                    FilterBank = new MelFrequencyFilterBank(lowFreq, hiFreq, numFilter);
                }

                SelectedDataProcessors.Add(FilterBank);

                if ((featParams.get("-remove_noise") == null) || (featParams.get("-remove_noise").Equals("yes")))
                {
                    Denoise = new Denoise(
                        typeof(Denoise).GetField<S4Double>("LambdaPower").DefaultValue,
                        typeof(Denoise).GetField<S4Double>("LambdaA").DefaultValue,
                        typeof(Denoise).GetField<S4Double>("LambdaB").DefaultValue,
                        typeof(Denoise).GetField<S4Double>("LambdaT").DefaultValue,
                        typeof(Denoise).GetField<S4Double>("MuT").DefaultValue,
                        typeof(Denoise).GetField<S4Double>("MaxGain").DefaultValue,
                        typeof(Denoise).GetField<S4Integer>("SmoothWindow").DefaultValue);

                    // denoise.newProperties();
                    Denoise.Predecessor = SelectedDataProcessors[SelectedDataProcessors.Count - 1];
                    SelectedDataProcessors.Add(Denoise);
                }

                if ((featParams.get("-transform") != null)
                    && (featParams.get("-transform").Equals("dct")))
                {
                    Dct = new DiscreteCosineTransform2(
                        numFilter,
                        typeof(DiscreteCosineTransform).GetField<S4Integer>("PropCepstrumLength").DefaultValue);
                }
                else if ((featParams.get("-transform") != null)
                  && (featParams.get("-transform").Equals("kaldi")))
                {
                    Dct = new KaldiDiscreteCosineTransform(numFilter,typeof(DiscreteCosineTransform).GetField<S4Integer>("PropCepstrumLength").DefaultValue);
                }
                else
                {
                    Dct = new DiscreteCosineTransform(numFilter,typeof(DiscreteCosineTransform).GetField<S4Integer>("PropCepstrumLength").DefaultValue); 
                }
                Dct.Predecessor = SelectedDataProcessors[SelectedDataProcessors.Count - 1];
                SelectedDataProcessors.Add(Dct);

                if (featParams.get("-lifter") != null)
                {
                    Lifter = new Lifter(int.Parse(featParams.get("-lifter"), CultureInfo.InvariantCulture.NumberFormat));
                    Lifter.Predecessor = SelectedDataProcessors[SelectedDataProcessors.Count - 1];
                    SelectedDataProcessors.Add(Lifter);
                }


                this.LogInfo("Cepstrum component auto-configured as follows: " + ToString());

            }
            catch (Exception ex)
            {
                throw new RuntimeException(ex);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            foreach (IDataProcessor dataProcessor in SelectedDataProcessors)
                dataProcessor.Initialize();
        }

        /// <summary>
        /// Returns the processed Data output, basically calls <code>getData()</code> on the last processor.
        /// </summary>
        /// <returns>
        /// A Data object that has been processed by the cepstrum.
        /// </returns>
        public override IData GetData()
        {
            IDataProcessor dp;
            dp = SelectedDataProcessors[SelectedDataProcessors.Count - 1];
            return dp.GetData();
        }

        /// <summary>
        /// Sets the predecessor for this DataProcessor. The predecessor is actually the spectrum builder.
        /// </summary>
        /// <value>
        /// The predecessor of this DataProcessor
        /// </value>
        public override IDataProcessor Predecessor
        {
            set { FilterBank.Predecessor = value; }
        }


        /// <summary>
        /// Returns a description of this Cepstrum component in the format:
        /// <cepstrum name> {<DataProcessor1>, <DataProcessor2> ...
        /// <DataProcessorN>}
        /// </summary>
        /// <returns>
        /// A description of this Cepstrum.
        /// </returns>
        public override string ToString()
        {
            StringBuilder description = new StringBuilder(base.ToString())
                    .Append(" {");
            foreach (IDataProcessor dp in SelectedDataProcessors)
                description.Append(dp).Append(", ");
            description.Length = description.Length - 2;
            return description.Append('}').ToString();
        }
    }
}
