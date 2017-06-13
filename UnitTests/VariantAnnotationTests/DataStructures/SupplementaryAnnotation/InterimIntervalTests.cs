using System.IO;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.Binary;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.SupplementaryAnnotation
{
    public class InterimIntervalTests
    {
        [Fact]
        public void InterimIntervalReaderAndWriterTest()
        {
            var interval = new InterimInterval("testData","chr1",100,200, "\"chromosome\":\"1\",\"begin\":1,\"end\":2300000,\"variantType\":\"copy_number_loss\",\"variantFreqAll\":0.02564,\"id\":\"nsv482937\",\"sampleSize\":39,\"observedLosses\":1", ReportFor.StructuralVariants);
            var ms = new MemoryStream();
            var writer = new ExtendedBinaryWriter(ms);

            interval.Write(writer);
           
            InterimInterval observedInterval;
            ms.Position = 0;
            using (var reader = new ExtendedBinaryReader(ms))
            {
                observedInterval = new InterimInterval(reader);
            }
            writer.Dispose();

            Assert.Equal(0,interval.CompareTo(observedInterval));
            Assert.Equal("testData",observedInterval.KeyName);
            Assert.Equal("chr1",observedInterval.ReferenceName);
            Assert.Equal(100,observedInterval.Start);
            Assert.Equal(200, observedInterval.End);
            Assert.Equal("\"chromosome\":\"1\",\"begin\":1,\"end\":2300000,\"variantType\":\"copy_number_loss\",\"variantFreqAll\":0.02564,\"id\":\"nsv482937\",\"sampleSize\":39,\"observedLosses\":1", observedInterval.JsonString);
            Assert.Equal(ReportFor.StructuralVariants,observedInterval.ReportingFor);
        }
        
        [Fact]
        public void InterimIntervalCompareOnlyrefNameAndStart()
        {
            var interval1 = new InterimInterval("testData1","chr1",100,200,"test1",ReportFor.StructuralVariants);
            var interval2 = new InterimInterval("testData2", "chr1", 100, 300, "test2", ReportFor.StructuralVariants);
            var interval3 = new InterimInterval("testData1", "chr2", 100, 200, "test1", ReportFor.StructuralVariants);
            var interval4 = new InterimInterval("testData4", "chr1", 250, 200, "test4", ReportFor.StructuralVariants);

            Assert.Equal(0,interval1.CompareTo(interval2));
            Assert.Equal(-1, interval1.CompareTo(interval3));
            Assert.Equal(-1,interval1.CompareTo(interval4));
            Assert.Equal(1,interval3.CompareTo(interval4));
        }
        
    }
}