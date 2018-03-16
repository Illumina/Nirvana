namespace Vcf.Sample
{
    public sealed class FormatIndices
    {
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

        // SMN1
        internal int? MAD;
        internal int? SCH;
        internal int? PLG;
        internal int? PCN;
        internal int? DCS;
        internal int? DID;
        internal int? DST;
        internal int? PCH;
        internal int? CHC;
        // ReSharper restore InconsistentNaming

        // PEPE
        internal int? AQ;
        internal int? LQ;

        /// <summary>
        /// extracts the index from each genotype format field
        /// </summary>
        internal static FormatIndices Extract(string formatColumn)
        {
            // sanity check: make sure we have a format column
            if (formatColumn == null) return null;

            var formatIndices = new FormatIndices();
            var formatCols    = formatColumn.Split(':');

            for (var index = 0; index < formatCols.Length; index++)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
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
                    case "MAD":
                        formatIndices.MAD = index;
                        break;
                    case "SCH":
                        formatIndices.SCH = index;
                        break;
                    case "PLG":
                        formatIndices.PLG = index;
                        break;
                    case "PCN":
                        formatIndices.PCN = index;
                        break;
                    case "DCS":
                        formatIndices.DCS = index;
                        break;
                    case "DID":
                        formatIndices.DID = index;
                        break;
                    case "DST":
                        formatIndices.DST = index;
                        break;
                    case "PCH":
                        formatIndices.PCH = index;
                        break;
                    case "CHC":
                        formatIndices.CHC = index;
                        break;
                    case "AQ":
                        formatIndices.AQ = index;
                        break;
                    case "LQ":
                        formatIndices.LQ = index;
                        break;
                }
            }

            return formatIndices;
        }
    }
}
