using System;
using System.Collections.Generic;
using Syn.Speech.Results;
//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    /// <summary>
    /// High-level wrapper for {@link Result} instance.
    /// </summary>
    public sealed class SpeechResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechResult"/> class.
        /// </summary>
        /// <param name="result">Recognition result returned by <see cref="Syn.Speech.Recognizers.Recognizer"/>.</param>
        public SpeechResult(Result result) 
        {
            Result = result;
            Lattice = new Lattice(result);
            new LatticeOptimizer(Lattice).Optimize();
        }

        /// <summary>
        /// Returns {@link List} of words of the recognition result.
        /// Within the list words are ordered by time frame.
        /// </summary>
        /// <returns>words that form the result</returns>
        public List<WordResult> GetWords() 
        {
            return Result.GetTimedBestResult(true);
        }

        /// <summary>
        /// Returns string representation of the result.
        /// </summary>
        /// <returns></returns>
        public string GetHypothesis() 
        {
	        return Result.GetBestResultNoFiller();
        }

        /// <summary>
        /// Return N best hypothesis.
        /// </summary>
        /// <param name="n">n number of hypothesis to return.</param>
        /// <returns>List of several best hypothesis</returns>
        public ICollection<String> GetNbest(int n) 
        {
            return new Nbest(Lattice).GetNbest(n);
        }

        /// <summary>
        /// Returns lattice for the recognition result.
        /// </summary>
        /// <value>lattice object</value>
        public Lattice Lattice { get; private set; }

        /// <summary>
        /// Return Result object of current SpeechResult
        /// </summary>
        /// <value>Result object stored in this.result</value>
        public Result Result { get; private set; }
    }
}
