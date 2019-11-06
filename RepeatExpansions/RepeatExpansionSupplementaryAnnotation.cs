using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.SA;

namespace RepeatExpansions
{
    public sealed class RepeatExpansionSupplementaryAnnotation : ISupplementaryAnnotation
    {
        private readonly List<string> _jsonEntries;
        public string JsonKey => "repeatExpansionPhenotypes";

        public RepeatExpansionSupplementaryAnnotation(List<string> jsonEntries) => _jsonEntries = jsonEntries;

        public void SerializeJson(StringBuilder sb) => sb.Append($"[{string.Join(',', _jsonEntries)}]");
    }
}
