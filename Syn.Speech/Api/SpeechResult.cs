using System;
using System.Collections.Generic;
using Syn.Speech.Common;
using Syn.Speech.Result;
//PATROLLED
namespace Syn.Speech.Api
{
    /// <summary>
    /// High-level wrapper for {@link Result} instance.
    /// </summary>
    public sealed class SpeechResult: ISpeechResult
    {
        private IResult result;
        private Lattice lattice;


        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechResult"/> class.
        /// </summary>
        /// <param name="result">Recognition result returned by <see cref="Recognizer.Recognizer"/>.</param>
        public SpeechResult(IResult result) 
        {
            this.result = result;
            lattice = new Lattice(result);
            new LatticeOptimizer(lattice).optimize();
        }

        /// <summary>
        /// Returns {@link List} of words of the recognition result.
        /// Within the list words are ordered by time frame.
        /// </summary>
        /// <returns>words that form the result</returns>
        public List<WordResult> getWords() 
        {
            return result.getTimedBestResult(true);
        }

        /// <summary>
        /// Returns string representation of the result.
        /// </summary>
        /// <returns></returns>
        public String getHypothesis() 
        {
	        return result.getBestResultNoFiller();
        }

        /// <summary>
        /// Return N best hypothesis.
        /// </summary>
        /// <param name="n">n number of hypothesis to return.</param>
        /// <returns>List of several best hypothesis</returns>
        public List<String> getNbest(int n) 
        {
            return new Nbest(lattice).getNbest(n);
        }

        /// <summary>
        /// Returns lattice for the recognition result.
        /// </summary>
        /// <returns>lattice object</returns>
        public Lattice getLattice() {
            return lattice;
        }
    
        /// <summary>
        /// Return Result object of current SpeechResult
        /// </summary>
        /// <returns>Result object stored in this.result</returns>
        public IResult getResult() {
    	    return result;
        }
    }
}
