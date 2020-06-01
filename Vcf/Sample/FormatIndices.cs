using OptimizedCore;

namespace Vcf.Sample
{
    public sealed class FormatIndices
    {
        // ReSharper disable InconsistentNaming
        internal int? AD;
        internal int? AQ;
        internal int? CN;
        internal int? DN;
        internal int? DP;
        internal int? DST;
        internal int? FT;
        internal int? GQ;
        internal int? GT;
        internal int? LQ;
        internal int? PR;
        internal int? REPCN;
        internal int? SR;
        internal int? VF;
        internal int? MCN;
        internal int? SQ;

        //legacy fields
        internal int? TAR;
        internal int? TIR;
        internal int? AU;
        internal int? CU;
        internal int? GU;
        internal int? TU;
        internal int? MCC;
        internal int? GQX;
        internal int? DPI;
        internal int? DQ;
        // ReSharper restore InconsistentNaming

        internal int NumColumns;

        private void Clear()
        {
            AD    = null;
            AQ    = null;
            CN    = null;
            DN    = null;
            DP    = null;
            DST   = null;
            FT    = null;
            GQ    = null;
            GT    = null;
            LQ    = null;
            PR    = null;
            REPCN = null;
            SR    = null;
            VF    = null;
            MCN   = null;
            SQ    = null;
            
            // legacy sample fields
            TAR = null;
            TIR = null;
            AU  = null;
            CU  = null;
            GU  = null;
            TU  = null;
            MCC = null;
            GQX = null;
            DPI = null;
            DQ  = null;

        }

        internal void Set(string formatColumn)
        {
            Clear();

            if (formatColumn == null) return;

            string[] formatCols = formatColumn.OptimizedSplit(':');
            NumColumns = formatCols.Length;

            for (var index = 0; index < NumColumns; index++)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (formatCols[index])
                {
                    case "AD":
                        AD = index;
                        break;
                    case "AQ":
                        AQ = index;
                        break;
                    case "CN":
                        CN = index;
                        break;
                    case "DN":
                        DN = index;
                        break;
                    case "DP":
                        DP = index;
                        break;
                    case "DST":
                        DST = index;
                        break;
                    case "FT":
                        FT = index;
                        break;
                    case "GQ":
                        GQ = index;
                        break;
                    case "GT":
                        GT = index;
                        break;
                    case "LQ":
                        LQ = index;
                        break;
                    case "PR":
                        PR = index;
                        break;
                    case "REPCN":
                        REPCN = index;
                        break;
                    case "SR":
                        SR = index;
                        break;
                    case "VF":
                        VF = index;
                        break;
                        case "MCN":
                        MCN = index;
                        break;
                    case "SQ":
                        SQ = index;
                        break;
                    //LEGACY
                    case "TAR":
                        TAR = index;
                        break;
                    case "TIR":
                        TIR = index;
                        break;
                    case "AU":
                        AU = index;
                        break;
                    case "GU":
                        GU = index;
                        break;
                    case "CU":
                        CU = index;
                        break;
                    case "TU":
                        TU = index;
                        break;
                    case "GQX":
                        GQX = index;
                        break;
                    case "DPI":
                        DPI = index;
                        break;
                    case "MCC":
                        MCC = index;
                        break;
                    case "DQ":
                        DQ = index;
                        break;
                }
            }
        }
    }
}
