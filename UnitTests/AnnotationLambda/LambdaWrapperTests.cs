using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3.Model;
using AnnotationLambda;
using Cloud;
using Compression.Utilities;
using Genome;
using IO;
using Moq;
using Tabix;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.AnnotationLambda
{
    public sealed class LambdaWrapperTests
    {
        private readonly Dictionary<string, IChromosome> _refNameToChromosome = new Dictionary<string, IChromosome> { ["chr22"] = new Chromosome("chr22", "22", 21) };

        private readonly S3Path _testInputS3Path = new S3Path
        {
            bucketName = "test",
            path = "input.vcf.gz"
        };

        [Fact]
        public void GetTabixVirtualPosition_AsExpected()
        {
            var tabixS3Path = new S3Path
            {
                bucketName = _testInputS3Path.bucketName,
                path = _testInputS3Path.path + NirvanaHelper.TabixSuffix
            };

            var annotationConfig = new AnnotationConfig
            {
                inputVcf = _testInputS3Path,
                annotationRange = new AnnotationRange
                {
                    chromosome = "chr22",
                    start = 20_000_000,
                    end = 30_000_000
                }
            };

            var tabixPath = Resources.TopPath("Mother_chr22.genome.vcf.gz.tbi");
            var tabixStream = FileUtilities.GetReadStream(tabixPath);
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.Setup(x => x.GetStream(
                It.Is<S3Path>(i => i.bucketName == tabixS3Path.bucketName && i.path == tabixS3Path.path),
                It.Is<ByteRange>(z => z == null))).Returns(tabixStream);

            var indexReader = GZipUtilities.GetAppropriateBinaryReader(tabixPath);
            var expectedPosition = Reader.Read(indexReader, _refNameToChromosome).GetOffset(new Chromosome("chr22", "22", 21), annotationConfig.annotationRange.start);

            var virtualPosition = LambdaWrapper.GetTabixVirtualPosition(annotationConfig, s3ClientMock.Object, _refNameToChromosome);

            Assert.Equal(expectedPosition, virtualPosition);
        }

        [Fact]
        public void GetTabixVirtualPostion_ReturnZeroWhenNoRangeSpecified()
        {
            var annotationConfig = new AnnotationConfig
            {
                inputVcf = _testInputS3Path,
                annotationRange = null
            };

            Assert.Equal(0, LambdaWrapper.GetTabixVirtualPosition(annotationConfig, null, null));
        }

    }
}