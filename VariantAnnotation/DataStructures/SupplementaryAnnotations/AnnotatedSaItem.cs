using System.Collections.Generic;
using System.Text;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class AnnotatedSaItem : IAnnotatedSA
    {
        public bool? IsAlleleSpecific { get; }

        public string KeyName { get; }

        public string VcfKeyName { get; }
        public bool IsArray { get; }

        private string[] JsonStrings { get; }
        private string VcfString { get; }


        public AnnotatedSaItem(ISaDataSource satItem, bool? isAlleleSpecific)
        {
            KeyName          = satItem.KeyName;
            VcfKeyName       = satItem.VcfkeyName;
            JsonStrings      = satItem.JsonStrings;
            VcfString        = satItem.VcfString;
            IsAlleleSpecific = isAlleleSpecific;
            IsArray          = satItem.IsArray;
        }

        public IList<string> GetStrings(string format)
        {
            switch (format)
            {
                case "json":
                    return FormatJsonString();
                case "vcf":
                    return VcfString.Split(',');
                default:
                    return null;

            }
        }

        private IList<string> FormatJsonString()
        {
            var outStrings = new List<string>();
            if (JsonStrings == null) return null;
            foreach (var jsonString in JsonStrings)
            {
                var sb = new StringBuilder();
                sb.Append(JsonObject.OpenBrace);
                sb.Append(jsonString);
                if (IsAlleleSpecific != null && IsAlleleSpecific.Value)
                    sb.Append(JsonObject.Comma + "\"isAlleleSpecific\":true");
                sb.Append(JsonObject.CloseBrace);

                outStrings.Add(sb.ToString());
            }

            return outStrings;
        }
    }
}