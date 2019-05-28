using System;
using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;
using Variants;

namespace Vcf.Info
{
    public static class VcfInfoParser
    {
        public static IInfoData Parse(string infoField)
        {
            if (string.IsNullOrEmpty(infoField)) return null;

            Dictionary<string, string> infoKeyValue = ExtractInfoFields(infoField);

            int[] ciEnd                    = null;
            int[] ciPos                    = null;
            int? end                       = null;
            int? refRepeatCount            = null;
            string repeatUnit              = null;
            int? jointSomaticNormalQuality = null;
            double? strandBias             = null;
            int? svLen                     = null;
            VariantType svType             = VariantType.unknown;

            foreach ((string key, string value) in infoKeyValue)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "CIEND":
                        ciEnd = value.SplitToArray();
                        break;
                    case "CIPOS":
                        ciPos = value.SplitToArray();
                        break;
                    case "END":
                        end = value.GetNullableInt();
                        break;
                    case "REF":
                        refRepeatCount = Convert.ToInt32(value);
                        break;
                    case "RU":
                        repeatUnit = value;
                        break;
                    case "SB":
                        strandBias = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "SOMATICSCORE":
                        jointSomaticNormalQuality = value.GetNullableInt();
                        break;
                    case "SVLEN":
                        svLen = value.GetNullableInt();
                        if (svLen != null)
                            svLen = Math.Abs(svLen.Value);
                        break;
                    case "SVTYPE":
                        svType = GetSvType(value);
                        break;
                }
            }

            return new InfoData(ciEnd, ciPos, end, jointSomaticNormalQuality, refRepeatCount, repeatUnit, strandBias,
                svLen, svType);
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

        private static readonly Dictionary<string, string> EmptyDictionary = new Dictionary<string, string>();

        private static Dictionary<string, string> ExtractInfoFields(string infoField)
        {
            if (infoField == ".") return EmptyDictionary;

            var infoKeyValue = new Dictionary<string, string>();

            foreach (string field in infoField.OptimizedSplit(';'))
            {
                (string key, string value) = field.OptimizedKeyValue();
                if (value == null) value = "true";
                infoKeyValue[key] = value;
            }

            return infoKeyValue;
        }
    }
}