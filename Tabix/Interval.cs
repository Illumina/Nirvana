namespace Tabix
{
    public struct Interval
    {
        public readonly ulong Begin;
        public readonly ulong End;

        public Interval(ulong begin, ulong end)
        {
            Begin = begin;
            End   = end;
        }
    }
}