namespace CacheUtils.DataDumperImport.FileHandling
{
    public static class GlobalImportCommon
    {
        #region members

        public const string Header = "NirvanaGlobalImport";
        public const int NumHeaderColumns = 4;

        public enum FileType : byte
        {
            Gene,
            Regulatory,
            Transcript,
            Intron,
            Exon,
            MicroRna,
            Sift,
            PolyPhen,
            CDna,
            Peptide
        }

        #endregion
    }
}
