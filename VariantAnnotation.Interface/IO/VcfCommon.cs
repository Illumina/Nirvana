﻿using System.Collections.Generic;

namespace VariantAnnotation.Interface.IO
{
    public static class VcfCommon
    {
        public const string ChromosomeHeader = "#CHROM";
        public const string GatkNonRefAllele = "<NON_REF>";
        private const string MissingValue    = ".";

        public const int MinNumColumnsSampleGenotypes = 10;

        // define the column names
        public const int ChromIndex    = 0;
        public const int PosIndex      = 1;
        public const int IdIndex       = 2;
        public const int RefIndex      = 3;
        public const int AltIndex      = 4;
        public const int QualIndex     = 5;
        public const int FilterIndex   = 6;
        public const int InfoIndex     = 7;
        public const int FormatIndex   = 8;
        public const int GenotypeIndex = 9;

        private static readonly HashSet<string> NonInformativeAltAllele =
            new HashSet<string> {"<*>", "*", "<M>", GatkNonRefAllele};

        public static readonly HashSet<string> ReferenceAltAllele =
            new HashSet<string> {MissingValue, GatkNonRefAllele};

        public static bool IsNonInformativeAltAllele(string altAllele) => NonInformativeAltAllele.Contains(altAllele);
    }
}