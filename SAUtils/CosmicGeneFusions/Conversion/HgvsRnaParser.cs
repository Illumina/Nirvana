using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SAUtils.CosmicGeneFusions.Cache;
using VariantAnnotation.GeneFusions.Utilities;

namespace SAUtils.CosmicGeneFusions.Conversion
{
    public static class HgvsRnaParser
    {
        private static readonly Regex HgvsRegex = new(@"(ENST[^\(]+)", RegexOptions.Compiled);

        public static (string[] GeneSymbols, ulong FusionKey) GetTranscripts(string hgvsNotation, TranscriptCache transcriptCache)
        {
            (string transcriptId5, string transcriptId3) = Parse(hgvsNotation);

            (string geneId5, string geneSymbol5) = transcriptCache.GetGene(transcriptId5);
            (string geneId3, string geneSymbol3) = transcriptCache.GetGene(transcriptId3);

            ulong fusionKey = GeneFusionKey.Create(geneId5, geneId3);

            return (new[] {geneSymbol5, geneSymbol3}, fusionKey);
        }

        public static (string TranscriptId5, string TranscriptId3) Parse(string hgvsString)
        {
            // the only gene fusion involving 3 transcripts. The middle one is a bit suspicious, so we'll use the other two. (GRCh37)
            if (hgvsString == "ENST00000305877.8(BCR):r.1_2866::ENST00000372348.2(ABL1):r.511-?_511-?::ENST00000318560.5(ABL1):r.461_5766")
                return ("ENST00000305877.8", "ENST00000318560.5");

            // same situation in GRCh38
            if (hgvsString == "ENST00000305877.12(BCR):r.1_2866::ENST00000372348.6(ABL1):r.511-?_511-?::ENST00000318560.5(ABL1):r.461_5766")
                return ("ENST00000305877.12", "ENST00000318560.5");

            var transcriptIds = new List<string>();
            foreach (Match match in HgvsRegex.Matches(hgvsString)) transcriptIds.Add(match.Value);

            string[] uniqueTranscriptIds = transcriptIds.Distinct().ToArray();
            if (uniqueTranscriptIds.Length != 2) throw new InvalidDataException($"Could not identify 2 transcripts in HGVS RNA parser: {hgvsString}");

            return (uniqueTranscriptIds[0], uniqueTranscriptIds[1]);
        }
    }
}