using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Interface.Caches;

namespace CacheUtils.Helpers
{
    public static class RegulatoryRegionTypeHelper
    {
        private static readonly Dictionary<string, RegulatoryRegionType> StringToRegulatoryRegionTypes;

        static RegulatoryRegionTypeHelper()
        {
            StringToRegulatoryRegionTypes = new Dictionary<string, RegulatoryRegionType>
            {
                ["CTCF_binding_site"]        = RegulatoryRegionType.CTCF_binding_site,
                ["TF_binding_site"]          = RegulatoryRegionType.TF_binding_site,
                ["enhancer"]                 = RegulatoryRegionType.enhancer,
                ["open_chromatin_region"]    = RegulatoryRegionType.open_chromatin_region,
                ["promoter"]                 = RegulatoryRegionType.promoter,
                ["promoter_flanking_region"] = RegulatoryRegionType.promoter_flanking_region
            };
        }

        public static RegulatoryRegionType GetRegulatoryRegionType(string s)
        {
            if (!StringToRegulatoryRegionTypes.TryGetValue(s, out var ret))
            {
                throw new InvalidDataException($"Unable to convert [{s}] to a regulatory region");
            }

            return ret;
        }
    }
}
