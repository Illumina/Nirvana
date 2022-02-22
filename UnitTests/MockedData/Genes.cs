using Cache.Data;

// ReSharper disable StringLiteralTypo
// ReSharper disable IdentifierTypo
// ReSharper disable InconsistentNaming
namespace UnitTests.MockedData
{
    public static class Genes
    {
        public static readonly Gene MED8   = new("112950", "ENSG00000159479", true, 19971) {Symbol  = "MED8"};
        public static readonly Gene SAMD13 = new("148418", "ENSG00000203943", false, 24582) {Symbol = "SAMD13"};
        public static readonly Gene POTEI  = new("653269", "ENSG00000196834", true, 37093) {Symbol  = "POTEI"};
        public static readonly Gene PTPN18 = new("26469", "ENSG00000072135", false, 9649) {Symbol   = "PTPN18"};

        public static readonly Gene AL078459_1 = new(string.Empty, "ENSG00000223653", false, null)
            {Symbol = "AL078459.1"};

        public static readonly Gene VEGFA = new("7422", "ENSG00000112715", false, 12680) {Symbol = "VEGFA"};
        public static readonly Gene RYBP  = new("23429", "ENSG00000163602", true, 10480) {Symbol = "RYBP"};
    }
}