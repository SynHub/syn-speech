using System;
using System.Collections.Generic;
using Syn.Speech.Logging;
using Syn.Speech.FrontEnds;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture.Comparer;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate.Tiedmixture
{
    /// <summary>
    /// MixtureComponentsSet - phonetically tied set of gaussians
    /// </summary>
    public class MixtureComponentSet
    {
        private int _scoresQueueLen;
        private bool _toStoreScore;
        private readonly LinkedList<MixtureComponentSetScores> _storedScores;
        MixtureComponentSetScores _curScores;

        private readonly List<PrunableMixtureComponent[]> _components;
        private readonly List<PrunableMixtureComponent[]> _topComponents;
        private readonly int _numStreams;
        private long _gauCalcSampleNumber;

        public MixtureComponentSet(List<PrunableMixtureComponent[]> components, int topGauNum)
        {
            _components = components;
            _numStreams = components.Count;
            TopGauNum = topGauNum;
            GauNum = components[0].Length;
            _topComponents = new List<PrunableMixtureComponent[]>();
            for (var i = 0; i < _numStreams; i++)
            {
                var featTopComponents = new PrunableMixtureComponent[topGauNum];
                for (var j = 0; j < topGauNum; j++)
                    featTopComponents[j] = components[i][j];
                _topComponents.Add(featTopComponents);
            }
            _gauCalcSampleNumber = -1;
            _toStoreScore = false;
            _storedScores = new LinkedList<MixtureComponentSetScores>();
            _curScores = null;
        }

        private void StoreScores(MixtureComponentSetScores scores)
        {
            _storedScores.Add(scores);
            while (_storedScores.Count > _scoresQueueLen)
                _storedScores.Poll();
        }

        private MixtureComponentSetScores GetStoredScores(long frameFirstSample)
        {
            if (_storedScores.IsEmpty())
                return null;
            if (_storedScores.PeekLast().FrameStartSample < frameFirstSample)
                //new frame
                return null;
            foreach (var scores in _storedScores)
            {
                if (scores.FrameStartSample == frameFirstSample)
                    return scores;
            }
            //Failed to find score. Seems it wasn't calculated yet
            return null;
        }

        private MixtureComponentSetScores CreateFromTopGau(long firstFrameSample)
        {
            var scores = new MixtureComponentSetScores(_numStreams, TopGauNum, firstFrameSample);
            for (var i = 0; i < _numStreams; i++)
            {
                for (var j = 0; j < TopGauNum; j++)
                {
                    scores.SetScore(i, j, _topComponents[i][j].StoredScore);
                    scores.SetGauId(i, j, _topComponents[i][j].Id);
                }
            }
            return scores;
        }

        private static void InsertTopComponent(PrunableMixtureComponent[] topComponents, PrunableMixtureComponent component)
        {
            int i;
            for (i = 0; i < topComponents.Length - 1; i++)
            {
                if (component.PartialScore < topComponents[i].PartialScore)
                {
                    topComponents[i - 1] = component;
                    return;
                }
                topComponents[i] = topComponents[i + 1];
            }
            if (component.PartialScore < topComponents[topComponents.Length - 1].PartialScore)
                topComponents[topComponents.Length - 2] = component;
            else
                topComponents[topComponents.Length - 1] = component;
        }

        public bool IsInTopComponents(PrunableMixtureComponent[] topComponents, PrunableMixtureComponent component)
        {
            foreach (var topComponent in topComponents)
                if (topComponent.Id == component.Id)
                    return true;
            return false;
        }

        private void UpdateTopScores(float[] featureVector)
        {
            var step = featureVector.Length / _numStreams;

            var streamVector = new float[step];
            for (var i = 0; i < _numStreams; i++)
            {
                Array.Copy(featureVector, i * step, streamVector, 0, step);
                var featTopComponents = _topComponents[i];
                var featComponents = _components[i];

                //update scores in top gaussians from previous frame
                foreach (var topComponent in featTopComponents)
                    topComponent.UpdateScore(streamVector);
                Array.Sort(featTopComponents, _componentComparator);

                //Check if there is any gaussians that should float into top
                var threshold = featTopComponents[0].PartialScore;
                foreach (var component in featComponents)
                {
                    if (IsInTopComponents(featTopComponents, component))
                        continue;
                    if (component.IsTopComponent(streamVector, threshold))
                    {
                        InsertTopComponent(featTopComponents, component);
                        threshold = featTopComponents[0].PartialScore;
                    }
                }
            }
        }

        public void UpdateTopScores(IData feature)
        {

            if (feature is DoubleData)
                Console.WriteLine("DoubleData conversion required on mixture level!");

            var firstSampleNumber = FloatData.ToFloatData(feature).FirstSampleNumber;
            if (_toStoreScore)
            {
                _curScores = GetStoredScores(firstSampleNumber);
            }
            else
            {
                if (_curScores != null && _curScores.FrameStartSample != firstSampleNumber)
                    _curScores = null;
            }
            if (_curScores != null)
                //component scores for this frame was already calculated
                return;
            var featureVector = FloatData.ToFloatData(feature).Values;
            UpdateTopScores(featureVector);
            //store just calculated score in list
            _curScores = CreateFromTopGau(firstSampleNumber);
            if (_toStoreScore)
                StoreScores(_curScores);
        }


        private void UpdateScores(float[] featureVector)
        {
            var step = featureVector.Length / _numStreams;
            var streamVector = new float[step];
            for (var i = 0; i < _numStreams; i++)
            {
                Array.Copy(featureVector, i * step, streamVector, 0, step);
                foreach (var component in _components[i])
                {
                    component.UpdateScore(streamVector);
                }
            }
        }



        public void UpdateScores(IData feature)
        {
            if (feature is DoubleData)
                this.LogInfo("DoubleData conversion required on mixture level!");

            var firstSampleNumber = FloatData.ToFloatData(feature).FirstSampleNumber;
            if (_gauCalcSampleNumber != firstSampleNumber)
            {
                var featureVector = FloatData.ToFloatData(feature).Values;
                UpdateScores(featureVector);
                _gauCalcSampleNumber = firstSampleNumber;
            }
        }

        /// <summary>
        /// Should be called on each new utterance to scores for old frames.
        /// </summary>
        public void ClearStoredScores()
        {
            _storedScores.Clear();
        }


        /**
         * How long scores for previous frames should be stored.
         * For fast match this value is lookahead_window_length + 1)
         */
        public void SetScoreQueueLength(int scoresQueueLen)
        {
            _toStoreScore = scoresQueueLen > 0;
            _scoresQueueLen = scoresQueueLen;
        }

        public int TopGauNum { get; private set; }

        public int GauNum { get; private set; }

        public float GetTopGauScore(int streamId, int topGauId)
        {
            return _curScores.GetScore(streamId, topGauId);
        }

        public int GetTopGauId(int streamId, int topGauId)
        {
            return _curScores.GetGauId(streamId, topGauId); 
        }

        public float GetGauScore(int streamId, int topGauId)
        {
            return _components[streamId][topGauId].StoredScore;
        }

        public int GetGauId(int streamId, int topGauId)
        {
            return _components[streamId][topGauId].Id;
        }

        private static T[] Concatenate<T>(T[] A, T[] B)
        {
            var aLen = A.Length;
            var bLen = B.Length;

            //@SuppressWarnings("unchecked")
            //T[] C = (T[])Array.newInstance(A.getClass().getComponentType(), aLen + bLen);
            //TODO: CHECK SEMATICS
            var c = (T[])Activator.CreateInstance(A.GetType());
            Array.Copy(A, 0, c, 0, aLen);
            Array.Copy(B, 0, c, aLen, bLen);

            return c;
        }

        protected internal MixtureComponent[] ToArray()
        {
            var allComponents = new PrunableMixtureComponent[0];
            for (var i = 0; i < _numStreams; i++)
                Concatenate(allComponents, _components[i]);
            return allComponents;
        }

        protected internal int Dimension()
        {
            var dimension = 0;
            for (var i = 0; i < _numStreams; i++)
            {
                dimension += _components[i][0].Mean.Length;
            }
            return dimension;
        }

        protected internal int Size()
        {
            var size = 0;
            for (var i = 0; i < _numStreams; i++)
            {
                size += _components[0].Length;
            }
            return size;
        }

        private readonly IComparer<PrunableMixtureComponent> _componentComparator = new ComponentComparer();

    }

}
