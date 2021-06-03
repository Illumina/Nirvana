using System;
using Genome;
using Intervals;
using Variants;

namespace VariantAnnotation.AnnotatedPositions
{
    public static class HgvsgNotation
    {
        private const char NotationType     = 'g';
        private const char MitoNotationType = 'm';

        public static string GetNotation(string refseqAccession, ISimpleVariant variant, ISequence refSequence,
                                         IInterval referenceInterval)
        {
            ISimpleVariant rotatedVariant = VariantRotator.Right(variant, referenceInterval, refSequence, false);
            int            start          = Math.Min(rotatedVariant.Start, rotatedVariant.End);
            int            end            = Math.Max(rotatedVariant.Start, rotatedVariant.End);
            string         referenceBases = rotatedVariant.RefAllele;
            string         alternateBases = rotatedVariant.AltAllele;
            GenomicChange  type           = HgvsCodingNomenclature.GetGenomicChange(referenceInterval, false, refSequence, rotatedVariant);

            if (type == GenomicChange.Duplication && variant.Type == VariantType.insertion)
            {
                referenceBases = alternateBases;
                end            = start;
                start          = end - referenceBases.Length + 1;
            }

            char notationType = variant.Chromosome.UcscName == "chrM" ? MitoNotationType : NotationType;
            return HgvsUtilities.FormatDnaNotation(start.ToString(), end.ToString(), refseqAccession, referenceBases, alternateBases, type,
                notationType);
        }
    }
}