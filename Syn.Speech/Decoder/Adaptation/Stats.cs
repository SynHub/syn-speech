using System;
using Syn.Speech.Api;
using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.FrontEnd;
using Syn.Speech.Linguist;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util;

//PATROLLED
namespace Syn.Speech.Decoder.Adaptation
{
    public class Stats
    {
        private ClusteredDensityFileData means;
        private double[][][][][] regLs;
        private double[][][][] regRs;
        private int nrOfClusters;
        private Sphinx3Loader loader;
        private float varFlor;
        private LogMath logMath = LogMath.getLogMath();

        public Stats(ILoader loader, ClusteredDensityFileData means)
        {
            this.loader = (Sphinx3Loader)loader;
            this.nrOfClusters = means.getNumberOfClusters();
            this.means = means;
            this.varFlor = (float)1e-5;
            this.invertVariances();
            this.init();
        }


        private void init()
        {
            int len = loader.getVectorLength()[0];
            this.regLs = new double[nrOfClusters][][][][];
            this.regRs = new double[nrOfClusters][][][];

            for (int i = 0; i < nrOfClusters; i++)
            {
                this.regLs[i] = new double[loader.getNumStreams()][][][];
                this.regRs[i] = new double[loader.getNumStreams()][][];

                for (int j = 0; j < loader.getNumStreams(); j++)
                {
                    len = loader.getVectorLength()[j];
                    //TODO: CHECK SEMANTICS
                    ///this.regLs[i][j] = new double[len][len + 1][len + 1];
                    /// this.regRs[i][j] = new double[len][len + 1];
                    this.regLs[i][j] = new double[len][][];
                    this.regRs[i][j] = new double[len][];
                }
            }
        }

        public ClusteredDensityFileData getClusteredData()
        {
            return this.means;
        }

        public double[][][][][] getRegLs()
        {
            return regLs;
        }

        public double[][][][] getRegRs()
        {
            return regRs;
        }

        private void invertVariances()
        {

            for (int i = 0; i < loader.getNumStates(); i++)
            {
                for (int k = 0; k < loader.getNumGaussiansPerState(); k++)
                {
                    for (int l = 0; l < loader.getVectorLength()[0]; l++)
                    {
                        if (loader.getVariancePool().get(
                                i * loader.getNumGaussiansPerState() + k)[l] <= 0f)
                        {
                            this.loader.getVariancePool().get(
                                    i * loader.getNumGaussiansPerState() + k)[l] = (float)0.5;
                        }
                        else if (loader.getVariancePool().get(
                              i * loader.getNumGaussiansPerState() + k)[l] < varFlor)
                        {
                            this.loader.getVariancePool().get(
                                    i * loader.getNumGaussiansPerState() + k)[l] = (float)(1f / varFlor);
                        }
                        else
                        {
                            this.loader.getVariancePool().get(
                                    i * loader.getNumGaussiansPerState() + k)[l] = (float)(1f / loader
                                    .getVariancePool().get(
                                            i * loader.getNumGaussiansPerState()
                                                    + k)[l]);
                        }
                    }
                }
            }
        }

        private float[] computePosterios(float[] componentScores, int numStreams)
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
                    posteriors[j] = (float)logMath.logToLinear(posteriors[j] - max);
                }
                startIdx += step;
            }

            return posteriors;
        }

        public void collect(SpeechResult result)  {
		Token token = result.getResult().getBestToken();
		float[] componentScore, featureVector, posteriors, tmean;
		int[] len;
		float dnom, wtMeanVar, wtDcountVar, wtDcountVarMean, mean;
		int mId, cluster;
		int numStreams, gauPerState;

		if (token == null)
			throw new Exception("Best token not found!");

		do {
			FloatData feature = (FloatData) token.getData();
			ISearchState ss = token.getSearchState();

			if (!(ss is IHMMSearchState && ss.isEmitting())) {
				token = token.getPredecessor();
				continue;
			}

			componentScore = token.calculateComponentScore(feature);
			featureVector = FloatData.toFloatData(feature).getValues();
			mId = (int) ((IHMMSearchState) token.getSearchState()).getHMMState()
					.getMixtureId();
            if (loader is Sphinx3Loader && ((Sphinx3Loader)loader).hasTiedMixtures())
                // use CI phone ID for tied mixture model
                mId = ((Sphinx3Loader)loader).getSenone2Ci()[mId];
			len = loader.getVectorLength();
			numStreams = loader.getNumStreams();
			gauPerState = loader.getNumGaussiansPerState();
			posteriors = this.computePosterios(componentScore, numStreams);
			int featVectorStartIdx = 0;

			for (int i = 0; i < numStreams; i++) {
				for (int j = 0; j < gauPerState; j++) {

					cluster = means.getClassIndex(mId * numStreams
							* gauPerState + i * gauPerState + j);
					dnom = posteriors[i * gauPerState + j];
					if (dnom > 0f) {
						tmean = loader.getMeansPool().get(
								mId * numStreams * gauPerState + i
										* gauPerState + j);

						for (int k = 0; k < len[i]; k++) {
							mean = posteriors[i * gauPerState + j]
									* featureVector[k + featVectorStartIdx];
							wtMeanVar = mean
									* loader.getVariancePool().get(
											mId * numStreams * gauPerState + i
													* gauPerState + j)[k];
							wtDcountVar = dnom
									* loader.getVariancePool().get(
											mId * numStreams * gauPerState + i
													* gauPerState + j)[k];

							for (int p = 0; p < len[i]; p++) {
								wtDcountVarMean = wtDcountVar * tmean[p];

								for (int q = p; q < len[i]; q++) {
									regLs[cluster][i][k][p][q] += wtDcountVarMean
											* tmean[q];
								}
								regLs[cluster][i][k][p][len[i]] += wtDcountVarMean;
								regRs[cluster][i][k][p] += wtMeanVar * tmean[p];
							}
							regLs[cluster][i][k][len[i]][len[i]] += wtDcountVar;
							regRs[cluster][i][k][len[i]] += wtMeanVar;

						}
					}
				}
				featVectorStartIdx += len[i];
			}
			token = token.getPredecessor();
		} while (token != null);
	}

        /// <summary>
        /// Fill lower part of Legetter's set of G matrices.
        /// </summary>
        public void fillRegLowerPart()
        {
            for (int i = 0; i < this.nrOfClusters; i++)
            {
                for (int j = 0; j < loader.getNumStreams(); j++)
                {
                    for (int l = 0; l < loader.getVectorLength()[j]; l++)
                    {
                        for (int p = 0; p <= loader.getVectorLength()[j]; p++)
                        {
                            for (int q = p + 1; q <= loader.getVectorLength()[j]; q++)
                            {
                                regLs[i][j][l][q][p] = regLs[i][j][l][p][q];
                            }
                        }
                    }
                }
            }
        }

        public Transform createTransform()
        {
            Transform transform = new Transform(loader, nrOfClusters);
            transform.update(this);
            return transform;
        }
    }
}
