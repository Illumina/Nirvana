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
        private const int PositionIndex              = 0;
        private const int AltIndex                   = 2;
        private const int VrfIndex                   = 3;
        private const int AlleleDepthIndex           = 4;

        private const string ResourceName = "MitoHeteroplasmy.Resources.MitoHeteroplasmy.tsv.gz";
        public static MitoHeteroplasmyProvider GetData()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName);
            if (stream == null) throw new NullReferenceException("Unable to read from the Mitochondrial Heteroplasmy file");

            using var gzStream = new GZipStream(stream, CompressionMode.Decompress);
            using var reader = new StreamReader(gzStream);
            reader.ReadLine();
            
            var mitoHeteroplasmyData = new MitoHeteroplasmyProvider();
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) break;

                var fields = line.OptimizedSplit('\t');
                var position = int.Parse(fields[PositionIndex]);
                var vrfs = fields[VrfIndex].Split(',').Select(double.Parse);
                var alleleDepths = fields[AlleleDepthIndex].Split(',').Select(int.Parse).ToArray();
                mitoHeteroplasmyData.Add(position, fields[AltIndex], vrfs, alleleDepths);
            }

            return mitoHeteroplasmyData;
        }
    }
}