namespace VariantAnnotation.DataStructures
{
    // ReSharper disable InconsistentNaming
    public enum GeneSymbolSource : byte
    {
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
    }
    // ReSharper restore InconsistentNaming
}
