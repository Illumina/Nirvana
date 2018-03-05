using System;
using System.Collections.Generic;
using System.Linq;

namespace Phantom.DataStructures
{
    public sealed class AlleleIndexBlock : IEquatable<AlleleIndexBlock>
    {

        public List<int> AlleleIndexes { get; }
        public int PositionIndex { get; }

        public AlleleIndexBlock(int positionIndex, List<int> alleleIndexes)
        {
            PositionIndex = positionIndex;
            AlleleIndexes = alleleIndexes;
        }


        public static Dictionary<AlleleIndexBlock, List<SampleAllele>> GetAlleleIndexBlockToSampleIndex(Dictionary<(string, int), List<int>> genotypeToSample, HashSet<string>[] allelesWithUnsupportedType, int[] starts, List<int> functionBlockRanges)
        {
            var alleleIndexBlockToSampleIndex = new Dictionary<AlleleIndexBlock, List<SampleAllele>>();
            foreach (var key in genotypeToSample.Keys)
            {
                var (genotypes, startIndexInBlock) = key;
                var sampleIndexes = genotypeToSample[key];
                //var alleleIndexBlocks = new List<AlleleIndexBlock>();
                int ploidy = GetMaxPloidy(genotypes);
                GenotypeBlock currentBlockInfo = null;
                var genotypeArray = genotypes.Split(";");
                var lastNonRefPositions = new int[ploidy];
                for (int i = 0; i < ploidy; i++) lastNonRefPositions[i] = -1;
                for (int i = 0; i < genotypeArray.Length; i++)
                {
                    int indexInBlock = i + startIndexInBlock;
                    var genotype = genotypeArray[i];
                    var genotypeIndexes = genotype?.Split('/', '|').ToArray();
                    bool isRefPosition = IsRefPosition(genotypeIndexes);

                    bool blockTerminationConditionMet = string.IsNullOrEmpty(genotype) || 
                                                        HasReducedPloidy(genotypeIndexes, ploidy) ||
                                                        HasUndeterminedOrUnsupportedGenotype(genotypeIndexes,
                                                            allelesWithUnsupportedType[indexInBlock]) ||
                                                        IsUnphasedHeterozygote(genotype);
                    // block terminates at this position
                    if (blockTerminationConditionMet)
                    {
                        if (currentBlockInfo != null)
                        {
                            foreach (var subBlock in currentBlockInfo.Split(starts, functionBlockRanges))
                            {
                                UpdateBlockToSampleAlleleMapping(alleleIndexBlockToSampleIndex, subBlock,
                                    sampleIndexes);
                            }
                            currentBlockInfo = null;
                        }
                    }
                    else
                    {
                        // current block is empty
                        if (currentBlockInfo == null)
                        {
                            if (isRefPosition) continue; // don't build a block if the first position is ref
                            currentBlockInfo = GenotypeBlock.CreateWithGenotypes(indexInBlock, genotypeIndexes);
                        }
                        else 
                        {
                            currentBlockInfo.AddAlleleIndexes(genotypeIndexes);
                        }
                    }
                }
                if (currentBlockInfo != null)
                {
                    foreach (var subBlock in currentBlockInfo.Split(starts, functionBlockRanges))
                    {
                        UpdateBlockToSampleAlleleMapping(alleleIndexBlockToSampleIndex, subBlock,
                            sampleIndexes);
                    }
                }
            }
            return alleleIndexBlockToSampleIndex;
        }

        private static bool IsRefPosition(string[] genotypeIndexes) => genotypeIndexes.All(x => x == "0");

        private static void UpdateBlockToSampleAlleleMapping(Dictionary<AlleleIndexBlock, List<SampleAllele>> alleleIndexBlockToSampleAllele, GenotypeBlock genotypeBlock, List<int> sampleIndexes)
        {
            if (genotypeBlock == null) return;
            var genotypeIndexBlocks = genotypeBlock.Genotypes;
            for (int alleleIndex = 0; alleleIndex < genotypeIndexBlocks.Length; alleleIndex++)
            {
                var genotypeIndexBlock = genotypeIndexBlocks[alleleIndex].ToList();
                if (genotypeIndexBlock.Count <= 1) continue;
                var alleleIndexBlock = new AlleleIndexBlock(genotypeBlock.PosIndex, genotypeIndexBlock);
                var currentSampleAlleles = GetSampleAlleles(sampleIndexes, (byte)alleleIndex);

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
            for (int i = 0; i < sampleIndexes.Count; i++)
            {
                sampleAlleleList[i] = new SampleAllele(sampleIndexes[i], alleleIndex);
            }
            return sampleAlleleList.ToList();
        }

        internal static int GetMaxPloidy(string genotypes)
        {
            int ploidy = -1;
            foreach (var genotype in genotypes.Split(";"))
            {
                var currentPloidy = string.IsNullOrEmpty(genotype) || genotype == "." ? 0 : genotype.Split('|', '/').Length;
                if (currentPloidy > ploidy) ploidy = currentPloidy;
            }
            return ploidy;
        }

        internal static void TrimTrailingRefAlleles(List<int>[] blocks)
        {
            if (blocks.Length == 0) return;
            for (int index = blocks[0].Count - 1; index >= 0; index--)
            {
                if (blocks.Any(x => x[index] != 0)) break;
                foreach (var block in blocks) block.RemoveAt(index);
            }
        }

        private static bool HasReducedPloidy(string[] genotypeIndexes, int blockPloidy) => genotypeIndexes.Length < blockPloidy;

        private static bool HasUndeterminedOrUnsupportedGenotype(string[] genotypeIndexes, HashSet<string> indexOfSkippedTypes) => genotypeIndexes.Any(x => x == "." || indexOfSkippedTypes.Contains(x));

        private static bool IsUnphasedHeterozygote(string genotype)
        {
            var alleleIndexes = genotype.Split('/');
            return alleleIndexes.Length > 1 && alleleIndexes.Distinct().Count() > 1;
        }

        public bool Equals(AlleleIndexBlock other) => PositionIndex == other.PositionIndex && AlleleIndexes.SequenceEqual(other.AlleleIndexes);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = PositionIndex.GetHashCode();
                AlleleIndexes.ForEach(x => hashCode = (hashCode * 1201) ^ x.GetHashCode());
                return hashCode;
            }
        }
    }

    public sealed class GenotypeBlock
    {
        public readonly int PosIndex;
        public readonly List<int>[] Genotypes;

        private GenotypeBlock(int posIndex, int ploidy)
        {
            PosIndex = posIndex;
            Genotypes = new List<int>[ploidy];
            for (var i = 0; i < ploidy; i++) Genotypes[i] = new List<int>();
        }

        private GenotypeBlock(int posIndex, List<int>[] genotypes)
        {
            PosIndex = posIndex;
            Genotypes = genotypes;
        }

        public static GenotypeBlock CreateWithGenotypes(int posIndex, string[] genotypeIndexes)
        {
            var blockInfo = new GenotypeBlock(posIndex, genotypeIndexes.Length);
            blockInfo.AddAlleleIndexes(genotypeIndexes);
            return blockInfo;
        }

        public void AddAlleleIndexes(string[] genotypeIndexes)
        {
            int ploidy = genotypeIndexes.Length;
            if (ploidy != Genotypes.Length) throw new Exception($"The ploidy of provided genotype indexes ({ploidy}) doesn't that of the block ({Genotypes.Length}) ");
            for (var alleleIndex = 0; alleleIndex < ploidy; alleleIndex++)
                Genotypes[alleleIndex].Add(int.Parse(genotypeIndexes[alleleIndex]));
        }

        public IEnumerable<GenotypeBlock> Split(int[] starts, List<int> functionBlockRanges)
        {
            int numBreaks = Genotypes[0].Count - 1;
            if (numBreaks == 0) return new List<GenotypeBlock>();
            int ploidy = Genotypes.Length;
            var breaks = new bool[ploidy][];
            for (int i = 0; i < ploidy; i++)
            {
                breaks[i] = GetAlleleBreaks(Genotypes[i], starts, functionBlockRanges);
            }
            bool[] finalBreaks = GetFinalBreaks(breaks);
            return GetGenotypeBlocks(finalBreaks);
        }

        private bool[] GetAlleleBreaks(List<int> alleleGenotypes, int[] starts, List<int> functionBlockRanges)
        {
            int numBreaks = Genotypes[0].Count - 1;
            var alleleBreaks = new bool[numBreaks];
            int lastNonRefPosition = -1;

            // function block checking
            for (int gtIndex = 0; gtIndex < Genotypes[0].Count; gtIndex++)
            {
                int genotype = alleleGenotypes[gtIndex];
                if (genotype == 0)
                {
                    // check leading ref positions
                    if (gtIndex == 0 || alleleBreaks[gtIndex - 1] && gtIndex < numBreaks)
                    {
                        alleleBreaks[gtIndex] = true;
                    }
                    // check tailing ref positions 
                    else if (gtIndex == numBreaks)
                    {
                        MakeBreakAndCheckTailingRefPositions(gtIndex - 1, alleleBreaks, alleleGenotypes);
                    }
                }
                else
                {
                    if (gtIndex > 0)
                    {
                        bool outOfRange = lastNonRefPosition != -1 && starts[PosIndex + gtIndex] > functionBlockRanges[lastNonRefPosition];
                        if (outOfRange)
                            MakeBreakAndCheckTailingRefPositions(gtIndex - 1, alleleBreaks, alleleGenotypes);
                    }
                    lastNonRefPosition = PosIndex + gtIndex;
                }
            }
            return alleleBreaks;
        }

        private static void MakeBreakAndCheckTailingRefPositions(int breakIndex, bool[] alleleBreaks, List<int> alleleGenotypes)
        {
            alleleBreaks[breakIndex] = true;
            // check "tailing" referece positions before this break
            for (int i = breakIndex - 1; i >= 0; i--)
            {
                // stop when the break bool is true or the allele is non-ref
                if (alleleBreaks[i] || alleleGenotypes[i + 1] != 0) break;
                alleleBreaks[i] = true;
            }
        }

        private List<GenotypeBlock> GetGenotypeBlocks(bool[] finalBreaks)
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
                        subGenotypeBlocks.Add(GetSubBlock(subBlockStart, subBlockEnd));
                    }
                    subBlockStart = i + 1;
                    subBlockEnd = i + 1;
                }
            }
            if (subBlockEnd - subBlockStart >= 1) subGenotypeBlocks.Add(GetSubBlock(subBlockStart, subBlockEnd));
            return subGenotypeBlocks;
        }

        private GenotypeBlock GetSubBlock(int subBlockStart, int subBlockEnd)
        {
            int newPosIndex = PosIndex + subBlockStart;
            int ploidy = Genotypes.Length;
            int numPositions = subBlockEnd - subBlockStart + 1;
            var newGenotypes = new List<int>[ploidy];
            for (int i = 0; i < ploidy; i++)
            {
                newGenotypes[i] = new ArraySegment<int>(Genotypes[i].ToArray(), subBlockStart, numPositions).ToList();
            }
            return new GenotypeBlock(newPosIndex, newGenotypes);
        }

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
    }
}