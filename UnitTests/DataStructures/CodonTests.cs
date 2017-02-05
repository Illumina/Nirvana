using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class CodonTests
    {
        [Fact]
        public void DesiredCodons()
        {
            // A|frameshift_variant|HIGH|PDGFRA|ENSG00000134853|Transcript|ENST00000257290|protein_coding|12/23||ENST00000257290.5:c.1694_1695insA|ENSP00000257290.5:p.Ser566GlnfsTer6|2025-2026|1694-1695|565|I/IX|atc/atAc|||1|HGNC|8803|YES|CCDS3495.1|ENSP00000257290|||hmmpanther:PTHR24416:SF52&hmmpanther:PTHR24416&Gene3D:3.30.200.20&PIRSF_domain:PIRSF500950&PIRSF_domain:PIRSF000615|||||
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000257290_chr4_Ensembl84"),
                "chr4\t55141048\t.\tT\tTA\t1000\tPASS\tC\t.", "ENST00000257290", "A");
            Assert.NotNull(transcriptAllele);

            Assert.Equal("2025-2026", transcriptAllele.ComplementaryDnaPosition);
            Assert.Equal("1694-1695", transcriptAllele.CdsPosition);
            Assert.Equal("565",       transcriptAllele.ProteinPosition);
            Assert.Equal("12/23",     transcriptAllele.Exons);
            Assert.Equal("atc/atAc",  transcriptAllele.Codons);
            Assert.Equal("I/IX",      transcriptAllele.AminoAcids);
        }
    }
}