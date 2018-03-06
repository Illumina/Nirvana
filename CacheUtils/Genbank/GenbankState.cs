namespace CacheUtils.Genbank
{
    internal enum GenbankState : byte
    {
        Header,
        Features,
        Origin
    }

    internal enum FeaturesState : byte
    {
        Unknown,
        Cds,
        Exon,
        Gene
    }
}
