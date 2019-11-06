namespace Tabix
{
    internal static class Constants
    {
        internal const int TabixMagic = 21578324;
        internal const int MinShift   = 14;
        internal const int NumLevels  = 5;
        // ReSharper disable once UnusedMember.Global
        internal const int VcfFormat  = 2;

        internal const int InitialShift       = 29;
        internal const int MaxReferenceLength = 536_870_912;
    }
}
