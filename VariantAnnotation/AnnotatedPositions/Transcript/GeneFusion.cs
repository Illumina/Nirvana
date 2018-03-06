using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class GeneFusion : IGeneFusion
    {
        public int? Exon { get; }
        public int? Intron { get; }
        public string HgvsCoding { get; }

        public GeneFusion(int? exon, int? intron, string hgvsCoding)
        {
            Exon       = exon;
            Intron     = intron;
            HgvsCoding = hgvsCoding;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("hgvsc", HgvsCoding);
            jsonObject.AddIntValue("exon", Exon);
            jsonObject.AddIntValue("intron", Intron);
            sb.Append(JsonObject.CloseBrace);
        }
    }
}