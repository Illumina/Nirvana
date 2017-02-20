using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.DataStructures
{
    public class AnnotationSourceTests
    {
        [Fact]
        public void AnnotationSourceConstructor()
        {
            var saPath              = Resources.TopPath("DirectoryIntegrity");
            var cacheStub           = Resources.CacheGRCh37("ENST00000579787_chr1_Ensembl84");
            var customAnnoPath      = Resources.TopPath("DirectoryIntegrity");
            var customIntervalsPath = Resources.TopPath("DirectoryIntegrity");

            var conservationScoreReader = new PhylopReader(saPath);

            var transcriptStream = ResourceUtilities.GetReadStream(cacheStub + ".ndb");
            var siftStream       = ResourceUtilities.GetReadStream(cacheStub + ".sift");
            var polyPhenStream   = ResourceUtilities.GetReadStream(cacheStub + ".polyphen");
            var referenceStream  = ResourceUtilities.GetReadStream(cacheStub + ".bases");

            var streams = new AnnotationSourceStreams(transcriptStream, siftStream, polyPhenStream, referenceStream);

            var customAnnotationProvider = new CustomAnnotationProvider(new[] {customAnnoPath});
            var customIntervalProvider   = new CustomIntervalProvider(new[] { customIntervalsPath });
            var saProvider               = new SupplementaryAnnotationProvider(saPath);

            var annotationSource = new NirvanaAnnotationSource(streams, saProvider, conservationScoreReader,
                customAnnotationProvider, customIntervalProvider, null);
            Assert.NotNull(annotationSource);
        }
    }
}
