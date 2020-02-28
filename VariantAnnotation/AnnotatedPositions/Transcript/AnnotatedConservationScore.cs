using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedConservationScore : IJsonSerializer
    {
        private readonly IEnumerable<double> _scores;

        public AnnotatedConservationScore(IEnumerable<double> scores) => _scores = scores;

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValues("scores", _scores.Select(x => x.ToString("0.##")), false);
            sb.Append(JsonObject.CloseBrace);
        }
    }
}