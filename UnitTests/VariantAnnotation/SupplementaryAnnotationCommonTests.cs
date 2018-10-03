using System.Collections.Generic;
using Genome;
using UnitTests.TestUtilities;
using VariantAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation
{

    public sealed class SupplementaryAnnotationCommonTests
    {
        [Theory]
        [InlineData("A","T","T")]
        [InlineData("A", "<DEL>", "<DEL>")]
        [InlineData("TGC","","3")]
        [InlineData("","AGT","iAGT")]
        [InlineData("CC", "AGT", "2AGT")]
        [InlineData("CC", "NNN", "NNN")]
        public void GetReducedAlleleTest(string refAllele, string altAllele, string expectedOut)
        {
            var reducedAllele = SaReaderUtils.GetReducedAllele(refAllele, altAllele);
            Assert.Equal(expectedOut,reducedAllele);
        }

	    
    }
}