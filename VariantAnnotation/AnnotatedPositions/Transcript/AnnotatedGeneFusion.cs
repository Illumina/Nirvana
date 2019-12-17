using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AnnotatedGeneFusion : IAnnotatedGeneFusion
    {
        public int? Exon { get; }
        public int? Intron { get; }
        public IGeneFusion[] GeneFusions { get; }

        public AnnotatedGeneFusion(int? exon, int? intron, IGeneFusion[] geneFusions)
        {
            Exon        = exon;
            Intron      = intron;
            GeneFusions = geneFusions;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddIntValue("exon", Exon);
            jsonObject.AddIntValue("intron", Intron);
            jsonObject.AddObjectValues("fusions", GeneFusions);
            sb.Append(JsonObject.CloseBrace);
        }
    }
}