namespace VariantAnnotation.Interface.AnnotatedPositions
{
    // AminoAcidEdit describes an override where a triplet at a particular position always codes for an
    // alternative amino acid (e.g. non-AUG start codon).
    public readonly struct AminoAcidEdit
    {
        public readonly int  Position;
        public readonly char AminoAcid;

        public AminoAcidEdit(int position, char aminoAcid)
        {
            Position  = position;
            AminoAcid = aminoAcid;
        }
    }
}