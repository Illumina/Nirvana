namespace SAUtils.DbSnpRemapper
{
    public struct GenomicLocation
    {
        public readonly string Chrom;
        public readonly int Position;
        
        public GenomicLocation(string chrom, int pos)
        {
            Chrom = chrom;
            Position = pos;
        }
    }
}