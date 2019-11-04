using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using Phantom.PositionCollections;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;
using Vcf.VariantCreator;

namespace Phantom.Recomposer
{
    public sealed class VariantGenerator : IVariantGenerator
    {
        private readonly ISequenceProvider _sequenceProvider;

        public VariantGenerator(ISequenceProvider sequenceProvider) => _sequenceProvider = sequenceProvider;

        public IEnumerable<ISimplePosition> Recompose(List<ISimplePosition> simplePositions,
            List<int> functionBlockRanges)
        {
            var positionSet = PositionSet.CreatePositionSet(simplePositions, functionBlockRanges);
            var alleleSet = positionSet.AlleleSet;
            var alleleIndexBlockToSampleIndex = positionSet.AlleleBlockToSampleHaplotype;
            int numSamples = positionSet.NumSamples;
            _sequenceProvider.LoadChromosome(alleleSet.Chromosome);
            int regionStart = alleleSet.Starts[0];
            string lastRefAllele = alleleSet.VariantArrays.Last()[0];
            int regionEnd = alleleSet.Starts.Last() + lastRefAllele.Length + 100; // make it long enough
            if (regionEnd > _sequenceProvider.Sequence.Length) regionEnd = _sequenceProvider.Sequence.Length;
            string totalRefSequence = _sequenceProvider.Sequence.Substring(regionStart - 1, regionEnd - regionStart); // VCF positions are 1-based
            var recomposedAlleleSet = new RecomposedAlleleSet(positionSet.ChrName, numSamples);

            foreach (var (alleleIndexBlock, sampleAlleles) in alleleIndexBlockToSampleIndex)
            {
                (int start, _, string refAllele, string altAllele, var varPosIndexesInAlleleBlock,
                    List<string> decomposedVids) = GetPositionsAndRefAltAlleles(alleleIndexBlock, alleleSet,
                    totalRefSequence, regionStart, simplePositions, _sequenceProvider.Sequence);

                if (start == default) continue;

                var variantSite = new VariantSite(start, refAllele);
                if (!recomposedAlleleSet.RecomposedAlleles.TryGetValue(variantSite, out var variantInfo))
                {
                    variantInfo = GetVariantInfo(positionSet, alleleIndexBlock);
                    recomposedAlleleSet.RecomposedAlleles[variantSite] = variantInfo;
                }
                variantInfo.AddAllele(altAllele, sampleAlleles, decomposedVids);
                variantInfo.UpdateSampleFilters(varPosIndexesInAlleleBlock, sampleAlleles);
            }

            DeDuplicateLinkedVids(simplePositions);

            return recomposedAlleleSet.GetRecomposedPositions(_sequenceProvider.RefNameToChromosome);
        }

        private static void DeDuplicateLinkedVids(List<ISimplePosition> simplePositions)
        {
            foreach (var simplePosition in simplePositions)
            {
                for (var i = 0; i < simplePosition.LinkedVids.Length; i++)
                {
                    if (simplePosition.LinkedVids[i] == null) continue; // perhaps only needed because of mocked unit tests
                    simplePosition.LinkedVids[i] = simplePosition.LinkedVids[i].Distinct().ToList();
                }
            }
        }

        private static VariantInfo GetVariantInfo(PositionSet positionSet, AlleleBlock alleleBlock)
        {
            var positions = positionSet.SimplePositions;
            int startIndex = alleleBlock.PositionIndex;
            int numPositions = alleleBlock.AlleleIndexes.Length;
            int numSamples = positionSet.NumSamples;

            string qual = GetStringWithMinValueOrDot(Enumerable.Range(startIndex, numPositions)
                .Select(x => positions[x].VcfFields[VcfCommon.QualIndex]));
            var filters = Enumerable.Range(startIndex, numPositions)
                .Select(i => positions[i].VcfFields[VcfCommon.FilterIndex])
                .ToArray();

            var gqValues = new string[numSamples];
            for (var i = 0; i < numSamples; i++)
                gqValues[i] = GetStringWithMinValueOrDot(
                    new ArraySegment<string>(positionSet.GqInfo.Values[i], startIndex, numPositions).ToArray());

            var psValues = new string[numSamples];
            for (var i = 0; i < numSamples; i++)
            {
                var psTagsThisSample =
                    new ArraySegment<string>(positionSet.PsInfo.Values[i], startIndex, numPositions);
                var isHomozygous = new ArraySegment<bool>(
                    positionSet.GtInfo.Values[i].Select(x => x.IsHomozygous).ToArray(), startIndex, numPositions);
                psValues[i] = GetPhaseSetForRecomposedVariant(psTagsThisSample, isHomozygous);
            }

            var homoReferenceSamplePloidy = new int?[numSamples];
            for (var i = 0; i < numSamples; i++)
            {
                if (Genotype.IsAllHomozygousReference(positionSet.GtInfo.Values[i], startIndex, numPositions))
                    homoReferenceSamplePloidy[i] = positionSet.GtInfo.Values[i][startIndex].AlleleIndexes.Length;
            }

            var sampleFilters = new List<bool>[numSamples];
            for (var i = 0; i < numSamples; i++)
            {
                sampleFilters[i] = new List<bool>();
            }

            return new VariantInfo(qual, filters, gqValues, psValues, homoReferenceSamplePloidy, sampleFilters);
        }

        private static string GetStringWithMinValueOrDot(IEnumerable<string> strings)
        {
            var currentString = ".";
            float currentValue = float.MaxValue;
            foreach (string thisString in strings)
            {
                if (thisString == ".") continue;

                float thisValue = float.Parse(thisString);
                if (thisValue >= currentValue) continue;
                currentString = thisString;
                currentValue = thisValue;
            }

            return currentString;
        }

        private static string GetPhaseSetForRecomposedVariant(IEnumerable<string> psTagsThisSample,
            IEnumerable<bool> isHomozygous)
        {
            foreach ((string psTag, bool homozygosity) in psTagsThisSample.Zip(isHomozygous,
                (a, b) => new Tuple<string, bool>(a, b)))
            {
                if (!homozygosity) return psTag;
            }

            return ".";
        }

        internal static (int Start, int End, string Ref, string Alt, List<int> VarPosIndexesInAlleleBlock, List<string>
            decomposedVids) GetPositionsAndRefAltAlleles(AlleleBlock alleleBlock, AlleleSet alleleSet,
                string totalRefSequence, int regionStart, List<ISimplePosition> simplePositions, ISequence sequence)
        {
            int numPositions = alleleBlock.AlleleIndexes.Length;
            int firstPositionIndex = alleleBlock.PositionIndex;
            int lastPositionIndex = alleleBlock.PositionIndex + numPositions - 1;

            int blockStart = alleleSet.Starts[firstPositionIndex];
            int blockEnd = alleleSet.Starts[lastPositionIndex];
            string lastRefAllele = alleleSet.VariantArrays[lastPositionIndex][0];
            int blockRefLength = blockEnd - blockStart + lastRefAllele.Length;
            string refSequence = totalRefSequence.Substring(blockStart - regionStart, blockRefLength);

            var refSequenceStart = 0;
            var altSequenceSegments = new LinkedList<string>();
            var variantPosIndexesInAlleleBlock = new List<int>();
            var vidListsNeedUpdate = new List<List<string>>();
            var decomposedVids = new List<string>();

            if (FindConflictAllele(alleleBlock, alleleSet)) return default;

            for (int positionIndex = firstPositionIndex; positionIndex <= lastPositionIndex; positionIndex++)
            {
                int indexInBlock = positionIndex - firstPositionIndex;
                int alleleIndex = alleleBlock.AlleleIndexes[indexInBlock];
                //only non-reference alleles considered
                if (alleleIndex == 0) continue;

                string refAllele = alleleSet.VariantArrays[positionIndex][0];
                string altAllele = alleleSet.VariantArrays[positionIndex][alleleIndex];
                int positionOnRefSequence = alleleSet.Starts[positionIndex] - blockStart;
                int refRegionBetweenTwoAltAlleles = positionOnRefSequence - refSequenceStart;

                // skip this position if it is the same as previous one
                if (refRegionBetweenTwoAltAlleles == -1) continue;

                variantPosIndexesInAlleleBlock.Add(positionIndex - firstPositionIndex);
                string refSequenceBefore = refSequence.Substring(refSequenceStart, refRegionBetweenTwoAltAlleles);
                altSequenceSegments.AddLast(refSequenceBefore);
                altSequenceSegments.AddLast(altAllele);
                refSequenceStart = positionOnRefSequence + refAllele.Length;

                if (simplePositions == null) continue;
                var thisPosition = simplePositions[positionIndex];
                // alleleIndex is 1-based for altAlleles
                int varIndex = alleleIndex - 1;

                //Only SNVs get recomposed for now
                if (thisPosition.Vids[varIndex] == null)
                {
                    thisPosition.Vids[varIndex] = VariantId.Create(sequence, VariantCategory.SmallVariant, null,
                        alleleSet.Chromosome, thisPosition.Start,
                        thisPosition.Start + thisPosition.RefAllele.Length - 1, thisPosition.RefAllele,
                        thisPosition.AltAlleles[varIndex]);
                    thisPosition.IsDecomposed[varIndex] = true;
                }

                decomposedVids.Add(thisPosition.Vids[varIndex]);

                if (thisPosition.LinkedVids[varIndex] == null)
                    thisPosition.LinkedVids[varIndex] = new List<string>();
                vidListsNeedUpdate.Add(thisPosition.LinkedVids[varIndex]);
            }

            altSequenceSegments.AddLast(refSequence.Substring(refSequenceStart));
            string recomposedAllele = string.Concat(altSequenceSegments);
            int blockRefEnd = blockStart + blockRefLength - 1;

            // trim recomposed alleles
            (int trimmedBlockStart, string trimmedRefSequence, string trimmedRecomposedAllele) = BiDirectionalTrimmer.Trim(blockStart, refSequence, recomposedAllele);
            string recomposedVariantId = VariantId.Create(sequence, VariantCategory.SmallVariant, null,
                alleleSet.Chromosome, trimmedBlockStart, trimmedBlockStart + trimmedRefSequence.Length - 1, trimmedRefSequence, trimmedRecomposedAllele);

            vidListsNeedUpdate.ForEach(x => x.Add(recomposedVariantId));
            return (blockStart, blockRefEnd, refSequence, recomposedAllele, variantPosIndexesInAlleleBlock,
                decomposedVids);
        }

        private static bool FindConflictAllele(AlleleBlock alleleBlock, AlleleSet alleleSet)
        {
            var starts = alleleSet.Starts;
            int firstPositionIndex = alleleBlock.PositionIndex;

            int previousStart = starts[firstPositionIndex];
            var allelesToCheck = new List<string>();

            int previousAlleleIndex = alleleBlock.AlleleIndexes[0];
            if (previousAlleleIndex != 0 || !IsDeletionOrMnvSite(alleleSet.VariantArrays[firstPositionIndex][0]))
                allelesToCheck.Add(alleleSet.VariantArrays[firstPositionIndex][previousAlleleIndex]);

            for (var i = 1; i < alleleBlock.AlleleIndexes.Length; i++)
            {
                int currentAlleleIndex = alleleBlock.AlleleIndexes[i];
                if (currentAlleleIndex == 0 && IsDeletionOrMnvSite(alleleSet.VariantArrays[firstPositionIndex + i][0])) continue;

                int currentStart = starts[firstPositionIndex + i];
                if (currentStart != previousStart)
                {
                    if (CheckPreviousAlleles(allelesToCheck, alleleSet.Chromosome.UcscName, previousStart)) return true;

                    previousStart = currentStart;
                    allelesToCheck.Clear();
                }

                allelesToCheck.Add(alleleSet.VariantArrays[firstPositionIndex + i][currentAlleleIndex]);
            }

            return CheckPreviousAlleles(allelesToCheck, alleleSet.Chromosome.UcscName, previousStart);
        }

        private static bool IsDeletionOrMnvSite(string refAllele) => refAllele.Length > 1;

        private static bool CheckPreviousAlleles(IEnumerable<string> allelesToCheck, string chromName, int position)
        {
            var distinctAlleles = allelesToCheck.Distinct().ToArray();
            if (distinctAlleles.Length <= 1) return false;

            Console.WriteLine($"WARNING: Conflicting alternative alleles identified at {chromName}:{position}. The following alleles are present: {string.Join(' ', distinctAlleles)}.");
            return true;
        }
    }
}