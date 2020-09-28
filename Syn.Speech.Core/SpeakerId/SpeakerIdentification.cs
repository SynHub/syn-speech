using System;
using System.Collections.Generic;
using System.IO;
using Syn.Speech.FrontEnds;
using Syn.Speech.FrontEnds.Util;
using Syn.Speech.Helper;
using Syn.Speech.Helper.Mathematics.Linear;
using Syn.Speech.Helper.Mathematics.Stat.Correlation;
using Syn.Speech.Properties;
using Syn.Speech.Util.Props;
//REFACTORED
 namespace Syn.Speech.SpeakerId
{
    /// <summary>
    /// Provides method for detecting the number of speakers from a given input file
    /// </summary>
    public class SpeakerIdentification : Identification
    {

        public readonly String FrontendName = "plpFrontEnd";

        private readonly FrontEnds.FrontEnd _frontEnd;
        private readonly StreamDataSource _audioSource;
        private ConfigurationManager cm;

        public SpeakerIdentification()
        {
            URL url = new URL(URLType.Resource, Resources.speakerid_frontend_config);
            cm = new ConfigurationManager(url);
            _audioSource = cm.Lookup("streamDataSource") as StreamDataSource;
            _frontEnd = cm.Lookup(FrontendName) as FrontEnds.FrontEnd;
        }

        /**
         * @return The list of feature vectors from the fileStream used by
         *         audioSource
         */
        private List<float[]> GetFeatures()
        {
            List<float[]> ret = new List<float[]>();
            try
            {
                int featureLength = -1;
                IData feature = _frontEnd.GetData();
                while (!(feature is DataEndSignal))
                {
                    if (feature is DoubleData)
                    {
                        double[] featureData = ((DoubleData)feature).Values;
                        if (featureLength < 0)
                        {
                            featureLength = featureData.Length;
                        }
                        float[] convertedData = new float[featureData.Length];
                        for (int i = 0; i < featureData.Length; i++)
                        {
                            convertedData[i] = (float)featureData[i];
                        }
                        ret.Add(convertedData);
                    }
                    else if (feature is FloatData)
                    {
                        float[] featureData = ((FloatData)feature).Values;
                        if (featureLength < 0)
                        {
                            featureLength = featureData.Length;

                        }
                        ret.Add(featureData);
                    }
                    feature = _frontEnd.GetData();
                }
            }
            catch (Exception e)
            {
                e.PrintStackTrace();
            }
            return ret;
        }

        /**
         * 
         * @param bicValue
         *            The bicValue of the model represented by only one Gaussian.
         *            This parameter it's useful when this function is called
         *            repeatedly for different frame values and the same features
         *            parameter
         * @param frame
         *            the frame which is tested for being a change point
         * @param features
         *            the feature vectors matrix
         * @return the likelihood ratio
         */

        static double GetLikelihoodRatio(double bicValue, int frame, Array2DRowRealMatrix features)
        {
            double bicValue1, bicValue2;
            int d = Segment.FeaturesSize;
            double penalty = 0.5 * (d + 0.5 * d * (d + 1)) * Math.Log(features.getRowDimension()) * 2;
            int nrows = features.getRowDimension(), ncols = features.getColumnDimension();
            Array2DRowRealMatrix sub1, sub2;
            sub1 = (Array2DRowRealMatrix)features.getSubMatrix(0, frame - 1, 0, ncols - 1);
            sub2 = (Array2DRowRealMatrix)features.getSubMatrix(frame, nrows - 1, 0, ncols - 1);
            bicValue1 = GetBICValue(sub1);
            bicValue2 = GetBICValue(sub2);
            return (bicValue - bicValue1 - bicValue2 - penalty);
        }

        /**
         * @param start
         *            The starting frame
         * @param length
         *            The length of the interval, as numbers of frames
         * @param features
         *            The matrix build with feature vectors as rows
         * @return Returns the changing point in the input represented by features
         * 
         */

        private static int GetPoint(int start, int length, int step, Array2DRowRealMatrix features)
        {
            double max = Double.NegativeInfinity;
            int ncols = features.getColumnDimension(), point = 0;
            var sub = (Array2DRowRealMatrix)features.getSubMatrix(start, start + length - 1, 0, ncols - 1);
            double bicValue = GetBICValue(sub);
            for (int i = Segment.FeaturesSize + 1; i < length - Segment.FeaturesSize; i += step)
            {
                double aux = GetLikelihoodRatio(bicValue, i, sub);
                if (aux > max)
                {
                    max = aux;
                    point = i;
                }
            }
            if (max < 0)
                point = Integer.MIN_VALUE;
            return point + start;
        }

        /**
         * 
         * @param features
         *            Matrix with feature vectors as rows
         * @return A list with all changing points detected in the file
         */
        private LinkedList<Integer> GetAllChangingPoints(Array2DRowRealMatrix features)
        {
            LinkedList<Integer> ret = new LinkedList<Integer>();
            ret.Add(0);
            int framesCount = features.getRowDimension(), step = 500;
            int start = 0, end = step, cp;
            while (end < framesCount)
            {
                cp = GetPoint(start, end - start + 1, step / 10, features);
                if (cp > 0)
                {
                    start = cp;
                    end = start + step;
                    ret.Add(cp);
                }
                else
                    end += step;
            }
            ret.Add(framesCount);
            return ret;
        }

        /**
         * @param mat
         *            A matrix with feature vectors as rows.
         * @return Returns the BICValue of the Gaussian model that approximates the
         *         the feature vectors data samples
         */
        public static double GetBICValue(Array2DRowRealMatrix mat)
        {
            double ret = 0;
            EigenDecomposition ed = new EigenDecomposition(new Covariance(mat).getCovarianceMatrix());
            double[] re = ed.getRealEigenvalues();
            for (int i = 0; i < re.Length; i++)
                ret += Math.Log(re[i]);
            return ret * (mat.getRowDimension() / 2);
        }

        /**
         * @param inputFileName The name of the file used for diarization
         * @return A cluster for each speaker found in the input file
         */
        public List<SpeakerCluster> Cluster(Stream stream)
        {
            _audioSource.SetInputStream(stream);
            List<float[]> features = GetFeatures();
            return Cluster(features);
        }

        /**
         * @param features The feature vectors to be used for clustering
         * @return A cluster for each speaker detected based on the feature vectors provided
         */
        public List<SpeakerCluster> Cluster(List<float[]> features)
        {
            List<SpeakerCluster> ret = new List<SpeakerCluster>();
            Array2DRowRealMatrix featuresMatrix = ArrayToRealMatrix(features, features.Count);
            LinkedList<Integer> l = GetAllChangingPoints(featuresMatrix);
            var it = l.GetEnumerator();
            int curent;
            it.MoveNext();
            int previous = it.Current;
            while (it.MoveNext())
            {
                curent = it.Current;
                Segment s = new Segment(previous * Segment.FrameLength, (curent - previous)
                        * (Segment.FrameLength));
                Array2DRowRealMatrix featuresSubset = (Array2DRowRealMatrix)featuresMatrix.getSubMatrix(
                        previous, curent - 1, 0, 12);
                ret.Add(new SpeakerCluster(s, featuresSubset, GetBICValue(featuresSubset)));
                previous = curent;
            }
            int clusterCount = ret.Count;

            Array2DRowRealMatrix distance;
            distance = new Array2DRowRealMatrix(clusterCount, clusterCount);
            distance = UpdateDistances(ret);
            while (true)
            {
                double distmin = 0;
                int imin = -1, jmin = -1;

                for (int i = 0; i < clusterCount; i++)
                    for (int j = 0; j < clusterCount; j++)
                        if (i != j)
                            distmin += distance.getEntry(i, j);
                distmin /= (clusterCount * (clusterCount - 1) * 4);

                for (int i = 0; i < clusterCount; i++)
                {
                    for (int j = 0; j < clusterCount; j++)
                    {
                        if (distance.getEntry(i, j) < distmin && i != j)
                        {
                            distmin = distance.getEntry(i, j);
                            imin = i;
                            jmin = j;
                        }
                    }
                }
                if (imin == -1)
                {
                    break;
                }
                ret[imin].MergeWith(ret[jmin]);
                UpdateDistances(ret, imin, jmin, distance);
                ret.Remove(jmin);
                clusterCount--;
            }
            return ret;
        }

        /**
         * @param Clustering
         *            The array of clusters
         * @param posi
         *            The index of the merged cluster
         * @param posj
         *            The index of the cluster that will be eliminated from the
         *            clustering
         * @param distance
         *            The distance matrix that will be updated
         */
        void UpdateDistances(List<SpeakerCluster> clustering, int posi, int posj, Array2DRowRealMatrix distance)
        {
            int clusterCount = clustering.Count;
            for (int i = 0; i < clusterCount; i++)
            {
                distance.setEntry(i, posi, ComputeDistance(clustering[i], clustering[posi]));
                distance.setEntry(posi, i, distance.getEntry(i, posi));
            }
            for (int i = posj; i < clusterCount - 1; i++)
                for (int j = 0; j < clusterCount; j++)
                    distance.setEntry(i, j, distance.getEntry(i + 1, j));

            for (int i = 0; i < clusterCount; i++)
                for (int j = posj; j < clusterCount - 1; j++)
                    distance.setEntry(i, j, distance.getEntry(i, j + 1));
        }

        /**
         * @param Clustering
         *            The array of clusters
         */
        Array2DRowRealMatrix UpdateDistances(List<SpeakerCluster> clustering)
        {
            int clusterCount = clustering.Count;
            Array2DRowRealMatrix distance = new Array2DRowRealMatrix(clusterCount, clusterCount);
            for (int i = 0; i < clusterCount; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    distance.setEntry(i, j, ComputeDistance(clustering[i], clustering[j]));
                    distance.setEntry(j, i, distance.getEntry(i, j));
                }
            }
            return distance;
        }

        static double ComputeDistance(SpeakerCluster c1, SpeakerCluster c2)
        {
            int rowDim = c1.FeatureMatrix.getRowDimension() + c2.FeatureMatrix.getRowDimension();
            int colDim = c1.FeatureMatrix.getColumnDimension();
            Array2DRowRealMatrix combinedFeatures = new Array2DRowRealMatrix(rowDim, colDim);
            combinedFeatures.setSubMatrix(c1.FeatureMatrix.getData(), 0, 0);
            combinedFeatures.setSubMatrix(c2.FeatureMatrix.getData(), c1.FeatureMatrix.getRowDimension(), 0);
            double bicValue = GetBICValue(combinedFeatures);
            double d = Segment.FeaturesSize;
            double penalty = 0.5 * (d + 0.5 * d * (d + 1)) * Math.Log(combinedFeatures.getRowDimension()) * 2;
            return bicValue - c1.GetBicValue() - c2.GetBicValue() - penalty;
        }

        /**
         * @param lst
         *            An ArrayList with all the values being vectors of the same
         *            dimension
         * @return The RealMatrix with the vectors from the ArrayList on columns
         */

        static Array2DRowRealMatrix ArrayToRealMatrix(List<float[]> lst, int size)
        {
            int length = lst[1].Length;
            var ret = new Array2DRowRealMatrix(size, length);
            int i = 0;
            for (i = 0; i < size; i++)
            {
                double[] converted = new double[length];
                for (int j = 0; j < length; j++)
                    converted[j] = ((lst[i])[j]);
                ret.setRow(i, converted);
            }
            return ret;
        }

        void PrintMatrix(Array2DRowRealMatrix a)
        {
            for (int i = 0; i < a.getRowDimension(); i++)
            {
                for (int j = 0; j < a.getColumnDimension(); j++)
                    Console.Write(a.getEntry(i, j) + " ");
                Console.WriteLine();
            }
        }

      
    }
    }

