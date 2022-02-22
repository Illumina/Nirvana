// ReSharper disable InconsistentNaming
namespace IO;

public enum FileType : ushort
{
    // reference
    Reference = 1,

    // cache
    GeneSymbol      = 1000,
    Transcript      = 1100,
    TranscriptIndex = 1200,

    // supplementary annotation
    SaVariants  = 2000,
    SaIntervals = 3000,
    SaGenes     = 4000,
    PhyloP      = 5000,
    SIFT        = 6000,
    PolyPhen    = 6100
}