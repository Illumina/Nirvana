﻿using UnitTests.TestUtilities;
using VariantAnnotation.Pools;
using Variants;
using Xunit;

namespace UnitTests.Variants
{
    public sealed class VariantTests
    {
        [Fact]
        public void Variant_Set()
        {
            const int expectedStart        = 100;
            const int expectedEnd          = 102;
            const string expectedRef       = "AT";
            const string expectedAlt       = "";
            const VariantType expectedType = VariantType.deletion;
            const string expectedVid       = "1:100:A:C";
            const bool expectedRefMinor    = true;
            const bool expectedDecomposed  = false;
            const bool expectedRecomposed  = true;
            var expectedLinkedVids         = new[] { "1:102:T:G" };
            var expectedBehavior           = AnnotationBehavior.SmallVariants;

            var variant                    = VariantPool.Get(ChromosomeUtilities.Chr1, expectedStart, expectedEnd, expectedRef, expectedAlt,
                expectedType, expectedVid, expectedRefMinor, expectedDecomposed, expectedRecomposed, expectedLinkedVids,
                expectedBehavior, false);

            Assert.Equal(ChromosomeUtilities.Chr1, variant.Chromosome);
            Assert.Equal(expectedStart,      variant.Start);
            Assert.Equal(expectedEnd,        variant.End);
            Assert.Equal(expectedRef,        variant.RefAllele);
            Assert.Equal(expectedAlt,        variant.AltAllele);
            Assert.Equal(expectedType,       variant.Type);
            Assert.Equal(expectedVid,        variant.VariantId);
            Assert.Equal(expectedRefMinor,   variant.IsRefMinor);
            Assert.Equal(expectedDecomposed, variant.IsDecomposed);
            Assert.Equal(expectedRecomposed, variant.IsRecomposed);
            Assert.Equal(expectedLinkedVids, variant.LinkedVids);
            Assert.Equal(expectedBehavior,   variant.Behavior);
            
            VariantPool.Return(variant);
        }
    }
}
