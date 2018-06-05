using System;
using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.Info
{
    public static class VcfInfoParser
    {
        private static readonly HashSet<string> TagsToRemove = new HashSet<string>
        {
            "CSQ",
            "CSQR",
            "CSQT",
            "AF1000G",
            "AA",
            "cosmic",
            "clinvar",
            "EVS",
            "GMAF",
            "phyloP",
            "RefMinor"
        };

        public static IInfoData Parse(string infoField)
        {
            if (string.IsNullOrEmpty(infoField)) return null;

            var infoKeyValue = ExtractInfoFields(infoField, out string updatedInfoField);

            int? svLen                     = null;
            int? end                       = null;
            int? copyNumber                = null;
            int? depth                     = null;
            int? jointSomaticNormalQuality = null;
            VariantType svType             = VariantType.unknown;
            var colocalizedWithCnv         = false;
            int[] ciPos                    = null;
            int[] ciEnd                    = null;
            double? strandBias             = null;
            double? recalibratedQuality    = null;
            var isInv3                     = false;
            var isInv5                     = false;
            int? refRepeatCount            = null;
            string repeatUnit              = null;

            foreach (var kvp in infoKeyValue)
            {
                string key   = kvp.Key;
                string value = kvp.Value;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "SB":
                        strandBias = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "QSI_NT":
                    case "SOMATICSCORE":
                    case "QSS_NT":
                        jointSomaticNormalQuality = value.GetNullableInt();
                        break;
                    case "VQSR":
                        recalibratedQuality = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "CN": // SENECA
                        copyNumber = value.GetNullableInt();
                        break;
                    case "DP": // Pisces
                        depth = value.GetNullableInt();
                        break;
                    case "CIPOS":
                        ciPos = value.SplitToArray();
                        break;
                    case "CIEND":
                        ciEnd = value.SplitToArray();
                        break;
                    case "SVLEN":
                        svLen = value.GetNullableInt();
                        if (svLen != null)
                            svLen = Math.Abs(svLen.Value);
                        break;
                    case "SVTYPE":
                        svType = GetSvType(value);
                        break;
                    case "END":
                        end = value.GetNullableInt();
                        break;
                    case "INV3":
                        isInv3 = true;
                        break;
                    case "INV5":
                        isInv5 = true;
                        break;
                    case "ColocalizedCanvas":
                        colocalizedWithCnv = true;
                        break;
                    case "REF":
                        refRepeatCount = Convert.ToInt32(value);
                        break;
                    case "RU":
                        repeatUnit = value;
                        break;
                }
            }

            var infoData = new InfoData(end, svLen, svType, strandBias, recalibratedQuality, jointSomaticNormalQuality,
                copyNumber, depth, colocalizedWithCnv, ciPos, ciEnd, isInv3, isInv5, updatedInfoField, repeatUnit, refRepeatCount);
            return infoData;
        }

        private static VariantType GetSvType(string value)
        {
            switch (value)
            {
                case "DEL":
                    return VariantType.deletion;
                case "INS":
                    return VariantType.insertion;
                case "DUP":
                    return VariantType.duplication;
                case "INV":
                    return VariantType.inversion;
                case "TDUP":
                    return VariantType.tandem_duplication;
                case "BND":
                    return VariantType.translocation_breakend;
                case "CNV":
                    return VariantType.copy_number_variation;
                case "STR":
                    return VariantType.short_tandem_repeat_variation;
                case "ALU":
                    return VariantType.mobile_element_insertion;
                case "LINE1":
                    return VariantType.mobile_element_insertion;
                case "LOH":
                    return VariantType.copy_number_variation;
                case "SVA":
                    return VariantType.mobile_element_insertion;
                default:
                    return VariantType.unknown;

            }
        }

        private static Dictionary<string, string> ExtractInfoFields(string infoField, out string updatedInfoField)
        {
            var infoKeyValue = new Dictionary<string, string>();

            if (infoField == ".")
            {
                updatedInfoField = "";
                return infoKeyValue;
            }

            var infoFields = infoField.OptimizedSplit(';');

            var sb = StringBuilderCache.Acquire();

            foreach (string field in infoFields)
            {
                (string key, string value) = field.OptimizedKeyValue();
                if (TagsToRemove.Contains(key)) continue;

                sb.Append(field);
                sb.Append(';');

                if (value == null) infoKeyValue[key] = "true";
                else infoKeyValue[key] = value;
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1); //removing the last semi-colon

                updatedInfoField = StringBuilderCache.GetStringAndRelease(sb);
            }
            else updatedInfoField = "";

            return infoKeyValue;
        }
    }
}