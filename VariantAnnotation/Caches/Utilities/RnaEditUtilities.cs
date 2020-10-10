using System;
using System.Linq;
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
        
        public static void SetTypes(IRnaEdit[] rnaEdits)
        {
            if (rnaEdits == null) return;
            foreach (var rnaEdit in rnaEdits)
            {
                rnaEdit.Type = GetRnaEditType(rnaEdit);
            }
            
        }

        public static void SetTypesAndSort(IRnaEdit[] rnaEdits)
        {
            if (rnaEdits == null) return;
            foreach (var rnaEdit in rnaEdits)
            {
                if (rnaEdit.Type != VariantType.unknown) return;
                rnaEdit.Type = GetRnaEditType(rnaEdit);
            }

            Array.Sort(rnaEdits);
        }

        public static int GetRnaEditedPosition( int cdnaPosition, IRnaEdit[] rnaEdits)
        {
            if (rnaEdits == null) return cdnaPosition;
            var offset = 0;
            foreach (var rnaEdit in rnaEdits)
            {
                if (cdnaPosition < rnaEdit.Start) break;
                switch (rnaEdit.Type)
                {
                    case VariantType.deletion:
                        offset -= rnaEdit.End - rnaEdit.Start + 1;
                        break;
                    case VariantType.insertion:
                        offset += rnaEdit.Bases.Length;
                        break;
                    
                }
            }
            return cdnaPosition + offset;
        }
        public static IRnaEdit[] RemoveDeletions(IRnaEdit[] rnaEdits)
        {
            return rnaEdits?.Where(x => x.Type != VariantType.deletion).ToArray();
        }
    }
}