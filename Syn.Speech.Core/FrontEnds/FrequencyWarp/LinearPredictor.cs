using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.FrequencyWarp
{
    /// <summary>
    /// Computes the linear predictive model using the Levinson-Durbin algorithm. Linear prediction assumes that a signal can
    /// be model as a linear combination of previous samples, that is, the current sample x[i] can be modeled as:
    /// <p/>
    /// <pre> x[i] = a[0] + a[1] /// x[i - 1] + a[2] /// x[i - 2] + ... </pre>
    /// <p/>
    /// The summation on the right hand side of the equation involves a finite number of terms. The number of previous
    /// samples used is the order of the linear prediction.
    /// <p/>
    /// This class also provides a method to compute LPC cepstra, that is, the cepstra computed from LPC coefficients, as
    /// well as a method to compute the bilinear transformation of the LPC
    /// </summary>
    public class LinearPredictor
    {
        private int _order;
        private int _cepstrumOrder;
        private double[] _reflectionCoeffs;
        private double[] _arParameters;
        private double _alpha;
        private double[] _cepstra;
        private readonly double[] _bilinearCepstra;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearPredictor"/> class with the given order.
        /// </summary>
        /// <param name="order">The order of the LinearPredictor.</param>
        public LinearPredictor(int order)
        {
            _order = order;

            // Set the rest to null values
            _reflectionCoeffs = null;
            _arParameters = null;
            _alpha = 0;
            _cepstra = null;
            _bilinearCepstra = null;
        }

        /// <summary>
        /// Method to compute Linear Prediction Coefficients for a frame of speech. Assumes the following sign convention:<br> prediction(x[t]) = Sum_i {Ar[i] * x[t-i]}</br>
        /// </summary>
        /// <param name="autocor">The autocor.</param>
        /// <returns>The energy of the frame (alpha in the Levinson recursion).</returns>
        public double[] GetARFilter(double[] autocor)
        {
            //No signal
            if (autocor[0] == 0)
            {
                return null;
            }
            _reflectionCoeffs = new double[_order + 1];
            _arParameters = new double[_order + 1];
            var backwardPredictor = new double[_order + 1];

            _alpha = autocor[0];
            _reflectionCoeffs[1] = -autocor[1] / autocor[0];
            _arParameters[0] = 1.0;
            _arParameters[1] = _reflectionCoeffs[1];
            _alpha *= (1 - _reflectionCoeffs[1] * _reflectionCoeffs[1]);

            for (var i = 2; i <= _order; i++)
            {
                for (var j = 1; j < i; j++)
                {
                    backwardPredictor[j] = _arParameters[i - j];
                }
                _reflectionCoeffs[i] = 0;
                for (var j = 0; j < i; j++)
                {
                    _reflectionCoeffs[i] -= _arParameters[j] * autocor[i - j];
                }
                _reflectionCoeffs[i] /= _alpha;

                for (var j = 1; j < i; j++)
                {
                    _arParameters[j] += _reflectionCoeffs[i] * backwardPredictor[j];
                }
                _arParameters[i] = _reflectionCoeffs[i];
                _alpha *= (1 - _reflectionCoeffs[i] * _reflectionCoeffs[i]);
                if (_alpha <= 0.0)
                {
                    return null;
                }
            }
            return _arParameters;
        }

        /// <summary>
        /// Computes AR parameters from a given set of reflection coefficients.
        /// </summary>
        /// <param name="rc">Double array of reflection coefficients. The RC array must begin at 1 (RC[0] is a dummy value).</param>
        /// <param name="lpcorder">AR order desired.</param>
        /// <returns>AR parameters</returns>
        public double[] ReflectionCoeffsToArParameters(double[] rc, int lpcorder)
        {
            //double[][] tmp = new double[lpcorder + 1][lpcorder + 1];
            var tmp = new double[lpcorder + 1][];

            _order = lpcorder;
            _reflectionCoeffs = rc.Clone() as double[];

            for (var i = 1; i <= lpcorder; i++)
            {
                for (var m = 1; m < i; m++)
                {
                    tmp[i][m] = tmp[i - 1][m] - rc[i] * tmp[i - 1][i - m];
                }
                tmp[i][i] = rc[i];
            }
            _arParameters[0] = 1;
            for (var m = 1; m <= lpcorder; m++)
            {
                _arParameters[m] = tmp[m][m];
            }
            return _arParameters;
        }

        /// <summary>
        /// Computes LPC Cepstra from the AR predictor parameters and alpha using a recursion invented by Oppenheim et al. 
        /// The literature shows the optimal value of cepstral order to be:<p/><pre>0.75 * LPCorder <= ceporder <= 1.25 * LPCorder</pre>
        /// </summary>
        /// <param name="ceporder">The order of the LPC cepstral vector to be computed.</param>
        /// <returns>LPC cepstra</returns>
        public double[] GetData(int ceporder)
        {
            int i;
            double sum;

            if (ceporder <= 0)
            {
                return null;
            }

            _cepstrumOrder = ceporder;
            _cepstra = new double[_cepstrumOrder];

            _cepstra[0] = Math.Log(_alpha);
            if (_cepstrumOrder == 1)
            {
                return _cepstra;
            }

            _cepstra[1] = -_arParameters[1];

            for (i = 2; i < Math.Min(_cepstrumOrder, _order + 1); i++)
            {
                sum = i * _arParameters[i];
                for (var j = 1; j < i; j++)
                {
                    sum += _arParameters[j] * _cepstra[i - j] * (i - j);
                }
                _cepstra[i] = -sum / i;
            }
            for (; i < _cepstrumOrder; i++)
            { // Only if cepstrumOrder > order+1
                sum = 0;
                for (var j = 1; j <= _order; j++)
                {
                    sum += _arParameters[j] * _cepstra[i - j] * (i - j);
                }
                _cepstra[i] = -sum / i;
            }
            return _cepstra;
        }

        /// <summary>
        /// Computes a bi-linear frequency warped version of the LPC cepstrum from the LPC cepstrum. The recursive algorithm
        /// used is defined in Oppenheim's paper in Proceedings of IEEE, June 1972 The program has been written using g[x,y]
        /// = g_o[x,-y] where g_o is the array used by Oppenheim. To handle the reversed array index the recursion has been
        /// done DOWN the array index.
        /// </summary>
        /// <param name="warp">The warping coefficient. For 16KHz speech 0.6 is good valued..</param>
        /// <param name="nbilincepstra">The number of bilinear cepstral values to be computed from the linear frequencycepstrum.</param>
        /// <returns>A bi-linear frequency warped version of the LPC cepstrum.</returns>
        public double[] GetBilinearCepstra(double warp, int nbilincepstra)
        {
            //double[][] g = new double[nbilincepstra][cepstrumOrder];
            var g = new double[nbilincepstra][];

            // Make a local copy as this gets destroyed
            var lincep = Java.CopyOf(_cepstra, _cepstrumOrder);

            _bilinearCepstra[0] = lincep[0];
            lincep[0] = 0;
            g[0][_cepstrumOrder - 1] = lincep[_cepstrumOrder - 1];
            for (var i = 1; i < nbilincepstra; i++)
            {
                g[i][_cepstrumOrder - 1] = 0;
            }

            for (var i = _cepstrumOrder - 2; i >= 0; i--)
            {
                g[0][i] = warp * g[0][i + 1] + lincep[i];
                g[1][i] = (1 - warp * warp) * g[0][i + 1] + warp * g[1][i + 1];
                for (var j = 2; j < nbilincepstra; j++)
                {
                    g[j][i] = warp * (g[j][i + 1] - g[j - 1][i]) + g[j - 1][i + 1];
                }
            }

            for (var i = 1; i <= nbilincepstra; i++)
            {
                _bilinearCepstra[i] = g[i][0];
            }

            return _bilinearCepstra;
        }
    }
}
