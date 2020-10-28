using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using OptimizedCore;

namespace MitoHeteroplasmy
{
    public static class MitoHeteroplasmyReader
    {
        private const int PositionIndex  = 1;
        private const int RefIndex       = 2;
        private const int AltIndex       = 3;
        private const int VrfBinsIndex   = 4;
        private const int VrfCountsIndex = 5;

        private const string ResourceName = "MitoHeteroplasmy.Resources.MitoHeteroplasmy.tsv.gz";
        public static MitoHeteroplasmyProvider GetProvider()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null) throw new NullReferenceException("Unable to read from the Mitochondrial Heteroplasmy file");

            using var gzStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzStream);

            string line;
            
            var heteroplasmyProvider = new MitoHeteroplasmyProvider();
            while ((line = reader.ReadLine())!=null)
            {
                if(line.StartsWith("#")) continue;
                
                var fields    = line.OptimizedSplit('\t');
                var position  = int.Parse(fields[PositionIndex]);
                var refAllele = fields[RefIndex];
                var altAllele = fields[AltIndex];
                if (altAllele=="." || !(refAllele.Length == 1 && altAllele.Length == 1)) continue;
                
                var vrfs         = fields[VrfBinsIndex].Split(',').Select(double.Parse);
                var alleleDepths = fields[VrfCountsIndex].Split(',').Select(int.Parse).ToArray();
                heteroplasmyProvider.Add(position, altAllele, vrfs.ToArray(), alleleDepths);
            }

            return heteroplasmyProvider;
        }
    }
}