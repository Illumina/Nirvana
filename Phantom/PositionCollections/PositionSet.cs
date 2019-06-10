using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Phantom.PositionCollections
{
    public sealed class PositionSet : IPositionSet
    {
        public List<ISimplePosition> SimplePositions { get; }
        private List<int> FunctionBlockRanges { get; }
        private static readonly HashSet<VariantType> SupportedVariantTypes = new HashSet<VariantType> { VariantType.SNV };
        public int NumSamples { get; }
        public string ChrName { get; }
        private readonly int _numPositions;
        public AlleleSet AlleleSet { get; private set; }
        public Dictionary<AlleleBlock, List<SampleHaplotype>> AlleleBlockToSampleHaplotype { get; private set; }
        private HashSet<int>[] _allelesWithUnsupportedTypes;
        private SampleInfo _sampleInfo;
        public TagInfo<string> GqInfo;
        public TagInfo<string> PsInfo;
        public TagInfo<Genotype> GtInfo;

        internal PositionSet(List<ISimplePosition> simpleSimplePositions, List<int> functionBlockRanges)
        {
            SimplePositions = simpleSimplePositions;
            FunctionBlockRanges = functionBlockRanges;
            NumSamples = SimplePositions[0].VcfFields.Length - VcfCommon.GenotypeIndex;
            ChrName = simpleSimplePositions[0].VcfFields[VcfCommon.ChromIndex];
            _numPositions = simpleSimplePositions.Count;
        }

        public static PositionSet CreatePositionSet(List<ISimplePosition> simpleSimplePositions, List<int> functionBlockRanges)
        {
            var positionSet = new PositionSet(simpleSimplePositions, functionBlockRanges);
            positionSet.AlleleSet = GenerateAlleleSet(positionSet);
            positionSet._allelesWithUnsupportedTypes = GetAllelesWithUnsupportedTypes(positionSet);
            positionSet._sampleInfo = GetSampleInfo(positionSet);

            var phaseSetAndGqIndexes = positionSet.GetSampleTagIndexes(new[] { "GT", "PS", "GQ" });
            positionSet.GtInfo = TagInfo<Genotype>.GetTagInfo(positionSet._sampleInfo, phaseSetAndGqIndexes[0], ExtractSampleValue, Genotype.GetGenotype);
            positionSet.PsInfo = TagInfo<string>.GetTagInfo(positionSet._sampleInfo, phaseSetAndGqIndexes[1], ExtractSampleValue, x => x);
            positionSet.GqInfo = TagInfo<string>.GetTagInfo(positionSet._sampleInfo, phaseSetAndGqIndexes[2], ExtractSampleValue, x => x);

            var genotypeToSampleIndex = GetGenotypeToSampleIndex(positionSet);
            var alleleBlockToSampleHaplotype = AlleleBlock.GetAlleleBlockToSampleHaplotype(genotypeToSampleIndex, positionSet._allelesWithUnsupportedTypes, positionSet.AlleleSet.Starts, positionSet.FunctionBlockRanges, out var alleleBlockGraph);
            positionSet.AlleleBlockToSampleHaplotype = AlleleBlockMerger.Merge(alleleBlockToSampleHaplotype, alleleBlockGraph);
            return positionSet;
        }

        private static SampleInfo GetSampleInfo(PositionSet positionSet)
        {
            var sampleInfo = new string[positionSet._numPositions, positionSet.NumSamples][];
            for (var i = 0; i < positionSet._numPositions; i++)
            {
                for (int sampleIndex = 0; sampleIndex < positionSet.NumSamples; sampleIndex++)
                {
                    int sampleColIndex = sampleIndex + VcfCommon.GenotypeIndex;
                    sampleInfo[i, sampleIndex] = positionSet.SimplePositions[i].VcfFields[sampleColIndex].OptimizedSplit(':');
                }
            }
            return new SampleInfo(sampleInfo);
        }

        private static AlleleSet GenerateAlleleSet(PositionSet positionSet)
        {
            var alleleArrays = new string[positionSet._numPositions][];
            var starts = positionSet.SimplePositions.Select(x => x.Start).ToArray();
            for (var index = 0; index < positionSet._numPositions; index++)
            {
                var position = positionSet.SimplePositions[index];
                alleleArrays[index] = new string[position.AltAlleles.Length + 1];
                alleleArrays[index][0] = position.RefAllele;
                position.AltAlleles.CopyTo(alleleArrays[index], 1);
            }
            return new AlleleSet(positionSet.SimplePositions[0].Chromosome, starts, alleleArrays);
        }

        internal int[][] GetSampleTagIndexes(string[] tagsToExtract)
        {
            int numTagsToExtract = tagsToExtract.Length;
            var indexes = new int[numTagsToExtract][];
            var tagIndexDict = new Dictionary<string, int>();
            for (int tagIndex = 0; tagIndex < numTagsToExtract; tagIndex++)
            {
                tagIndexDict[tagsToExtract[tagIndex]] = tagIndex;
                indexes[tagIndex] = Enumerable.Repeat(-1, _numPositions).ToArray();
            }

            for (int i = 0; i < _numPositions; i++)
            {
                var tags = SimplePositions[i].VcfFields[VcfCommon.FormatIndex].OptimizedSplit(':');
                int numTagsExtracted = 0;
                for (int j = 0; j < tags.Length; j++)
                {
                    if (tagIndexDict.TryGetValue(tags[j], out int tagIndex))
                    {
                        indexes[tagIndex][i] = j;
                        numTagsExtracted++;
                    }
                    if (numTagsExtracted == numTagsToExtract) break;
                }
            }
            return indexes;
        }

        internal static string ExtractSampleValue(int tagIndex, string[] sampleInfo) => tagIndex == -1 || sampleInfo.Length <= tagIndex ? "." : sampleInfo[tagIndex];

        private static Dictionary<GenotypeBlock, List<int>> GetGenotypeToSampleIndex(PositionSet positionSet)
        {
            var genotypeToSample = new Dictionary<GenotypeBlock, List<int>>();
            for (int sampleIndex = 0; sampleIndex < positionSet.NumSamples; sampleIndex++)
            {
                var genotypesAndStartIndexes = GetGenotypeBlocks(positionSet, sampleIndex);
                foreach (var genotypeAndStartIndex in genotypesAndStartIndexes)
                {
                    if (genotypeToSample.ContainsKey(genotypeAndStartIndex)) genotypeToSample[genotypeAndStartIndex].Add(sampleIndex);
                    else genotypeToSample[genotypeAndStartIndex] = new List<int> { sampleIndex };
                }
            }
            return genotypeToSample;
        }


        // GenotypeBlocks can be shared by multiple samples
        // We mainly utilize phase set information at this step to avoid duplicated calculation
        // These GenotypeBlocks could be further segmented when more details considered
        private static IEnumerable<GenotypeBlock> GetGenotypeBlocks(PositionSet positionSet, int sampleIndex)
        {
            var genotypes = positionSet.GtInfo.Values[sampleIndex];
            var entireBlock = new GenotypeBlock(genotypes);
            var blockRanges = GetGenotypeBlockRange(positionSet.PsInfo.Values[sampleIndex], genotypes.Select(x => x.IsPhased).ToArray(), genotypes.Select(x => x.IsHomozygous).ToArray());
            var genotypeBlocks = new List<GenotypeBlock>();
            foreach (var range in blockRanges)
                genotypeBlocks.Add(entireBlock.GetSubBlock(range.StartIndex, range.PositionCount));

            return genotypeBlocks;
        }

        // Just do a minimal process here as only phase set informatin is sample specific
        // Non sample specific information (i.e. genotype) can be shared by multiple samples and will further processed 
        private static IEnumerable<(int StartIndex, int PositionCount)> GetGenotypeBlockRange(string[] phaseSetIds, bool[] isPhased, bool[] isHomomzygous)
        {
            var blocks = new List<(int, int)>();
            int numPositions = phaseSetIds.Length;
            int startCurrentHomoBlock = -1;
            int startCurrentBlock = 0;
            string previousPhaseSetId = isPhased[0] ? phaseSetIds[0] : null;
            for (var i = 1; i < numPositions; i++)
            {
                bool bothPhasedAndDiffPhaseSet = previousPhaseSetId != null && isPhased[i] && phaseSetIds[i] != previousPhaseSetId;
                if (!isHomomzygous[i])
                {
                    if (bothPhasedAndDiffPhaseSet)
                    {
                        blocks.Add((startCurrentBlock, i - startCurrentBlock));
                        startCurrentBlock = startCurrentHomoBlock == -1 ? i : startCurrentHomoBlock;
                    }

                    startCurrentHomoBlock = -1;
                }
                else if (startCurrentHomoBlock == -1)
                { 
                    startCurrentHomoBlock = i;
                }

                if (isPhased[i]) previousPhaseSetId = phaseSetIds[i];
            }
            blocks.Add((startCurrentBlock, numPositions - startCurrentBlock));
            return blocks;
        }

        private static HashSet<int>[] GetAllelesWithUnsupportedTypes(PositionSet positionSet)
        {
            var allelesWithUnsupportedTypes = new HashSet<int>[positionSet._numPositions];
            for (int posIndex = 0; posIndex < positionSet._numPositions; posIndex++)
            {
                allelesWithUnsupportedTypes[posIndex] = new HashSet<int>();
                var thisPosition = positionSet.SimplePositions[posIndex];
                for (int varIndex = 0; varIndex < thisPosition.AltAlleles.Length; varIndex++)
                {
                    if (!(IsSupportedVariantType(thisPosition.RefAllele, thisPosition.AltAlleles[varIndex]) || thisPosition.VcfFields[VcfCommon.AltIndex] == VcfCommon.GatkNonRefAllele))
                        allelesWithUnsupportedTypes[posIndex].Add(varIndex + 1); // GT tag is 1-based
                }
            }
            return allelesWithUnsupportedTypes;
        }

        private static bool IsSupportedVariantType(string refAllele, string altAllele)
        {
            return !(altAllele.OptimizedStartsWith('<') || altAllele == "*") && SupportedVariantTypes.Contains(Vcf.VariantCreator.SmallVariantCreator.GetVariantType(refAllele, altAllele));
        }
    }

    public sealed class TagInfo<T>
    {
        public readonly T[][] Values;

        private TagInfo(T[][] values)
        {
            Values = values;
        }

        public static TagInfo<T> GetTagInfo(SampleInfo positionSetSampleInfo, int[] tagIndexes, Func<int, string[], string> tagExtractionMethod, Func<string, T> tagProcessingMethod)
        {
            int numPositions = positionSetSampleInfo.NumPositions;
            int numSamples = positionSetSampleInfo.NumSamples;
            if (numPositions != tagIndexes.Length) throw new InvalidDataException($"The inconsistent numbers of positions: {numPositions} in sample info array, {tagIndexes.Length} in GQ index array");
            var tagInfo = new T[numSamples][];
            for (var sampleIndex = 0; sampleIndex < numSamples; sampleIndex++)
            {
                tagInfo[sampleIndex] = new T[numPositions];
                for (var i = 0; i < numPositions; i++)
                {
                    tagInfo[sampleIndex][i] = tagProcessingMethod(tagExtractionMethod(tagIndexes[i], positionSetSampleInfo.Values[i, sampleIndex]));
                }
            }
            return new TagInfo<T>(tagInfo);
        }
    }

    public static class TagInfoExtension
    {
        public static void Update(this TagInfo<string> tagInfo, TagInfo<string> newTagInfo)
        {
            for (var i = 0; i < tagInfo.Values.Length; i++)
                for (var j = 0; j < tagInfo.Values[0].Length; j++)
                {
                    if (newTagInfo.Values[i][j] != ".")
                    {
                        tagInfo.Values[i][j] = newTagInfo.Values[i][j];
                    }
                }
        }
    }
}