using System.Text;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedSupplementaryInterval : IAnnotatedSupplementaryInterval
    {
        public ISupplementaryInterval SupplementaryInterval { get; }
        public double? ReciprocalOverlap { get; }

        public AnnotatedSupplementaryInterval(ISupplementaryInterval supplementaryInterval, double? reciprocalOverlap)
        {
            SupplementaryInterval = supplementaryInterval;
            ReciprocalOverlap     = reciprocalOverlap;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(JsonObject.OpenBrace + SupplementaryInterval.JsonString);
            if(ReciprocalOverlap!=null)
                sb.Append(JsonObject.Comma + "\"reciprocalOverlap\":" + ReciprocalOverlap.Value.ToString("0.#####"));
            sb.Append(JsonObject.CloseBrace);

            return sb.ToString();
        }
    }
}