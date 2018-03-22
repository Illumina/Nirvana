namespace CacheUtils.DataDumperImport.DataStructures
{
    public enum GeneSymbolSource : byte
    {
        // ReSharper disable InconsistentNaming
        Unknown,
        CloneBasedEnsemblGene,
        CloneBasedVegaGene,
        EntrezGene,
        HGNC,
        LRG,
        NCBI,
        miRBase,
        RFAM,
        UniProtGeneName
        // ReSharper restore InconsistentNaming
    }
}
