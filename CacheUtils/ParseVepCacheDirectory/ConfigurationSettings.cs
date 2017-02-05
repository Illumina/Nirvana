namespace CacheUtils.ParseVepCacheDirectory
{
    public static class ConfigurationSettings
    {
        // filenames
        public static string InputVepDirectory;
        public static string InputReferencePath;

        public static string OutputStub;

        // parameters
        public static string VepReleaseDate;
        public static string GenomeAssembly;
        public static ushort VepVersion = 0;

        public static bool ImportRefSeqTranscripts;
        public static bool ImportEnsemblTranscripts;
    }
}
