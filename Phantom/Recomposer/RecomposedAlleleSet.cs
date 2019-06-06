using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using Vcf;

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

        public IEnumerable<SimplePosition> GetRecomposedPositions(IDictionary<string, IChromosome> refNameToChromosome)
        {
            foreach (var variantSite in RecomposedAlleles.Keys.OrderBy(x => x))
            {
                var varInfo = RecomposedAlleles[variantSite];
                var altAlleleList = new List<string>();
                var genotypeIndex = 1; // genotype index of alt allele
                var sampleGenotypes = new List<int>[_numSamples];
                for (var i = 0; i < _numSamples; i++) sampleGenotypes[i] = new List<int>();
                List<List<string>> allLinkedVids = new List<List<string>>();
                foreach (string altAllele in varInfo.AltAlleleToSample.Keys.OrderBy(x => x))
                {
                    var (sampleAlleles, linkedVids) = varInfo.AltAlleleToSample[altAllele];
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
                        allLinkedVids.Add(linkedVids);
                    }
                    foreach (var sampleAllele in sampleAlleles)
                    {
                        SetGenotypeWithAlleleIndex(sampleGenotypes[sampleAllele.SampleIndex], sampleAllele.HaplotypeIndex,
                            currentGenotypeIndex);
                    }
                }
                string altAlleleColumn = string.Join(",", altAlleleList);
                var vcfFields = GetVcfFields(variantSite, varInfo, altAlleleColumn, sampleGenotypes);
                var position = SimplePosition.GetSimplePosition(vcfFields, new NullVcfFilter(), refNameToChromosome, true);
                for (var i = 0; i < allLinkedVids.Count; i++) position.LinkedVids[i] = allLinkedVids[i];

                yield return position;
            }
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

        private string[] GetVcfFields(VariantSite varSite, VariantInfo variantInfo, string altAlleleColumn, List<int>[] sampleGenoTypes, string variantId = VariantId, string info = InfoTag)
        {
            var vcfFields = new List<string>
            {
                _chrName,
                varSite.Start.ToString(),
                variantId,
                varSite.RefAllele,
                altAlleleColumn,
                variantInfo.Qual,
                variantInfo.GetMnvFilterTag(),
                info
            };

            AddFormatAndSampleColumns(sampleGenoTypes, variantInfo, ref vcfFields);
            return vcfFields.ToArray();
        }

        private static void AddFormatAndSampleColumns(List<int>[] sampleGenoTypes, VariantInfo variantInfo, ref List<string> vcfFields)
        {
            var formatTags = "GT";
            var hasGq = false;
            var hasPs = false;
            int numSamples = sampleGenoTypes.Length;

            string[] sampleGenotypeStrings = GetSampleGenotypeStrings(sampleGenoTypes, variantInfo, ref hasGq, ref hasPs, numSamples);

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
                string sampleGenotypeStr = sampleGenotypeStrings[index];

                if (sampleGenotypeStr == null || sampleGenotypeStr == ".") vcfFields.Add(".");
                else
                {
                    var nonMissingFields = new string[numFields];
                    nonMissingFields[0] = sampleGenotypeStr;
                    var fieldIndex = 1;
                    if (hasGq)
                    {
                        nonMissingFields[fieldIndex] = variantInfo.SampleGqs[index];
                        fieldIndex++;
                    }
                    if (hasPs)
                    {
                        nonMissingFields[fieldIndex] = variantInfo.SamplePhaseSets[index];
                    }

                    var sampleColumnStr = string.Join(":", TrimTrailingMissValues(nonMissingFields));
                    vcfFields.Add(sampleColumnStr);
                }
            }
        }

        private static string[] GetSampleGenotypeStrings(IReadOnlyList<List<int>> sampleGenoTypes, VariantInfo variantInfo, ref bool hasGq, ref bool hasPs, int numSamples)
        {
            var sampleGenotypeStrings = new string[numSamples];
            for (var index = 0; index < numSamples; index++)
            {
                var homoReferenceSamplePloidy = variantInfo.HomoReferenceSamplePloidies[index];
                sampleGenotypeStrings[index] = GetGenotype(sampleGenoTypes[index], homoReferenceSamplePloidy);
                if (sampleGenotypeStrings[index] == ".") continue;

                if (variantInfo.SampleGqs[index] != ".") hasGq = true;
                if (variantInfo.SamplePhaseSets[index] != ".") hasPs = true;
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

        private static string GetGenotype(IReadOnlyCollection<int> sampleGenotype, int? homoReferenceSamplePloidy)
        {
            if (sampleGenotype.Count != 0) return string.Join("|", sampleGenotype);

            return homoReferenceSamplePloidy != null ? string.Join("|", Enumerable.Repeat("0", homoReferenceSamplePloidy.Value)) : ".";
        }
    }
}