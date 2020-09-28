using System;
using System.Collections.Generic;
using Syn.Speech.Decoders.Search;
using Syn.Speech.Helper;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders
{
    public class Decoder: AbstractDecoder
    {
        public Decoder()
        {
            // Keep this or else XML configuration fails.
        }

        /// <summary>
        /// The property for the number of features to recognize at once.
        /// </summary>
        [S4Integer(DefaultValue = Integer.MAX_VALUE)]
        public static string PropFeatureBlockSize = "featureBlockSize";
        private int _featureBlockSize;

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            _featureBlockSize = ps.GetInt(PropFeatureBlockSize);
        }

        public Decoder(ISearchManager searchManager, bool fireNonFinalResults, bool autoAllocate,
            List<IResultListener> resultListeners, int featureBlockSize)
            :base(searchManager, fireNonFinalResults, autoAllocate,resultListeners)
        {
            _featureBlockSize = featureBlockSize;
        }


     
       

        /// <summary>
        /// Decode frames until recognition is complete.
        /// </summary>
        /// <param name="referenceText">referenceText the reference text (or null)</param>
        /// <returns>a result</returns>
        public override Result Decode(String referenceText)
        {
            SearchManager.StartRecognition();
            Result result;
            do
            {
                result = SearchManager.Recognize(_featureBlockSize);
                if (result != null)
                {
                    result.ReferenceText = referenceText;
                    FireResultListeners(result);
                }
            } while (result != null && !result.IsFinal());
            SearchManager.StopRecognition();
            return result;
        }
    }
}
