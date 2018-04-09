using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Phantom.Interfaces;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Phantom.DataStructures
{
    public sealed class PositionSet : IPositionSet
    {
        public List<ISimplePosition> SimplePositions { get; }
        public List<int> FunctionBlockRanges { get; }
        private static readonly HashSet<VariantType> SupportedVariantTypes = new HashSet<VariantType> { VariantType.SNV };
        public int NumSamples { get; private set; }
        public string ChrName { get; }
        private readonly int _numPositions;
        public AlleleSet AlleleSet { get; private set; }
        public Dictionary<AlleleIndexBlock, List<SampleAllele>> AlleleIndexBlockToSampleIndex { get; private set; }
        private HashSet<string>[] _allelesWithUnsupportedTypes;
        private string[,][] _sampleInfo;
        public string[][] GqInfo;
        public string[][] PsInfo;

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
            var phaseSetAndGqIndexes = positionSet.GetSampleTagIndexes(new[] { "PS", "GQ" });
            positionSet.PsInfo = GetTagInfo(positionSet._sampleInfo, phaseSetAndGqIndexes[0],ExtractSamplePhaseSet);
            positionSet.GqInfo = GetTagInfo(positionSet._sampleInfo, phaseSetAndGqIndexes[1],ExtractSampleGq);
            var genotypeToSampleIndex = GetGenotypeToSampleIndex(positionSet);
            positionSet.AlleleIndexBlockToSampleIndex = AlleleIndexBlock.GetAlleleIndexBlockToSampleIndex(genotypeToSampleIndex, positionSet._allelesWithUnsupportedTypes, positionSet.AlleleSet.Starts, positionSet.FunctionBlockRanges);
            return positionSet;
        }


        private static string[][] GetTagInfo(string[,][] positionSetSampleInfo, int[] tagIndexes, Func<int, string[], string> tagExtractionMethod)
        {
            int numPositions = positionSetSampleInfo.GetLength(0);
            int numSamples = positionSetSampleInfo.GetLength(1);
            if (numPositions != tagIndexes.Length) throw new InvalidDataException($"The inconsistent numbers of positions: {numPositions} in sample info array, {tagIndexes.Length} in GQ index array");
            var tagInfo = new String[numSamples][];
            for (int sampleIndex = 0; sampleIndex < numSamples; sampleIndex++)
            {
                tagInfo[sampleIndex] = new string[numPositions];
                for (var i = 0; i < numPositions; i++)
                {
                    tagInfo[sampleIndex][i] = tagExtractionMethod(tagIndexes[i], positionSetSampleInfo[i, sampleIndex]);
                }
            }
            return tagInfo;
        }

        private static string[,][] GetSampleInfo(PositionSet positionSet)
        {
            var sampleInfo = new String[positionSet._numPositions, positionSet.NumSamples][];
            for (var i = 0; i < positionSet._numPositions; i++)
            {
                for (int sampleIndex = 0; sampleIndex < positionSet.NumSamples; sampleIndex++)
                {
                    int sampleColIndex = sampleIndex + VcfCommon.GenotypeIndex;
                    sampleInfo[i, sampleIndex] = positionSet.SimplePositions[i].VcfFields[sampleColIndex].Split(":");
                }
            }
            return sampleInfo;
        }

        private static AlleleSet GenerateAlleleSet(PositionSet positionSet)
        {
            var alleleArrays = new string[positionSet._numPositions][];
            var starts = positionSet.SimplePositions.Select(x => x.Start).ToArray();
            for (int index = 0; index < positionSet._numPositions; index++)
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
            int numTagsToExtact = tagsToExtract.Length;
            var indexes = new int[numTagsToExtact][];
            var tagIndexDict = new Dictionary<string, int>();
            for (int tagIndex = 0; tagIndex < numTagsToExtact; tagIndex++)
            {
                tagIndexDict[tagsToExtract[tagIndex]] = tagIndex;
                indexes[tagIndex] = Enumerable.Repeat(-1, _numPositions).ToArray();
            }

            for (int i = 0; i < _numPositions; i++)
            {
                var tags = SimplePositions[i].VcfFields[VcfCommon.FormatIndex].Split(":");
                int numTagsExtracted = 0;
                for (int j = 0; j < tags.Length; j++)
                {
                    if (tagIndexDict.TryGetValue(tags[j], out int tagIndex))
                    {
                        indexes[tagIndex][i] = j;
                        numTagsExtracted++;
                    }
                    if (numTagsExtracted == numTagsToExtact) break;
                }
            }
            return indexes;
        }

        internal static string ExtractSamplePhaseSet(int phaseSetTagIndex, string[] sampleInfo)
        {
            if (phaseSetTagIndex == -1 || sampleInfo.Length <= phaseSetTagIndex) return ".";
            if (sampleInfo.Length == 1 && sampleInfo[0] == ".") return ".";
            var phaseSet = sampleInfo[phaseSetTagIndex];
            return phaseSet;
        }

        internal static string ExtractSampleGq(int gqTagIndex, string[] sampleInfo) => gqTagIndex == -1 || sampleInfo.Length <= gqTagIndex ? "." : sampleInfo[gqTagIndex];

        private static Dictionary<(string Genotypes, int Start), List<int>> GetGenotypeToSampleIndex(PositionSet positionSet)
        {
            var genotypeToSample = new Dictionary<(string, int), List<int>>();
            for (int sampleIndex = 0; sampleIndex < positionSet.NumSamples; sampleIndex++)
            {
                var genotypesAndStartIndexes = GetGenotypesAndStarts(positionSet, sampleIndex);
                foreach (var genotypeAndSartIndex in genotypesAndStartIndexes)
                {
                    if (genotypeToSample.ContainsKey(genotypeAndSartIndex)) genotypeToSample[genotypeAndSartIndex].Add(sampleIndex);
                    else genotypeToSample[genotypeAndSartIndex] = new List<int> { sampleIndex };
                }
            }
            return genotypeToSample;
        }

        private static List<(string, int)> GetGenotypesAndStarts(PositionSet positionSet, int sampleIndex)
        {
            var genotypesAndStartIndexes = new List<(string, int)>();
            var gtTags = new List<string>();
            int startIndexInBlock = 0;
            string previousPhaseSetId = null;
            for (var i = 0; i < positionSet._numPositions; i++)
            {
                var thisSampleInfo = positionSet._sampleInfo[i, sampleIndex];
                string currentPhaseSetId = positionSet.PsInfo[sampleIndex][i];
                if (previousPhaseSetId == null)
                {
                    // ReSharper disable once RedundantAssignment
                    previousPhaseSetId = currentPhaseSetId;
                }
                else if (currentPhaseSetId != previousPhaseSetId)
                {
                    if (gtTags.Count > 1)
                    {
                        genotypesAndStartIndexes.Add((string.Join(";", gtTags), startIndexInBlock));
                    }
                    gtTags = new List<string>();
                    startIndexInBlock = i;
                }
                gtTags.Add(thisSampleInfo[0]);
                previousPhaseSetId = currentPhaseSetId;
            }
            if (gtTags.Count > 1)
            {
                genotypesAndStartIndexes.Add((string.Join(";", gtTags), startIndexInBlock));
            }
            return genotypesAndStartIndexes;
        }

        private static HashSet<string>[] GetAllelesWithUnsupportedTypes(PositionSet positionSet)
        {
            var allelesWithUnsupportedTypes = new HashSet<string>[positionSet._numPositions];
            for (int posIndex = 0; posIndex < positionSet._numPositions; posIndex++)
            {
                allelesWithUnsupportedTypes[posIndex] = new HashSet<string>();
                var thisPosition = positionSet.SimplePositions[posIndex];
                for (int varIndex = 0; varIndex < thisPosition.AltAlleles.Length; varIndex++)
                {
                    if (!(IsSupportedVariantType(thisPosition.RefAllele, thisPosition.AltAlleles[varIndex]) || thisPosition.VcfFields[VcfCommon.AltIndex] == VcfCommon.GatkNonRefAllele)) //todo: simplify the logic
                        allelesWithUnsupportedTypes[posIndex].Add((varIndex + 1).ToString()); // GT tag is 1-based
                }
            }
            return allelesWithUnsupportedTypes;
        }

        private static bool IsSupportedVariantType(string refAllele, string altAllele)
        {
            return !(altAllele.StartsWith("<") || altAllele == "*") && SupportedVariantTypes.Contains(Vcf.VariantCreator.SmallVariantCreator.GetVariantType(refAllele, altAllele));
        }

    }
}