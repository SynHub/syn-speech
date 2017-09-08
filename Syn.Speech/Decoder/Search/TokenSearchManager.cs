using System;
using Syn.Speech.Util.Props;
//PATROLLED
namespace Syn.Speech.Decoder.Search
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TokenSearchManager:ISearchManager
    {
        /// <summary>
        /// The property that specifies whether to build a word lattice. 
        /// </summary>
        [S4Boolean(defaultValue = true)]
        public static String PROP_BUILD_WORD_LATTICE = "buildWordLattice";

        /// <summary>
        /// The property that controls whether or not we keep all tokens. If this is
        /// set to false, only word tokens are retained, otherwise all tokens are
        /// retained.
        /// </summary>
        [S4Boolean(defaultValue = false)]
        public static String PROP_KEEP_ALL_TOKENS = "keepAllTokens";

        protected Boolean buildWordLattice=false;
        protected Boolean keepAllTokens=false;

        void IConfigurable.newProperties(PropertySheet ps)
        {
            buildWordLattice = ps.getBoolean(PROP_BUILD_WORD_LATTICE);
            keepAllTokens = ps.getBoolean(PROP_KEEP_ALL_TOKENS);
            //newProperties(ps);
        }

        /// </summary>
        ///Find the token to use as a predecessor in resultList given a candidate
        ///predecessor. There are three cases here:
        ///
        ///<ul>
        ///<li>We want to store everything in resultList. In that case
        ///{@link #keepAllTokens} is set to true and we just store everything that
        ///was built before.
        ///<li>We are only interested in sequence of words. In this case we just
        ///keep word tokens and ignore everything else. In this case timing and
        ///scoring information is lost since we keep scores in emitting tokens.
        ///<li>We want to keep words but we want to keep scores to build a lattice
        ///from the result list later and {@link #buildWordLattice} is set to true.
        ///In this case we want to insert intermediate token to store the score and
        ///this token will be used during lattice path collapse to get score on
        ///edge. See {@link edu.cmu.sphinx.result.Lattice} for details of resultList
        ///compression.
        ///</ul>
        ///
        ///@param token
        ///           the token of interest
        ///@return the immediate successor word token
        /// </summary>
        protected Token getResultListPredecessor(Token token) 
        {

            if (keepAllTokens) {
                return token;
            }

            if(!buildWordLattice) {
                if (token.isWord())
                    return token;
                else
                    return token.getPredecessor();
            }

            float logAcousticScore = 0.0f;
            float logLanguageScore = 0.0f;
            float logInsertionScore = 0.0f;

            while (token != null && !token.isWord()) {
                logAcousticScore += token.getAcousticScore();
                logLanguageScore += token.getLanguageScore();
                logInsertionScore += token.getInsertionScore();
                token = token.getPredecessor();
            }

            return new Token(token, token.getScore(), logInsertionScore, logAcousticScore, logLanguageScore);
        }

        //void allocate()
        //{
        //    allocate();
        //}

        void ISearchManager.deallocate()
        {
            throw new NotImplementedException();
        }

        virtual public void startRecognition()
        {
            throw new NotImplementedException("startRecognition not implemented in derived class!");
        }

        void ISearchManager.startRecognition()
        {
            startRecognition();
        }

        void ISearchManager.stopRecognition()
        {
            stopRecognition();
        }

        Results.Result ISearchManager.recognize(int nFrames)
        {
            return recognize(nFrames);
        }



        public abstract void newProperties(PropertySheet ps);
        public abstract void allocate();
        public abstract Results.Result recognize(int nFrames);
        public abstract void stopRecognition();
    }
}
