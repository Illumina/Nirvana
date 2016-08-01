using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomAnnotationAccuracy
{
	public static class ConfigurationSettings
	{
		#region members
		// filenames
		public static string CacheDirectory;
		public static string VcfPath;
		public static string CompressedReferencePath;

		// parameters
		public static bool DoNotStopOnDifference = false;
		public static bool Silent = false;
		public static bool IsRefSeq = false;

		#endregion
	}
}
