using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{

    ///<summary>
    /// Defines the Bark frequency warping function. This class provides methods to convert frequencies from a linear scale
    /// to the bark scale. The bark scale is originated from measurements of the critical bandwidth. Please find more details
    /// in books about psychoacoustics or speech analysis/recognition.
    /// @author <a href="mailto:rsingh@cs.cmu.edu">rsingh</a>
    /// @version 1.0
    ///</summary>
    public class FrequencyWarper
    {
        /// <summary>
        /// Compute Bark frequency from linear frequency in Hertz.The function is:bark = 6.0*log(hertz/600 + sqrt((hertz/600)^2 + 1)).
        /// </summary>
        /// <param name="hertz">The input frequency in Hertz.</param>
        /// <returns>The frequency in a Bark scale</returns>
        public double HertzToBark(double hertz)
        {
            var x = hertz / 600;
            return (6.0 * Math.Log(x + Math.Sqrt(x * x + 1)));
        }

        /// <summary>
        /// Compute linear frequency in Hertz from Bark frequency. The function is: hertz = 300*(exp(bark/6.0) - exp(-bark/6.0))
        /// </summary>
        /// <param name="bark">The input frequency in Barks.</param>
        /// <returns>The frequency in Hertz.</returns>
        public double BarkToHertz(double bark)
        {
            var x = bark / 6.0;
            return (300.0 * (Math.Exp(x) - Math.Exp(-x)));
        }
    }
}
