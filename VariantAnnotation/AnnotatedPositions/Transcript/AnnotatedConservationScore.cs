using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedConservationScore:IJsonSerializer
    {
        public readonly IEnumerable<double> Scores;

        public AnnotatedConservationScore(IEnumerable<double> scores)
        {
            Scores = scores;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValues("scores", Scores.Select(x => x.ToString("0.##")), false);
            sb.Append(JsonObject.CloseBrace);
        }
    }
}