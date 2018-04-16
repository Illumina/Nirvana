using System;
using System.Collections.Generic;
using System.Linq;

namespace Phantom.DataStructures
{
    public sealed class AlleleIndexBlock : IEquatable<AlleleIndexBlock>
    {

        public int[] AlleleIndexes { get; }
        public int PositionIndex { get; }

        public AlleleIndexBlock(int positionIndex, int[] alleleIndexes)
        {
            PositionIndex = positionIndex;
            AlleleIndexes = alleleIndexes;
        }

        public static Dictionary<AlleleIndexBlock, List<SampleAllele>> GetAlleleIndexBlockToSampleIndex(Dictionary<GenotypeBlock, List<int>> genotypeBlockToSample, HashSet<int>[] allelesWithUnsupportedType, int[] starts, List<int> functionBlockRanges)
        {
            var alleleIndexBlockToSampleIndex = new Dictionary<AlleleIndexBlock, List<SampleAllele>>();
            foreach (var (genotypeBlock, sampleIndexes) in genotypeBlockToSample.ToList())
            {
                var genotypeArray = genotypeBlock.Genotypes.ToArray();
                int startIndexInBlock = genotypeBlock.PosIndex;
                int ploidy = GetMaxPloidy(genotypeArray);
                var currentSubBlockStart = -1;

                for (int i = 0; i < genotypeArray.Length; i++)
                {
                    int indexInBlock = i + startIndexInBlock;
                    var genotype = genotypeArray[i];
                    var genotypeIndexes = genotype.AlleleIndexes;
                    bool isRefPosition = IsRefPosition(genotypeIndexes);

                    bool blockTerminationConditionMet = HasReducedPloidy(genotypeIndexes, ploidy) ||
                                                        HasUndeterminedOrUnsupportedGenotype(genotypeIndexes,
                                                            allelesWithUnsupportedType[indexInBlock]) ||
                                                        IsUnphasedHeterozygote(genotype);

                    // block terminates at this position
                    if (blockTerminationConditionMet)
                    {
                        if (currentSubBlockStart != -1)
                        {
                            // Don't include this position
                            ProcessCurrentSubBlock(genotypeBlock, currentSubBlockStart, i - currentSubBlockStart,
                                starts, functionBlockRanges, sampleIndexes, ref alleleIndexBlockToSampleIndex);
                            currentSubBlockStart = -1;
                        }
                    }
                    // current sub block is empty
                    else if (currentSubBlockStart == -1)
                    {
                        if (isRefPosition) continue; // don't build a block if the first position is ref
                        currentSubBlockStart = i;
                    }
                }
                if (currentSubBlockStart != -1)
                {
                    ProcessCurrentSubBlock(genotypeBlock, currentSubBlockStart, genotypeArray.Length - currentSubBlockStart,
                                starts, functionBlockRanges, sampleIndexes, ref alleleIndexBlockToSampleIndex);
                }
            }
            return alleleIndexBlockToSampleIndex;
        }

        private static void ProcessCurrentSubBlock(GenotypeBlock entireGenotypeBlock, int currentSubBlockStart, int currentSubBlockSize, int[] starts, List<int> functionBlockRanges, List<int> sampleIndexes, ref Dictionary<AlleleIndexBlock, List<SampleAllele>> alleleIndexBlockToSampleIndex)
        {
            GenotypeBlock currentSubBlock =
                entireGenotypeBlock.GetSubBlock(currentSubBlockStart, currentSubBlockSize);
            foreach (var functionBlock in currentSubBlock.Split(starts, functionBlockRanges))
            {
                UpdateBlockToSampleAlleleMapping(alleleIndexBlockToSampleIndex, functionBlock,
                    sampleIndexes);
            }
        }

        private static bool IsRefPosition(int[] genotypeIndexes) => genotypeIndexes.All(x => x == 0);

        private static void UpdateBlockToSampleAlleleMapping(Dictionary<AlleleIndexBlock, List<SampleAllele>> alleleIndexBlockToSampleAllele, GenotypeBlock genotypeBlock, List<int> sampleIndexes)
        {
            if (genotypeBlock == null) return;
            var genotypes = genotypeBlock.Genotypes.ToArray();
            if (genotypes.Length <= 1) return;
            int ploidy = genotypes[0].AlleleIndexes.Length;
            for (int haplotypeIndex = 0; haplotypeIndex < ploidy; haplotypeIndex++)
            {
                var alleleIndexes = new int[genotypes.Length];
                for (var i = 0; i < genotypes.Length; i++) alleleIndexes[i] = genotypes[i].AlleleIndexes[haplotypeIndex];
                var alleleIndexBlock = new AlleleIndexBlock(genotypeBlock.PosIndex, alleleIndexes);

                var currentSampleAlleles = GetSampleAlleles(sampleIndexes, (byte)haplotypeIndex);

                if (alleleIndexBlockToSampleAllele.TryGetValue(alleleIndexBlock, out var sampleAlleles))
                {
                    sampleAlleles.AddRange(currentSampleAlleles);
                }
                else
                {
                    alleleIndexBlockToSampleAllele.Add(alleleIndexBlock, currentSampleAlleles);
                }
            }
        }

        private static List<SampleAllele> GetSampleAlleles(List<int> sampleIndexes, byte alleleIndex)
        {
            var sampleAlleleList = new SampleAllele[sampleIndexes.Count];
            for (var i = 0; i < sampleIndexes.Count; i++)
            {
                sampleAlleleList[i] = new SampleAllele(sampleIndexes[i], alleleIndex);
            }
            return sampleAlleleList.ToList();
        }

        internal static int GetMaxPloidy(IEnumerable<Genotype> genotypes) => genotypes.Select(x => x.AlleleIndexes.Length).Max();


        internal static void TrimTrailingRefAlleles(List<int>[] blocks)
        {
            if (blocks.Length == 0) return;
            for (int index = blocks[0].Count - 1; index >= 0; index--)
            {
                if (blocks.Any(x => x[index] != 0)) break;
                foreach (var block in blocks) block.RemoveAt(index);
            }
        }

        private static bool HasReducedPloidy(int[] genotypeIndexes, int blockPloidy) => genotypeIndexes.Length < blockPloidy;

        private static bool HasUndeterminedOrUnsupportedGenotype(int[] genotypeIndexes, HashSet<int> indexOfSkippedTypes) => genotypeIndexes.Any(x => x == -1 || indexOfSkippedTypes.Contains(x));

        private static bool IsUnphasedHeterozygote(Genotype genotype) => !genotype.IsPhased && !genotype.IsHomozygous;

        public bool Equals(AlleleIndexBlock other) => PositionIndex == other.PositionIndex && AlleleIndexes.SequenceEqual(other.AlleleIndexes);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PositionIndex.GetHashCode();
                foreach (int alleleIndex in AlleleIndexes)
                {
                    hashCode = (hashCode * 1201) ^ alleleIndex.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}