using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.JsonAnnotations
{
	public sealed class JsonSupplementaryInterval : IAnnotatedSupplementaryInterval
	{
		public double? ReciprocalOverlap { get; set; }
		public ISupplementaryInterval SupplementaryInterval { get; }

	    public JsonSupplementaryInterval(ISupplementaryInterval suppInterval)
		{
            SupplementaryInterval = suppInterval;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			

			// data section
			sb.Append(JsonObject.OpenBrace);
			sb.Append(SupplementaryInterval.GetJsonContent());

			//add reciprocal overlap
			if (ReciprocalOverlap != null)
			{
				sb.Append(",");
				var jsonObject = new JsonObject(sb);
				jsonObject.AddStringValue("reciprocalOverlap", ReciprocalOverlap.Value.ToString("0.#####"), false);
			}
				

			sb.Append(JsonObject.CloseBrace.ToString());

			return sb.ToString();
		}
	}
}
