using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using Variants;

namespace SAUtils.InputFileParsers.ClinGen
{
    public sealed class ClinGenReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        private readonly IDictionary<string, IChromosome> _refNameDict;

        #endregion
        
        public ClinGenReader(StreamReader reader, IDictionary<string, IChromosome> refNameDict)
        {
            _reader = reader;
            _refNameDict = refNameDict;
        }

        public IEnumerable<ClinGenItem> GetItems()
        {
            using (var reader = _reader)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (IsClinGenHeader(line)) continue;

                    var cols         = line.OptimizedSplit('\t');
                    string id        = cols[0];
                    string ucscChrom = cols[1];
                    if(!_refNameDict.ContainsKey(ucscChrom)) continue;

                    var chrom              = _refNameDict[ucscChrom];
                    int start              = int.Parse(cols[2]);
                    int end                = int.Parse(cols[3]);
                    int observedGains      = int.Parse(cols[4]);
                    int observedLosses     = int.Parse(cols[5]);
                    var variantType        = GetVariantType(cols[6]);
                    var clinInterpretation = GetClinInterpretation(cols[7]);
                    bool validated         = cols[8].Equals("True");
                    var phenotypes         = cols[9] == "" ? null : new HashSet<string>(cols[9].OptimizedSplit(','));
                    var phenotypeIds       = cols[10] == "" ? null : new HashSet<string>(cols[10].OptimizedSplit(','));

                    var currentItem = new ClinGenItem(id, chrom, start, end, variantType, observedGains, observedLosses,
                        clinInterpretation, validated, phenotypes, phenotypeIds);
                    yield return currentItem;
                }
            }
        }

        private static VariantType GetVariantType(string variantTypeDescription)
        {
            switch (variantTypeDescription)
            {
                case "copy_number_gain":
                    return VariantType.copy_number_gain;
                case "copy_number_loss":
                    return VariantType.copy_number_loss;
                case "copy_number_variation":
                    return VariantType.copy_number_variation;
                default:
                    return VariantType.unknown;
            }
        }

        private static ClinicalInterpretation GetClinInterpretation(string s)
        {
            switch (s)
            {
                case "pathogenic":
                    return ClinicalInterpretation.pathogenic;
                case "benign":
                    return ClinicalInterpretation.benign;
                case "likely_pathogenic":
                    return ClinicalInterpretation.likely_pathogenic;
                case "likely_benign":
                    return ClinicalInterpretation.likely_benign;
                case "uncertain_significance":
                    return ClinicalInterpretation.uncertain_significance;
                default:
                    return ClinicalInterpretation.unknown;
            }
        }

        private static bool IsClinGenHeader(string line)
        {
            return line.OptimizedStartsWith('#');
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }
    }
}
