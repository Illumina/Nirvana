using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.DataStructures;
using Phantom.Interfaces;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;

namespace Phantom.Workers
{
    public sealed class VariantGenerator : IVariantGenerator
    {
        private readonly ISequenceProvider _sequenceProvider;

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
            int regionEnd = alleleSet.Starts.Last() + lastRefAllele.Length - 1;
            int totalSequenceEnd = regionEnd + 100; // make this sequence long enough
            string totalRefSequence = totalSequenceEnd > _sequenceProvider.Sequence.Length ? _sequenceProvider.Sequence.Substring(regionStart - 1, _sequenceProvider.Sequence.Length - regionStart) : _sequenceProvider.Sequence.Substring(regionStart - 1, totalSequenceEnd - regionStart); // VCF positions are 1-based
            var recomposedAlleleSet = new RecomposedAlleleSet(positionSet.ChrName, numSamples);
            var decomposedPosVarIndex = new HashSet<(int PosIndex, int VarIndex)>();
            foreach (var alleleIndexBlock in alleleIndexBlockToSampleIndex.Keys)
            {
                var (start, end, refAllele, altAllele) = GetPositionsAndRefAltAlleles(alleleIndexBlock, alleleSet, totalRefSequence, regionStart, decomposedPosVarIndex);
                var sampleAlleles = alleleIndexBlockToSampleIndex[alleleIndexBlock];
                recomposedAlleleSet.AddAllele(start, end, refAllele, altAllele, sampleAlleles);
            }
            // Set decomposed tag to positions used for recomposition
            foreach (var indexTuple in decomposedPosVarIndex)
            {
                recomposablePositions[indexTuple.PosIndex].IsDecomposed[indexTuple.VarIndex] = true;
            }
            return recomposedAlleleSet.GetRecomposedVcfRecords().Select(x => SimplePosition.GetSimplePosition(x, _sequenceProvider.RefNameToChromosome, true));
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
            var altSequenceBuilder = new StringBuilder();
            var refRegionBetweenVariants = (Start: int.MaxValue, Length: -1); // region of the ref sequence between alt alleles
            for (int positionIndex = firstPositionIndex; positionIndex <= lastPositionIndex; positionIndex++)
            {

                refRegionBetweenVariants.Length = alleleSet.Starts[positionIndex] - refRegionBetweenVariants.Start;
                if (refRegionBetweenVariants.Length > 0)
                {
                    string refSeqBetweenPositions = refSequence.Substring(refRegionBetweenVariants.Start - blockStart,
                        refRegionBetweenVariants.Length);
                    altSequenceBuilder.Append(refSeqBetweenPositions);
                }
                string refAllele = alleleSet.VariantArrays[positionIndex][0];
                refRegionBetweenVariants.Start = alleleSet.Starts[positionIndex] + refAllele.Length;
                int indexInBlock = positionIndex - firstPositionIndex;
                int alleleIndex = alleleIndexBlock.AlleleIndexes[indexInBlock];
                string altAllele = alleleSet.VariantArrays[positionIndex][alleleIndex];

                // non-ref variant is preferred in case there are conflicting genotype information
                bool ignoreThisPosition = false;
                if (positionIndex != firstPositionIndex &&
                    alleleSet.Starts[positionIndex] == alleleSet.Starts[positionIndex - 1] &&
                    alleleIndexBlock.AlleleIndexes[indexInBlock - 1] != 0) // don't append alt sequence if previous allele is already non-ref
                {
                    ignoreThisPosition = true;
                    if (alleleIndex != 0)  // if both variants are non-ref, use the first one, but a warning would be given.
                    {
                        Console.WriteLine($"Warning: Conflictual altnative alleles at {alleleSet.Chromosome.UcscName} position {alleleSet.Starts[positionIndex]}: {alleleSet.VariantArrays[positionIndex - 1][alleleIndexBlock.AlleleIndexes[indexInBlock - 1]]} and {altAllele}");
                    }
                }

                //only mark positions with non-reference alleles being recomposed as "decomposed"
                // alleleIndex is 1-based for altAlleles
                if (alleleIndex != 0 && !ignoreThisPosition) decomposedPosVarIndex.Add((positionIndex, alleleIndex - 1));

                //trim the reference sequence if it overlapps with next variant
                if (positionIndex != lastPositionIndex && alleleSet.Starts[positionIndex + 1] < refRegionBetweenVariants.Start)
                {
                    refRegionBetweenVariants.Start = alleleSet.Starts[positionIndex + 1];
                    if (alleleIndex == 0 && !ignoreThisPosition)
                    {
                        altAllele = altAllele.Substring(0, alleleSet.Starts[positionIndex + 1] - alleleSet.Starts[positionIndex]);
                    }
                }

                if (!ignoreThisPosition) altSequenceBuilder.Append(altAllele);
            }
            return (blockStart, blockStart + blockRefLength - 1, refSequence, altSequenceBuilder.ToString());
        }
    }

    internal sealed class RecomposedAlleleSet
    {
        private readonly Dictionary<int, Dictionary<string, Dictionary<string, List<SampleAllele>>>> _recomposedAlleles;
        private readonly int _nSamples;
        private readonly string _chrName;
        private const string VariantId = ".";
        private const string Qual = ".";
        private const string Filter = "PASS";
        private const string InfoTag = "RECOMPOSED";
        private const string FormatTag = "GT";


        public RecomposedAlleleSet(string chrName, int nSamples)
        {
            _nSamples = nSamples;
            _chrName = chrName;
            _recomposedAlleles = new Dictionary<int, Dictionary<string, Dictionary<string, List<SampleAllele>>>>();
        }

        public void AddAllele(int start, int end, string refAllele, string altAllele, List<SampleAllele> sampleAlleles)
        {
            if (!_recomposedAlleles.ContainsKey(start))
            {
                _recomposedAlleles.Add(start, new Dictionary<string, Dictionary<string, List<SampleAllele>>>());
            }
            if (!_recomposedAlleles[start].ContainsKey(refAllele))
            {
                _recomposedAlleles[start].Add(refAllele, new Dictionary<string, List<SampleAllele>>());
            }
            _recomposedAlleles[start][refAllele].Add(altAllele, sampleAlleles);
        }

        public List<string[]> GetRecomposedVcfRecords()
        {
            var vcfRecords = new List<string[]>();
            // todo: warning: nested for loop 
            foreach (int start in _recomposedAlleles.Keys.OrderBy(x => x))
            {
                foreach (var refAllele in _recomposedAlleles[start].Keys.OrderBy(x => x))
                {
                    var altAlleles = _recomposedAlleles[start][refAllele];
                    var altAlleleList = new List<string>();
                    int genotypeIndex = 1; // genotype index of alt allele
                    var sampleGenotypes = new List<int>[_nSamples];
                    for (int i = 0; i < _nSamples; i++) sampleGenotypes[i] = new List<int>();
                    foreach (var altAllele in altAlleles.Keys.OrderBy(x => x))
                    {
                        var sampleAlleles = altAlleles[altAllele];
                        int currentGenotypeIndex;
                        if (altAllele == refAllele)
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
                    vcfRecords.Add(GetVcfFields(start, refAllele, altAlleleColumn, sampleGenotypes));
                }
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

        private string[] GetVcfFields(int start, string refAllele, string altAlleleColumn, List<int>[] sampleGenoTypes, string variantId = VariantId, string qual = Qual, string filter = Filter, string info = InfoTag, string format = FormatTag)
        {

            var vcfFields = new List<string>
            {
                _chrName,
                start.ToString(),
                variantId,
                refAllele,
                altAlleleColumn,
                qual,
                filter,
                info,
                format
            };
            foreach (var sampleGenotype in sampleGenoTypes)
            {
                vcfFields.Add(GetGenotype(sampleGenotype));
            }
            return vcfFields.ToArray();
        }

        private static string GetGenotype(List<int> sampleGenotype) => sampleGenotype.Count == 0 ? "." : string.Join("|", sampleGenotype);
    }
}