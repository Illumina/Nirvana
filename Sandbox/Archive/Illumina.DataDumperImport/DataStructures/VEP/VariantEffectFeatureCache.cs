namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class VariantEffectFeatureCache
    {
        public Intron[] Introns                                      = null; // null
        public Exon[] Exons                                          = null; // null
        public ProteinFunctionPredictions ProteinFunctionPredictions = null; // null
        public TranscriptMapper Mapper                               = null; // null

        public string Peptide; // null        
        public string TranslateableSeq;
    }
}
