using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using ErrorHandling.Exceptions;
using Genome;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.PhyloP
{
    public static class PhylopCommon
    {
        public const string Header = "NirvanaPhylopDB";
        public const ushort SchemaVersion = 4;
        public const ushort DataVersion = 1;

        public const int MaxIntervalLength = 4048;

        public static IEnumerable<IDataSourceVersion> GetDataSourceVersions(string saDir)
        {
			if (saDir == null)
				return new List<IDataSourceVersion>();
			var phylopFiles = Directory.GetFiles(saDir, "*.npd");

			return  phylopFiles.Length == 0 ? new List<IDataSourceVersion>() : new List<IDataSourceVersion> { GetPhylopHeader(saDir).Version };
        }

        public static GenomeAssembly GetAssembly(string saDir)
        {
			if (saDir == null)
				return GenomeAssembly.Unknown;

			var phylopFiles = Directory.GetFiles(saDir, "*.npd");

			return phylopFiles.Length == 0 ? GenomeAssembly.Unknown : GetPhylopHeader(saDir).Item1;
        }

        private static (GenomeAssembly Assembly, IDataSourceVersion Version) GetPhylopHeader(string saDir)
        {
            var phylopFiles = Directory.GetFiles(saDir, "*.npd");
            if (phylopFiles == null || phylopFiles.Length==0) throw new UserErrorException($"Unable to find any phyloP files in the following directory: {saDir}");

            IDataSourceVersion version;
            GenomeAssembly genomeAssembly;

            using (var reader = new PhylopReader(FileUtilities.GetReadStream(phylopFiles.First())))
            {
                version        = reader.GetDataSourceVersion();
                genomeAssembly = reader.GetAssembly();
            }

            return (genomeAssembly, version);
        }

        public static Stream GetStream(string directory, string ucscReferenceName)
        {
            if (string.IsNullOrEmpty(directory)) return null;

            var phylopPath = Path.Combine(directory, ucscReferenceName + ".npd");

            return !File.Exists(phylopPath)
                ? null
                : FileUtilities.GetReadStream(phylopPath);
        }
    }
}
