using System;
using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;

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
            string svType                  = null;
            double? normalLod              = null;
            double? tumorLod               = null;

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
                        svType = value;
                        break;
                    case "NLOD":
                        normalLod = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "TLOD":
                        tumorLod = value.GetNullableValue<double>(double.TryParse);
                        break;
                }
            }

            return new InfoData(ciEnd, ciPos, end, jointSomaticNormalQuality, refRepeatCount, repeatUnit, strandBias,
                svLen, svType);
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