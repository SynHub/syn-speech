using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Transform
{

    /// <summary>
    /// DCT implementation that conforms to one used in Kaldi.
    /// </summary>
public class KaldiDiscreteCosineTransform : DiscreteCosineTransform {

    public KaldiDiscreteCosineTransform(int numberMelFilters, int cepstrumSize):base(numberMelFilters, cepstrumSize)
    {

    }

    public KaldiDiscreteCosineTransform() {
    }


    protected override void ComputeMelCosine() {
        
        Melcosine = Java.CreateArray<double[][]>(CepstrumSize, NumberMelFilters); //melcosine = new double[cepstrumSize][numberMelFilters];
        Arrays.Fill(Melcosine[0],Math.Sqrt(1f / NumberMelFilters));

        var normScale = Math.Sqrt(2f / NumberMelFilters);

        for (var i = 1; i < CepstrumSize; i++) {
            var frequency = Math.PI * i / NumberMelFilters;

            for (var j = 0; j < NumberMelFilters; j++)
                Melcosine[i][j] = normScale * Math.Cos(frequency * (j + 0.5));
        }
    }


    protected override double[] ApplyMelCosine(double[] melspectrum) {
        var cepstrum = new double[CepstrumSize];

        for (var i = 0; i < cepstrum.Length; i++) {
                for (var j = 0; j < NumberMelFilters; j++)
                    cepstrum[i] += melspectrum[j] * Melcosine[i][j];
        }
        
        return cepstrum;
    }
}

}
