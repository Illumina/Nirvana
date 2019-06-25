using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.NSA;
using Xunit;

namespace UnitTests.VariantAnnotation.NSA
{
    public sealed class SuppAnnotationsOutputTests
    {
        [Fact]
        public void Output_positional_not_array()
        {
            var sa = new SupplementaryAnnotation("Anno", false, true, "pathogenic", null);

            var sb = new StringBuilder();
            sa.SerializeJson(sb);
            Assert.Equal("pathogenic", sb.ToString());
        }

        [Fact]
        public void Output_not_positional_not_array()
        {
            var sa = new SupplementaryAnnotation("alleleFreq", false, false, "pathogenic", null);

            var sb = new StringBuilder();
            sa.SerializeJson(sb);
            Assert.Equal("{pathogenic}", sb.ToString());
        }

        [Fact]
        public void Output_not_positional_array()
        {
            //e.g. clinvar
            var sa = new SupplementaryAnnotation("spliceAi", true, false, null, new []{"likely pathogenic", "unknown pathogenicity"});

            var sb = new StringBuilder();
            sa.SerializeJson(sb);
            Assert.Equal("[{likely pathogenic},{unknown pathogenicity}]", sb.ToString());
        }

        [Fact]
        public void Output_emptyJsonStrings_array()
        {
            Assert.Throws<UserErrorException>(()=>new SupplementaryAnnotation("svAnno", true, true, "pathogenic", null));
        }
        [Fact]
        public void Output_emptyJsonString_not_array()
        {
            Assert.Throws<UserErrorException>(() => new SupplementaryAnnotation("svAnno", false, true, null, new []{"pathogenic"}));
        }
    }
}