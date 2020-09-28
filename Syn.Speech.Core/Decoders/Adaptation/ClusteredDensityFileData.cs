using System;
using System.Collections.Generic;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Adaptation
{
    public class ClusteredDensityFileData
    {
        private readonly int _numberOfClusters;
        private int[] _corespondingClass;

        public ClusteredDensityFileData(ILoader loader, int numberOfClusters)
        {
            _numberOfClusters = numberOfClusters;
            KMeansClustering(loader, 30);
        }

        public int GetNumberOfClusters()
        {
            return _numberOfClusters;
        }

        public int GetClassIndex(int gaussian)
        {
            return _corespondingClass[gaussian];
        }

        private float EuclidianDistance(float[] a, float[] b)
        {
            double s = 0, d;

            for (int i = 0; i < a.Length; i++)
            {
                d = a[i] - b[i];
                s += d * d;
            }

            return (float)Math.Sqrt(s);
        }

        private bool IsEqual(float[] a, float[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        private void KMeansClustering(ILoader loader, int maxIterations) {
        var initialData = loader.MeansPool;
        List<float[]> oldCentroids = new List<float[]>(_numberOfClusters);
        List<float[]> centroids = new List<float[]>(_numberOfClusters);
        int numberOfElements = initialData.Size, nrOfIterations = maxIterations, index;
        int[] count = new int[_numberOfClusters];
        double distance, min;
        float[] currentValue, centroid;
        //var array = new float[numberOfClusters][numberOfElements][];
        var array = new float[_numberOfClusters][][];
        bool converged = false;
        Random randomGenerator = new Random();

        for (int i = 0; i < _numberOfClusters; i++) {
            index = randomGenerator.Next(numberOfElements);
            centroids.Add(initialData.Get(index));
            oldCentroids.Add(initialData.Get(index));
            count[i] = 0;
        }

        index = 0;

        while (!converged && nrOfIterations > 0) {
            _corespondingClass = new int[initialData.Size];
            //array = new float[numberOfClusters][numberOfElements][];
            array = new float[_numberOfClusters][][];
            for (int i = 0; i < _numberOfClusters; i++)
            {
                oldCentroids[i] = centroids[i];
                //oldCentroids.set(i, centroids[i]);
                count[i] = 0;
            }

            for (int i = 0; i < initialData.Size; i++) {
                currentValue = initialData.Get(i);
                min = EuclidianDistance(oldCentroids[0], currentValue);
                index = 0;

                for (int k = 1; k < _numberOfClusters; k++) {
                    distance = EuclidianDistance(oldCentroids[k],
                            currentValue);

                    if (distance < min) {
                        min = distance;
                        index = k;
                    }
                }

                array[index][count[index]] = currentValue;
                _corespondingClass[i] = index;
                count[index]++;

            }

            for (int i = 0; i < _numberOfClusters; i++) {
                centroid = new float[initialData.Get(0).Length];

                if (count[i] > 0) {

                    for (int j = 0; j < count[i]; j++) {
                        for (int k = 0; k < initialData.Get(0).Length; k++) {
                            centroid[k] += array[i][j][k];
                        }
                    }

                    for (int k = 0; k < initialData.Get(0).Length; k++) {
                        centroid[k] /= count[i];
                    }
                    centroids[i] = centroid;
                    //centroids.set(i, centroid);
                }
            }

            converged = true;

            for (int i = 0; i < _numberOfClusters; i++) {
                converged = converged
                        && (IsEqual(centroids[i], oldCentroids[i]));
            }

            nrOfIterations--;
        }
    }
    }
}
