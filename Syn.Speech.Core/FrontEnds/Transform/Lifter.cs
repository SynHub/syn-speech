using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Transform
{
    /// <summary>
    /// Applies the Lifter to the input mel-cepstrum to 
    /// smooth cepstrum values
    /// 
    /// @author Horia Cucu
    /// </summary>
    public class Lifter:BaseDataProcessor
    {
        /// <summary>
        /// The property for the value of the lifterValue.
        /// </summary>
        [S4Integer(DefaultValue = 22)]
        public static string PropLifterValue = "lifterValue";
        protected int LifterValue;

        protected int CepstrumSize; // size of a Cepstrum
        protected double[] LifterWeights;

        public Lifter(int lifterValue) 
        {
            this.LifterValue = lifterValue;
        }

        public Lifter() {
        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            LifterValue = ps.GetInt(PropLifterValue);
        }


        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Returns the next DoubleData object, which is the lifted mel-cepstrum of the input mel-cepstrum. Signals are returned unmodified.
        /// </summary>
        /// <returns>
        /// The next available DoubleData lifted mel-cepstrum, or Signal object, or null if no Data is available.
        /// </returns>
        public override IData GetData()
        {
            var data = Predecessor.GetData(); // get the cepstrum
            if (data != null && data is DoubleData) {
                LiftCepstrum((DoubleData) data);
            }
            return data;
        }

        /// <summary>
        /// Lifts the input mel-cepstrum.
        /// </summary>
        /// <param name="input">A mel-cepstrum frame.</param>
        /// <exception cref="System.ArgumentException">MelCepstrum size is incorrect: 
        ///                                 + melcepstrum.length ==  + melCepstrum.Length
        ///                                 + , cepstrumSize ==  + CepstrumSize</exception>
        private void LiftCepstrum(DoubleData input) 
        {
            var melCepstrum = input.Values;

            if (LifterWeights == null) {
                CepstrumSize = melCepstrum.Length;
                ComputeLifterWeights();
            } else if (melCepstrum.Length != CepstrumSize) {
                throw new ArgumentException(
                        "MelCepstrum size is incorrect: "
                                + "melcepstrum.length == " + melCepstrum.Length
                                + ", cepstrumSize == " + CepstrumSize);
            }

            for (var i = 0; i < melCepstrum.Length; i++) {
                melCepstrum[i] = melCepstrum[i]* LifterWeights[i];
            }
        }

        /// <summary>
        /// Computes the Lifter weights.
        /// </summary>
        private void ComputeLifterWeights() {
            LifterWeights = new double[CepstrumSize];
            for (var i = 0; i < CepstrumSize; i++) {
                LifterWeights[i] = 1 + LifterValue / 2
                       * Math.Sin(i* Math.PI / LifterValue);
            }
        }

    }
}
