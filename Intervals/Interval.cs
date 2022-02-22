namespace Intervals;

public struct Interval : IInterval
{
    public int Start { get; }
    public int End   { get; }

    public Interval(int start, int end)
    {
        Start = start;
        End   = end;
    }
    
    public static Interval Empty => new (0, 0);
}