using System;
using Syn.Speech.Api;
using Syn.Speech.Decoders.Search;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Adaptation
{
    public class Stats
    {
        private readonly ClusteredDensityFileData _means;
        private readonly int _nrOfClusters;
        private readonly Sphinx3Loader _loader;
        private readonly float _varFlor;
        private readonly LogMath _logMath = LogMath.GetLogMath();

        public Stats(ILoader loader, ClusteredDensityFileData means)
        {
            _loader = (Sphinx3Loader)loader;
            _nrOfClusters = means.GetNumberOfClusters();
            _means = means;
            _varFlor = (float)1e-5;
            InvertVariances();
            Init();
        }

        private void Init()
        {
            int len = _loader.VectorLength[0];
            RegLs = new double[_nrOfClusters][][][][];
            RegRs = new double[_nrOfClusters][][][];

            for (int i = 0; i < _nrOfClusters; i++)
            {
                RegLs[i] = new double[_loader.NumStreams][][][];
                RegRs[i] = new double[_loader.NumStreams][][];

                for (int j = 0; j < _loader.NumStreams; j++)
                {
                    len = _loader.VectorLength[j];
                    RegLs[i][j] = Java.CreateArray<double[][][]>(len, len + 1, len + 1); //this.regLs[i][j] = new double[len][len + 1][len + 1];
                    RegRs[i][j] = Java.CreateArray<double[][]>(len, len + 1);// this.regRs[i][j] = new double[len][len + 1];
                }
            }
        }

        public ClusteredDensityFileData ClusteredData
        {
            get { return _means; }
        }

        public double[][][][][] RegLs { get; private set; }

        public double[][][][] RegRs { get; private set; }

        private void InvertVariances()
        {

            for (int i = 0; i < _loader.NumStates; i++)
            {
                for (int k = 0; k < _loader.NumGaussiansPerState; k++)
                {
                    for (int l = 0; l < _loader.VectorLength[0]; l++)
                    {
                        if (_loader.VariancePool.Get(
                                i * _loader.NumGaussiansPerState + k)[l] <= 0f)
                        {
                            _loader.VariancePool.Get(
                                    i * _loader.NumGaussiansPerState + k)[l] = (float)0.5;
                        }
                        else if (_loader.VariancePool.Get(
                              i * _loader.NumGaussiansPerState + k)[l] < _varFlor)
                        {
                            _loader.VariancePool.Get(
                                    i * _loader.NumGaussiansPerState + k)[l] = 1f / _varFlor;
                        }
                        else
                        {
                            _loader.VariancePool.Get(
                                    i * _loader.NumGaussiansPerState + k)[l] = 1f / _loader.VariancePool.Get(
                                            i * _loader.NumGaussiansPerState
                                            + k)[l];
                        }
                    }
                }
            }
        }

        private float[] ComputePosterios(float[] componentScores, int numStreams)
        {
            float[] posteriors = componentScores;

            int step = componentScores.Length / numStreams;
            int startIdx = 0;
            for (int i = 0; i < numStreams; i++)
            {
                float max = posteriors[startIdx];
                for (int j = startIdx + 1; j < startIdx + step; j++)
                {
                    if (posteriors[j] > max)
                    {
                        max = posteriors[j];
                    }
                }

                for (int j = startIdx; j < startIdx + step; j++)
                {
                    posteriors[j] = (float)_logMath.LogToLinear(posteriors[j] - max);
                }
                startIdx += step;
            }

            return posteriors;
        }

        public void Collect(SpeechResult result)  {
		Token token = result.Result.GetBestToken();
		float[] componentScore, featureVector, posteriors, tmean;
		int[] len;
		float dnom, wtMeanVar, wtDcountVar, wtDcountVarMean, mean;
		int mId, cluster;
		int numStreams, gauPerState;

		if (token == null)
			throw new Exception("Best token not found!");

		do {
			FloatData feature = (FloatData) token.Data;
			ISearchState ss = token.SearchState;

			if (!(ss is IHMMSearchState && ss.IsEmitting)) {
				token = token.Predecessor;
				continue;
			}

			componentScore = token.CalculateComponentScore(feature);
			featureVector = FloatData.ToFloatData(feature).Values;
			mId = (int) ((IHMMSearchState) token.SearchState).HmmState
					.GetMixtureId();
            if (_loader is Sphinx3Loader && _loader.HasTiedMixtures())
                // use CI phone ID for tied mixture model
                mId = _loader.Senone2Ci[mId];
			len = _loader.VectorLength;
			numStreams = _loader.NumStreams;
			gauPerState = _loader.NumGaussiansPerState;
			posteriors = ComputePosterios(componentScore, numStreams);
			int featVectorStartIdx = 0;

			for (int i = 0; i < numStreams; i++) {
				for (int j = 0; j < gauPerState; j++) {

					cluster = _means.GetClassIndex(mId * numStreams
							* gauPerState + i * gauPerState + j);
					dnom = posteriors[i * gauPerState + j];
					if (dnom > 0f) {
						tmean = _loader.MeansPool.Get(
								mId * numStreams * gauPerState + i
										* gauPerState + j);

						for (int k = 0; k < len[i]; k++) {
							mean = posteriors[i * gauPerState + j]
									* featureVector[k + featVectorStartIdx];
							wtMeanVar = mean
									* _loader.VariancePool.Get(
											mId * numStreams * gauPerState + i
													* gauPerState + j)[k];
							wtDcountVar = dnom
									* _loader.VariancePool.Get(
											mId * numStreams * gauPerState + i
													* gauPerState + j)[k];

							for (int p = 0; p < len[i]; p++) {
								wtDcountVarMean = wtDcountVar * tmean[p];

								for (int q = p; q < len[i]; q++) {
									RegLs[cluster][i][k][p][q] += wtDcountVarMean
											* tmean[q];
								}
								RegLs[cluster][i][k][p][len[i]] += wtDcountVarMean;
								RegRs[cluster][i][k][p] += wtMeanVar * tmean[p];
							}
							RegLs[cluster][i][k][len[i]][len[i]] += wtDcountVar;
							RegRs[cluster][i][k][len[i]] += wtMeanVar;

						}
					}
				}
				featVectorStartIdx += len[i];
			}
			token = token.Predecessor;
		} while (token != null);
	}

        /// <summary>
        /// Fill lower part of Legetter's set of G matrices.
        /// </summary>
        public void FillRegLowerPart()
        {
            for (int i = 0; i < _nrOfClusters; i++)
            {
                for (int j = 0; j < _loader.NumStreams; j++)
                {
                    for (int l = 0; l < _loader.VectorLength[j]; l++)
                    {
                        for (int p = 0; p <= _loader.VectorLength[j]; p++)
                        {
                            for (int q = p + 1; q <= _loader.VectorLength[j]; q++)
                            {
                                RegLs[i][j][l][q][p] = RegLs[i][j][l][p][q];
                            }
                        }
                    }
                }
            }
        }

        public Transform CreateTransform()
        {
            var transform = new Transform(_loader, _nrOfClusters);
            transform.Update(this);
            return transform;
        }
    }
}
