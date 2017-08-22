using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public class GeneFusion:IGeneFusion
    {
        public int? Exon { get; }
        public int? Intron { get; }
        public string HgvsCodingName { get; }

        public GeneFusion(int? exon, int?intron, string hgvsCodingName)
        {
            Exon = exon;
            Intron = intron;
            HgvsCodingName = hgvsCodingName;
        }


        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("hgvsc", HgvsCodingName);
            jsonObject.AddIntValue("exon",Exon);
            jsonObject.AddIntValue("intron",Intron);
            sb.Append(JsonObject.CloseBrace);

        }
    }
}