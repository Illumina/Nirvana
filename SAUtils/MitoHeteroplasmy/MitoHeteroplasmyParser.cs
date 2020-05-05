using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IO;
using Newtonsoft.Json;
using OptimizedCore;

namespace SAUtils.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyParser : IDisposable
    {
        private readonly Stream _stream;

        public MitoHeteroplasmyParser(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        public IEnumerable<string> GetOutputLines()
        {
            using var reader = FileUtilities.GetStreamReader(_stream);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                // Skip empty lines.
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Skip comments, headers
                if (line.OptimizedStartsWith('#')) continue;

                foreach (string item in ExtractItems(line))
                    yield return item;
            }
        }

        //MT      5       6       {"C:A":{"ad":[1],"allele_type":"alt","vrf":[0.006329113924050633],"vrf_stats":{"kurtosis":241.00408163265314,"max":0.0063291139240506328,"mean":2.5728105382319646e-05,"min":0.0,"nobs":246,"skewness":15.588588185998534,"stdev":0.00040352956522996095,"variance":1.6283611001468132e-07}}}
        private static IEnumerable<string> ExtractItems(string line)
        {
            var splits = line.Split('\t');
            if (splits.Length < 4) yield break;

            var position = int.Parse(splits[1]) + 1; // since this is a bed file
            var info = splits[3];
            var stats = DeserializeStats(info);

            foreach ((string refAllele, string altAllele, AlleleStats alleleStats) in GetAlleleStats(stats))
            {
                (string formattedVrfs, string alleleDepths) = MergeAndSortByVrf(alleleStats);
                yield return string.Join('\t', position, refAllele, altAllele, formattedVrfs, alleleDepths);
            }

        }

        private static (string formattedVrfs, string alleleDepths) MergeAndSortByVrf(AlleleStats alleleStats)
        {
            var vrfToAd = new Dictionary<string, int>();
            foreach ((string vrf, int ad) in alleleStats.vrf.Select(x => x.ToString("0.###"))
                                                            .Zip(alleleStats.ad, (a, b) => (a, b)))
            {
                if (vrfToAd.ContainsKey(vrf)) vrfToAd[vrf] += ad;
                else vrfToAd[vrf] = ad;
            }

            var formattedVrfs = new string[vrfToAd.Count];
            var alleleDepths = new int[vrfToAd.Count];
            var i = 0;
            foreach (var vrf in vrfToAd.Keys.OrderBy(x => double.Parse(x)))
            {
                formattedVrfs[i] = vrf;
                alleleDepths[i] = vrfToAd[vrf];
                i++;
            }

            return (string.Join(',',formattedVrfs), string.Join(',', alleleDepths));
        }

        private static IEnumerable<(string, string, AlleleStats)> GetAlleleStats(PositionStats stats)
        {
            if (stats.A_C != null) yield return ("A", "C", stats.A_C);
            if (stats.A_G != null) yield return ("A", "G", stats.A_G);
            if (stats.A_T != null) yield return ("A", "T", stats.A_T);

            if (stats.C_A != null) yield return ("C", "A", stats.C_A);
            if (stats.C_G != null) yield return ("C", "G", stats.C_G);
            if (stats.C_T != null) yield return ("C", "T", stats.C_T);

            if (stats.G_A != null) yield return ("G", "A", stats.G_A);
            if (stats.G_C != null) yield return ("G", "C", stats.G_C);
            if (stats.G_T != null) yield return ("G", "T", stats.G_T);

            if (stats.T_A != null) yield return ("T", "A", stats.T_A);
            if (stats.T_C != null) yield return ("T", "C", stats.T_C);
            if (stats.T_G != null) yield return ("T", "G", stats.T_G);
        }

        public static PositionStats DeserializeStats(string s)
        {
            var charArray = s.ToCharArray();
            for (var i = 0; i < charArray.Length - 3; i++)
            {
                if (IsNucleotide(charArray[i])
                    && charArray[i + 1] == ':'
                    && IsNucleotide(charArray[i + 2]))
                    charArray[i + 1] = '_';
            }
            return JsonConvert.DeserializeObject<PositionStats>(new string(charArray));
        }

        private static bool IsNucleotide(char c)
        {
            return c == 'A' || c == 'C' || c == 'G' || c == 'T';
        }
    }
}