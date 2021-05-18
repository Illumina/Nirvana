using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;

// ReSharper disable InconsistentNaming
namespace UnitTests.MockedData
{
    public static class Genes
    {
        public static readonly Gene MED8 =
            new(ChromosomeUtilities.Chr1, 43383908, 43389812, true, "MED8", 19971, CompactId.Convert("112950"), CompactId.Convert("ENSG00000159479"));

        public static readonly Gene SAMD13 =
            new(ChromosomeUtilities.Chr1, 84298366, 84389957, false, "SAMD13", 24582, CompactId.Convert("148418"), CompactId.Convert(
                "ENSG00000203943"));

        public static readonly Gene POTEI =
            new(ChromosomeUtilities.Chr2, 130459455, 131626428, true, "POTEI", 37093, CompactId.Convert("653269"), CompactId.Convert(
                "ENSG00000196834"));

        public static readonly Gene PTPN18 =
            new(ChromosomeUtilities.Chr2, 130356007, 130375409, false, "PTPN18", 9649, CompactId.Convert("26469"), CompactId.Convert(
                "ENSG00000072135"));

        public static readonly Gene AL078459_1 =
            new(ChromosomeUtilities.Chr1, 85276715, 85448124, false, "AL078459.1", -1, CompactId.Empty, CompactId.Convert("ENSG00000223653"));
    }
}