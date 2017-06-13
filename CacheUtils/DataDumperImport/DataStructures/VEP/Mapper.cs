namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class Mapper
    {
        public PairCodingDna PairCodingDna = null; // null
        public PairGenomic PairGenomic     = null; // null
        public string FromType;
        public string ToType;
        public int PairCount;
        public bool IsSorted;

        public override string ToString()
        {
            return
                $"Mapper: from: {FromType}, to: {ToType}. Pair count: {PairCount}. Sorted: {(IsSorted ? "sorted" : "unsorted")}";
        }
    }
}
