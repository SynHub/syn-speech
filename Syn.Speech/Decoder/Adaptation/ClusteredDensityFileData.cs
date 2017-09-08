using System;
using System.Collections.Generic;
using Syn.Speech.Common;

//PATROLLED
namespace Syn.Speech.Decoder.Adaptation
{
    public class ClusteredDensityFileData
    {
        private int numberOfClusters;
        private int[] corespondingClass;

        public ClusteredDensityFileData(ILoader loader, int numberOfClusters)
        {
            this.numberOfClusters = numberOfClusters;
            kMeansClustering(loader, 30);
        }

        public int getNumberOfClusters()
        {
            return this.numberOfClusters;
        }

        public int getClassIndex(int gaussian)
        {
            return corespondingClass[gaussian];
        }

        private float euclidianDistance(float[] a, float[] b)
        {
            double s = 0, d;

            for (int i = 0; i < a.Length; i++)
            {
                d = a[i] - b[i];
                s += d * d;
            }

            return (float)Math.Sqrt(s);
        }

        private bool isEqual(float[] a, float[] b)
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

        private void kMeansClustering(ILoader loader, int maxIterations) {
        var initialData = loader.getMeansPool();
        List<float[]> oldCentroids = new List<float[]>(numberOfClusters);
        List<float[]> centroids = new List<float[]>(numberOfClusters);
        int numberOfElements = initialData.size(), nrOfIterations = maxIterations, index;
        int[] count = new int[numberOfClusters];
        double distance, min;
        float[] currentValue, centroid;
        //var array = new float[numberOfClusters][numberOfElements][];
        var array = new float[numberOfClusters][][];
        bool converged = false;
        Random randomGenerator = new Random();

        for (int i = 0; i < numberOfClusters; i++) {
            index = randomGenerator.Next(numberOfElements);
            centroids.Add(initialData.get(index));
            oldCentroids.Add(initialData.get(index));
            count[i] = 0;
        }

        index = 0;

        while (!converged && nrOfIterations > 0) {
            corespondingClass = new int[initialData.size()];
            //array = new float[numberOfClusters][numberOfElements][];
            array = new float[numberOfClusters][][];
            for (int i = 0; i < numberOfClusters; i++)
            {
                oldCentroids[i] = centroids[i];
                //oldCentroids.set(i, centroids[i]);
                count[i] = 0;
            }

            for (int i = 0; i < initialData.size(); i++) {
                currentValue = initialData.get(i);
                min = this.euclidianDistance(oldCentroids[0], currentValue);
                index = 0;

                for (int k = 1; k < numberOfClusters; k++) {
                    distance = this.euclidianDistance(oldCentroids[k],
                            currentValue);

                    if (distance < min) {
                        min = distance;
                        index = k;
                    }
                }

                array[index][count[index]] = currentValue;
                corespondingClass[i] = index;
                count[index]++;

            }

            for (int i = 0; i < numberOfClusters; i++) {
                centroid = new float[initialData.get(0).Length];

                if (count[i] > 0) {

                    for (int j = 0; j < count[i]; j++) {
                        for (int k = 0; k < initialData.get(0).Length; k++) {
                            centroid[k] += array[i][j][k];
                        }
                    }

                    for (int k = 0; k < initialData.get(0).Length; k++) {
                        centroid[k] /= count[i];
                    }
                    centroids[i] = centroid;
                    //centroids.set(i, centroid);
                }
            }

            converged = true;

            for (int i = 0; i < numberOfClusters; i++) {
                converged = converged
                        && (this.isEqual(centroids[i], oldCentroids[i]));
            }

            nrOfIterations--;
        }
    }
    }
}
