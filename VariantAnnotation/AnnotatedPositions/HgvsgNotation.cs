using System;
using Genome;
using Intervals;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsgNotation
    {
        private const char NotationType = 'g';

        public static string GetNotation(string referenceAssertion, ISimpleVariant variant, ISequence refSequence,
            IInterval referenceInterval)
        {
            var rotatedVariant = VariantRotator.Right(variant, referenceInterval, refSequence, false);
            var start          = Math.Min(rotatedVariant.Start, rotatedVariant.End);
            var end            = Math.Max(rotatedVariant.Start, rotatedVariant.End);
            var referenceBases = rotatedVariant.RefAllele;
            var alternateBases = rotatedVariant.AltAllele;
            var type           = HgvsCodingNomenclature.GetGenomicChange(referenceInterval, false, refSequence, rotatedVariant);

            if (type == GenomicChange.Duplication && variant.Type == VariantType.insertion)
            {
                referenceBases = alternateBases;
                end            = start;
                start          = end - referenceBases.Length + 1;
            }

            return HgvsUtilities.FormatDnaNotation(start.ToString(), end.ToString(), referenceAssertion, referenceBases, alternateBases, type, NotationType);
        }
    }
}