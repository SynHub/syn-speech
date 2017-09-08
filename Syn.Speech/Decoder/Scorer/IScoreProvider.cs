using Syn.Speech.Common.FrontEnd;
//PATROLLED
namespace Syn.Speech.Decoder.Scorer
{
    /// <summary>
    /// Thing that can provide the score
    /// </summary>
    public interface ScoreProvider
    {
        /// <summary>
        /// Provides the score.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>the score</returns>
        float getScore(IData data);

        /// <summary>
        /// Provides component score
        /// </summary>
        /// <param name="feature">The feature.</param>
        /// <returns>the score</returns>
        float[] getComponentScore(IData feature);
    }
}
