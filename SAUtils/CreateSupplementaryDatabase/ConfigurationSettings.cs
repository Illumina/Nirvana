using System.Collections.Generic;

namespace SAUtils.CreateSupplementaryDatabase
{
    public static class ConfigurationSettings
    {
        #region members

        // filenames
        public static string CompressedReference;
        public static string InputDbSnpFileName;
        public static string InputCosmicVcfFileName;
        public static string InputCosmicTsvFileName;
        public static string InputClinVarFileName;
        public static string InputClinVarPubmedFileName;
        public static string InputClinVarEvaluatedDateFileName;
        public static string Input1000GFileName;
        public static string InputEvsFile;
        public static string InputExacFile;
        public static string InputDgvFile;
        public static string Input1000GSvFileName;
        public static string InputClinGenFileName;
        public static string OutputSupplementaryDirectory;
        public static readonly List<string> CustomAnnotationFiles = new List<string>();


        #endregion

        public static int NumberOfProvidedInputFiles()
        {
            int count = 0;

            if (Input1000GFileName   != null) count++;
	        if (InputClinVarFileName != null) count++;
	        if (InputCosmicVcfFileName  != null) count++;
	        if (InputDbSnpFileName   != null) count++;
	        if (InputEvsFile         != null) count++;
	        if (InputExacFile		 != null) count++;
	        if (InputDgvFile		 != null) count++;
	        if (Input1000GSvFileName != null) count++;
	        if (InputClinGenFileName != null) count++;

            count += CustomAnnotationFiles.Count;

            return count;
        }
    }
}
