using System.Collections.Generic;

namespace SAUtils.MergeInterimTsvs
{
    public static class ConfigurationSettings
    {
        #region members

        // filenames
        public static string OutputDirectory;
        public static readonly List<string> IntermediateFiles= new List<string>();
		public static readonly List<string> IntervalFiles = new List<string>();
        public static readonly List<string> GeneTsvFiles = new List<string>();
        public static  string MiscFile;
	    public static  string TsvFilesDirectory;
        public static string CompressedReference;

	    #endregion
		
    }
}
