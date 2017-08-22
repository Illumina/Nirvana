using System.Collections.Generic;
using Moq;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using VariantAnnotation.TranscriptAnnotation;
using Vcf;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public class ReducedTranscriptAnnotatorTests
    {
        [Fact(Skip ="need mock" )]
        public void CodingRegionOverlap_is_not_genefusion()
        {
            var transcript1 = new Mock<ITranscript>();
            transcript1.SetupGet(x => x.Translation.CodingRegion).Returns(new CdnaCoordinateMap(100, 400, 1, 301));
            transcript1.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            var cdnaMap = new CdnaCoordinateMap(100, 400, 1, 301);
            transcript1.SetupGet(x => x.CdnaMaps).Returns(new[] {(ICdnaCoordinateMap)cdnaMap});

            var transcript2 = new Mock<ITranscript>();
            transcript2.SetupGet(x => x.Translation.CodingRegion).Returns(new CdnaCoordinateMap(350, 900, 1, 451));
            transcript2.SetupGet(x => x.Gene.OnReverseStrand).Returns(false);
            var cdnaMap2 = new CdnaCoordinateMap(200, 900, 1, 701);
            transcript1.SetupGet(x => x.CdnaMaps).Returns(new[] { (ICdnaCoordinateMap)cdnaMap2 });

            var breakEnd = new BreakEnd(new Chromosome("chr1","1",0),new Chromosome("chr1","1",0),200,800,false,true);

            var geneFusion = ReducedTranscriptAnnotator.ComputeGeneFusions(new IBreakEnd[]{ breakEnd}, transcript1.Object,
                new List<ITranscript> {transcript2.Object});

            Assert.Null(geneFusion);

        }
    }
}