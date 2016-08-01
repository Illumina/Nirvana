using VariantAnnotation.DataStructures;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Algorithms.Consequences
{
    public class BasicVariantEffects
    {
        #region members

        public bool IsCoding { get; private set; }

        public bool IsDeletion { get; private set; }

        public bool IsInsertion { get; private set; }

        // Nirvana additions
        public readonly string ReferenceCodon;
        public readonly string AlternateCodon;

        public readonly int ReferenceCodonLen;
        public int AlternateCodonLen;

        public readonly string ReferenceAminoAcids;
        public readonly string AlternateAminoAcids;

        public readonly int ReferenceAminoAcidsLen;
        public readonly int AlternateAminoAcidsLen;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public BasicVariantEffects(TranscriptAnnotation ta)
        {
            var altAllele = ta.AlternateAllele;

            // codons
            ReferenceCodon = ta.ReferenceCodon ?? "";
            AlternateCodon = ta.AlternateCodon ?? "";

            ReferenceCodonLen = ReferenceCodon.Length;
            AlternateCodonLen = AlternateCodon.Length;

            // amino acids
            ReferenceAminoAcids = ta.ReferenceAminoAcids ?? "";
            AlternateAminoAcids = ta.AlternateAminoAcids ?? "";

            ReferenceAminoAcidsLen = ReferenceAminoAcids.Length;
            AlternateAminoAcidsLen = AlternateAminoAcids.Length;

            // variant type-specific
            IsDeletion  = altAllele.VepVariantType == VariantType.deletion;
            IsInsertion = altAllele.VepVariantType == VariantType.insertion;

            // within coding region
            if (ta.HasValidCdsStart || ta.HasValidCdsEnd) IsCoding = true;
        }
    }
}
