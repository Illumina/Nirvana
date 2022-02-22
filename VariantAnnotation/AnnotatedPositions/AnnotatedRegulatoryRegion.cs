using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cache.Data;
using JSON;
using VariantAnnotation.Interface.AnnotatedPositions;
using JsonObject = VariantAnnotation.IO.JsonObject;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedRegulatoryRegion : IJsonSerializer
    {
        public RegulatoryRegion RegulatoryRegion { get; }
        public IEnumerable<ConsequenceTag> Consequences { get; }

        public AnnotatedRegulatoryRegion(RegulatoryRegion regulatoryRegion, List<ConsequenceTag> consequences)
        {
            RegulatoryRegion = regulatoryRegion;
            Consequences     = consequences;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("id", RegulatoryRegion.Id);
            jsonObject.AddStringValue("type", RegulatoryRegion.BioType.ToString());
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
            sb.Append(JsonObject.CloseBrace);
        }
    }
}