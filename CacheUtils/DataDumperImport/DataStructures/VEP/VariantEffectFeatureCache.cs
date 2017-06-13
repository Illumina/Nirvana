namespace CacheUtils.DataDumperImport.DataStructures.VEP
{
    public sealed class VariantEffectFeatureCache
    {
        public Intron[] Introns                                      = null;
        public Exon[] Exons                                          = null;
        public ProteinFunctionPredictions ProteinFunctionPredictions = null;
        public TranscriptMapper Mapper                               = null;

        public string Peptide;        
        public string TranslateableSeq;
    }
}
