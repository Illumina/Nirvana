using System;
using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface;
using Variants;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class LegacyVariantIdTests
    {
        private static readonly IChromosome Chr1 = new Chromosome("chr1", "1", 0);
        private static readonly IChromosome Chr2 = new Chromosome("chr2", "2", 1);
        private static readonly IChromosome Chr3 = new Chromosome("chr3", "3", 2);

        private static readonly IDictionary<string, IChromosome> RefNameToChromosome = new Dictionary<string, IChromosome>();

        private readonly LegacyVariantId _vidCreator;

        public LegacyVariantIdTests()
        {
            RefNameToChromosome["chr1"] = Chr1;
            RefNameToChromosome["1"]    = Chr1;
            RefNameToChromosome["chr2"] = Chr2;
            RefNameToChromosome["2"]    = Chr2;
            RefNameToChromosome["chr3"] = Chr3;
            RefNameToChromosome["3"]    = Chr3;

            _vidCreator = new LegacyVariantId(RefNameToChromosome);
        }

        [Theory]
        [InlineData(66507, 66507, "T", "A", "1:66507:A")]
        [InlineData(66522, 66521, "", "ATATA", "1:66522:66521:ATATA")]
        [InlineData(66573, 66574, "TA", "", "1:66573:66574")]
        [InlineData(66573, 66572, "", "TACTATATATTA", "1:66573:66572:TACTATATATTA")]
        [InlineData(100, 104, "TAGGT", "ACTTA", "1:100:104:ACTTA")]
        [InlineData(100, 104, "TAGGT", "", "1:100:104")]
        [InlineData(101, 100, "", "CGA", "1:101:100:CGA")]
        [InlineData(100, 100, "T", "A", "1:100:A")]
        [InlineData(100, 104, "TAGGT", "CGA", "1:100:104:CGA")]
        [InlineData(100, 99, "", "ACTGACGTACGAAGTTGCCGTACGTACTTGTCC", "1:100:99:3bd631d37e62d5db0f6d5d6db3cdcb60")]
        [InlineData(66366, 66378, "ATATAATATATAA", "TATATATATTATTATATAATATAATATATATTATATAATATATTTTATTATATAATATAATATATATTATATAATATAATATATTTTATTATATAAATATATATTATATTATATAATATAATATATATTAATATAAATATATATTAT", "1:66366:66378:17b72647da13e3c186348467b29b0492")]
        [InlineData(100, 300, "", "<M>", "1:100:*")]
        public void Create_SmallVariants_ReturnVid(int start, int end, string refAllele, string altAllele, string expectedVid)
        {
            string observedVid = _vidCreator.Create(null, VariantCategory.SmallVariant, null, Chr1, start, end, refAllele, altAllele, null);
            Assert.Equal(expectedVid, observedVid);
        }

        [Theory]
        [InlineData(66507, 66507, "T", ".", "1:66507:66507:T")]
        [InlineData(100, 100, "T", "T", "1:100:100:T")]
        [InlineData(100, 100, "T", ".", "1:100:100:T")]
        public void Create_Reference_ReturnVid(int start, int end, string refAllele, string altAllele, string expectedVid)
        {
            string observedVid = _vidCreator.Create(null, VariantCategory.Reference, null, Chr1, start, end, refAllele, altAllele, null);
            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Create_TranslocationBreakend_ReturnVid()
        {
            string observedVid = _vidCreator.Create(null, VariantCategory.SV, "BND", Chr2, 321681, 321681, "G", "G[13:123460[", null);
            Assert.Equal("2:321681:+:13:123460:+", observedVid);
        }

        [Theory]
        [InlineData(1000, 3001000, "<ROH>", null, "ROH", VariantCategory.ROH, "1:1001:3001000:ROH")]
        [InlineData(1350082, 1351320, "<DEL>", null, "DEL", VariantCategory.SV, "1:1350083:1351320")]
        [InlineData(999, 2015, "<DUP>", null, "DUP", VariantCategory.SV, "1:1000:2015:DUP")]
        [InlineData(1477854, 1477984, "<DUP:TANDEM>", null, "DUP", VariantCategory.SV, "1:1477855:1477984:TDUP")]
        [InlineData(1477968, 1477968, "<INS>", null, "INS", VariantCategory.SV, "1:1477969:1477968:INS")]
        [InlineData(2000, 5000, "<CNV>", null, "CNV", VariantCategory.CNV, "1:2001:5000:CNV")]
        [InlineData(2000, 5000, "<CN3>", null, "CNV", VariantCategory.CNV, "1:2001:5000:CN3")]
        [InlineData(2000, 5000, "<DUP>", null, "CNV", VariantCategory.CNV, "1:2001:5000:CDUP")]
        [InlineData(2000, 5000, "<DEL>", null, "CNV", VariantCategory.CNV, "1:2001:5000:CDEL")]
        [InlineData(2000, 5000, "<ALU>", null, "ALU", VariantCategory.SV, "1:2001:5000:MEI")]
        [InlineData(2000, 5000, "<LINE1>", null, "LINE1", VariantCategory.SV, "1:2001:5000:MEI")]
        [InlineData(2000, 5000, "<SVA>", null, "SVA", VariantCategory.SV, "1:2001:5000:MEI")]
        [InlineData(2000, 5000, "<BOB>", null, "BOB", VariantCategory.SV, "1:2001:5000")]
        [InlineData(1715898, 1750149, "<DUP>", null, "CNV", VariantCategory.CNV, "1:1715899:1750149:CDUP")]
        [InlineData(2650426, 2653074, "<DEL>", null, "CNV", VariantCategory.CNV, "1:2650427:2653074:CDEL")]
        [InlineData(321682, 421681, "<INV>", null, "INV", VariantCategory.SV, "1:321683:421681:Inverse")]
        [InlineData(199, 202, "<STR5>", "TTG", "", VariantCategory.RepeatExpansion, "1:200:202:TTG:5")]
        public void Create_StructuralVariants_ReturnVid(int start, int end, string altAllele, string repeatUnit,
            string svType, VariantCategory category, string expectedVid)
        {
            string observedVid = _vidCreator.Create(null, category, svType, Chr1, start, end, "", altAllele, repeatUnit);
            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Create_LOH_ReturnsCnvVid()
        {
            const string altAllele       = "<CNV>";
            const string svType          = "LOH";
            var          variantCategory = VariantFactory.GetVariantCategory(altAllele, svType);

            string observedVid = _vidCreator.Create(null, variantCategory, svType, Chr1, 787923, 887923, "N", altAllele, null);
            Assert.Equal("1:787924:887923:CNV", observedVid);
        }

        [Fact]
        public void GetSmallVariantVid_UnknownVariantType_ThrowsException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                string vid = LegacyVariantId.GetSmallVariantVid(Chr1, 100, 200, "A", VariantType.complex_structural_alteration);
            });
        }
    }
}