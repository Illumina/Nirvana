using System.Collections.Generic;
using System.Linq;
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
            int regionEnd = alleleSet.Starts.Last() + lastRefAllele.Length + 100; // make it long enough
            if (regionEnd > _sequenceProvider.Sequence.Length) regionEnd = _sequenceProvider.Sequence.Length;
            string totalRefSequence = _sequenceProvider.Sequence.Substring(regionStart - 1, regionEnd - regionStart); // VCF positions are 1-based
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
                string refSequenceBefore =
                    refSequence.Substring(refSequenceStart, positionOnRefSequence - refSequenceStart);
                altSequenceSegsegments.AddLast(refSequenceBefore);
                altSequenceSegsegments.AddLast(altAllele);
                refSequenceStart = positionOnRefSequence + refAllele.Length;
            }
            altSequenceSegsegments.AddLast(refSequence.Substring(refSequenceStart));
            return (blockStart, blockStart + blockRefLength - 1, refSequence, string.Concat(altSequenceSegsegments));
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