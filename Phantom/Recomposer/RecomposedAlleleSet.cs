using System;
using System.Collections.Generic;
using System.Linq;

namespace Phantom.Recomposer
{
    public sealed class RecomposedAlleleSet
    {
        public readonly Dictionary<VariantSite, VariantInfo> RecomposedAlleles;
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
                        SetGenotypeWithAlleleIndex(sampleGenotypes[sampleAllele.SampleIndex], sampleAllele.HaplotypeIndex,
                            currentGenotypeIndex);
                    }
                }
                string altAlleleColumn = string.Join(",", altAlleleList);
                vcfRecords.Add(GetVcfFields(variantSite, altAlleleColumn, varInfo.Qual, varInfo.Filter, sampleGenotypes, varInfo.SampleGqs, varInfo.SamplePhaseSets));
            }

            return vcfRecords;
        }

        private static void SetGenotypeWithAlleleIndex(List<int> sampleGenotype, byte sampleAlleleAlleleIndex, int currentGenotypeIndex)
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

            string[] sampleGenotypeStrings = GetSampleGenotypeStrings(sampleGenoTypes, sampleGqs, samplePhasesets, ref hasGq, ref hasPs, numSamples);

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
                if (sampleGenotypeStr == null || sampleGenotypeStr == ".") vcfFields.Add(".");
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

        private static string[] GetSampleGenotypeStrings(List<int>[] sampleGenoTypes, string[] sampleGqs, string[] samplePhasesets, ref bool hasGq, ref bool hasPs, int numSamples)
        {
            var sampleGenotypeStrings = new string[numSamples];
            for (var index = 0; index < numSamples; index++)
            {
                sampleGenotypeStrings[index] = GetGenotype(sampleGenoTypes[index]);
                if (sampleGenotypeStrings[index] == ".") continue;

                if (sampleGqs[index] != ".") hasGq = true;
                if (samplePhasesets[index] != ".") hasPs = true;
            }

            return sampleGenotypeStrings;
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
}