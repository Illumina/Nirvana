using System.Collections.Generic;
using System.IO;
using Cloud.Messages.Annotation;
using Compression.Utilities;
using Genome;
using IO;
using Tabix;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.AnnotationLambda
{
    public sealed class AnnotationLambdaTests
    {
        private readonly Dictionary<string, IChromosome> _refNameToChromosome = new Dictionary<string, IChromosome> { ["chr22"] = new Chromosome("chr22", "22", 21) };

        [Fact]
        public void GetTabixVirtualPosition_AsExpected()
        {
            var annotationConfig = new AnnotationConfig
            {
                vcfUrl = "anywhere/input.vcf.gz",
                tabixUrl = Resources.TopPath("Mother_chr22.genome.vcf.gz.tbi"),
                annotationRange = new AnnotationRange(new AnnotationPosition("chr22", 20_000_000),
                    new AnnotationPosition("chr22", 30_000_000))
            };

            var tabixStream = FileUtilities.GetReadStream(annotationConfig.tabixUrl);
            
            var indexReader = new BinaryReader(GZipUtilities.GetAppropriateReadStream(annotationConfig.tabixUrl));
            var expectedPosition = Reader.Read(indexReader, _refNameToChromosome).GetOffset("chr22", annotationConfig.annotationRange.Start.Position);

            var virtualPosition = global::AnnotationLambda.AnnotationLambda.GetTabixVirtualPosition(annotationConfig.annotationRange, tabixStream, _refNameToChromosome);

            Assert.Equal(expectedPosition, virtualPosition);
        }

        [Fact]
        public void GetTabixVirtualPosition_ReturnZeroWhenNoRangeSpecified()
        {
            Assert.Equal(0, global::AnnotationLambda.AnnotationLambda.GetTabixVirtualPosition(null, null, null));
        }
    }
}