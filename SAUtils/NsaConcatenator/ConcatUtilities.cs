using IO;
using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.NsaConcatenator
{
    public static class ConcatUtilities
    {
        private static (IDataSourceVersion version, string jsonKey, bool matchByAllele, bool isArray, bool isPositional, GenomeAssembly assembly) GetIndexFields(List<NsaReader> nsaReaders)
        {
            var version       = nsaReaders[0].Version;
            var jsonKey       = nsaReaders[0].JsonKey;
            var matchByAllele = nsaReaders[0].MatchByAllele;
            var isArray       = nsaReaders[0].IsArray;
            var isPositional  = nsaReaders[0].IsPositional;
            var assembly      = nsaReaders[0].Assembly;

            var versionComparer = new DataSourceVersionComparer();
            for (var i = 1; i < nsaReaders.Count; i++) {
                if (!versionComparer.Equals(version, nsaReaders[i].Version)
                    || jsonKey       != nsaReaders[i].JsonKey
                    || matchByAllele != nsaReaders[i].MatchByAllele
                    || isArray       != nsaReaders[i].IsArray
                    || isPositional  != nsaReaders[i].IsPositional
                    || assembly      != nsaReaders[i].Assembly
                ) 
                    return (null, null, false, false, false, GenomeAssembly.Unknown);
            }

            return (version, jsonKey, matchByAllele, isArray, isPositional, assembly);
        }

        private static NsaReader GetNsaReader(ushort chromIndex, List<NsaReader> nsaReaders)
        {
            if (nsaReaders == null) return null;

            var hasDataArray = nsaReaders.Select(x => x.HasDataBlocks(chromIndex)).ToArray();
            var count = hasDataArray.Count(x => x);

            if (count > 1) throw new DataMisalignedException("Only one of the NSA files should have data for a given chromosome.");

            for (var i = 0; i < hasDataArray.Length; i++) {
                if (hasDataArray[i] == false) continue;

                return nsaReaders[i];
            }
            return null;
        }

        public static void ConcatenateNsaFiles(IEnumerable<string> filePaths, string outFilePrefix) {
            if(filePaths == null || !filePaths.Any()) return;

            var nsaReaders = new List<NsaReader>();

            foreach (var fileName in filePaths)
            {
                nsaReaders.Add(new NsaReader(FileUtilities.GetReadStream(fileName), FileUtilities.GetReadStream(fileName + SaCommon.IndexSufix)));
            }

            Console.WriteLine($"Merging {nsaReaders.Count} NSA files...");

            var (version, jsonKey, matchByAllele, isArray, isPositional, assembly) = GetIndexFields(nsaReaders);

            using (var nsaStream = FileUtilities.GetCreateStream(outFilePrefix + SaCommon.SaFileSuffix))
            using (var indexStream = FileUtilities.GetCreateStream(outFilePrefix + SaCommon.SaFileSuffix + SaCommon.IndexSufix))
            using (var nsaWriter = new NsaWriter(nsaStream, indexStream, version, null, jsonKey, matchByAllele, isArray, SaCommon.SchemaVersion, isPositional, true, false, SaCommon.DefaultBlockSize, assembly))
            {
                var chromIndices = GetChromIndices(nsaReaders);

                foreach (var chromIndex in chromIndices)
                {
                    Console.WriteLine($"Working on chromosome index: {chromIndex}");

                    nsaWriter.Write(chromIndex, GetNsaReader(chromIndex, nsaReaders));
                }

            }
        }

        private static IEnumerable<ushort> GetChromIndices(List<NsaReader> nsaReaders)
        {
            var indices = new List<ushort>();
            if (nsaReaders == null) return indices;
            foreach (var reader in nsaReaders) {
                indices.AddRange(reader.ChromosomeIndices);
            }
            return indices.Distinct();
        }
    }
}
