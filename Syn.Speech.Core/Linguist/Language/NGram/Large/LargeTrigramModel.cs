using System;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    /// A wrapper for LargeNGramModel base on the old LargeTrigramModel class. 
    /// @author Anthony Rousseau, LIUM
    /// </summary>
    public class LargeTrigramModel : LargeNGramModel
    {
        /// <summary>
        /// The property that defines that maximum number of trigrams to be cached.
        /// </summary>
        [S4Integer(DefaultValue = 100000)]
        public static string PropTrigramCacheSize = "trigramCacheSize";

        /**
        /// @param format
        /// @param urlLocation
        /// @param ngramLogFile
        /// @param maxTrigramCacheSize
        /// @param maxBigramCacheSize
        /// @param clearCacheAfterUtterance
        /// @param maxDepth
        /// @param dictionary
        /// @param applyLanguageWeightAndWip
        /// @param languageWeight
        /// @param wip
        /// @param unigramWeight
        /// @param fullSmear
         */
        public LargeTrigramModel(String format, URL urlLocation,
                string ngramLogFile, int maxTrigramCacheSize,
                int maxBigramCacheSize, Boolean clearCacheAfterUtterance,
                int maxDepth, IDictionary dictionary,
                Boolean applyLanguageWeightAndWip, float languageWeight,
                double wip, float unigramWeight, Boolean fullSmear) 
            :base(format, urlLocation, ngramLogFile, maxTrigramCacheSize,
                    clearCacheAfterUtterance, (maxDepth > 3 ? 3
                            : maxDepth), dictionary,
                    applyLanguageWeightAndWip, languageWeight, wip, unigramWeight,
                    fullSmear)
        {
            // Inline conditional statement to prevent maxDepth being > to 3
            // We are in a Trigram wrapper, after all
            
        }


        public LargeTrigramModel() 
        {

        }

        public override void NewProperties(PropertySheet ps)
        {
            Location = ConfigurationManagerUtils.GetResource(PropLocation, ps);
            NgramLogFile = ps.GetString(PropQueryLogFile);
            ClearCacheAfterUtterance = ps.GetBoolean(PropClearCachesAfterUtterance);
            MaxDepth = ps.GetInt(PropMaxDepth);
            NgramCacheSize = ps.GetInt(PropTrigramCacheSize);
            Dictionary = (IDictionary) ps.GetComponent(PropDictionary);
            ApplyLanguageWeightAndWip = ps.GetBoolean(PropApplyLanguageWeightAndWip);
            LanguageWeight = ps.GetFloat(PropLanguageWeight);
            Wip = ps.GetDouble(PropWordInsertionProbability);
            UnigramWeight = ps.GetFloat(PropUnigramWeight);
            FullSmear = ps.GetBoolean(PropFullSmear);
        }
    }
}
