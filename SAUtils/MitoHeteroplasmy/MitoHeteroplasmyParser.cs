using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using IO;
using Newtonsoft.Json;
using OptimizedCore;
using SAUtils.PrimateAi;
using VariantAnnotation.Interface.Providers;

namespace SAUtils.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyParser: IDisposable
    {
        private readonly Stream _stream;
        private readonly ISequenceProvider _referenceProvider;

        public MitoHeteroplasmyParser(Stream stream, ISequenceProvider referenceProvider)
        {
            _stream = stream;
            _referenceProvider = referenceProvider;
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _referenceProvider?.Dispose();
        }

        public IEnumerable<MitoHeteroplasmyItem> GetItems()
        {

            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip comments, headers
                    if (line.OptimizedStartsWith('#')) continue;

                    var items = ExtractItems(line);
                    foreach (var item in items)
                    {
                        if (item == null) continue;
                        yield return item;
                    }
                }
            }

        }
        //MT      5       6       {"C:A":{"ad":[1],"allele_type":"alt","vrf":[0.006329113924050633],"vrf_stats":{"kurtosis":241.00408163265314,"max":0.0063291139240506328,"mean":2.5728105382319646e-05,"min":0.0,"nobs":246,"skewness":15.588588185998534,"stdev":0.00040352956522996095,"variance":1.6283611001468132e-07}}}
        private IEnumerable<MitoHeteroplasmyItem> ExtractItems(string line)
        {
            var splits = line.Split('\t');
            if(splits.Length < 4) yield break;
            var chromosomeName = splits[0];
            if (!_referenceProvider.RefNameToChromosome.ContainsKey(chromosomeName)) yield break;

            var chromosome   = _referenceProvider.RefNameToChromosome[chromosomeName];
            var position     = int.Parse(splits[1])+1; // since this is a bed file
            var info         = splits[3];
            var stats        = DeserializeStats(info);

            foreach ((string refAllele, string altAllele, AlleleStats alleleStats) in GetAlleleStats(stats))
            {
                yield return new MitoHeteroplasmyItem(chromosome, position, refAllele, altAllele, alleleStats);
            }
            
        }

        private IEnumerable<(string, string, AlleleStats)> GetAlleleStats(PositionStats stats)
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
            for (int i = 0; i < charArray.Length - 3; i++)
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