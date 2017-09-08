using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED
using Syn.Speech.Helper.Mathematics.Linear;
//REFACTORED
namespace Syn.Speech.SpeakerId
{
    public class SpeakerCluster
    {
        private readonly SortedSet<Segment> _segmentSet;
        private double _bicValue;

        public double GetBicValue()
        {
            return _bicValue;
        }

        public void SetBicValue(double bicValue)
        {
            this._bicValue = bicValue;
        }

        public Array2DRowRealMatrix FeatureMatrix { get; private set; }

        public SpeakerCluster()
        {
            _segmentSet = new SortedSet<Segment>();
        }

        public SpeakerCluster(Segment s, Array2DRowRealMatrix featureMatrix, double bicValue)
        {
            _segmentSet = new SortedSet<Segment>();
            this.FeatureMatrix = new Array2DRowRealMatrix(featureMatrix.getData());
            this._bicValue = bicValue;
            AddSegment(s);
        }

        public SpeakerCluster(SpeakerCluster c)
        {
            _segmentSet = new SortedSet<Segment>();
            FeatureMatrix = new Array2DRowRealMatrix(c.FeatureMatrix.getData());
            var it = c._segmentSet.GetEnumerator();
            while (it.MoveNext())
                AddSegment(it.Current);
        }

        public SortedSet<Segment> GetSegments()
        {
            return _segmentSet;
        }

        public List<Segment> GetArrayOfSegments()
        {
            var it = _segmentSet.GetEnumerator();
            List<Segment> ret = new List<Segment>();
            while (it.MoveNext())
                ret.Add(it.Current);
            return ret;
        }

        public bool AddSegment(Segment s)
        {
            return _segmentSet.Add(s);
        }

        public bool RemoveSegment(Segment s)
        {
            return _segmentSet.Remove(s);
        }

        /**
         * Returns a 2 * n length array where n is the numbers of intervals assigned
         * to the speaker modeled by this cluster every pair of elements with
         * indexes (2 * i, 2 * i + 1) represents the start time and the length for
         * each interval
         * 
         * We may need a delay parameter to this function because the segments may
         * not be exactly consecutive
         */
        public List<Segment> GetSpeakerIntervals()
        {
            var it = _segmentSet.GetEnumerator();

            Segment curent = new Segment(0, 0);
            it.MoveNext(); //Added because it.Current will return null if no MoveNext is called.
            var previous = it.Current;
            int start = previous.StartTime;
            int length = previous.Length;
            int idx = 0;
            var ret = new List<Segment> {previous};
            while (it.MoveNext())
            {
                curent = it.Current;
                start = ret[idx].StartTime;
                length = ret[idx].Length;
                if ((start + length) == curent.StartTime)
                {
                    ret.Set(idx, new Segment(start, length + curent.Length));
                }
                else
                {
                    idx++;
                    ret.Add(curent);
                }
                previous = curent;
            }
            return ret;
        }

        public void MergeWith(SpeakerCluster target)
        {
            if (target == null)
                throw new NullReferenceException();
            var it = target._segmentSet.GetEnumerator();
            while (it.MoveNext())
            {
                if (!AddSegment(it.Current))
                    Console.WriteLine("Something doesn't work in mergeWith method, Cluster class");
            }
            int rowDim = FeatureMatrix.getRowDimension() + target.FeatureMatrix.getRowDimension();
            int colDim = FeatureMatrix.getColumnDimension();
            var combinedFeatures = new Array2DRowRealMatrix(rowDim, colDim);
            combinedFeatures.setSubMatrix(FeatureMatrix.getData(), 0, 0);
            combinedFeatures
                    .setSubMatrix(target.FeatureMatrix.getData(), FeatureMatrix.getRowDimension(), 0);
            _bicValue = SpeakerIdentification.GetBICValue(combinedFeatures);
            FeatureMatrix = new Array2DRowRealMatrix(combinedFeatures.getData());
        }
    }

}
