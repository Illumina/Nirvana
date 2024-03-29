﻿using Variants;

namespace SAUtils.ParseUtils
{
    public static class SaParseUtilities
    {
        public static VariantType GetSequenceAlterationType(string dgvType, string dgvSubType)
        {
            var sequenceAlterationType = VariantType.unknown;
            if (dgvType == "CNV")
            {
                switch (dgvSubType)
                {
                    case "deletion":
                        sequenceAlterationType = VariantType.copy_number_loss;
                        break;
                    case "duplication":
                        sequenceAlterationType = VariantType.copy_number_gain;
                        break;
                    case "gain":
                        sequenceAlterationType = VariantType.copy_number_gain;
                        break;
                    case "gain+loss":
                        sequenceAlterationType = VariantType.copy_number_variation;
                        break;
                    case "loss":
                        sequenceAlterationType = VariantType.copy_number_loss;
                        break;
                    case "insertion":
                        sequenceAlterationType = VariantType.insertion;
                        break;
                    case "mobile element insertion":
                        sequenceAlterationType = VariantType.mobile_element_insertion;
                        break;
                    case "novel sequence insertion":
                        sequenceAlterationType = VariantType.novel_sequence_insertion;
                        break;
                    case "tandem duplication":
                        sequenceAlterationType = VariantType.tandem_duplication;
                        break;
                    default:
                        sequenceAlterationType = VariantType.unknown;
                        break;
                }

            }
            else if (dgvType == "OTHER")
            {
                switch (dgvSubType)
                {
                    case "complex":
                        sequenceAlterationType = VariantType.complex_structural_alteration;
                        break;
                    case "inversion":
                        sequenceAlterationType = VariantType.inversion;
                        break;
                    case "sequence alteration":
                        sequenceAlterationType = VariantType.structural_alteration;
                        break;
                    default:
                        sequenceAlterationType = VariantType.unknown;
                        break;

                }
            }

            return sequenceAlterationType;
        }

        public static VariantType GetSequenceAlteration(string svType)
        {
            VariantType sequenceAlterationType;

            switch (svType)
            {
                case "DEL":
                    sequenceAlterationType = VariantType.copy_number_loss;
                    break;
                case "DUP":
                    sequenceAlterationType = VariantType.copy_number_gain;
                    break;
                case "CNV":
                    sequenceAlterationType = VariantType.copy_number_variation;
                    break;
                case "INS":
                    sequenceAlterationType = VariantType.insertion;
                    break;
                case "ALU":
                    sequenceAlterationType = VariantType.mobile_element_insertion;
                    break;
                case "LINE1":
                    sequenceAlterationType = VariantType.mobile_element_insertion;
                    break;
                case "SVA":
                    sequenceAlterationType = VariantType.mobile_element_insertion;
                    break;
                case "INV":
                    sequenceAlterationType = VariantType.inversion;
                    break;
                default:
                    sequenceAlterationType = VariantType.unknown;
                    break;
            }

            return sequenceAlterationType;
        }

        public static int? Add(int? x, int? y)
        {
            if (x == null && y == null) return null;
            if (x == null) return y;
            if (y == null) return x;
            return x + y;
        }
    }
}