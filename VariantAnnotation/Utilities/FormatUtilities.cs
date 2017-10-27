using System;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Utilities
{
	public static class FormatUtilities
	{
		public static string CombineIdAndVersion(ICompactId id, byte version) => id + "." + version;

		public static (string Id, byte Version) SplitVersion(string id)
		{
			if (id == null) return (null, 0);
			int lastPeriod = id.LastIndexOf('.');
			return lastPeriod == -1
			    ? (id, (byte) 0)
			    : (id.Substring(0, lastPeriod), byte.Parse(id.Substring(lastPeriod + 1)));
		}
	}
}