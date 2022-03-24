using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Genome;
using Variants;

namespace VariantAnnotation.GeneFusions.Calling
{
    public static class BreakEndAdjacencyFactory
    {
        private const           string ReverseBracket = "]";
        private static readonly Regex  ForwardRegex   = new(@"\w+([\[\]])(.+):(\d+)([\[\]])", RegexOptions.Compiled);
        private static readonly Regex  ReverseRegex   = new(@"([\[\]])(.+):(\d+)([\[\]])\w+", RegexOptions.Compiled);

        public static BreakEndAdjacency[] CreateAdjacencies(ISimpleVariant variant, IDictionary<string, Chromosome> refNameToChromosome, bool isInv3,
            bool isInv5) => variant.Type == VariantType.translocation_breakend
            ? CreateFromTranslocation(variant, refNameToChromosome)
            : CreateFromSymbolicAllele(variant, variant.Type, isInv3, isInv5);
        
        public static BreakEndAdjacency[] CreateFromTranslocation(ISimpleVariant variant,
            IDictionary<string, Chromosome> refNameToChromosome) => variant.AltAllele.StartsWith(variant.RefAllele)
            ? ConvertTranslocation(variant, ForwardRegex, false, 4, refNameToChromosome)
            : ConvertTranslocation(variant, ReverseRegex, true,  1, refNameToChromosome);

        private static BreakEndAdjacency[] ConvertTranslocation(ISimpleVariant variant, Regex regex,
            bool onReverseStrand, int partnerBracketIndex, IDictionary<string, Chromosome> refNameToChromosome)
        {
            Match match = regex.Match(variant.AltAllele);
            if (!match.Success)
                throw new InvalidDataException(
                    $"Unable to successfully parse the complex rearrangements for the following allele: {variant.AltAllele}");

            bool        partnerOnReverseStrand = match.Groups[partnerBracketIndex].Value == ReverseBracket;
            var         partnerPosition        = Convert.ToInt32(match.Groups[3].Value);
            string      partnerReferenceName   = match.Groups[2].Value;
            Chromosome partnerChromosome      = ReferenceNameUtilities.GetChromosome(refNameToChromosome, partnerReferenceName);

            var origin  = new BreakPoint(variant.Chromosome, variant.Start,   onReverseStrand);
            var partner = new BreakPoint(partnerChromosome,  partnerPosition, partnerOnReverseStrand);

            return new[] {new BreakEndAdjacency(origin, partner)};
        }

        public static BreakEndAdjacency[] CreateFromSymbolicAllele(IChromosomeInterval interval, VariantType variantType, bool isInv3, bool isInv5)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            return variantType switch
            {
                VariantType.deletion           => CreateFromDeletion(interval),
                VariantType.tandem_duplication => CreateFromDuplication(interval),
                VariantType.inversion          => CreateFromInversion(interval, isInv3, isInv5),
                _                              => null
            };
        }

        // ReSharper disable once UseDeconstructionOnParameter
        private static BreakEndAdjacency Flip(this BreakEndAdjacency adjacency)
        {
            var origin  = new BreakPoint(adjacency.Partner.Chromosome, adjacency.Partner.Position, !adjacency.Partner.OnReverseStrand);
            var partner = new BreakPoint(adjacency.Origin.Chromosome,  adjacency.Origin.Position,  !adjacency.Origin.OnReverseStrand);
            return new BreakEndAdjacency(origin, partner);
        }

        private static BreakEndAdjacency[] CreateFromDeletion(IChromosomeInterval interval)
        {
            // 1 10 . N N[1:21[
            var origin    = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
            var remote    = new BreakPoint(interval.Chromosome, interval.End   + 1, false);
            var adjacency = new BreakEndAdjacency(origin, remote);

            return new[] {adjacency, adjacency.Flip()};
        }

        private static BreakEndAdjacency[] CreateFromDuplication(IChromosomeInterval interval)
        {
            // 1 1 . N ]1:10]N
            var origin    = new BreakPoint(interval.Chromosome, interval.End,       false);
            var remote    = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
            var adjacency = new BreakEndAdjacency(origin, remote);

            return new[] {adjacency, adjacency.Flip()};
        }

        private static BreakEndAdjacency[] CreateFromInversion(IChromosomeInterval interval, bool isInv3, bool isInv5)
        {
            // 1 10 . N N]1:20]
            // 1 11 . N [1:21[N
            BreakPoint origin, origin2, remote, remote2;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (!isInv3 && !isInv5)
            {
                origin = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
                remote = new BreakPoint(interval.Chromosome, interval.End,       true);

                origin2 = new BreakPoint(interval.Chromosome, interval.End + 1, true);
                remote2 = new BreakPoint(interval.Chromosome, interval.Start,   false);
            }
            else if (isInv3)
            {
                origin = new BreakPoint(interval.Chromosome, interval.Start - 1, false);
                remote = new BreakPoint(interval.Chromosome, interval.End,       true);

                origin2 = new BreakPoint(interval.Chromosome, interval.End,       false);
                remote2 = new BreakPoint(interval.Chromosome, interval.Start - 1, true);
            }
            else // isInv5
            {
                origin = new BreakPoint(interval.Chromosome, interval.Start,   true);
                remote = new BreakPoint(interval.Chromosome, interval.End + 1, false);

                origin2 = new BreakPoint(interval.Chromosome, interval.End + 1, true);
                remote2 = new BreakPoint(interval.Chromosome, interval.Start,   false);
            }

            return new[] {new BreakEndAdjacency(origin, remote), new BreakEndAdjacency(origin2, remote2)};
        }
    }
}