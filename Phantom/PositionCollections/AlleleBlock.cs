using System;
using System.Collections.Generic;
using System.Linq;
using Phantom.Graph;

namespace Phantom.PositionCollections
{
    public sealed class AlleleBlock : IEquatable<AlleleBlock>, IComparable<AlleleBlock>
    {

        public int[] AlleleIndexes { get; }
        public int PositionIndex { get; }
        public int NumRefPositionsBefore { get; }
        public int NumRefPositionsAfter { get; }

        public AlleleBlock(int positionIndex, int[] alleleIndexes, int numRefPositionsBefore, int numRefPositionsAfter)
        {
            PositionIndex = positionIndex;
            AlleleIndexes = alleleIndexes;
            NumRefPositionsBefore = numRefPositionsBefore;
            NumRefPositionsAfter = numRefPositionsAfter;
        }

        public static Dictionary<AlleleBlock, List<SampleHaplotype>> GetAlleleBlockToSampleHaplotype(Dictionary<GenotypeBlock, List<int>> genotypeBlockToSample, HashSet<int>[] allelesWithUnsupportedType, int[] starts, List<int> functionBlockRanges, out Graph<AlleleBlock> alleleIndexBlockGraph)
        {
            var alleleIndexBlockToSampleIndex = new Dictionary<AlleleBlock, List<SampleHaplotype>>();
            alleleIndexBlockGraph = new Graph<AlleleBlock>(false);
            foreach (var (genotypeBlock, sampleIndexes) in genotypeBlockToSample.ToList())
            {
                var genotypeArray = genotypeBlock.Genotypes.ToArray();
                int startIndexInBlock = genotypeBlock.PosIndex;
                int ploidy = GetMaxPloidy(genotypeArray);
                int currentSubBlockStart = -1;

                for (var i = 0; i < genotypeArray.Length; i++)
                {
                    int indexInBlock = i + startIndexInBlock;
                    var genotype = genotypeArray[i];
                    var genotypeIndexes = genotype.AlleleIndexes;

                    // Segmentation using genotype information
                    bool blockTerminationConditionMet = HasReducedPloidy(genotypeIndexes, ploidy) ||
                                                        HasUndeterminedOrUnsupportedGenotype(genotypeIndexes, allelesWithUnsupportedType[indexInBlock]) ||
                                                        IsUnphasedHeterozygote(genotype);

                    if (blockTerminationConditionMet && currentSubBlockStart != -1)
                    {
                        // Don't include the termination position
                        AddAlleleIndexBlocks(genotypeBlock.GetSubBlock(currentSubBlockStart, i - currentSubBlockStart), sampleIndexes, starts, functionBlockRanges, alleleIndexBlockToSampleIndex, alleleIndexBlockGraph);
                        currentSubBlockStart = -1;
                    }
                    else if (!blockTerminationConditionMet && currentSubBlockStart == -1)
                    {
                        currentSubBlockStart = i;
                    }
                }
                if (currentSubBlockStart != -1)
                {
                    AddAlleleIndexBlocks(genotypeBlock.GetSubBlock(currentSubBlockStart, genotypeArray.Length - currentSubBlockStart), sampleIndexes, starts, functionBlockRanges, alleleIndexBlockToSampleIndex, alleleIndexBlockGraph);
                }
            }
            return alleleIndexBlockToSampleIndex;
        }

        private static void AddAlleleIndexBlocks(GenotypeBlock genotypeBlock, List<int> sampleIndexes, int[] starts,
            List<int> functionBlockRanges, IDictionary<AlleleBlock, List<SampleHaplotype>> alleleIndexBlockToSampleAllele, Graph<AlleleBlock> alleleIndexBlockGraph)
        {
            int startPosition = genotypeBlock.PosIndex;
            int ploidy = genotypeBlock.Genotypes[0].AlleleIndexes.Length;
            var isRefPositions = genotypeBlock.Genotypes.Select(x => IsRefPosition(x.AlleleIndexes)).ToArray();
            foreach (var functionBlock in genotypeBlock.Split(starts, functionBlockRanges))
            {

                int startInThisBlock = functionBlock.PosIndex - startPosition;
                int numPositions = functionBlock.Genotypes.Length;
                int endInThisBlock = startInThisBlock + numPositions - 1;
                var (numRefPosBefore, numRefPosAfter) = GetRefPosNums(isRefPositions, startInThisBlock, endInThisBlock);

                var alleleIndexBlocks = new List<AlleleBlock>();
                for (int haplotypeIndex = 0; haplotypeIndex < ploidy; haplotypeIndex++)
                {
                    var alleleIndexes = new int[numPositions];
                    for (var i = 0; i < numPositions; i++)
                        alleleIndexes[i] = functionBlock.Genotypes[i].AlleleIndexes[haplotypeIndex];
                    var alleleIndexBlock = new AlleleBlock(functionBlock.PosIndex, alleleIndexes, numRefPosBefore,
                        numRefPosAfter);
                    alleleIndexBlocks.Add(alleleIndexBlock);
                    var currentSampleAlleles = GetSampleAlleles(sampleIndexes, (byte)haplotypeIndex);
                    UpdateBlockToSampleAlleleMapping(alleleIndexBlock, currentSampleAlleles,
                        alleleIndexBlockToSampleAllele);
                }
                for (var i = 0; i < alleleIndexBlocks.Count; i++)
                    for (int j = i + 1; j < alleleIndexBlocks.Count; j++)
                    {
                        alleleIndexBlockGraph.AddEdge(alleleIndexBlocks[i], alleleIndexBlocks[j]);
                    }
            }
        }

        private static (int, int ) GetRefPosNums(bool[] isRefPositions, int startInThisBlock, int endInThisBlock)
        {
            int numRefPosBefore = 0;
            int numRefPosAfter = 0;

            for (int i = startInThisBlock - 1; i >= 0; i--)
            {
                if (isRefPositions[i]) numRefPosBefore++;
                else break;
            }
            for (int i = endInThisBlock + 1; i < isRefPositions.Length; i++)
            {
                if (isRefPositions[i]) numRefPosAfter++;
                else break;
            }

            return (numRefPosBefore, numRefPosAfter);
        }

        private static bool IsRefPosition(int[] genotypeIndexes) => genotypeIndexes.All(x => x == 0);

        private static void UpdateBlockToSampleAlleleMapping(AlleleBlock alleleBlock, List<SampleHaplotype> currentSampleAlleles, IDictionary<AlleleBlock, List<SampleHaplotype>> alleleIndexBlockToSampleAllele)
        {
            if (alleleIndexBlockToSampleAllele.TryGetValue(alleleBlock, out var sampleAlleles))
            {
                foreach (var currentSampleAllele in currentSampleAlleles)
                    sampleAlleles.Add(currentSampleAllele);
            }
            else
            {
                alleleIndexBlockToSampleAllele.Add(alleleBlock, currentSampleAlleles);
            }
        }

        private static List<SampleHaplotype> GetSampleAlleles(IEnumerable<int> sampleIndexes, byte alleleIndex)
        {
            var sampleAlleleList = new List<SampleHaplotype>();
            foreach (int sampleIndex in sampleIndexes)
            {
                sampleAlleleList.Add(new SampleHaplotype(sampleIndex, alleleIndex));
            }
            return sampleAlleleList;
        }

        internal static int GetMaxPloidy(IEnumerable<Genotype> genotypes) => genotypes.Select(x => x.AlleleIndexes.Length).Max();

        private static bool HasReducedPloidy(int[] genotypeIndexes, int blockPloidy) => genotypeIndexes.Length < blockPloidy;

        private static bool HasUndeterminedOrUnsupportedGenotype(int[] genotypeIndexes, ICollection<int> indexOfSkippedTypes) => genotypeIndexes.Any(x => x == -1 || indexOfSkippedTypes.Contains(x));

        private static bool IsUnphasedHeterozygote(Genotype genotype) => !genotype.IsPhased && !genotype.IsHomozygous;

        public bool Equals(AlleleBlock other) => PositionIndex == other.PositionIndex && NumRefPositionsBefore == other.NumRefPositionsBefore && NumRefPositionsAfter == other.NumRefPositionsAfter && AlleleIndexes.SequenceEqual(other.AlleleIndexes);

        public int CompareTo(AlleleBlock other)
        {
            // first sort ascending by start point of the blocks
            int positionCompare = PositionIndex.CompareTo(other.PositionIndex);
            if (positionCompare != 0) return positionCompare;

            // then sort descending by end point of the blocks
            int sizeCompare = AlleleIndexes.Length.CompareTo(other.AlleleIndexes.Length);
            if (sizeCompare != 0) return -sizeCompare;

            // then sort ascending by start point of the blocks w/ trimmed ref position considered
            int numRefPosBeforeCompare = NumRefPositionsBefore.CompareTo(other.NumRefPositionsBefore);
            if (numRefPosBeforeCompare != 0) return numRefPosBeforeCompare;

            // then sort descending by end point of the blocks w/ trimmed ref position considered
            int numRefPosAfterCompare = NumRefPositionsAfter.CompareTo(other.NumRefPositionsAfter);
            if (numRefPosAfterCompare != 0) return -numRefPosAfterCompare;

            // then sort ascending by each allele index
            for (int i = 0; i < AlleleIndexes.Length; i++)
            {
                int alleleCompare = AlleleIndexes[i].CompareTo(other.AlleleIndexes[i]);
                if (alleleCompare != 0) return alleleCompare;
            }
            return 0;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PositionIndex.GetHashCode();
                hashCode = (hashCode * 1201) ^ NumRefPositionsBefore;
                hashCode = (hashCode * 1201) ^ NumRefPositionsAfter;
                foreach (int alleleIndex in AlleleIndexes)
                {
                    hashCode = (hashCode * 1201) ^ alleleIndex.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}