using System;
using System.Collections.Generic;
using OptimizedCore;
using VariantAnnotation.IO;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class VcvItem : IComparable<int>, IComparable<VcvItem>
    {
        public readonly int          VariantId;
        public readonly string       Accession;
        public readonly string       Version;
        public readonly DateTime     LastUpdatedDate;
        public readonly ClinVarCommon.ReviewStatus ReviewStatus;
        public readonly IEnumerable<string> Significances;
        
        public VcvItem(string accession, string version, long updatedDateTicks, ClinVarCommon.ReviewStatus reviewStatus, IEnumerable<string> significances)
        {
            
            Accession       = accession;
            Version         = version;
            LastUpdatedDate = new DateTime(updatedDateTicks);
            ReviewStatus    = reviewStatus;
            Significances   = significances;

            VariantId = int.Parse(accession.Substring(3));
        }
        
        public string GetJsonString()
        {
            var sb         = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", $"{Accession}.{Version}");
            jsonObject.AddStringValue("reviewStatus", ClinVarCommon.ReviewStatusStrings[ReviewStatus]);
            jsonObject.AddStringValue("lastUpdatedDate", LastUpdatedDate.ToString("yyyy-MM-dd"));
            jsonObject.AddStringValues("significance", Significances);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public int CompareTo(int vcvId)
        {
            return VariantId.CompareTo(vcvId);
        }

        public int CompareTo(VcvItem other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return VariantId.CompareTo(other.VariantId);
        }
    }
}