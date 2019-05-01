using System;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;

namespace VariantAnnotation.Caches.Utilities
{
    public static class RnaEditUtilities
    {
        public static VariantType GetRnaEditType(IRnaEdit rnaEdit)
        {
            if (string.IsNullOrEmpty(rnaEdit.Bases)) return VariantType.deletion;

            if (rnaEdit.Start == rnaEdit.End && rnaEdit.Bases.Length == 1) return VariantType.SNV;

            if (rnaEdit.Start == rnaEdit.End + 1 && !string.IsNullOrEmpty(rnaEdit.Bases)) return VariantType.insertion;

            if (Math.Abs(rnaEdit.End - rnaEdit.Start) + 1 == rnaEdit.Bases.Length) return VariantType.MNV;

            return VariantType.unknown;
        }

        public static void SetTypesAndSort(IRnaEdit[] rnaEdits)
        {
            foreach (var rnaEdit in rnaEdits)
            {
                if (rnaEdit.Type != VariantType.unknown) return;
                rnaEdit.Type = GetRnaEditType(rnaEdit);
            }

            Array.Sort(rnaEdits);
        }
    }
}