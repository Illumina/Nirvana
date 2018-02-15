using CommonUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class GeneFusionAnnotation : IGeneFusionAnnotation
    {
        public int? Exon { get; }
        public int? Intron { get; }
        public IGeneFusion[] GeneFusions { get; }

        public GeneFusionAnnotation(int? exon, int? intron, IGeneFusion[] geneFusions)
        {
            Exon        = exon;
            Intron      = intron;
            GeneFusions = geneFusions;
        }

        public override string ToString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddIntValue("exon", Exon);
            jsonObject.AddIntValue("intron", Intron);
            jsonObject.AddObjectValues("fusions", GeneFusions);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}