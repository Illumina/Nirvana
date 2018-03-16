using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using VariantAnnotation.Utilities;

namespace VariantAnnotation
{
    public static class SaReaderUtils
    {
        public static GeneDatabaseReader GetGeneAnnotationDatabaseReader(List<string> saDirs)
        {
            if (saDirs == null) return null;
            if (!saDirs.Any()) return null;

            foreach (var saDir in saDirs)
            {
                var geneAnnotationPath = Path.Combine(saDir, SaDataBaseCommon.GeneLevelAnnotationFileName);
                if (File.Exists(geneAnnotationPath)) return new GeneDatabaseReader(FileUtilities.GetReadStream(geneAnnotationPath));
            }

            return null;
        }

        private static ISupplementaryAnnotationReader GetReader(string saDir, string ucscReferenceName)
        {
            if (string.IsNullOrEmpty(saDir)) return null;

            var saPath = Path.Combine(saDir, ucscReferenceName + ".nsa");
            if (!File.Exists(saPath)) return null;

            var stream = FileUtilities.GetReadStream(saPath);
            var idxStream = FileUtilities.GetReadStream(saPath + ".idx");
            return new SaReader(stream, idxStream);
        }

        public static List<ISupplementaryAnnotationReader> GetReaders(IEnumerable<string> saDirs, string ucscReferenceName)
        {
            var readers = new List<ISupplementaryAnnotationReader>();
            if (saDirs == null) return readers;

            readers.AddRange(saDirs.Select(dir => GetReader(dir, ucscReferenceName)).Where(reader => reader != null));
            return readers;
        }


        /// <summary>
        /// get a reduced allele to represent both (trimmed) refAllele and (trimmed) altAllele 
        /// </summary>
        /// <param name="refAllele"></param>
        /// <param name="altAllele"></param>
        /// <returns></returns>
        public static string GetReducedAllele(string refAllele, string altAllele)
        {
            if (!NeedsReduction(refAllele, altAllele)) return altAllele;

            if (string.IsNullOrEmpty(altAllele))
                return  refAllele.Length.ToString(CultureInfo.InvariantCulture);


            if (string.IsNullOrEmpty(refAllele))
                return 'i' + altAllele;

            if (refAllele.Length == altAllele.Length) return altAllele;

            // its a delins 
            return refAllele.Length.ToString(CultureInfo.InvariantCulture) + altAllele;

        }
        private static bool NeedsReduction(string refAllele, string altAllele)
        {
            if (string.IsNullOrEmpty(altAllele)) return true;

            if (!string.IsNullOrEmpty(refAllele) && altAllele.All(x => x == 'N')) return false;

            return !(altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0]));
        }


        public static GenomeAssembly GetGenomeAssembly(List<string> saDirs)
        {
            if (saDirs == null || saDirs.Count == 0) return GenomeAssembly.Unknown;

            var genomeAssemblies = new HashSet<GenomeAssembly>();

            foreach (var saDir in saDirs)
            {
                var assemblies = GetGenomeAssemblies(saDir);
                genomeAssemblies.UnionWith(assemblies);
            }

            if (genomeAssemblies.Count > 1)
            {
                var assembliesInfo = string.Join(",", genomeAssemblies);
                throw new UserErrorException($"Found {genomeAssemblies.Count} set of Genome Assemblies represented in sa Dirs: {assembliesInfo}");
            }

            return genomeAssemblies.Count == 1 ? genomeAssemblies.First() : GenomeAssembly.Unknown;
        }

	    private static IEnumerable<GenomeAssembly> GetGenomeAssemblies(string saDir)
	    {
			var saFiles = Directory.GetFiles(saDir, "*.nsa");
		    if (saFiles == null) throw new UserErrorException($"Unable to find any supplementary annotation files in the following directory: {saDir}");

			var assemblies = new HashSet<GenomeAssembly>();
		    foreach (var saFile in saFiles)
		    {
			    assemblies.Add(GetHeader(saFile).GenomeAssembly);
		    }
		    return assemblies;
	    }


		private static ISupplementaryAnnotationHeader GetSaHeader(string saDir)
        {
            var saFiles = Directory.GetFiles(saDir, "*.nsa");
            if (saFiles == null) throw new UserErrorException($"Unable to find any supplementary annotation files in the following directory: {saDir}");

            return saFiles.Length > 0 ? GetHeader(saFiles.First()) : null;
        }

        private static ISupplementaryAnnotationHeader GetHeader(string saPath)
        {
            ISupplementaryAnnotationHeader header;

            using (var stream = FileUtilities.GetReadStream(saPath))
            using (var reader = new ExtendedBinaryReader(stream))
            {
                header = SaReader.GetHeader(reader);
            }

            return header;
        }

        public static IEnumerable<IDataSourceVersion> GetDataSourceVersions(List<string> saDirs)
        {
            var dataSourceVersions = new HashSet<IDataSourceVersion>(new DataSourceVersionComparer());
            if (saDirs == null || saDirs.Count == 0) return dataSourceVersions;

            foreach (var saDir in saDirs)
            {
                var header = GetSaHeader(saDir);
                if (header != null) foreach (var version in header.DataSourceVersions) dataSourceVersions.Add(version);
            }

            return dataSourceVersions;
        }
    }
}