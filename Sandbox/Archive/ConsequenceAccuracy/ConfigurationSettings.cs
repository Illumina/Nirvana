namespace CdnaEndPointInvestigation
{
    public static class ConfigurationSettings
    {
        #region members

        // filenames
        public static string InputNirvanaDirectory;
        public static string InputVcfPath;
        public static string OutputVcfPath;

        // parameters
        public static bool DoNotStopOnDifference = false;
        public static bool Silent = false;

        #endregion
    }
}
