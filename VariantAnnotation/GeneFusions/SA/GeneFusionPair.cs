using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.GeneFusions.SA
{
    public sealed record GeneFusionPair(ulong GeneKey, string[] GeneSymbols) : IGeneFusionPair
    {
        public bool Equals(GeneFusionPair other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return GeneKey == other.GeneKey;
        }

        public override int GetHashCode() => GeneKey.GetHashCode();
    }
}