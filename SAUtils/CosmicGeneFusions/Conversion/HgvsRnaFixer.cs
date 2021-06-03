using System;
using System.Text;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public static class HgvsRnaFixer
    {
        // COSMIC isn't using the correct HGVS notation, so we're just going to add the proper junction string (::) between each transcript
        public static string Fix(string hgvsNotation)
        {
            var                sb        = new StringBuilder();
            ReadOnlySpan<char> delimiter = "_ENST".AsSpan();
            ReadOnlySpan<char> hgvsSpan  = hgvsNotation.AsSpan();

            var numTranscripts = 0;

            while (true)
            {
                int index = hgvsSpan.IndexOf(delimiter);
                numTranscripts++;
                
                if (index == -1)
                {
                    sb.Append(hgvsSpan);
                    break;
                }

                sb.Append(hgvsSpan.Slice(0, index));
                sb.Append("::");
                hgvsSpan = hgvsSpan.Slice(index + 1);
            }

            // this is to capture HGVS entries like "ENST00000283243.12(PLA2R1):r.1_2802" which is not actually a gene fusion
            return numTranscripts == 1 ? null : sb.ToString();
        }
    }
}