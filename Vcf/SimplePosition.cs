﻿using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Vcf
{
    public sealed class SimplePosition : ISimplePosition
    {
        public int Start { get; }
        public int End { get; private set; }
        public Chromosome Chromosome { get; }
        public string RefAllele { get; }
        public string[] AltAlleles { get; }
        public string[] VcfFields { get; private set; }
        public bool[] IsDecomposed { get; private set; }
        public bool IsRecomposed { get; private set; }
        public string[] Vids { get; private set; }
        public List<string>[] LinkedVids { get; private set; }

        internal SimplePosition(Chromosome chromosome, int start, string refAllele, string[] altAlleles)
        {
            Chromosome = chromosome;
            Start      = start;
            RefAllele  = refAllele;
            AltAlleles = altAlleles;
        }

        public static SimplePosition GetSimplePosition(Chromosome chromosome, int position, string[] vcfFields, IVcfFilter vcfFilter)
        {
            if (vcfFilter.PassedTheEnd(chromosome, position)) return null;

            string refAllele      = vcfFields[VcfCommon.RefIndex];
            string altAlleleField = vcfFields[VcfCommon.AltIndex];
            string[] altAlleles   = altAlleleField.OptimizedSplit(',');
            int numAltAlleles     = altAlleles.Length;

            return new SimplePosition(chromosome, position, refAllele, altAlleles)
            {
                End          = altAlleleField.OptimizedStartsWith('<') || altAlleleField == "*" ? -1 : position + refAllele.Length - 1,
                VcfFields    = vcfFields,
                IsDecomposed = new bool[numAltAlleles],
                Vids         = new string[numAltAlleles],
                LinkedVids   = new List<string>[numAltAlleles]
            };
        }
    }
}