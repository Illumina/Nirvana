using System;
using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.Interface.Positions;

namespace Vcf.Info
{
    public static class VcfInfoParser
    {
        private static readonly InfoDataBuilder            Builder         = new();
        private static readonly Dictionary<string, string> EmptyDictionary = new();

        public static IInfoData Parse(string infoField, HashSet<string> customInfoKeys=null)
        {
            if (string.IsNullOrEmpty(infoField)) return null;
            
            Dictionary<string, string> infoKeyValue = ExtractInfoFields(infoField);
            Builder.Reset();
            
            foreach ((string key, string value) in infoKeyValue)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "CIEND":
                        Builder.CiEnd = value.SplitToArray();
                        break;
                    case "CIPOS":
                        Builder.CiPos = value.SplitToArray();
                        break;
                    case "END":
                        Builder.End = value.GetNullableInt();
                        break;
                    case "EVENT":
                        Builder.BreakendEventId = value;
                        break;
                    case "REF":
                        Builder.RefRepeatCount = Convert.ToInt32(value);
                        break;
                    case "RU":
                        Builder.RepeatUnit = value;
                        break;
                    case "SB":
                        Builder.StrandBias = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "FS":
                        Builder.FisherStrandBias = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "MQ":
                        Builder.MappingQuality = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "QSI_NT":
                    case "SOMATICSCORE":
                    case "QSS_NT":
                        Builder.JointSomaticNormalQuality = value.GetNullableInt();
                        break;
                    case "SVLEN":
                        Builder.SvLength = value.GetNullableInt();
                        if (Builder.SvLength != null) Builder.SvLength = Math.Abs(Builder.SvLength.Value);
                        break;
                    case "SVTYPE":
                        Builder.SvType = value;
                        break;
                    case "VQSR":
                        Builder.RecalibratedQuality = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "IMPRECISE":
                        Builder.IsImprecise = true;
                        break;
                    case "INV3":
                        Builder.IsInv3 = true;
                        break;
                    case "INV5":
                        Builder.IsInv5 = true;
                        break;
                    case "LOD":
                        Builder.LogOddsRatio = Convert.ToDouble(value);
                        break;
                }

                if (customInfoKeys != null && customInfoKeys.Contains(key))
                {
                    Builder.CustomInfoData.Add(key, value);
                }
            }

            return Builder.Create();
        }

        private static Dictionary<string, string> ExtractInfoFields(string infoField)
        {
            if (infoField == ".") return EmptyDictionary;

            var infoKeyValue = new Dictionary<string, string>();

            foreach (string field in infoField.OptimizedSplit(';'))
            {
                (string key, string value) =   field.OptimizedKeyValue();
                value                      ??= "true";
                infoKeyValue[key]          =   value;
            }

            return infoKeyValue;
        }
    }
}