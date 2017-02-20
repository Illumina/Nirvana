using System.Collections.Generic;
using UnitTests.Fixtures;
using UnitTests.Mocks;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class CustomIntervalTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public CustomIntervalTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void AddVariantToJsonOutputTests()
        {
            var customIntervals = new List<ICustomInterval>
            {
                new VariantAnnotation.DataStructures.CustomInterval("chr1", 118165685, 118165692, "Test", null, null)
            };

            var customIntervalProvider = new MockCustomIntervalProvider(customIntervals, _renamer);
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000006101_chr17_Ensembl84"), null, null, customIntervalProvider);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "chr1	118165691	rs1630312	C	T	156.00	PASS	.	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14");
            var observedResult = annotatedVariant.ToString();

            Assert.Contains("Start", observedResult);
            Assert.Contains("Test", observedResult);
            Assert.DoesNotContain("customIntervals", observedResult);
        }

        [Fact]
        public void DirectoryIntegrityTest()
        {
            var dataSourceVersions = new List<DataSourceVersion>();
            CustomIntervalCommon.CheckDirectoryIntegrity(Resources.Top, dataSourceVersions);
            Assert.Equal(1, dataSourceVersions.Count);
        }

        [Fact]
        public void CutomIntervalCompare()
        {
            var interval1 = new VariantAnnotation.DataStructures.CustomInterval("chr1", 1000, 2000, "test",
                new Dictionary<string, string> {["k1"] = "v1"}, null);
            var interval2 = new VariantAnnotation.DataStructures.CustomInterval("chr1", 1000, 2000, "test",
                new Dictionary<string, string> {["k1"] = "v1"}, null);
            var interval3 = new VariantAnnotation.DataStructures.CustomInterval("chr1", 1010, 2000, "test",
                new Dictionary<string, string> {["k1"] = "v1"}, null);
            var interval4 = new VariantAnnotation.DataStructures.CustomInterval("chr2", 1010, 2000, "test",
                new Dictionary<string, string> {["k1"] = "v1"}, null);

            Assert.True(interval1.Equals(interval2));
            Assert.Equal(-1, interval1.CompareTo(interval3));
            Assert.Equal(-1, interval1.CompareTo(interval4));

            var intervalHash = new HashSet<VariantAnnotation.DataStructures.CustomInterval> { interval4, interval1 };

            Assert.Equal(2, intervalHash.Count);
        }
    }
}