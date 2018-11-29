using ErrorHandling.Exceptions;
using Genome;
using Xunit;

namespace UnitTests.Genome
{
    public sealed class GenomeAssemblyHelperTests
    {
        [Theory]
        [InlineData("GRCH37", GenomeAssembly.GRCh37)]
        [InlineData("GRCH38", GenomeAssembly.GRCh38)]
        [InlineData("HG19",   GenomeAssembly.hg19)]
        [InlineData("",       GenomeAssembly.Unknown)]
        [InlineData("RCRS",   GenomeAssembly.rCRS)]
        public void Convert_GenomeAssemblyExists(string s, GenomeAssembly expectedGenomeAssembly)
        {
            var observedResult = GenomeAssemblyHelper.Convert(s);
            Assert.Equal(expectedGenomeAssembly, observedResult);
        }

        [Fact]
        public void Convert_GenomeAssemblyDoesNotExist()
        {
            Assert.Throws<UserErrorException>(delegate
            {
                GenomeAssemblyHelper.Convert("dummy");
            });
        }
    }
}
