namespace SAUtils.CreateCustomIntervalDatabase
{
	public static class ConfigurationSettings
	{
		#region members
		// output filename
		public static string OutputDirectory;
		// input filenames
		public static string BedFile;
		public static string CompressedReference;

		#endregion

		public static int NumberOfProvidedInputFiles()
		{
			return BedFile==null? 0 : 1;
		}
	}
}
