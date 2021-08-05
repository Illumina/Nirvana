namespace VariantAnnotation.AnnotatedPositions.AminoAcids
{
    internal readonly struct AminoAcidEntry
    {
        public readonly int  Triplet;
        public readonly char AminoAcid;

        public AminoAcidEntry(int triplet, char aminoAcid)
        {
            Triplet   = triplet;
            AminoAcid = aminoAcid;
        }
    }
}