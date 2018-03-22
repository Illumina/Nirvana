using System;
using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.Positions;

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
            if (string.IsNullOrEmpty(infoField) ) return null;

            var infoKeyValue = ExtractInfoFields(infoField, out string updatedInfoField);

            int? svLen = null, end = null, copyNumber = null, depth = null, jointSomaticNormalQuality = null;
			VariantType svType = VariantType.unknown;
            var colocalizedWithCnv = false;
            int[] ciPos = null, ciEnd = null;
            double? strandBias = null, recalibratedQuality = null;
	        bool isInv3=false, isInv5=false;
	        int? refRepeatCount=null;
	        string repeatUnit=null;

			foreach (var kvp in infoKeyValue)
            {
                var key = kvp.Key;
                var value = kvp.Value;

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (key)
                {
                    case "SB":
                        strandBias = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "QSI_NT":
                    case "SOMATICSCORE":
                    case "QSS_NT":
                        jointSomaticNormalQuality = value.GetNullableValue<int>(int.TryParse);
                        break;
                    case "VQSR":
                        recalibratedQuality = value.GetNullableValue<double>(double.TryParse);
                        break;
                    case "CN": // SENECA
                        copyNumber = value.GetNullableValue<int>(int.TryParse);
                        break;
                    case "DP": // Pisces
                        depth = value.GetNullableValue<int>(int.TryParse);
                        break;
                    case "CIPOS":
                        ciPos = value.SplitToArray<int>(',', int.TryParse);
                        break;
                    case "CIEND":
                        ciEnd = value.SplitToArray<int>(',', int.TryParse);
                        break;
                    case "SVLEN":
                        svLen = value.GetNullableValue<int>(int.TryParse);
                        if (svLen != null)
                            svLen = Math.Abs(svLen.Value);
                        break;
					case "SVTYPE":
						svType = GetSvType(value);
						break;
                    case "END":
                        end = value.GetNullableValue<int>(int.TryParse);
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
            var infoFields = infoField.Split(';');

            var sb = StringBuilderCache.Acquire();

            foreach (var field in infoFields)
            {
                var keyValue = field.Split('=');

                var key = keyValue[0];
                if (TagsToRemove.Contains(key)) continue;

                sb.Append(field);
                sb.Append(';');

                if (keyValue.Length == 1) infoKeyValue[key] = "true";
                if (keyValue.Length != 1) infoKeyValue[key] = keyValue[1];
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