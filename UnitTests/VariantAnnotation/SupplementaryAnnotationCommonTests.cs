using System.Collections.Generic;
using System.Linq;
using UnitTests.TestUtilities;
using VariantAnnotation;
using VariantAnnotation.Interface.Sequence;
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

	    [Fact]
	    public void GetGenomeAssembly_grch37()
	    {
		    var grch37SaDir = Resources.SaGRCh37("");
		    var genomeAssembly = SaReaderUtils.GetGenomeAssembly(new List<string> {grch37SaDir});

			Assert.Equal(GenomeAssembly.GRCh37, genomeAssembly);
	    }

	    [Fact]
	    public void GetDataSourceVersions_grch37()
	    {
		 
			var grch37SaDir = Resources.SaGRCh37("");
			var dataSourceVersions = SaReaderUtils.GetDataSourceVersions(new List<string> { grch37SaDir });

			Assert.True(dataSourceVersions.Any());

		}
    }
}