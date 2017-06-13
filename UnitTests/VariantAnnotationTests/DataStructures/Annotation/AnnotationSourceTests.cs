using System.Collections.Generic;
using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.Annotation
{
    public class AnnotationSourceTests
    {
        [Fact]
        public void AnnotationSourceConstructor()
        {
            var saPath    = Resources.TopPath("DirectoryIntegrity");
            var saPaths   = new List<string> { saPath };
            var cacheStub = Resources.CacheGRCh37("ENST00000579787_chr1_Ensembl84");

            var conservationScoreReader = new PhylopReader(saPaths);

            var transcriptStream = ResourceUtilities.GetReadStream(cacheStub + ".ndb");
            var siftStream       = ResourceUtilities.GetReadStream(cacheStub + ".sift");
            var polyPhenStream   = ResourceUtilities.GetReadStream(cacheStub + ".polyphen");
            var referenceStream  = ResourceUtilities.GetReadStream(cacheStub + ".bases");

            var streams = new AnnotationSourceStreams(transcriptStream, siftStream, polyPhenStream, referenceStream);

            var saProvider = new SupplementaryAnnotationProvider(saPaths);

            var annotationSource = new NirvanaAnnotationSource(streams, saProvider, conservationScoreReader, saPaths);
            Assert.NotNull(annotationSource);
        }
    }
}
