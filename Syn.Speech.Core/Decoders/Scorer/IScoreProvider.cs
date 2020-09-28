using Syn.Speech.FrontEnds;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// Thing that can provide the score
    /// </summary>
    public interface IScoreProvider
    {
        /// <summary>
        /// Provides the score.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>the score</returns>
        float GetScore(IData data);

        /// <summary>
        /// Provides component score
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>the score</returns>
        float[] GetComponentScore(IData feature);
    }
}
