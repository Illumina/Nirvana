using System;
using System.Collections.Generic;
using System.Linq;
using Genome;

namespace MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyProvider : IMitoHeteroplasmyProvider
    {
        private const string MitoChromUcscName = "chrM";
        private static readonly Dictionary<string, int> AlleleToInt = new Dictionary<string, int> { { "A", 0 }, { "C", 1 }, { "G", 2 }, { "T", 3 } };
        private const int SequenceLengthMax = int.MaxValue / 4;

        private readonly Dictionary<int, (int[] Vrfs, int[] AlleleDepths)> _alleleToVrf = new Dictionary<int, (int[], int[])>();

        public void Add(int position, string altAllele, IEnumerable<double> vrfs, int[] alleleDepths)
        {
            var vrfsInt = vrfs.Select(ToIntVrfForm).ToArray();
            _alleleToVrf[EncodeMitoPositionAndAltAllele(position, altAllele)] = (vrfsInt, alleleDepths);
        }

        public double?[] GetVrfPercentiles(string genotypes, IChromosome chrom, int position, string[] altAlleles, double[] vrfs)
        {
            if (vrfs == null) return null;
            if (chrom.UcscName != MitoChromUcscName) return null;

            var sampleAlleles = genotypes.Split('|', '/').Where(x => x != "0") 
                                                       .Select(x => GetAlleleByGenotype(x, altAlleles));

            var percentiles = vrfs.Zip(sampleAlleles, (vrf, allele) => GetVrfPercentile(position, allele, vrf)).ToArray();
            return percentiles.All(x => x == null) ? null : percentiles;
        }

        private static string GetAlleleByGenotype(string genotypeIndex, string[] altAlleles) => altAlleles[int.Parse(genotypeIndex) - 1];

        private double? GetVrfPercentile(int position, string altAllele, double vrf)
        {
            if (string.IsNullOrEmpty(altAllele)) return null;

            var positionAndAltAlleleIntForm = EncodeMitoPositionAndAltAllele(position, altAllele);

            if (!_alleleToVrf.TryGetValue(positionAndAltAlleleIntForm, out (int[] Vrfs, int[] AlleleDepths) data)) return null;

            var scaledVrf = vrf * 1000;
            int nearestBiggerVrfIndex;
            for (nearestBiggerVrfIndex = 0; nearestBiggerVrfIndex < data.Vrfs.Length; nearestBiggerVrfIndex++)
            {
                if (data.Vrfs[nearestBiggerVrfIndex] > scaledVrf) break;
            }

            var numSmallerOrEqualAlleles = 0.0;
            var numAllAlleles = 0;
            for (var i = 0; i < data.AlleleDepths.Length; i++)
            {
                if (i < nearestBiggerVrfIndex) numSmallerOrEqualAlleles += data.AlleleDepths[i];
                numAllAlleles += data.AlleleDepths[i];
            }

            return numSmallerOrEqualAlleles / numAllAlleles;
        }

        private static int ToIntVrfForm(double vrf) => Convert.ToInt32(vrf * 1000);

        private static int EncodeMitoPositionAndAltAllele(int position, string altAllele) => SequenceLengthMax * AlleleToInt[altAllele] + position;
    }
}