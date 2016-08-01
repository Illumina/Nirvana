using System.Collections.Generic;
using UnitTests.Utilities;
using Xunit;
using DS = VariantAnnotation.DataStructures;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class CustomIntervalTests
    {
        [Fact]
        public void AddVariantToJsonOutputTests()
        {
            var customIntervals = new List<DS.CustomInterval>
            {
                new DS.CustomInterval("chr1", 118165685, 118165692, "Test", null, null)
            };

            var annotationSource = ResourceUtilities.GetAnnotationSource(null);
            annotationSource.AddCustomIntervals(customIntervals);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "chr1	118165691	rs1630312	C	T	156.00	PASS	.	GT:GQ:GQX:DP:DPF:AD	0/1:169:155:27:0:13,14");

            Assert.Contains("Start", annotatedVariant.ToString());
            Assert.Contains("Test", annotatedVariant.ToString());
            Assert.DoesNotContain("customIntervals", annotatedVariant.ToString());
        }
    }
}