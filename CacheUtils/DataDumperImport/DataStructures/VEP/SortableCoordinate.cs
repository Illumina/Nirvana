namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public class SortableCoordinate
    {
        public readonly ushort ReferenceIndex;
        public readonly int Start;
        public readonly int End;

        protected SortableCoordinate(ushort referenceIndex, int start, int end)
        {
            ReferenceIndex = referenceIndex;
            Start          = start;
            End            = end;
        }
    }
}
