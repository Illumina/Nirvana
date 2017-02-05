using System;
using System.Collections.Generic;

namespace VariantAnnotation.DataStructures
{
    public enum RegulatoryElementType : byte
    {
        // ReSharper disable InconsistentNaming
        CTCF_binding_site,        
        enhancer,
        open_chromatin_region,
        promoter,
        promoter_flanking_region,
        TF_binding_site
        // ReSharper restore InconsistentNaming
    }

    public static class RegulatoryElementUtilities
    {
        #region members

        private static readonly Dictionary<string, RegulatoryElementType> StringToRegulatoryElementType = new Dictionary<string, RegulatoryElementType>();

        private const string CtcfBindingSiteKey        = "CTCF_binding_site";
        private const string EnhancerKey               = "enhancer";
        private const string OpenChromatinRegionKey    = "open_chromatin_region";
        private const string PromoterKey               = "promoter";
        private const string PromoterFlankingRegionKey = "promoter_flanking_region";
        private const string TfbsKey                   = "TF_binding_site";

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        static RegulatoryElementUtilities()
        {
            AddRegulatoryElementType(CtcfBindingSiteKey,        RegulatoryElementType.CTCF_binding_site);
            AddRegulatoryElementType(EnhancerKey,               RegulatoryElementType.enhancer);
            AddRegulatoryElementType(OpenChromatinRegionKey,    RegulatoryElementType.open_chromatin_region);
            AddRegulatoryElementType(PromoterKey,               RegulatoryElementType.promoter);
            AddRegulatoryElementType(PromoterFlankingRegionKey, RegulatoryElementType.promoter_flanking_region);
            AddRegulatoryElementType(TfbsKey,                   RegulatoryElementType.TF_binding_site);
        }

        /// <summary>
        /// adds the gene symbol source to both dictionaries
        /// </summary>
        private static void AddRegulatoryElementType(string s, RegulatoryElementType regulatoryElementType)
        {
            StringToRegulatoryElementType[s] = regulatoryElementType;
        }

        /// <summary>
        /// returns the gene symbol source given the string representation
        /// </summary>
        public static RegulatoryElementType GetRegulatoryElementTypeFromString(string s)
        {
            if (s == null) throw new ArgumentNullException();

            RegulatoryElementType ret;
            if (!StringToRegulatoryElementType.TryGetValue(s, out ret))
            {
                throw new KeyNotFoundException();
            }

            return ret;
        }
    }
}
