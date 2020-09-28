using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.SpeakerId
{
   
public class Segment : IComparable<Segment> {
    public const int FeaturesSize = 13;

    public const int FrameLength = 10;

    public Segment(Segment @ref) {
        StartTime = @ref.StartTime;
        Length = @ref.Length;
    }

    public Segment(int startTime, int length) {
        StartTime = startTime;
        Length = length;
    }

    public Segment(int startTime, int length, float[] features) {
        StartTime = startTime;
        Length = length;
    }

    public Segment() {
        StartTime = Length = 0;
    }

    public int StartTime { get; set; }

    public int Length { get; set; }

    public int Equals(Segment @ref) {
        return (StartTime == @ref.StartTime) ? 1 : 0;
    }

    public override string ToString() {
        return StartTime + " " + Length + "\n";
    }

    public int CompareTo(Segment @ref) {
        return (StartTime - @ref.StartTime);
    }
}

}
