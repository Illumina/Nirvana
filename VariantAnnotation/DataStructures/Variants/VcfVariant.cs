using System;
using VariantAnnotation.FileHandling.VCF;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.Variants
{
    public sealed class VcfVariant : IVariant
    {
        public string ReferenceName { get; }
        public int ReferencePosition { get; }
        public string ReferenceAllele { get; }
        public string[] AlternateAlleles { get; }

        public bool IsGatkGenomeVcf { get; }
        public string VcfLine { get; }

        public string[] Fields { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public VcfVariant(string[] fields, string vcfLine, bool isGatkGenomeVcf)
        {
            Fields            = fields;
            ReferenceName     = Fields[VcfCommon.ChromIndex];
            ReferencePosition = Convert.ToInt32(Fields[VcfCommon.PosIndex]);
            ReferenceAllele   = Fields[VcfCommon.RefIndex];
            AlternateAlleles  = Fields[VcfCommon.AltIndex].Split(',');
            IsGatkGenomeVcf   = isGatkGenomeVcf;
            VcfLine           = vcfLine;
        }
    }
}
