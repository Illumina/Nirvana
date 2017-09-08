using System;
using System.Text;
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
            var rotatedVariant = VariantRotator.Right(variant, refernceInterval, refSequence, false,
                out _);
            var start = Math.Min(rotatedVariant.Start, rotatedVariant.End);
            var end = Math.Max(rotatedVariant.Start, rotatedVariant.End);
            var referenceBases = rotatedVariant.RefAllele;
            var alternateBases = rotatedVariant.AltAllele;
            var type = HgvsCodingNomenclature.GetGenomicChange(refernceInterval, false, refSequence, rotatedVariant);
            if (type == GenomicChange.Duplication && variant.Type == VariantType.insertion)
            {
                referenceBases = alternateBases;
                end = start;
                start = end - referenceBases.Length + 1;
            }

            return FormatNotation(start,end,referenceAssertion,referenceBases,alternateBases,type);
        }




        private static string FormatNotation(int start,int end,string referenceAsserionNumber,string referenceBases,string alternateBases, GenomicChange type)
        {
            var sb = new StringBuilder();
            // all start with transcript name & numbering type
            sb.Append(referenceAsserionNumber + ':' + NotationType + '.');

            // handle single and multiple positions
            string coordinates = start == end
                ? start.ToString()
                : start.ToString() + '_' + end;

            // format rest of string according to type
            // note: inversion and multiple are never assigned as genomic changes
            switch (type)
            {
                case GenomicChange.Deletion:
                    sb.Append(coordinates + "del" + referenceBases);
                    break;
                case GenomicChange.Inversion:
                    sb.Append(coordinates + "inv" + referenceBases);
                    break;
                case GenomicChange.Duplication:
                    sb.Append(coordinates + "dup" + referenceBases);
                    break;
                case GenomicChange.Substitution:
                    sb.Append(start + referenceBases + '>' + alternateBases);
                    break;
                case GenomicChange.DelIns:
                    sb.Append(coordinates + "delins" + alternateBases);
                    break;
                case GenomicChange.Insertion:
                    sb.Append(coordinates + "ins" + alternateBases);
                    break;

                default:
                    throw new InvalidOperationException("Unhandled genomic change found: " + type);
            }

            return sb.ToString();
        }
    }
}