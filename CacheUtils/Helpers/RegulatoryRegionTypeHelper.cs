using System;
using System.Collections.Generic;
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
            if (s == null) throw new ArgumentNullException(nameof(s));
            if (!StringToRegulatoryRegionTypes.TryGetValue(s, out var ret)) throw new InvalidOperationException($"The specified regulatory region type ({s}) was not found in the RegulatoryRegionType enum.");
            return ret;
        }
    }
}
