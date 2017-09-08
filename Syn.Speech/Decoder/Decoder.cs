using System;
using System.Collections.Generic;
using Syn.Speech.Common;
using Syn.Speech.Decoder.Search;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder
{
    public class Decoder: AbstractDecoder
    {

        /// <summary>
        /// The property for the number of features to recognize at once.
        /// </summary>
        [S4Integer(defaultValue = 100000)]
        public static String PROP_FEATURE_BLOCK_SIZE = "featureBlockSize";
        private int featureBlockSize;


        public Decoder() 
        {
            // Keep this or else XML configuration fails.
        }

        public Decoder(ISearchManager searchManager, bool fireNonFinalResults, bool autoAllocate,
            List<IResultListener> resultListeners, int featureBlockSize):base(searchManager, fireNonFinalResults, autoAllocate,resultListeners)
        {
            this.featureBlockSize = featureBlockSize;
        }


     
        override public void newProperties(PropertySheet ps)
        {
            featureBlockSize = ps.getInt(PROP_FEATURE_BLOCK_SIZE);
        }

        /// <summary>
        /// Decode frames until recognition is complete.
        /// </summary>
        /// <param name="referenceText">referenceText the reference text (or null)</param>
        /// <returns>a result</returns>
        public override Results.Result decode(String referenceText)
        {
            searchManager.startRecognition();
            Results.Result result;
            do
            {
                result = searchManager.recognize(featureBlockSize);
                if (result != null)
                {
                    result.setReferenceText(referenceText);
                    fireResultListeners(result);
                }
            } while (result != null && !result.isFinal());
            searchManager.stopRecognition();
            return result;
        }
    }
}
