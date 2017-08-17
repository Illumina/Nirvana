using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public class GeneFusionAnnotation:IGeneFusionAnnotation
    {
        public int? Exon { get; }
        public int? Intron { get; }
        public IGeneFusion[] GeneFusions { get; }

        public GeneFusionAnnotation(int? exon, int? intron, IEnumerable<IGeneFusion> geneFusions)
        {
            Exon = exon;
            Intron = intron;
            GeneFusions = geneFusions.ToArray();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddIntValue("exon",Exon);
            jsonObject.AddIntValue("intron",Intron);
            jsonObject.AddObjectValues("fusions",GeneFusions);
            sb.Append(JsonObject.CloseBrace);

            return sb.ToString();
        }
    }
}