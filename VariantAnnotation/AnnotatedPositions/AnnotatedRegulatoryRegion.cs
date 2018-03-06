using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedRegulatoryRegion : IAnnotatedRegulatoryRegion
    {
        public IRegulatoryRegion RegulatoryRegion { get; }
        public IEnumerable<ConsequenceTag> Consequences { get; }

        public AnnotatedRegulatoryRegion(IRegulatoryRegion regulatoryRegion, List<ConsequenceTag> consequences)
        {
            RegulatoryRegion = regulatoryRegion;
            Consequences     = consequences;
        }

        public void SerializeJson(StringBuilder sb)
        {
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("id", RegulatoryRegion.Id.WithoutVersion);
            jsonObject.AddStringValue("type", RegulatoryRegion.Type.ToString());
            jsonObject.AddStringValues("consequence", Consequences?.Select(ConsequenceUtil.GetConsequence));
            sb.Append(JsonObject.CloseBrace);
        }
    }
}