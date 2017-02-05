using System.Collections.Generic;
using System.Linq;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class SvVcfParsingTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public SvVcfParsingTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void Cnv()
        {
            const string vcfLine = "chr1	713044	DUP_gs_CNV_1_713044_755966	C	<CN0>,<CN2>	100	PASS	AC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045";

            var altAllele = new VariantAlternateAllele(713045, 755966, "C", "CN0")
            {
                VepVariantType      = VariantType.copy_number_variation,
                IsStructuralVariant = true
            };

            var altAllele2 = new VariantAlternateAllele(713045, 755966, "C", "CN2")
            {
                VepVariantType      = VariantType.copy_number_variation,
                IsStructuralVariant = true
            };

            var expectedReferenceName     = "chr1";
            var expectedAlternateAlleles  = new List<VariantAlternateAllele> { altAllele, altAllele2 };
            var expectedVcfReferenceBegin = altAllele.Start - 1;
            var expectedVcfReferenceEnd   = altAllele.End;

            var variant = VcfUtilities.GetVariant(vcfLine, _renamer);

            Assert.Equal(expectedReferenceName,     variant.ReferenceName);
            Assert.Equal(expectedVcfReferenceBegin, variant.VcfReferenceBegin);
            Assert.Equal(expectedVcfReferenceEnd,   variant.VcfReferenceEnd);
            Assert.True(expectedAlternateAlleles.SequenceEqual(variant.AlternateAlleles));
        }

        [Fact]
        public void Duplication()
        {
            const string vcfLine = "chr1	115251155	.	G	<DUP>	100	PASS	IMPRECISE;SVTYPE=DUP;END=115258781;SVLEN=7627;CIPOS=-1,1;CIEND=-1,1;DP=2635";

            var altAllele = new VariantAlternateAllele(115251156, 115258781, "G", "duplication")
            {
                VepVariantType      = VariantType.duplication,
                IsStructuralVariant = true,
                AlternateAllele     = "duplication"
            };

            var expectedReferenceName     = "chr1";
            var expectedAlternateAlleles  = new List<VariantAlternateAllele> { altAllele };
            var expectedVcfReferenceBegin = altAllele.Start - 1;
            var expectedVcfReferenceEnd   = altAllele.End;

            var variant = VcfUtilities.GetVariant(vcfLine, _renamer);
            Assert.Equal(expectedReferenceName,     variant.ReferenceName);
            Assert.Equal(expectedVcfReferenceBegin, variant.VcfReferenceBegin);
            Assert.Equal(expectedVcfReferenceEnd,   variant.VcfReferenceEnd);
            Assert.True(expectedAlternateAlleles.SequenceEqual(variant.AlternateAlleles));
        }

        [Fact]
        public void Duplication2()
        {
            const string vcfLine = "chrX	66764988	.	G	<DUP>	100	PASS	IMPRECISE;SVTYPE=DUP;END=66943683;SVLEN=178696;CIPOS=-1,1;CIEND=-1,1;DP=2635";

            var altAllele = new VariantAlternateAllele(66764989, 66943683, "G", "duplication")
            {
                VepVariantType      = VariantType.duplication,
                IsStructuralVariant = true,
                AlternateAllele     = "duplication"
            };

            var expectedReferenceName     = "chrX";
            var expectedAlternateAlleles  = new List<VariantAlternateAllele> { altAllele };
            var expectedVcfReferenceBegin = altAllele.Start - 1;
            var expectedVcfReferenceEnd   = altAllele.End;

            var variant = VcfUtilities.GetVariant(vcfLine, _renamer);
            Assert.Equal(expectedReferenceName,     variant.ReferenceName);
            Assert.Equal(expectedVcfReferenceBegin, variant.VcfReferenceBegin);
            Assert.Equal(expectedVcfReferenceEnd,   variant.VcfReferenceEnd);
            Assert.True(expectedAlternateAlleles.SequenceEqual(variant.AlternateAlleles));
        }

        [Fact]
        public void Insertion()
        {
            const string vcfLine = "chr22	15883626	P1_MEI_4726	T	<INS>	40	.	SVTYPE=INS;CIPOS=-23,23;IMPRECISE;NOVEL;SVMETHOD=SR;NSF5=1;NSF3=0";

            var altAllele = new VariantAlternateAllele(15883627, 15883626, "T", "insertion")
            {
                VepVariantType      = VariantType.insertion,
                IsStructuralVariant = true,
                AlternateAllele     = "insertion"
            };

            var expectedReferenceName     = "chr22";
            var expectedAlternateAlleles  = new List<VariantAlternateAllele> { altAllele };
            var expectedVcfReferenceBegin = altAllele.Start - 1;
            var expectedVcfReferenceEnd   = altAllele.End;

            var variant = VcfUtilities.GetVariant(vcfLine, _renamer);
            Assert.Equal(expectedReferenceName,     variant.ReferenceName);
            Assert.Equal(expectedVcfReferenceBegin, variant.VcfReferenceBegin);
            Assert.Equal(expectedVcfReferenceEnd,   variant.VcfReferenceEnd);
            Assert.True(expectedAlternateAlleles.SequenceEqual(variant.AlternateAlleles));
        }
    }
}