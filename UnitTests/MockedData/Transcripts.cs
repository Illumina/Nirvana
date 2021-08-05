using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace UnitTests.MockedData
{
    public static class Transcripts
    {
        // the following 5 transcripts were chosen to stress test our gene fusions:
        public static readonly ITranscript ENST00000290663 = new Transcript(ChromosomeUtilities.Chr1, 43383917,
            43389808, CompactId.Convert("ENST00000290663", 10), Translations.ENST00000290663, BioType.protein_coding,
            Genes.MED8, 1483, 0, true, TranscriptRegions.ENST00000290663, 8, null, 0, 0, Source.Ensembl, false, false,
            null);

        public static readonly ITranscript ENST00000370673 = new Transcript(ChromosomeUtilities.Chr1, 84298366,
            84350798, CompactId.Convert("ENST00000370673", 7), Translations.ENST00000370673, BioType.protein_coding,
            Genes.SAMD13, 1567, 0, false, TranscriptRegions.ENST00000370673, 4, null, 0, 0, Source.Ensembl, false,
            false, null);

        public static readonly ITranscript ENST00000615053 = new Transcript(ChromosomeUtilities.Chr2, 130463799,
            130509287, CompactId.Convert("ENST00000615053", 3), Translations.ENST00000615053, BioType.protein_coding,
            Genes.POTEI, 1926, 0, false, TranscriptRegions.ENST00000615053, 13, null, 0, 0, Source.Ensembl, false,
            false, null);

        public static readonly ITranscript ENST00000347849 = new Transcript(ChromosomeUtilities.Chr2, 130356045,
            130374571, CompactId.Convert("ENST00000347849", 7), Translations.ENST00000347849, BioType.protein_coding,
            Genes.PTPN18, 2472, 0, false, TranscriptRegions.ENST00000347849, 11, null, 0, 0, Source.Ensembl, false,
            false, null);

        // antisense RNA
        public static readonly ITranscript ENST00000427819 = new Transcript(ChromosomeUtilities.Chr1, 85276715,
            85399963, CompactId.Convert("ENST00000427819", 5), null, BioType.antisense_RNA, Genes.AL078459_1, 1950, 0,
            false, TranscriptRegions.ENST00000427819, 5, null, 0, 0, Source.Ensembl, false, false, null);

        // used to test non-AUG start codons
        public static readonly ITranscript NM_001025366_2 = new Transcript(ChromosomeUtilities.Chr6, 43737946, 43754224,
            CompactId.Convert("NM_001025366", 2), Translations.NM_001025366_2, BioType.protein_coding, Genes.VEGFA,
            3662, 0, true, TranscriptRegions.NM_001025366_2, 8, null, 0, 0, Source.RefSeq, false, false,
            RnaEdits.NM_001025366_2);
    }
}