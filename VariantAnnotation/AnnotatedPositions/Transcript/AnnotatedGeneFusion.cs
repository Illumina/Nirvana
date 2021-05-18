using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    // ReSharper disable InconsistentNaming
    public sealed record AnnotatedGeneFusion(ITranscript transcript, int? exon, int? intron, string hgvsr, bool isInFrame, ulong geneKey,
        string[] geneSymbols) : IAnnotatedGeneFusion
    {
        // ReSharper restore InconsistentNaming

        public void SerializeJson(StringBuilder sb)
        {
            string geneId = transcript.Source == Source.Ensembl
                ? transcript.Gene.EnsemblId.ToString()
                : transcript.Gene.EntrezGeneId.ToString();

            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("transcript", transcript.Id.WithVersion);
            jsonObject.AddStringValue("bioType",    AnnotatedTranscript.GetBioType(transcript.BioType));
            jsonObject.AddIntValue("exon",   exon);
            jsonObject.AddIntValue("intron", intron);
            jsonObject.AddStringValue("geneId", geneId);
            jsonObject.AddStringValue("hgnc",   transcript.Gene.Symbol);
            jsonObject.AddStringValue("hgvsr",  hgvsr);
            jsonObject.AddBoolValue("inFrame", isInFrame);
            sb.Append(JsonObject.CloseBrace);
        }
    }
}