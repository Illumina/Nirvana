using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Genome;
using Variants;

namespace Vcf.VariantCreator
{
    public static class BreakEndCreator
    {
        private const string ForwardBreakEnd = "[";

        public static IBreakEnd[] CreateFromTranslocations(IDictionary<string, IChromosome> refNameToChromosome, IChromosome chromosome1, string refAllele, string altAllele, int position1)
        {
            (IChromosome chromosome2, int position2, bool isSuffix1, bool isSuffix2) = ParseBreakendAltAllele(refNameToChromosome, refAllele, altAllele);
            return new IBreakEnd[] { new BreakEnd(chromosome1, chromosome2, position1, position2, isSuffix1, isSuffix2) };
        }

        public static IBreakEnd[] CreateFromDelDupInv(IDictionary<string, IChromosome> refNameToChromosome, string ensemblName, int start, string svType, int? svEnd)
        {
            if (svEnd == null) return null;

            int end = svEnd.Value;
            var breakEnds = new IBreakEnd[2];
            var chromosome = ReferenceNameUtilities.GetChromosome(refNameToChromosome, ensemblName);

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (svType)
            {
                case "DEL":
                    breakEnds[0] = new BreakEnd(chromosome, chromosome, start, end + 1, false, true);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, end + 1, start, true, false);
                    break;

                case "DUP":
                    breakEnds[0] = new BreakEnd(chromosome, chromosome, end, start, false, true);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, start, end, true, false);
                    break;

                case "INV":
                    breakEnds[0] = new BreakEnd(chromosome, chromosome, start, end, false, false);
                    breakEnds[1] = new BreakEnd(chromosome, chromosome, end + 1, start + 1, true, true);
                    break;

                default:
                    return null;
            }

            return breakEnds;
        }

        private static (IChromosome Chromosome2, int Position2, bool IsSuffix1, bool IsSuffix2) ParseBreakendAltAllele(IDictionary<string, IChromosome> refNameToChromosome, string refAllele, string altAllele)
        {
            string referenceName2;
            int position2;
            bool isSuffix2;

            // (\w+)([\[\]])([^:]+):(\d+)([\[\]])
            // ([\[\]])([^:]+):(\d+)([\[\]])(\w+)
            if (altAllele.StartsWith(refAllele))
            {
                var forwardRegex = new Regex(@"\w+([\[\]])([^:]+):(\d+)([\[\]])", RegexOptions.Compiled);
                var match = forwardRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[4].Value == ForwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(refNameToChromosome, referenceName2), position2, false, isSuffix2);
            }
            else
            {
                var reverseRegex = new Regex(@"([\[\]])([^:]+):(\d+)([\[\]])\w+", RegexOptions.Compiled);
                var match = reverseRegex.Match(altAllele);

                if (!match.Success)
                    throw new InvalidDataException(
                        "Unable to successfully parse the complex rearrangements for the following allele: " + altAllele);

                isSuffix2      = match.Groups[1].Value == ForwardBreakEnd;
                position2      = Convert.ToInt32(match.Groups[3].Value);
                referenceName2 = match.Groups[2].Value;

                return (ReferenceNameUtilities.GetChromosome(refNameToChromosome, referenceName2), position2, true, isSuffix2);
            }
        }
    }
}
