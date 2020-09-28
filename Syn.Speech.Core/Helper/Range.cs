namespace Syn.Speech.Helper
{
    public class Range
    {
        private readonly int start;
        private readonly int end;

        public Range(int start, int end)
        {
            this.start = start;
            this.end = end;
        }

        public bool contains(int shift)
        {
            return shift >= start && shift < end;
        }

        public int lowerEndpoint()
        {
            return start;
        }

        public int upperEndpoint()
        {
            return end;
        }
    }
}
