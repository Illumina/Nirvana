using System.Collections.Generic;
using System.Linq;
using Phantom.Interfaces;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Phantom.DataStructures
{
    public sealed class PositionSet : IPositionSet
    {
        private List<ISimplePosition> SimplePositions { get; }
        public List<int> FunctionBlockRanges { get;}
        private static readonly HashSet<VariantType> SupportedVariantTypes = new HashSet<VariantType> {VariantType.SNV};
        public int NumSamples { get; private set; }
        public string ChrName { get; }
        private readonly int _numColumns;
        private readonly int _numPositions;
        public AlleleSet AlleleSet { get; private set; }
        public Dictionary<AlleleIndexBlock, List<SampleAllele>> AlleleIndexBlockToSampleIndex { get; private set; }
        private HashSet<string>[] _allelesWithUnsupportedTypes;

        internal PositionSet(List<ISimplePosition> simpleSimplePositions, List<int> functionBlockRanges)
        {
            SimplePositions = simpleSimplePositions;
            FunctionBlockRanges = functionBlockRanges;
            _numColumns = SimplePositions[0].VcfFields.Length;
            NumSamples = _numColumns - VcfCommon.GenotypeIndex;
            ChrName = simpleSimplePositions[0].VcfFields[VcfCommon.ChromIndex];
            _numPositions = simpleSimplePositions.Count;
        }

        public static PositionSet CreatePositionSet(List<ISimplePosition> simpleSimplePositions, List<int> functionBlockRanges)
        {
            var positionSet = new PositionSet(simpleSimplePositions, functionBlockRanges);
            positionSet.AlleleSet = GenerateAlleleSet(positionSet);
            positionSet._allelesWithUnsupportedTypes = GetAllelesWithUnsupportedTypes(positionSet);
            var genotypeToSampleIndex = GetGenotypeToSampleIndex(positionSet);
            positionSet.AlleleIndexBlockToSampleIndex = AlleleIndexBlock.GetAlleleIndexBlockToSampleIndex(genotypeToSampleIndex,  positionSet._allelesWithUnsupportedTypes, positionSet.AlleleSet.Starts, positionSet.FunctionBlockRanges);
            return positionSet;
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

        internal int[] GetPhaseSetTagIndexes()
        {
            var indexes = new int[_numPositions];
            for (int i = 0; i < _numPositions; i++)
            {   
                var tags = SimplePositions[i].VcfFields[VcfCommon.FormatIndex].Split(":");
                bool tagFound = false;
                for (int j = 0; j < tags.Length; j++)
                {
                    if (tags[j] != "PS") continue;
                    indexes[i] = j;
                    tagFound = true;
                    break;
                }
                if (!tagFound) indexes[i] = -1;
            }
            return indexes;
        }

        internal static int ExtractSamplePhaseSet(int phaseSetTagIndex, string[] sampleInfo)
        {
            if (phaseSetTagIndex == -1) return 0;
            var phaseSet = sampleInfo[phaseSetTagIndex];
            return phaseSet == "." ? 0 : int.Parse(phaseSet);
        }

        private static Dictionary<(string Genotypes, int Start), List<int>> GetGenotypeToSampleIndex (PositionSet positionSet)
        {
            var genotypeToSample = new Dictionary<(string, int), List<int>>();
            var phaseSetTagIndexes = positionSet.GetPhaseSetTagIndexes();
            for (int sampleColIndex = VcfCommon.GenotypeIndex; sampleColIndex < positionSet._numColumns; sampleColIndex++)
            {
                var genotypesAndStartIndexes = GetGenotypesAndStarts(positionSet, sampleColIndex, phaseSetTagIndexes);
                foreach (var genotypeAndSartIndex in genotypesAndStartIndexes)
                {
                    if (genotypeToSample.ContainsKey(genotypeAndSartIndex)) genotypeToSample[genotypeAndSartIndex].Add(sampleColIndex - VcfCommon.GenotypeIndex);
                    else genotypeToSample[genotypeAndSartIndex] = new List<int> { sampleColIndex - VcfCommon.GenotypeIndex };
                }
            }
            return genotypeToSample;
        }

        private static List<(string, int)> GetGenotypesAndStarts(PositionSet positionSet, int sampleColIndex, int[] phaseSetTagIndexes)
        {
            var genotypesAndStartIndexes = new List<(string, int)>();
            var gtTags = new List<string>();
            int startIndexInBlock = 0;
            int previousPhaseSetId = -1;
            for (var i = 0; i < positionSet._numPositions; i++)
            {
                var sampleInfo = positionSet.SimplePositions[i].VcfFields[sampleColIndex].Split(":");
                int currentPhaseSetId = ExtractSamplePhaseSet(phaseSetTagIndexes[i], sampleInfo);
                if (IsNewPhaseSet(currentPhaseSetId, previousPhaseSetId))
                {
                    if (gtTags.Count > 1)
                    {
                        genotypesAndStartIndexes.Add((string.Join(";", gtTags), startIndexInBlock));
                    }
                    gtTags = new List<string>();
                    startIndexInBlock = i;
                }
                gtTags.Add(sampleInfo[0]);
                previousPhaseSetId = currentPhaseSetId;
            }
            if (gtTags.Count > 1)
            {
                genotypesAndStartIndexes.Add((string.Join(";", gtTags), startIndexInBlock));
            }
            return genotypesAndStartIndexes;
        }

        private static bool IsNewPhaseSet(int currentPhaseSetId, int previousPhaseSetId) => previousPhaseSetId != 0 && currentPhaseSetId !=0 && currentPhaseSetId != previousPhaseSetId;

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