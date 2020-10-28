using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using RepeatExpansions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyProvider : IMitoHeteroplasmyProvider
    {
        public string Name { get; } = "MitochondrialHeteroplasmy";
        public GenomeAssembly Assembly { get; } = GenomeAssembly.rCRS;
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private const string Version = "20180410";
        private const string Description = "Variant read frequency percentiles for the Mitochondrial reference";
        private const string MitoChromUcscName = "chrM";

        private static readonly long CreateDateTicks = new DateTime(2020, 5, 21).Ticks;
        private static readonly Dictionary<string, int> AlleleToInt = new Dictionary<string, int> { { "A", 0 }, { "C", 1 }, { "G", 2 }, { "T", 3 } };
        private const int SequenceLengthMax = int.MaxValue / 4;
        private readonly Dictionary<int, (double[] Vrfs, double[] Percentiles)> _alleleToDistribution = new Dictionary<int, (double[], double[])>();

        public MitoHeteroplasmyProvider()
        {
            var dataSourceVersion = new DataSourceVersion(Name, Version, CreateDateTicks, Description);
            DataSourceVersions = new[] {dataSourceVersion};
        }

        public void Add(int position, string altAllele, IEnumerable<double> vrfs, int[] alleleDepths)
        {
            var roundedVrfs = vrfs.Select(ToRoundedVrf).ToArray();
            double[] percentiles = PercentileUtilities.ComputePercentiles(roundedVrfs.Length, alleleDepths);
            _alleleToDistribution[EncodeMitoPositionAndAltAllele(position, altAllele)] = (roundedVrfs, percentiles);
        }
        
        public double?[] GetVrfPercentiles(IChromosome chrom, int position, string[] altAlleles, double[] vrfs)
        {
            if (vrfs == null) return null;
            if (chrom.UcscName != MitoChromUcscName) return null;

            var percentiles = vrfs.Zip(altAlleles, (vrf, allele) => GetVrfPercentile(position, allele, vrf)).ToArray();
            return percentiles.All(x => x == null) ? null : percentiles;
        }

        private double? GetVrfPercentile(int position, string altAllele, double vrf)
        {
            if (string.IsNullOrEmpty(altAllele) || !AlleleToInt.ContainsKey(altAllele)) return null;

            var positionAndAltAlleleIntForm = EncodeMitoPositionAndAltAllele(position, altAllele);

            if (!_alleleToDistribution.TryGetValue(positionAndAltAlleleIntForm, out (double[] Vrfs, double[] Percentiles) data)) return null;

            return PercentileUtilities.GetPercentile(vrf, data.Vrfs, data.Percentiles);
        }

        private static int EncodeMitoPositionAndAltAllele(int position, string altAllele) => SequenceLengthMax * AlleleToInt[altAllele] + position;
        
        private static double ToRoundedVrf(double vrf) => Math.Round(vrf, 3, MidpointRounding.AwayFromZero);
    }
}