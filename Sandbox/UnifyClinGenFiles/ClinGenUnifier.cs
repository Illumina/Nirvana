using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace UnifyClinGenFiles
{
    public sealed class ClinGenUnifier
    {
        #region members
        private const string IdTag = "parent";
        private const string VariantTypeTag = "var_type";
        private const string ClinicalInterpretationTag = "clinical_int";
        private const string ValidatedTag = "validated";
        private const string PhenotypeTag = "phenotype";
        private const string PhenotypeIdTag = "phenotype_id";
        private const string OutHeader = "#Id\tChr\tStart\tEnd\tGains\tLosses\tVarType\tclinInterpretation\tvalidated\tphenotypes\tphenotyIds\n";


        private readonly StreamReader _reader;
        private readonly Dictionary<Tuple<string, string, int, int>, ClinGenItem> _clinGenDictionary = new Dictionary<Tuple<string, string, int, int>, ClinGenItem>();
        private static Dictionary<string, string> _refNameDict;
        #endregion

        public ClinGenUnifier(FileInfo inputFileInfo, FileInfo refNameInfo = null)
        {

            _reader = GZipUtilities.GetAppropriateStreamReader(inputFileInfo.FullName);
            if (refNameInfo == null) return;

            _refNameDict = new Dictionary<string, string>();
            using (var refReader = GZipUtilities.GetAppropriateStreamReader(refNameInfo.FullName))
            {
                string line;
                while ((line = refReader.ReadLine()) != null)
                {
                    if (line.StartsWith("#")) continue;
                    var lineContents = line.Split('\t');
                    var ucscName = lineContents[0];
                    var ensemblName = lineContents[1];
                    var inVep = lineContents[2].Equals("YES");
                    if (inVep)
                    {
                        _refNameDict[ucscName] = ensemblName;
                    }
                }
            }

        }

        public void Unify()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    // skip header and empty lines
                    if (string.IsNullOrWhiteSpace(line) || IsClinGenHeader(line)) continue;
                    var clinGenItem = ExtractClinGenItem(line);
                    if (clinGenItem == null) continue;

                    var itemMapId = Tuple.Create(clinGenItem.Id, clinGenItem.Chromosome, clinGenItem.Start, clinGenItem.End);

                    if (_clinGenDictionary.ContainsKey(itemMapId))
                    {
                        _clinGenDictionary[itemMapId].MergeItem(clinGenItem);
                        continue;
                    }
                    _clinGenDictionary[itemMapId] = clinGenItem;
                }
            }
        }

        public void Write(string outFileName)
        {
            using (var writer = GZipUtilities.GetStreamWriter(outFileName))
            {
                writer.Write(OutHeader);
                foreach (var clinGenItem in _clinGenDictionary.Values)
                {
                    var varType = VariantType.unknown;
                    if (clinGenItem.ObservedGains > 0 && clinGenItem.ObservedLosses == 0)
                        varType = VariantType.copy_number_gain;
                    if (clinGenItem.ObservedGains > 0 && clinGenItem.ObservedLosses > 0)
                        varType = VariantType.copy_number_variation;
                    if (clinGenItem.ObservedGains == 0 && clinGenItem.ObservedLosses > 0)
                        varType = VariantType.copy_number_loss;

                    writer.Write(
                        $"{clinGenItem.Id}\t{clinGenItem.Chromosome}\t{clinGenItem.Start}\t{clinGenItem.End}\t" +
                        $"{clinGenItem.ObservedGains}\t{clinGenItem.ObservedLosses}\t{varType}\t" +
                        $"{clinGenItem.ClinicalInterpretation}\t{clinGenItem.Validated}\t" +
                        $"{string.Join(",", clinGenItem.Phenotypes.ToArray())}\t{string.Join(",", clinGenItem.PhenotypeIds.ToArray())}\n");
                }
            }
        }


        private static bool IsClinGenHeader(string line)
        {
            return line.StartsWith("#");
        }

        private static ClinGenItem ExtractClinGenItem(string line)
        {
            var cols = line.Split('\t');
            if (cols.Length < 12) return null;

            //string chromosome = cols[1];
            var ucscChrom = cols[1];
            string chromosome;
            if (_refNameDict != null)
            {
                if (!_refNameDict.ContainsKey(ucscChrom)) return null;
                chromosome = _refNameDict[ucscChrom];
            }
            else
            {
                chromosome = ucscChrom;
            }


            var start = int.Parse(cols[2]) + 1;
            var end = int.Parse(cols[3]);
            var tagField = cols[10];
            var infoField = cols[11];
            string id;
            VariantType variantType;
            int observedGains;
            int observedLosses;
            ClinicalInterpretation clinicalInterpretation;
            bool validated;
            HashSet<string> phenotypes;
            HashSet<string> phenotypeIds;
            ParseInfor(tagField, infoField, out id, out variantType, out observedGains, out observedLosses, out clinicalInterpretation, out validated, out phenotypes, out phenotypeIds);

            if (id == null || variantType == VariantType.unknown)
                throw new Exception($"error in parsing {line}\n");

            return new ClinGenItem(id, chromosome, start, end, variantType, observedGains, observedLosses, clinicalInterpretation, validated, phenotypes, phenotypeIds);
        }

        private static void ParseInfor(string tagFiled, string infoField, out string id, out VariantType variantType,
            out int observedGains, out int observedLosses, out ClinicalInterpretation clinicalInterpretation, out bool validated,
            out HashSet<string> phenotypes, out HashSet<string> phenotypeIds)
        {
            id = null;
            variantType = VariantType.unknown;
            observedGains = 0;
            observedLosses = 0;
            clinicalInterpretation = ClinicalInterpretation.unknown;
            validated = false;
            phenotypes = new HashSet<string>();
            phenotypeIds = new HashSet<string>();

            var tags = tagFiled.Split(',');
            var info = infoField.Split(',');
            if (tags.Length != info.Length)
                throw new Exception("Unequal length of attrTags and attrVals\n");

            var len = tags.Length;
            for (var i = 0; i < len; i++)
            {
                switch (tags[i])
                {
                    case IdTag:
                        id = info[i];
                        break;
                    case ClinicalInterpretationTag:
                        clinicalInterpretation = ParseClinicalInterpretation(info[i]);
                        break;
                    case ValidatedTag:
                        validated = info[i].Equals("Pass");
                        break;
                    case VariantTypeTag:
                        variantType = GetVariantType(info[i]);
                        break;
                    case PhenotypeTag:
                        phenotypes = GetPhenotypes(info[i]);
                        break;
                    case PhenotypeIdTag:
                        phenotypeIds = GetPhenotypeIds(info[i]);
                        break;
                }
            }
            if (variantType == VariantType.copy_number_gain) observedGains++;
            if (variantType == VariantType.copy_number_loss) observedLosses++;
        }

        private static ClinicalInterpretation ParseClinicalInterpretation(string description)
        {
            switch (description)
            {
                case "Pathogenic":
                    return ClinicalInterpretation.pathogenic;
                case "Likely pathogenic":
                    return ClinicalInterpretation.likely_pathogenic;
                case "Benign":
                    return ClinicalInterpretation.benign;
                case "Likely benign":
                    return ClinicalInterpretation.likely_benign;
                case "Uncertain significance":
                    return ClinicalInterpretation.uncertain_significance;
                default:
                    return ClinicalInterpretation.unknown;
            }
        }

        private static HashSet<string> GetPhenotypeIds(string phenotypeAttr)
        {
            var stringSeparators = new[] { "%2C" };
            return new HashSet<string>(phenotypeAttr.Split(stringSeparators, StringSplitOptions.None));

        }

        private static HashSet<string> GetPhenotypes(string phenotypeIdAttr)
        {

            var stringSeparators = new[] { "%2C" };
            return new HashSet<string>(phenotypeIdAttr.Split(stringSeparators, StringSplitOptions.None));

        }

        private static VariantType GetVariantType(string variantTypeDescription)
        {
            switch (variantTypeDescription)
            {
                case "copy_number_gain":
                    return VariantType.copy_number_gain;
                case "copy_number_loss":
                    return VariantType.copy_number_loss;
                default:
                    return VariantType.unknown;
            }
        }
    }
}