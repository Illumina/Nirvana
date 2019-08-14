namespace VariantAnnotation.SA
{
    public static class SaCommon
    {
        public const int DefaultBlockSize      = 8 * 1024 * 1024;
        public const ushort DataVersion        = 53;
        public const ushort SchemaVersion      = 22;

        public const double RefMinorThreshold  = 0.95;

        public const string IndexSufix         = ".idx";
        public const string SaFileSuffix       = ".nsa";
        public const string PhylopFileSuffix   = ".npd";
        public const string RefMinorFileSuffix = ".rma";
        public const string SiFileSuffix       = ".nsi";
        public const string NgaFileSuffix      = ".nga";
        public const string JsonSchemaSuffix   = ".schema";

        public const string DbsnpTag           = "dbsnp";
        public const string GlobalAlleleTag    = "globalAllele";
        public const string OneKgenTag         = "oneKg";
        public const string AncestralAlleleTag = "ancestralAllele";
        public const string RefMinorTag        = "refMinor";
        public const string GnomadTag          = "gnomad";
        public const string GnomadExomeTag     = "gnomadExome";
        public const string ClinvarTag         = "clinvar";
        public const string CosmicTag          = "cosmic";
        public const string CosmicCnvTag       = "cosmicCnv";
        public const string OnekSvTag          = "oneKg";
        public const string DgvTag             = "dgv";
        public const string ClinGenTag         = "clingen";
        public const string MitoMapTag         = "mitomap";
        public const string TopMedTag          = "topmed";
        public const string PhylopTag          = "phylopScore";
        public const string OmimTag            = "omim";
        public const string ExacScoreTag       = "exac";
        public const string GnomadGeneScoreTag = "gnomAD";
        public const string DosageSensitivityTag = "clingenDosageSensitivityMap";
        public const string SpliceAiTag        = "spliceAI";
    }
}