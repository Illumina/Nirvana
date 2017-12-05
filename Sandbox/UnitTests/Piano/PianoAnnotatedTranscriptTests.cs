using System;
using System.Collections.Generic;
using Moq;
using Piano;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using Xunit;

namespace UnitTests
{
    public class PianoAnnotatedTranscriptTests
    {
        [Fact]
        public void Empty_upstreamAminoAcids_return_dot()
        {
            var mockedTranscript = new Mock<ITranscript>();
            mockedTranscript.Setup(x => x.Source).Returns(Source.Ensembl);
            mockedTranscript.Setup(x => x.Gene.EnsemblId.ToString()).Returns("ENSG12345");
            mockedTranscript.Setup(x => x.Gene.Symbol).Returns("TestGene");
            mockedTranscript.Setup(x => x.Id).Returns(CompactId.Convert("ENST124"));
            mockedTranscript.Setup(x => x.Version).Returns(1);
            mockedTranscript.Setup(x => x.Translation.ProteinId).Returns(CompactId.Convert("ENSP123456"));
            mockedTranscript.Setup(x => x.Translation.ProteinVersion).Returns(2);



            var mappedPosition = new Mock<IMappedPositions>();
            mappedPosition.Setup(x => x.ProteinInterval).Returns(new NullableInterval(100, 100));
            var transcript = new PianoAnnotatedTranscript(mockedTranscript.Object, "A", "R",mappedPosition.Object, "", "ATYRGD",
                new List<ConsequenceTag> {ConsequenceTag.missense_variant});
            var expectedOut = "TestGene	ENSG12345	ENST124.1	ENSP123456.2	100	.	A/R	ATYRGD	missense_variant";

            Assert.Equal(expectedOut,transcript.ToString());
        }

        [Fact]
        public void refSeq_gene_return_entrezId()
        {
            var mockedTranscript = new Mock<ITranscript>();
            mockedTranscript.Setup(x => x.Source).Returns(Source.RefSeq);
            mockedTranscript.Setup(x => x.Gene.EntrezGeneId.ToString()).Returns("12345");
            mockedTranscript.Setup(x => x.Gene.Symbol).Returns("TestGene");
            mockedTranscript.Setup(x => x.Id).Returns(CompactId.Convert("NM_124"));
            mockedTranscript.Setup(x => x.Version).Returns(1);
            mockedTranscript.Setup(x => x.Translation.ProteinId).Returns(CompactId.Convert("NP_342"));
            mockedTranscript.Setup(x => x.Translation.ProteinVersion).Returns(2);



            var mappedPosition = new Mock<IMappedPositions>();
            mappedPosition.Setup(x => x.ProteinInterval).Returns(new NullableInterval(100, 101));
            var transcript = new PianoAnnotatedTranscript(mockedTranscript.Object, "AT", "GR", mappedPosition.Object, "KILGF", "ATYRGD",
                new List<ConsequenceTag> { ConsequenceTag.missense_variant ,ConsequenceTag.splice_region_variant});
            var expectedOut = "TestGene	12345	NM_124.1	NP_342.2	100-101	KILGF	AT/GR	ATYRGD	missense_variant,splice_region_variant";

            Assert.Equal(expectedOut, transcript.ToString());
        }
    }
}