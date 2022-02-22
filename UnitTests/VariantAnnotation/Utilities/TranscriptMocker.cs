using Cache.Data;
using Genome;
using UnitTests.TestUtilities;

namespace UnitTests.VariantAnnotation.Utilities;

public static class TranscriptMocker
{
    public static Transcript GetTranscript(bool onReverseStrand, TranscriptRegion[] regions, CodingRegion codingRegion,
        int start, int end)
    {
        var gene = new Gene(string.Empty, string.Empty, onReverseStrand, null);

        return new Transcript(ChromosomeUtilities.Chr1, start, end, string.Empty, BioType.mRNA, false, Source.RefSeq,
            gene, regions, string.Empty, codingRegion);
    }

    public static Transcript GetTranscript(bool onReverseStrand, string geneSymbol, TranscriptRegion[] regions,
        CodingRegion codingRegion, Chromosome chromosome, int start, int end, Source source, string transcriptId)
    {
        var gene = new Gene(string.Empty, string.Empty, onReverseStrand, null) {Symbol = geneSymbol};

        return new Transcript(chromosome, start, end, transcriptId, BioType.mRNA, false, source, gene, regions,
            string.Empty, codingRegion);
    }
}