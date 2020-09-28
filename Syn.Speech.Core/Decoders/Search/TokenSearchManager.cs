using System;
using Syn.Speech.Results;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Search
{
    public abstract class TokenSearchManager:ISearchManager
    {
        /// <summary>
        /// The property that specifies whether to build a word lattice. 
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropBuildWordLattice = "buildWordLattice";

        /// <summary>
        /// The property that controls whether or not we keep all tokens. If this is
        /// set to false, only word tokens are retained, otherwise all tokens are
        /// retained.
        /// </summary>
        [S4Boolean(DefaultValue = false)]
        public static string PropKeepAllTokens = "keepAllTokens";

        protected Boolean BuildWordLattice=false;
        protected Boolean KeepAllTokens=false;

        ///<summary>
        ///Find the token to use as a predecessor in resultList given a candidate predecessor.
        /// </summary>
        protected Token GetResultListPredecessor(Token token) 
        {

            if (KeepAllTokens) {
                return token;
            }

            if(!BuildWordLattice)
            {
                if (token.IsWord)
                    return token;
                return token.Predecessor;
            }

            var logAcousticScore = 0.0f;
            var logLanguageScore = 0.0f;
            var logInsertionScore = 0.0f;

            while (token != null && !token.IsWord) {
                logAcousticScore += token.AcousticScore;
                logLanguageScore += token.LanguageScore;
                logInsertionScore += token.InsertionScore;
                token = token.Predecessor;
            }

            return new Token(token, token.Score, logInsertionScore, logAcousticScore, logLanguageScore);
        }

        public abstract void Deallocate();

        virtual public void StartRecognition()
        {
            throw new NotImplementedException("startRecognition not implemented in derived class!");
        }
 

        Result ISearchManager.Recognize(int nFrames)
        {
            return Recognize(nFrames);
        }



        public virtual void NewProperties(PropertySheet ps)
        {
            BuildWordLattice = ps.GetBoolean(PropBuildWordLattice);
            KeepAllTokens = ps.GetBoolean(PropKeepAllTokens);
        }

        public abstract void Allocate();
        public abstract Result Recognize(int nFrames);
        public abstract void StopRecognition();
    }
}
