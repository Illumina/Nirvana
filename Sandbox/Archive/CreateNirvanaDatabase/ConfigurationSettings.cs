namespace CreateNirvanaDatabase
{
    public static class ConfigurationSettings
    {
        #region members

        // filenames
        public static string InputVepDirectory;
        public static string InputReferencePath;

        public static string InputGeneSymbolsPath;
        public static string InputHgncIdsPath;
        public static string InputLrgPath;

        public static string OutputNirvanaDirectory;

        // parameters
        public static string VepReleaseDate;
        public static string OnlyProcessReferenceSequenceName;
        public static string GenomeAssembly;
        public static bool SkipExistingNirvanaFiles;
        public static ushort VepVersion = 0;
        public static bool DoNotFilterTranscripts;

        public static bool ImportRefSeqTranscripts;
        public static bool ImportEnsemblTranscripts;

        #endregion
    }
}
