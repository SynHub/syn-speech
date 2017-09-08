using System;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Transform
{

    /// <summary>
    /// Applies the optimized MelCosine filter used in pocketsphinx to the given melspectrum.
    /// </summary>
    public class DiscreteCosineTransform2 : DiscreteCosineTransform
    {

        public DiscreteCosineTransform2(int numberMelFilters, int cepstrumSize)
            : base(numberMelFilters, cepstrumSize)
        {

        }

        public DiscreteCosineTransform2()
        {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        }

        /// <summary>
        /// Apply the optimized MelCosine filter used in pocketsphinx to the given melspectrum.
        /// </summary>
        /// <param name="melspectrum">The MelSpectrum data.</param>
        /// <returns>
        /// MelCepstrum data produced by apply the MelCosine filter to the MelSpectrum data.
        /// </returns>
        protected override double[] ApplyMelCosine(double[] melspectrum)
        {

            // create the cepstrum
            var cepstrum = new double[CepstrumSize];
            var sqrtInvN = Math.Sqrt(1.0 / NumberMelFilters);
            var sqrtInv_2N = Math.Sqrt(2.0 / NumberMelFilters);

            cepstrum[0] = melspectrum[0];
            for (var j = 1; j < NumberMelFilters; j++)
            {
                cepstrum[0] += melspectrum[j];
            }

            cepstrum[0] *= sqrtInvN;

            if (NumberMelFilters <= 0)
            {
                return cepstrum;
            }

            for (var i = 1; i < cepstrum.Length; i++)
            {
                var melcosineI = Melcosine[i];
                int j;
                cepstrum[i] = 0;
                for (j = 0; j < NumberMelFilters; j++)
                {
                    cepstrum[i] += (melspectrum[j] * melcosineI[j]);
                }
                cepstrum[i] *= sqrtInv_2N;
            }
            return cepstrum;
        }
    }

}
