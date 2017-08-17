using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class OverlappingTranscript:IOverlappingTranscript
    {
        public ICompactId Id { get; }
        public string GeneName { get; }
        public bool IsPartionalOverlap { get; }
        public bool IsCanonical { get; }
        public byte Version { get; }


        public OverlappingTranscript(ICompactId transcriptId, string geneName,byte version,bool isCanonical, bool isPartialOverlap)
        {
            Id = transcriptId;
            GeneName = geneName;
            IsPartionalOverlap = isPartialOverlap;
            Version = version;
            IsCanonical = isCanonical;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);
            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("transcript", Id+"."+Version);
            jsonObject.AddStringValue("hgnc",GeneName);
            jsonObject.AddBoolValue("isCanonical",IsCanonical);
            jsonObject.AddBoolValue("partialOverlap",IsPartionalOverlap);
            sb.Append(JsonObject.CloseBrace);

        }
    }
}