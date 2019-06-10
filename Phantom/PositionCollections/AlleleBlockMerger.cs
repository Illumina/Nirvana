using System;
using System.Collections.Generic;
using System.Linq;
using Phantom.Graph;

namespace Phantom.PositionCollections
{
    public static class AlleleBlockMerger
    {
        public static Dictionary<AlleleBlock, List<SampleHaplotype>> Merge(
            Dictionary<AlleleBlock, List<SampleHaplotype>> inputBlocks, Graph<AlleleBlock> alleleIndexBlockGraph)
        {
            var alleleToConnectedComponent = alleleIndexBlockGraph.FindAllConnectedComponents();
            var connectedComponentToAlleles = Graph<AlleleBlock>.GetComponentToMembers(alleleToConnectedComponent);

            var mergedBlocksToSampleHaplotype = new Dictionary<AlleleBlock, List<SampleHaplotype>>();
            var seedBlocks = new LinkedList<AlleleBlock>();

            foreach (var (_, alleleBlocks) in connectedComponentToAlleles.OrderBy(x => x.Key))
            {
                var blockProcessMethod = CheckAlleleBlocks(alleleBlocks, seedBlocks);
                foreach (var alleleBlock in alleleBlocks)
                {
                    var sampleHaplotypes = inputBlocks[alleleBlock];
                    var processedAlleleBlock = blockProcessMethod(alleleBlock);

                    if (mergedBlocksToSampleHaplotype.TryGetValue(processedAlleleBlock, out var processedSampleHaplotypes))
                    {
                        foreach (var sampleHaplotype in sampleHaplotypes)
                            processedSampleHaplotypes.Add(sampleHaplotype);
                    }
                    else
                    {
                        mergedBlocksToSampleHaplotype.Add(processedAlleleBlock, sampleHaplotypes);
                    }
                }
            }

            return mergedBlocksToSampleHaplotype;
        }

        private static Func<AlleleBlock, AlleleBlock> CheckAlleleBlocks(IReadOnlyList<AlleleBlock> alleleBlocks, LinkedList<AlleleBlock> seedBlocks)
        {
            var seedBlock = seedBlocks.First;
            while (seedBlock != null) 
            {
                var nextBlock = seedBlock.Next;
                if (SeedOutOfRange(alleleBlocks[0], seedBlock.Value))
                {
                    seedBlocks.Remove(seedBlock);
                    seedBlock = nextBlock;
                    continue;
                }

                foreach (var alleleBlock in alleleBlocks)
                {
                    
                    if (CanBeSame(alleleBlock, seedBlock.Value, out int missingBasesBefore, out int missingBasesAfter))
                        return ExtendAlleleBlockWithGivenNums(missingBasesBefore, missingBasesAfter);
                }
                seedBlock = nextBlock;
            }

            // seed block is not extended
            var blockProcessMethod = ExtendAlleleBlockWithGivenNums(0, 0);
            foreach (var alleleBlock in alleleBlocks)
            {
                seedBlocks.AddLast(blockProcessMethod(alleleBlock));
            }
            return ExtendAlleleBlockWithGivenNums(0, 0);
        }

        private static bool SeedOutOfRange(AlleleBlock alleleBlock, AlleleBlock seedBlock) => seedBlock.PositionIndex + seedBlock.AlleleIndexes.Length - 1 < alleleBlock.PositionIndex;


        private static bool CanBeSame(AlleleBlock alleleBlock, AlleleBlock seedBlock, out int missingBasesBefore, out int missingBasesAfter)
        {
            missingBasesBefore = 0;
            missingBasesAfter = 0;
            int endIndexSeedBlock = seedBlock.PositionIndex + seedBlock.AlleleIndexes.Length - 1;
            int endIndexThisBlock = alleleBlock.PositionIndex + alleleBlock.AlleleIndexes.Length - 1;
            if (endIndexThisBlock > endIndexSeedBlock) return false;

            missingBasesBefore = alleleBlock.PositionIndex - seedBlock.PositionIndex;
            if (alleleBlock.NumRefPositionsBefore < missingBasesBefore) return false;

            missingBasesAfter = endIndexSeedBlock - endIndexThisBlock;
            if (alleleBlock.NumRefPositionsAfter < missingBasesAfter) return false;

            for (var i = 0; i < missingBasesBefore; i++)
            {
                if (seedBlock.AlleleIndexes[i] != 0) return false;
            }

            for (var i = 0; i < alleleBlock.AlleleIndexes.Length; i++)
            {
                if (alleleBlock.AlleleIndexes[i] != seedBlock.AlleleIndexes[i + missingBasesBefore])
                    return false;
            }

            for (int i = missingBasesBefore + alleleBlock.AlleleIndexes.Length;
                i < seedBlock.AlleleIndexes.Length;
                i++)
            {
                if (seedBlock.AlleleIndexes[i] != 0) return false;
            }

            return true;
        }

        private static Func<AlleleBlock, AlleleBlock> ExtendAlleleBlockWithGivenNums(int extendBefore,
            int extendAfter) => origBlock => ExtendAlleleBlock(origBlock, extendBefore, extendAfter);

        internal static AlleleBlock ExtendAlleleBlock(AlleleBlock alleleBlock, int extendBefore, int extendAfter)
        {
            if (extendBefore > alleleBlock.NumRefPositionsBefore)
                throw new ArgumentOutOfRangeException($"Can't extend an allele block with {alleleBlock.NumRefPositionsBefore} trimmed leading reference positions by adding {extendBefore} reference positions before it");
            if (extendAfter > alleleBlock.NumRefPositionsAfter)
                throw new ArgumentOutOfRangeException($"Can't extend an allele block with {alleleBlock.NumRefPositionsAfter} trimmed trailing reference positions by adding {extendAfter} reference positions after it");
            int lengthOldAlleleIndexes = alleleBlock.AlleleIndexes.Length;
            var newAlleleIndexes = new int[extendBefore + lengthOldAlleleIndexes + extendAfter];
            Array.Copy(alleleBlock.AlleleIndexes, 0, newAlleleIndexes, extendBefore, lengthOldAlleleIndexes);
            return new AlleleBlock(alleleBlock.PositionIndex - extendBefore, newAlleleIndexes, -1, -1);
        }
    }
}