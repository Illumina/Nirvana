using System.Collections.Generic;
using ErrorHandling.Exceptions;
using Genome;
using SAUtils.Custom;
using Xunit;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class ParserUtilitiesTests
    {
        private readonly HashSet<GenomeAssembly> _allowedGenomeAssemblies = new HashSet<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38};

        [Fact]
        public void ParseTitle_Conflict_JsonTag()
        {

            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseTitle("#title=topmed"));
        }

        [Fact]
        public void ParseTitle_IncorrectFormat()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseTitle("#title:customSA"));
        }

        [Fact]
        public void ParseGenomeAssembly_UnsupportedAssembly_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseGenomeAssembly("#assembly=hg19", _allowedGenomeAssemblies));
        }

        [Fact]
        public void ParseGenomeAssembly_IncorrectFormat_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseGenomeAssembly("#assembly-GRCh38", _allowedGenomeAssemblies));
        }

        [Fact]
        public void CheckPrefix_InvalidPrefix_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.CheckPrefix("invalidPrefix=someValue", "expectedPrefix", "anyRow"));
        }

        [Fact]
        public void ParseTags_LessThanRequiredColumns_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseTags("#CHROM\tPOS\tREF", "#CHROM", 4, "rowNumber"));
        }

        [Theory]
        [InlineData("String")]
        [InlineData("NUMBER")]
        [InlineData("Bool")]
        public void ParseTypes_ValidType_Pass(string type)
        {
            string typeLine = $"#type\t.\t.\t.\t{type}";
            ParserUtilities.ParseTypes(typeLine, 4, 1, "rowNumber");
        }

        [Theory]
        [InlineData("boolean")]
        [InlineData("double")]
        [InlineData("int")]
        public void ParseTypes_InvalidType_ThrowException(string type)
        {
            string typeLine = $"#type\t.\t.\t.\t{type}";
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseTypes(typeLine, 4, 1, "rowNumber"));
        }

        [Fact]
        public void ParseCategories_InvalidValue_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => ParserUtilities.ParseCategories("#categories\tWOW", 1, 1, null, "rowNumber"));
        }
    }
}