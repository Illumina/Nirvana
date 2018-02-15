using System;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsgNotation
    {
        private const char NotationType = 'g';

        public static string GetNotation(string referenceAssertion, ISimpleVariant variant, ISequence refSequence,
            IInterval refernceInterval)
        {
            var rotated        = VariantRotator.Right(variant, refernceInterval, refSequence, false);
            var start          = Math.Min(rotated.Variant.Start, rotated.Variant.End);
            var end            = Math.Max(rotated.Variant.Start, rotated.Variant.End);
            var referenceBases = rotated.Variant.RefAllele;
            var alternateBases = rotated.Variant.AltAllele;
            var type           = HgvsCodingNomenclature.GetGenomicChange(refernceInterval, false, refSequence, rotated.Variant);

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