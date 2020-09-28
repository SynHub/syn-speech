using System;
using System.Diagnostics;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds.Feature
{
    /// <summary>
    /// Implements a linear feature transformation transformation.
    ///
    /// It might be a dimension reduction or just a decorrelation transform. This
    /// component requires a special model trained with LDA/MLLT transform.
    /// </summary>
    public class FeatureTransform: BaseDataProcessor
    {
        /// <summary>
        /// The name of the transform matrix file.
        /// </summary>
        [S4Component(Type = typeof(ILoader))]
        public static string PropLoader = "loader";

        float[][] _transform;
        protected ILoader Loader;

        int rows;
        int values;

        public FeatureTransform(ILoader loader) 
        {
            Init(loader);
        }

        public FeatureTransform() {
        }

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            Init((ILoader) ps.GetComponent(PropLoader));
        }

        private void Init(ILoader loader) 
        {
            Loader = loader;

            try {
                loader.Load();
            } 
            catch (Exception e) 
            {
                Trace.TraceError(e.ToString());
            }

            _transform = loader.TransformMatrix;
        }

        /// <summary>
        /// Returns the next Data object being processed by this LDA, or if it is a Signal, it is returned without modification.
        /// </summary>
        /// <returns>
        /// The next available Data object, returns null if no Data object is available.
        /// </returns>
        /// <exception cref="System.ArgumentException">dimenstion mismatch</exception>
        public override IData GetData()
        {
            IData data = Predecessor.GetData();

            if (null == _transform || null == data || !(data is FloatData))
                return data;

            FloatData floatData = (FloatData) data; 
            float[] features = floatData.Values;

            if (features.Length > _transform[0].Length + 1)
                throw new ArgumentException("dimenstion mismatch");

            float[] result = new float[_transform.Length];

            for (int i = 0; i < _transform.Length; ++i) 
            {
                for (int j = 0; j < features.Length; ++j)
                    result[i] += _transform[i][j]*features[j];
            }

            if (features.Length > _transform[0].Length) 
            {
                for (int i = 0; i < _transform.Length; ++i)
                    result[i] += _transform[i][features.Length];
            }

            return new FloatData(result,
                                 floatData.SampleRate,
                                 floatData.FirstSampleNumber);
        }

    }
}
