using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Genome;
using Variants;

namespace VariantAnnotation.TranscriptAnnotation
{
    public static class BreakEndUtilities
    {
        private const           string ReverseBracket = "]";
        private static readonly Regex  ForwardRegex   = new Regex(@"\w+([\[\]])([^:]+):(\d+)([\[\]])", RegexOptions.Compiled);
        private static readonly Regex  ReverseRegex   = new Regex(@"([\[\]])([^:]+):(\d+)([\[\]])\w+", RegexOptions.Compiled);

        public static BreakEndAdjacency[] CreateFromTranslocation(ISimpleVariant variant,
            IDictionary<string, IChromosome> refNameToChromosome) => variant.AltAllele.StartsWith(variant.RefAllele)
            ? ConvertTranslocation(variant, ForwardRegex, false, 4, refNameToChromosome)
            : ConvertTranslocation(variant, ReverseRegex, true, 1, refNameToChromosome);

        private static BreakEndAdjacency[] ConvertTranslocation(ISimpleVariant variant, Regex regex,
            bool onReverseStrand, int partnerBracketIndex, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var match = regex.Match(variant.AltAllele);
            if (!match.Success) throw new InvalidDataException($"Unable to successfully parse the complex rearrangements for the following allele: {variant.AltAllele}");

            bool partnerOnReverseStrand = match.Groups[partnerBracketIndex].Value == ReverseBracket;
            var partnerPosition         = Convert.ToInt32(match.Groups[3].Value);
            string partnerReferenceName = match.Groups[2].Value;
            var partnerChromosome       = ReferenceNameUtilities.GetChromosome(refNameToChromosome, partnerReferenceName);

            var origin  = new BreakPoint(variant.Chromosome, variant.Start, onReverseStrand);
            var partner = new BreakPoint(partnerChromosome, partnerPosition, partnerOnReverseStrand);

            return new[] { new BreakEndAdjacency(origin, partner) };
        }

        public static BreakEndAdjacency[] CreateFromSymbolicAllele(IChromosomeInterval interval, VariantType variantType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (variantType)
            {
                case VariantType.deletion:
                    return CreateFromDeletion(interval);

                case VariantType.tandem_duplication:
                    return CreateFromDuplication(interval);

                case VariantType.inversion:
                    return CreateFromInversion(interval);

                default:
                    return null;
            }
        }

        private static BreakEndAdjacency Flip(this BreakEndAdjacency adjacency)
        {
            var origin  = new BreakPoint(adjacency.Partner.Chromosome, adjacency.Partner.Position, !adjacency.Partner.OnReverseStrand);
            var partner = new BreakPoint(adjacency.Origin.Chromosome, adjacency.Origin.Position, !adjacency.Origin.OnReverseStrand);
            return new BreakEndAdjacency(origin, partner);
        }

        private static BreakEndAdjacency[] CreateFromDeletion(IChromosomeInterval interval)
        {
            // 1 10 . N N[1:21[
            var origin    = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
            var remote    = new BreakPoint(interval.Chromosome, interval.End + 1, false);
            var adjacency = new BreakEndAdjacency(origin, remote);

            return new[] {adjacency, adjacency.Flip()};
        }

        private static BreakEndAdjacency[] CreateFromDuplication(IChromosomeInterval interval)
        {
            // 1 1 . N ]1:10]N
            var origin    = new BreakPoint(interval.Chromosome, interval.End, false);
            var remote    = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
            var adjacency = new BreakEndAdjacency(origin, remote);

            return new[] { adjacency, adjacency.Flip() };
        }

        private static BreakEndAdjacency[] CreateFromInversion(IChromosomeInterval interval)
        {
            // 1 10 . N N]1:20]
            // 1 11 . N [1:21[N
            var origin    = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
            var remote    = new BreakPoint(interval.Chromosome, interval.End, true);
            var adjacency = new BreakEndAdjacency(origin, remote);

            var origin2    = new BreakPoint(interval.Chromosome, interval.End + 1, true);
            var remote2    = new BreakPoint(interval.Chromosome, interval.Start, false);
            var adjacency2 = new BreakEndAdjacency(origin2, remote2);

            return new[] { adjacency, adjacency2 };
        }
    }
}
