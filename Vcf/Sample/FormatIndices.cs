namespace Vcf.Sample
{
    public sealed class FormatIndices
    {
        #region members

        // ReSharper disable InconsistentNaming
        internal int? AU;
        internal int? CU;
        internal int? GU;
        internal int? TU;
        internal int? TAR;
        internal int? TIR;
        internal int? FT;
        internal int? GT;
        internal int? GQ;
        internal int? GQX;
        internal int? DP;
        internal int? DPI;
        internal int? AD;
        internal int? VF;
        internal int? MCC;
        internal int? CN;
        internal int? CI;//confidence interval for STRs
        internal int? NR;
        internal int? NV;
        internal int? DQ;
        internal int? PR;
        internal int? SR;
        internal int? RGT;
        // ReSharper restore InconsistentNaming

        #endregion

        /// <summary>
        /// extracts the index from each genotype format field
        /// </summary>
        internal static FormatIndices Extract(string formatColumn)
        {
            // sanity check: make sure we have a format column
            if (formatColumn == null) return null;

            var formatIndices = new FormatIndices();
            var formatCols = formatColumn.Split(':');

            for (var index = 0; index < formatCols.Length; index++)
            {
                switch (formatCols[index])
                {
                    case "AU":
                        formatIndices.AU = index;
                        break;
                    case "CU":
                        formatIndices.CU = index;
                        break;
                    case "GU":
                        formatIndices.GU = index;
                        break;
                    case "TU":
                        formatIndices.TU = index;
                        break;
                    case "TAR":
                        formatIndices.TAR = index;
                        break;
                    case "TIR":
                        formatIndices.TIR = index;
                        break;
                    case "FT":
                        formatIndices.FT = index;
                        break;
                    case "GT":
                        formatIndices.GT = index;
                        break;
                    case "GQ":
                        formatIndices.GQ = index;
                        break;
                    case "GQX":
                        formatIndices.GQX = index;
                        break;
                    case "DP":
                        formatIndices.DP = index;
                        break;
                    case "DPI":
                        formatIndices.DPI = index;
                        break;
                    case "AD":
                        formatIndices.AD = index;
                        break;
                    case "VF":
                        formatIndices.VF = index;
                        break;
                    case "MCC":
                        formatIndices.MCC = index;
                        break;
                    case "CN":
                        formatIndices.CN = index;
                        break;
                    case "CI":
                        formatIndices.CI = index;
                        break;
                    case "NR":
                        formatIndices.NR = index;
                        break;
                    case "NV":
                        formatIndices.NV = index;
                        break;
                    case "DQ":
                        formatIndices.DQ = index;
                        break;
                    case "PR":
                        formatIndices.PR = index;
                        break;
                    case "SR":
                        formatIndices.SR = index;
                        break;
                    case "RGT":
                        formatIndices.RGT = index;
                        break;
                }
            }

            return formatIndices;
        }
    }
}
