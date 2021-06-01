using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.GeneFusions.SA
{
    public sealed record GeneFusionPair
        (ulong FusionKey, string FirstGeneSymbol, uint FirstGeneKey, string SecondGeneSymbol, uint SecondGeneKey) : IGeneFusionPair
    {
        public bool Equals(GeneFusionPair other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return FusionKey == other.FusionKey;
        }

        public override int GetHashCode() => FusionKey.GetHashCode();
    }
}