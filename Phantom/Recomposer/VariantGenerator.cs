using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using Phantom.PositionCollections;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;

namespace Phantom.Recomposer
{
    public sealed class VariantGenerator : IVariantGenerator
    {
        private readonly ISequenceProvider _sequenceProvider;
        private const string FailedFilterTag = "FilteredVariantsRecomposed";

        public VariantGenerator(ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
        }

        public IEnumerable<ISimplePosition> Recompose(List<ISimplePosition> simplePositions, List<int> functionBlockRanges)
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
            var decomposedPosVarIndex = new HashSet<(int PosIndex, int VarIndex)>();
            foreach (var (alleleIndexBlock, sampleAlleles) in alleleIndexBlockToSampleIndex)
            {
                var (start, _, refAllele, altAllele) = GetPositionsAndRefAltAlleles(alleleIndexBlock, alleleSet, totalRefSequence, regionStart, decomposedPosVarIndex);
                var variantSite = new VariantSite(start, refAllele);

                if (!recomposedAlleleSet.RecomposedAlleles.TryGetValue(variantSite, out var variantInfo))
                {
                    variantInfo = GetVariantInfo(positionSet, alleleIndexBlock);
                    recomposedAlleleSet.RecomposedAlleles[variantSite] = variantInfo;
                }
                variantInfo.AddAllele(altAllele, sampleAlleles);
            }
            // Set decomposed tag to positions used for recomposition
            foreach (var indexTuple in decomposedPosVarIndex)
            {
                simplePositions[indexTuple.PosIndex].IsDecomposed[indexTuple.VarIndex] = true;
            }
            return recomposedAlleleSet.GetRecomposedVcfRecords().Select(x => SimplePosition.GetSimplePosition(x, new NullVcfFilter(), _sequenceProvider.RefNameToChromosome, true));
        }

        private static VariantInfo GetVariantInfo(PositionSet positionSet, AlleleBlock alleleBlock)
        {
            var positions = positionSet.SimplePositions;
            int startIndex = alleleBlock.PositionIndex;
            int numPositions = alleleBlock.AlleleIndexes.Length;
            int numSamples = positionSet.NumSamples;

            string qual = GetStringWithMinValueOrDot(Enumerable.Range(startIndex, numPositions).Select(x => positions[x].VcfFields[VcfCommon.QualIndex]));
            string filter = Enumerable.Range(startIndex, numPositions)
                            .Select(i => positions[i].VcfFields[VcfCommon.FilterIndex])
                            .Any(x => x != "PASS" && x != ".") ? FailedFilterTag : "PASS";

            var gqValues = new string[numSamples];
            for (var i = 0; i < numSamples; i++)
                gqValues[i] = GetStringWithMinValueOrDot(new ArraySegment<string>(positionSet.GqInfo.Values[i], startIndex, numPositions).ToArray());

            var psValues = new string[numSamples];
            for (var i = 0; i < numSamples; i++)
            {
                var psTagsThisSample =
                    new ArraySegment<string>(positionSet.PsInfo.Values[i], startIndex, numPositions);
                var isHomozygous = new ArraySegment<bool>(positionSet.GtInfo.Values[i].Select(x => x.IsHomozygous).ToArray(), startIndex, numPositions);
                psValues[i] = GetPhaseSetForRecomposedVarint(psTagsThisSample, isHomozygous);
            }

            return new VariantInfo(qual, filter, gqValues, psValues);
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

        private static string GetPhaseSetForRecomposedVarint(IEnumerable<string> psTagsThisSample, IEnumerable<bool> isHomozygous)
        {
            foreach (var (psTag, homozygousity) in psTagsThisSample.Zip(isHomozygous, (a, b) => new Tuple<string, bool>(a, b)))
            {
                if (!homozygousity) return psTag;
            }
            return ".";
        }

        internal static (int Start, int End, string Ref, string Alt) GetPositionsAndRefAltAlleles(AlleleBlock alleleBlock, AlleleSet alleleSet, string totalRefSequence, int regionStart, HashSet<(int, int)> decomposedPosVarIndex)
        {
            int numPositions = alleleBlock.AlleleIndexes.Length;
            int firstPositionIndex = alleleBlock.PositionIndex;
            int lastPositionIndex = alleleBlock.PositionIndex + numPositions - 1;

            int blockStart = alleleSet.Starts[firstPositionIndex];
            int blockEnd = alleleSet.Starts[lastPositionIndex];
            string lastRefAllele = alleleSet.VariantArrays[lastPositionIndex][0];
            int blockRefLength = blockEnd - blockStart + lastRefAllele.Length;
            var refSequence = totalRefSequence.Substring(blockStart - regionStart, blockRefLength);

            int refSequenceStart = 0;
            var altSequenceSegsegments = new LinkedList<string>();
            for (int positionIndex = firstPositionIndex; positionIndex <= lastPositionIndex; positionIndex++)
            {
                int indexInBlock = positionIndex - firstPositionIndex;
                int alleleIndex = alleleBlock.AlleleIndexes[indexInBlock];
                if (alleleIndex == 0) continue;

                //only mark positions with non-reference alleles being recomposed as "decomposed"
                // alleleIndex is 1-based for altAlleles
                decomposedPosVarIndex.Add((positionIndex, alleleIndex - 1));
                string refAllele = alleleSet.VariantArrays[positionIndex][0];
                string altAllele = alleleSet.VariantArrays[positionIndex][alleleIndex];
                int positionOnRefSequence = alleleSet.Starts[positionIndex] - blockStart;
                int refRegionBetweenTwoAltAlleles = positionOnRefSequence - refSequenceStart;

                if (refRegionBetweenTwoAltAlleles < 0)
                {
                    string previousAltAllele = alleleSet.VariantArrays[positionIndex - 1][alleleIndex];
                    throw new UserErrorException($"Conflicting alternative alleles identified at {alleleSet.Chromosome.UcscName}:{alleleSet.Starts[positionIndex]}: both \"{previousAltAllele}\" and \"{altAllele}\" are present.");
                }

                string refSequenceBefore = refSequence.Substring(refSequenceStart, refRegionBetweenTwoAltAlleles);
                altSequenceSegsegments.AddLast(refSequenceBefore);
                altSequenceSegsegments.AddLast(altAllele);
                refSequenceStart = positionOnRefSequence + refAllele.Length;
            }
            altSequenceSegsegments.AddLast(refSequence.Substring(refSequenceStart));
            return (blockStart, blockStart + blockRefLength - 1, refSequence, string.Concat(altSequenceSegsegments));
        }
    }

    public struct VariantSite : IComparable<VariantSite>
    {
        public readonly int Start;
        public readonly string RefAllele;

        public VariantSite(int start, string refAllele)
        {
            Start = start;
            RefAllele = refAllele;
        }

        public int CompareTo(VariantSite other) => Start != other.Start ? Start.CompareTo(other.Start) : string.Compare(RefAllele, other.RefAllele, StringComparison.Ordinal);
    }

    public sealed class VariantInfo
    {
        public readonly string Qual;
        public readonly string Filter;
        public readonly string[] SampleGqs;
        public readonly string[] SamplePhaseSets;
        public readonly Dictionary<string, List<SampleHaplotype>> AltAlleleToSample = new Dictionary<string, List<SampleHaplotype>>();

        public VariantInfo(string qual, string filter, string[] sampleGqs, string[] samplePhaseSets)
        {
            Qual = qual;
            Filter = filter;
            SampleGqs = sampleGqs;
            SamplePhaseSets = samplePhaseSets;
        }

        public void AddAllele(string altAllele, List<SampleHaplotype> sampleAlleles)
        {
            AltAlleleToSample.Add(altAllele, sampleAlleles);
        }
    }
}