using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using Phantom.DataStructures;
using Phantom.Interfaces;
using Phantom.Workers;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;

namespace Phantom.Workers
{
    public sealed class VariantGenerator : IVariantGenerator
    {
        private readonly ISequenceProvider _sequenceProvider;
        private const string FailedFilterTag = "FilteredVariantsRecomposed";

        public VariantGenerator(ISequenceProvider sequenceProvider)
        {
            _sequenceProvider = sequenceProvider;
        }

        public IEnumerable<ISimplePosition> Recompose(List<ISimplePosition> recomposablePositions, List<int> functionBlockRanges)
        {
            var positionSet = PositionSet.CreatePositionSet(recomposablePositions, functionBlockRanges);
            var alleleSet = positionSet.AlleleSet;
            var alleleIndexBlockToSampleIndex = positionSet.AlleleIndexBlockToSampleIndex;
            var numSamples = positionSet.NumSamples;
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
                recomposablePositions[indexTuple.PosIndex].IsDecomposed[indexTuple.VarIndex] = true;
            }
            return recomposedAlleleSet.GetRecomposedVcfRecords().Select(x => SimplePosition.GetSimplePosition(x, _sequenceProvider.RefNameToChromosome, true));
        }

        private static VariantInfo GetVariantInfo(PositionSet positionSet, AlleleIndexBlock alleleIndexBlock)
        {
            string filter = "PASS";
            var positions = positionSet.SimplePositions;
            var startIndex = alleleIndexBlock.PositionIndex;
            var numPositions = alleleIndexBlock.AlleleIndexes.Count;
            var numSamples = positionSet.NumSamples;

            string[] quals = new string[numPositions];
            for (int i = startIndex; i < startIndex + numPositions; i++)
            {
                quals[i - startIndex] = positions[i].VcfFields[VcfCommon.QualIndex];
                string thisFilter = positions[i].VcfFields[VcfCommon.FilterIndex];
                if (filter == "PASS" && thisFilter != "PASS" && thisFilter != ".")
                    filter = FailedFilterTag;
            }
            string qual = GetStringWithMinValueOrDot(quals);

            string[] gqValues = new string[numSamples];
            for (int i = 0; i < numSamples; i++)
                gqValues[i] = GetStringWithMinValueOrDot(new ArraySegment<string>(positionSet.GqInfo[i], startIndex, numPositions).ToArray());

            string[] psValues = new string[numSamples];
            for (int i = 0; i < numSamples; i++)
                // PS tags are the same in the decomposed variants
                psValues[i] = positionSet.PsInfo[i][startIndex];

            return new VariantInfo(qual, filter, gqValues, psValues);
        }

        private static string GetStringWithMinValueOrDot(string[] strings)
        {
            string currentString = ".";
            float currentValue = float.MaxValue;
            foreach (string thisString in strings)
            {
                if (thisString != ".")
                {
                    var thisValue = float.Parse(thisString);
                    if (thisValue >= currentValue) continue;
                    currentString = thisString;
                    currentValue = thisValue;
                }
            }
            return currentString;
        }

        internal static (int Start, int End, string Ref, string Alt) GetPositionsAndRefAltAlleles(AlleleIndexBlock alleleIndexBlock, AlleleSet alleleSet, string totalRefSequence, int regionStart, HashSet<(int, int)> decomposedPosVarIndex)
        {
            int numPositions = alleleIndexBlock.AlleleIndexes.Count;
            int firstPositionIndex = alleleIndexBlock.PositionIndex;
            int lastPositionIndex = alleleIndexBlock.PositionIndex + numPositions - 1;

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
                int alleleIndex = alleleIndexBlock.AlleleIndexes[indexInBlock];
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

                string refSequenceBefore =
                    refSequence.Substring(refSequenceStart, refRegionBetweenTwoAltAlleles);
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

        public int CompareTo(VariantSite that) => Start != that.Start ? Start.CompareTo(that.Start) : string.Compare(RefAllele, that.RefAllele, StringComparison.Ordinal);
    }

    public sealed class VariantInfo
    {
        public readonly string Qual;
        public readonly string Filter;
        public readonly string[] SampleGqs;
        public readonly string[] SamplePhaseSets;
        public readonly Dictionary<string, List<SampleAllele>> AltAlleleToSample = new Dictionary<string, List<SampleAllele>>();

        public VariantInfo(string qual, string filter, string[] sampleGqs, string[] samplePhaseSets)
        {
            Qual = qual;
            Filter = filter;
            SampleGqs = sampleGqs;
            SamplePhaseSets = samplePhaseSets;
        }

        public void AddAllele(string altAllele, List<SampleAllele> sampleAlleles)
        {
            AltAlleleToSample.Add(altAllele, sampleAlleles);
        }
    }

}

internal sealed class RecomposedAlleleSet
{
    public Dictionary<VariantSite, VariantInfo> RecomposedAlleles;
    private readonly int _numSamples;
    private readonly string _chrName;
    private const string VariantId = ".";
    private const string InfoTag = "RECOMPOSED";


    public RecomposedAlleleSet(string chrName, int numSamples)
    {
        _numSamples = numSamples;
        _chrName = chrName;
        RecomposedAlleles = new Dictionary<VariantSite, VariantInfo>();
    }

    public List<string[]> GetRecomposedVcfRecords()
    {
        var vcfRecords = new List<string[]>();
        foreach (var variantSite in RecomposedAlleles.Keys.OrderBy(x => x))
        {
            var varInfo = RecomposedAlleles[variantSite];
            var altAlleleList = new List<string>();
            int genotypeIndex = 1; // genotype index of alt allele
            var sampleGenotypes = new List<int>[_numSamples];
            for (int i = 0; i < _numSamples; i++) sampleGenotypes[i] = new List<int>();
            foreach (var altAllele in varInfo.AltAlleleToSample.Keys.OrderBy(x => x))
            {
                var sampleAlleles = varInfo.AltAlleleToSample[altAllele];
                int currentGenotypeIndex;
                if (altAllele == variantSite.RefAllele)
                {
                    currentGenotypeIndex = 0;
                }
                else
                {
                    currentGenotypeIndex = genotypeIndex;
                    genotypeIndex++;
                    altAlleleList.Add(altAllele);
                }
                foreach (var sampleAllele in sampleAlleles)
                {
                    SetGenotypeWithAlleleIndex(sampleGenotypes[sampleAllele.SampleIndex], sampleAllele.AlleleIndex,
                        currentGenotypeIndex);
                }
            }
            var altAlleleColumn = string.Join(",", altAlleleList);
            vcfRecords.Add(GetVcfFields(variantSite, altAlleleColumn, varInfo.Qual, varInfo.Filter, sampleGenotypes, varInfo.SampleGqs, varInfo.SamplePhaseSets));
        }

        return vcfRecords;
    }

    private void SetGenotypeWithAlleleIndex(List<int> sampleGenotype, byte sampleAlleleAlleleIndex, int currentGenotypeIndex)
    {
        if (sampleGenotype.Count == sampleAlleleAlleleIndex)
        {
            sampleGenotype.Add(currentGenotypeIndex);
            return;
        }

        if (sampleGenotype.Count < sampleAlleleAlleleIndex)
        {
            int extraSpace = sampleAlleleAlleleIndex - sampleGenotype.Count + 1;
            sampleGenotype.AddRange(Enumerable.Repeat(-1, extraSpace));
        }
        sampleGenotype[sampleAlleleAlleleIndex] = currentGenotypeIndex;
    }

    private string[] GetVcfFields(VariantSite varSite, string altAlleleColumn, string qual, string filter, List<int>[] sampleGenoTypes, string[] sampleGqs, string[] samplePhasesets, string variantId = VariantId, string info = InfoTag)
    {
        var vcfFields = new List<string>
            {
                _chrName,
                varSite.Start.ToString(),
                variantId,
                varSite.RefAllele,
                altAlleleColumn,
                qual,
                filter,
                info
            };

        AddFormatAndSampleColumns(sampleGenoTypes, sampleGqs, samplePhasesets, ref vcfFields);
        return vcfFields.ToArray();
    }

    private static void AddFormatAndSampleColumns(List<int>[] sampleGenoTypes, string[] sampleGqs, string[] samplePhasesets, ref List<string> vcfFields)
    {
        var formatTags = "GT";
        var hasGq = false;
        var hasPs = false;
        int numSamples = sampleGenoTypes.Length;

        var sampleGenotypeStrings = new string[numSamples];
        for (var index = 0; index < numSamples; index++)
        {
            sampleGenotypeStrings[index] = GetGenotype(sampleGenoTypes[index]);
            if (sampleGenotypeStrings[index] == ".") continue;
            if (sampleGqs[index] != ".") hasGq = true;
            if (samplePhasesets[index] != ".") hasPs = true;
            if (hasGq && hasPs) break;
        }

        int numFields = 1;

        if (hasGq)
        {
            formatTags += ":GQ";
            numFields++;
        }
        if (hasPs)
        {
            formatTags += ":PS";
            numFields++;
        }

        vcfFields.Add(formatTags);

        for (var index = 0; index < numSamples; index++)
        {
            var sampleGenotypeStr = sampleGenotypeStrings[index];
            if (sampleGenotypeStr == ".") vcfFields.Add(".");
            else
            {
                var nonMissingFields = new string[numFields];
                nonMissingFields[0] = sampleGenotypeStr;
                var fieldIndex = 1;
                if (hasGq)
                {
                    nonMissingFields[fieldIndex] = sampleGqs[index];
                    fieldIndex++;
                }
                if (hasPs)
                {
                    nonMissingFields[fieldIndex] = samplePhasesets[index];
                }

                var sampleColumnStr = string.Join(":", TrimTrailingMissValues(nonMissingFields));
                vcfFields.Add(sampleColumnStr);
            }
        }
    }

    private static string[] TrimTrailingMissValues(string[] values)
    {
        int indexLastRemainedValue = values.Length - 1;
        // Need to have at least one value remained
        for (; indexLastRemainedValue > 0; indexLastRemainedValue--)
        {
            if (values[indexLastRemainedValue] != ".") break;
        }
        return new ArraySegment<string>(values, 0, indexLastRemainedValue + 1).ToArray();
    }

    private static string GetGenotype(List<int> sampleGenotype) => sampleGenotype.Count == 0 ? "." : string.Join("|", sampleGenotype);
}