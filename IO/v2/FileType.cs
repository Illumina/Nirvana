namespace IO.v2
{
    public enum FileType : ushort
    {
        // reference
        Reference = 1,

        // cache
        GeneSymbol        = 1000,
        Gene              = 1100,
        EnsemblTranscript = 1200,
        RefseqTranscript  = 1300,
        SIFT              = 1400,
        PolyPhen          = 1500,

        // supplementary annotation
        SaVariants     = 2000,
        SaIntervals    = 3000,
        SaGenes        = 4000,
        FusionCatcher  = 4100,
        GeneFusionJson = 4101,
        PhyloP         = 5000,
        GsaWriter      = 6000,
        GsaIndex       = 6500,
    }
}