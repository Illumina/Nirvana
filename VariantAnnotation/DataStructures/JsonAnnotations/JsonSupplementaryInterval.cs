using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
	public sealed class JsonSupplementaryInterval : IAnnotatedSupplementaryInterval
	{
		public double? ReciprocalOverlap { get; set; }
		public string KeyName { get; }
		private List<string> JsonStrings { get; }
	    // ReSharper disable once UnassignedGetOnlyAutoProperty
		private string VcfString { get; }

		public JsonSupplementaryInterval(IInterimInterval interval)
		{
			KeyName = interval.KeyName;
			JsonStrings = new List<string> { interval.JsonString };

		}

		public IList<string> GetStrings(string format)
		{
			switch (format)
			{
				case "json":
					return FormatJsonString();
				case "vcf":
					return new List<string> { VcfString };
				default:
					return null;

			}
		}

		private IList<string> FormatJsonString()
		{
			var outStrings = new List<string>();
			foreach (var jsonString in JsonStrings)
			{
				var sb = new StringBuilder();
				sb.Append(JsonObject.OpenBrace);
				sb.Append(jsonString);
				if (ReciprocalOverlap != null)
					sb.Append(JsonObject.Comma + "\"reciprocalOverlap\":" + ReciprocalOverlap.Value.ToString("0.#####"));
				sb.Append(JsonObject.CloseBrace);

				outStrings.Add(sb.ToString());
			}

			return outStrings;
		}
	}
}
