namespace Syn.Speech.Linguist.Language.NGram
{
    /// <summary>
    /// Class for returning results from {@link BackoffLanguageModel} 
    /// </summary>
    public class ProbDepth
    {
        public float probability;
        public int depth;

        public ProbDepth(float probability, int depth)
        {
            this.probability = probability;
            this.depth = depth;
        }
    }
}
