using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedSaDataSource : IAnnotatedSaDataSource
    {
        public ISaDataSource SaDataSource { get; }
        public bool IsAlleleSpecific { get; }

        public AnnotatedSaDataSource(ISaDataSource dataSource, string saAltAllele)
        {
            SaDataSource = dataSource;
            if (!dataSource.MatchByAllele && dataSource.AltAllele == saAltAllele) IsAlleleSpecific = true;
        }

        public IList<string> GetJsonStrings()
        {
            var jsonStrings = SaDataSource.JsonStrings;
            if (jsonStrings == null) return null;

            var outStrings = new List<string>();

            foreach (var jsonString in jsonStrings)
            {
                var sb = StringBuilderCache.Acquire();
                sb.Append(JsonObject.OpenBrace);
                sb.Append(jsonString);
                if (IsAlleleSpecific) sb.Append(JsonObject.Comma + "\"isAlleleSpecific\":true");
                sb.Append(JsonObject.CloseBrace);
                outStrings.Add(StringBuilderCache.GetStringAndRelease(sb));
            }

            return outStrings;
        }

        public IEnumerable<string> GetVcfStrings()
        {
            return SaDataSource.VcfString.Split(',');
        }
    }
}