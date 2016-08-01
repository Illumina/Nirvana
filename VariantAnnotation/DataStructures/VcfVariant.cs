using System;
using System.Collections.Generic;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
{
    public class VcfVariant : IVariant
    {
        public string ReferenceName { get; }
        public int ReferencePosition { get; }
        public string ReferenceAllele { get; }
        public IEnumerable<string> AlternateAlleles { get; }
        public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        public string[] Fields { get; }

        public VcfVariant(string[] fields, string vcfLine, bool isGatkGenomeVcf)
        {
            Fields            = fields;
            ReferenceName     = Fields[VcfCommon.ChromIndex];
            ReferencePosition = Convert.ToInt32(Fields[VcfCommon.PosIndex]);
            ReferenceAllele   = Fields[VcfCommon.RefIndex];
            AlternateAlleles  = Fields[VcfCommon.AltIndex].Split(',');

            AdditionalInfo = new Dictionary<string, string> { { "vcfLine", vcfLine } };
            if (isGatkGenomeVcf) AdditionalInfo = new Dictionary<string, string> { { "vcfLine", vcfLine }, { "gatkGenomeVcf", "true" } };
        }
    }
}
