using Syn.Speech.FrontEnds;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture
{


    public class SetBasedGaussianMixture : GaussianMixture
    {

        private readonly MixtureComponentSet _mixtureComponentSet;

        public SetBasedGaussianMixture(GaussianWeights mixtureWeights, MixtureComponentSet mixtureComponentSet, int id)
            : base(mixtureWeights, null, id)
        {

            _mixtureComponentSet = mixtureComponentSet;
        }


        public override float CalculateScore(IData feature)
        {
            _mixtureComponentSet.UpdateTopScores(feature);
            float ascore = 0;
            for (var i = 0; i < MixtureWeights.StreamsNum; i++)
            {
                var logTotal = LogMath.LogZero;
                for (var j = 0; j < _mixtureComponentSet.TopGauNum; j++)
                {
                    var topGauScore = _mixtureComponentSet.GetTopGauScore(i, j);
                    var topGauId = _mixtureComponentSet.GetTopGauId(i, j);
                    var mixtureWeightx = MixtureWeights.Get(_Id, i, topGauId);
                    logTotal = LogMath.AddAsLinear(logTotal, topGauScore + mixtureWeightx);
                }
                ascore += logTotal;
            }
            return ascore;
        }

        /**
         * Calculates the scores for each component in the senone.
         *
         * @param feature the feature to score
         * @return the LogMath log scores for the feature, one for each component
         */

        public override float[] CalculateComponentScore(IData feature)
        {
            _mixtureComponentSet.UpdateScores(feature);
            var scores = new float[_mixtureComponentSet.Size()];
            var scoreIdx = 0;
            for (var i = 0; i < MixtureWeights.StreamsNum; i++)
            {
                for (var j = 0; j < _mixtureComponentSet.GauNum; j++)
                {
                    scores[scoreIdx++] = _mixtureComponentSet.GetGauScore(i, j) + MixtureWeights.Get(_Id, i, _mixtureComponentSet.GetGauId(i, j));
                }
            }
            return scores;
        }

        public override MixtureComponent[] MixtureComponents
        {
            get { return _mixtureComponentSet.ToArray(); }
        }


        public override int GetDimension()
        {
            return _mixtureComponentSet.Dimension();
        }

        /** @return the number of component densities of this <code>GaussianMixture</code>. */

        public override int NumComponents
        {
            get { return _mixtureComponentSet.Size(); }
        }
    }

}
