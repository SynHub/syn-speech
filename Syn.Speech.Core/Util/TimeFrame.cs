using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    public class TimeFrame
    {
        public static TimeFrame Null = new TimeFrame(0);
        public static TimeFrame Infinite = new TimeFrame(JLong.MAX_VALUE);

        public TimeFrame(long duration) 
            :this(0, duration)
        {
            
        }

        public TimeFrame(long start, long end) 
        {
            Start = start;
            End = end;
        }

        public long Start { get; private set; }

        public long End { get; private set; }

        public long Length
        {
            get { return End - Start; }
        }

        public override string ToString() 
        {
            return String.Format("{0:d}:{1:d}", Start, End);
        }
    }
}
