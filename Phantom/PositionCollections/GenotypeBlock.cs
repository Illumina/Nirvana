using System;
using System.Collections.Generic;
using System.Linq;

namespace Phantom.PositionCollections
{
    public sealed class GenotypeBlock : IEquatable<GenotypeBlock>
    {
        public readonly int PosIndex;
        public readonly Genotype[] Genotypes;

        public GenotypeBlock(Genotype[] genotypes, int posIndex = 0)
        {
            Genotypes = genotypes;
            PosIndex = posIndex;
        }

        public IEnumerable<GenotypeBlock> Split(int[] starts, List<int> functionBlockRanges)
        {
            int numBreaks = Genotypes.Length - 1;
            if (numBreaks == 0) return new List<GenotypeBlock>();
            int ploidy = Genotypes[0].AlleleIndexes.Length;
            var breaks = new bool[ploidy][];
            for (int i = 0; i < ploidy; i++)
            {
                breaks[i] = GetAlleleBreaks(i, starts, functionBlockRanges);
            }
            bool[] finalBreaks = GetFinalBreaks(breaks);
            return GetGenotypeBlocks(finalBreaks);
        }

        private bool[] GetAlleleBreaks(int haplotypeIndex, int[] starts, List<int> functionBlockRanges)
        {
            int numBreaks = Genotypes.Length - 1;
            var alleleBreaks = new bool[numBreaks];
            int lastNonRefPosition = -1;

            // function block checking
            for (int gtIndex = 0; gtIndex < Genotypes.Length; gtIndex++)
            {
                int alleleIndex = Genotypes[gtIndex].AlleleIndexes[haplotypeIndex];
                if (alleleIndex == 0)
                {
                    ProcessRefAllele(haplotypeIndex, numBreaks, alleleBreaks, gtIndex);
                }
                else
                {
                    lastNonRefPosition = ProcessNonRefAllele(haplotypeIndex, starts, functionBlockRanges, alleleBreaks, lastNonRefPosition, gtIndex);
                }
            }
            return alleleBreaks;
        }

        private int ProcessNonRefAllele(int haplotypeIndex, int[] starts, IReadOnlyList<int> functionBlockRanges, bool[] alleleBreaks, int lastNonRefPosition, int gtIndex)
        {
            if (gtIndex > 0)
            {
                bool outOfRange = lastNonRefPosition != -1 && starts[PosIndex + gtIndex] > functionBlockRanges[lastNonRefPosition];
                if (outOfRange)
                    MakeBreakAndCheckTailingRefPositions(gtIndex - 1, haplotypeIndex, alleleBreaks);
            }
            lastNonRefPosition = PosIndex + gtIndex;
            return lastNonRefPosition;
        }

        private void ProcessRefAllele(int haplotypeIndex, int numBreaks, bool[] alleleBreaks, int gtIndex)
        {
            // check leading ref positions
            if (gtIndex == 0 || alleleBreaks[gtIndex - 1] && gtIndex < numBreaks)
            {
                alleleBreaks[gtIndex] = true;
            }
            // check tailing ref positions 
            else if (gtIndex == numBreaks)
            {
                MakeBreakAndCheckTailingRefPositions(gtIndex - 1, haplotypeIndex, alleleBreaks);
            }
        }

        private void MakeBreakAndCheckTailingRefPositions(int breakIndex, int haplotypeIndex, bool[] alleleBreaks)
        {
            alleleBreaks[breakIndex] = true;
            // check "tailing" referece positions before this break
            for (int i = breakIndex - 1; i >= 0; i--)
            {
                // stop when the break bool is true or the allele is non-ref
                if (alleleBreaks[i] || Genotypes[i + 1].AlleleIndexes[haplotypeIndex] != 0) break;
                alleleBreaks[i] = true;
            }
        }

        private IEnumerable<GenotypeBlock> GetGenotypeBlocks(bool[] finalBreaks)
        {
            var subGenotypeBlocks = new List<GenotypeBlock>();
            int subBlockStart = 0;
            int subBlockEnd = 0;
            for (int i = 0; i < finalBreaks.Length; i++)
            {
                if (!finalBreaks[i])
                {
                    subBlockEnd++;
                }
                else
                {
                    int subBlockSize = subBlockEnd - subBlockStart + 1;
                    if (subBlockSize > 1)
                    {
                        subGenotypeBlocks.Add(GetSubBlock(subBlockStart, subBlockSize));
                    }
                    subBlockStart = i + 1;
                    subBlockEnd = i + 1;
                }
            }
            if (subBlockEnd - subBlockStart >= 1) subGenotypeBlocks.Add(GetSubBlock(subBlockStart, subBlockEnd - subBlockStart + 1));
            return subGenotypeBlocks;
        }

        public GenotypeBlock GetSubBlock(int subBlockStart, int numPositions) => new GenotypeBlock(new ArraySegment<Genotype>(Genotypes, subBlockStart, numPositions).ToArray(),
                PosIndex + subBlockStart);

        private static bool[] GetFinalBreaks(bool[][] breaks)
        {
            int ploidy = breaks.Length;
            int numBreaks = breaks[0].Length;
            bool[] finalBreaks = new bool[numBreaks];
            for (int i = 0; i < numBreaks; i++)
            {
                bool anyFalse = false;
                for (int j = 0; j < ploidy; j++)
                {
                    if (breaks[j][i]) continue;
                    anyFalse = true;
                    break;
                }
                if (!anyFalse) finalBreaks[i] = true;
            }
            return finalBreaks;
        }

        public bool Equals(GenotypeBlock other) => Genotypes.SequenceEqual(other.Genotypes) && PosIndex == other.PosIndex;

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PosIndex.GetHashCode();
                foreach (var genotype in Genotypes)
                {
                    hashCode = (hashCode * 1201) ^ genotype.GetHashCode();
                }
                return hashCode;
            }
        }

        
    }
}