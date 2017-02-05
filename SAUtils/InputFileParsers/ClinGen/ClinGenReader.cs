using System.Collections;
using System.Collections.Generic;
using System.IO;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.ClinGen
{
    public sealed class ClinGenReader : IEnumerable<ClinGenItem>
    {
        #region members

        private readonly FileInfo _clinGenFileInfo;
        private readonly ChromosomeRenamer _renamer;

        #endregion

        #region IEnumerable implementation

        public IEnumerator<ClinGenItem> GetEnumerator()
        {
            return GetClinGenItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        // constructor
        public ClinGenReader(FileInfo clinGenFileInfo, ChromosomeRenamer renamer)
        {
            _clinGenFileInfo = clinGenFileInfo;
            _renamer = renamer;
        }

        private IEnumerable<ClinGenItem> GetClinGenItems()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_clinGenFileInfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (IsClinGenHeader(line)) continue;

                    var cols         = line.Split('\t');
                    string id        = cols[0];
                    string ucscChrom = cols[1];
                    string chrom     = _renamer.GetEnsemblReferenceName(ucscChrom);

                    if (!InputFileParserUtilities.IsDesiredChromosome(chrom, _renamer)) continue;

                    int start              = int.Parse(cols[2]);
                    int end                = int.Parse(cols[3]);
                    int observedGains      = int.Parse(cols[4]);
                    int observedLosses     = int.Parse(cols[5]);
                    var variantType        = GetVariantType(cols[6]);
                    var clinInterpretation = GetClinInterpretation(cols[7]);
                    bool validated         = cols[8].Equals("True");
                    var phenotypes         = cols[9] == "" ? null : new HashSet<string>(cols[9].Split(','));
                    var phenotypeIds       = cols[10] == "" ? null : new HashSet<string>(cols[10].Split(','));

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
            return line.StartsWith("#");
        }
    }
}
