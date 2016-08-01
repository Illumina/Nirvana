namespace VariantAnnotation.DataStructures
{
    public enum TranscriptDataSource : byte
    {
        None, // TODO: Remove this, but only after we create new cache files
        RefSeq,
        Ensembl,
        BothRefSeqAndEnsembl
    }
}
